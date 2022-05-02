//
// Sitsim AR Editor, a Unity3D add-on and framework for creating mobile applications with indirect augmented reality.
// Copyright (c) 2019 University of Oslo, CodeGrind AB and CINE (a project under the NPA EU Interreg program)
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
//

#import "InventioSnapshotManager.h"

#import "InventioSnapshotCamera.h"
#import "InventioSnapshotUnity.h"

#import "InventioAppController.h"
#import "InventioAppViewController+Snapshot.h"

#import "InventioViewManager.h"

#import "InventioConfigManager.h"
#import "InventioSocialManager.h"

#import "InventioAnalyticsManager.h"

#import "InventioLocalizationManager.h"

#import "UIAlertView+Inventio.h"

#import "TrackerNGPlugin.h"

#import <SpatialManagerCore/SpatialManagerCore.h>
#import <SpatialManagerCore/SpatialManagerCore-Swift.h>

// UIView subclass to have a CAGradientLayer instead of ordinary CALayer
@interface InventioGradientView : UIView
+ (Class)layerClass;
@end

@implementation InventioGradientView
+ (Class)layerClass
{
    return [CAGradientLayer class];
}
@end

/******************************************************************************/

/*
 Three gesture recognizers at work on the wrapperView:
    moveWrapper (pan) - To move the wrapper view (with Unity) around over the camera view
    resizeWrapper (pinch) - To resize the wrapper view (with Unity)
    snapPicture (long press) - To snap the picture (0.5s press required)
 
 One gesture recognizer at work on the camera view:
    cancelSnapshot (long press) - To cancel/leave the snapshot mode (1.0s press required)
*/
@interface InventioSnapshotManager ()
@property (retain, nonatomic) InventioAppViewController *appViewController;
@property (retain, nonatomic) InventioGradientView *wrapperView;
@property (retain, nonatomic) InventioSnapshotCamera *camera;
@property (retain, nonatomic) InventioSnapshotUnity *snapshotUnity;

@property (retain, nonatomic) UILabel *instructionLabel;
@property (assign, nonatomic) CGRect wrapperFrame;
@property (assign, nonatomic) CGFloat wrapperInset;
@property (retain, nonatomic) UIPanGestureRecognizer *moveWrapperGestureRecognizer;
@property (retain, nonatomic) UIPinchGestureRecognizer *resizeWrapperGestureRecognizer;
@property (retain, nonatomic) UILongPressGestureRecognizer *snapPictureGestureRecognizer;
@property (retain, nonatomic) UILongPressGestureRecognizer *cancelSnapshotGestureRecognizer;

@property (readonly, getter = isPrepared) BOOL prepared;

- (BOOL)setupContent;
- (BOOL)addContentToAppView;
- (BOOL)setupGestures;
- (InventioGradientView *)newWrapperViewWithFrame:(CGRect)frame;

- (void)handleSnap:(UILongPressGestureRecognizer *)sender;
- (void)handleCancel:(UILongPressGestureRecognizer *)sender;

- (void)waitForSnapshotsInQueue:(NSOperationQueue *)queue;
- (void)combineCameraSnapshot:(UIImage *)cameraSnapshot unitySnapshot:(UIImage *)unitySnapshot;

- (void)showInstruction;
- (void)hideInstruction;
@end

@implementation InventioSnapshotManager

#pragma mark - Singleton
+ (InventioSnapshotManager *)sharedInstance
{
    static dispatch_once_t pred;
    static InventioSnapshotManager *shared = nil;
    
    dispatch_once(&pred, ^{
        shared = [[InventioSnapshotManager alloc] init];
    });
    return shared;
}

#pragma mark - Properties
- (BOOL)isPrepared
{
    return (self.appViewController &&
            self.wrapperView &&
            self.moveWrapperGestureRecognizer &&
            self.resizeWrapperGestureRecognizer &&
            self.snapPictureGestureRecognizer &&
            self.cancelSnapshotGestureRecognizer);
}

#pragma mark - Private methods
- (BOOL)prepareSnapshot
{
    UIViewController *vc = inventioGetUnityVC();
    if (![vc isKindOfClass:[InventioAppViewController class]]) {
        return NO;
    }
    self.appViewController = (InventioAppViewController *)vc;
    
    return [self setupContent];
}

- (BOOL)setupContent
{
    if (self.wrapperView || self.camera) {
        if (self.isPrepared) {
            // Seems to be successfylly setup already
            return YES;
        }
        else {
            NSLog(@"InventioSnapshotManager:setupContent seems to be setup already, badly!");
            return NO;
        }
    }
    CGRect screenFrame = [InventioViewManager screenBounds]; //

    // Full screen camera
    _camera = [[InventioSnapshotCamera alloc] initWithViewFrame:screenFrame];

    // Have the unity view inset inside the wrapper view
    self.wrapperInset = ceil(screenFrame.size.height / 80.0);
    
    // Make wrapperView.frame bigger than screen to hide border...
    self.wrapperFrame = CGRectInset(screenFrame, -self.wrapperInset, -self.wrapperInset);
    _wrapperView = [self newWrapperViewWithFrame:self.wrapperFrame];
    
    return [self addContentToAppView];
}

- (BOOL)addContentToAppView
{
    if (self.wrapperView && self.camera.view) {
        if ([self.appViewController placeUnityInWrapperView:self.wrapperView
                                                 frameInset:self.wrapperInset] &&
            [self.appViewController addCameraView:self.camera.view]) {
            return ([InventioSnapshotCamera isAuthorized] && [self setupGestures]);
        }
    }
    NSLog(@"InventioSnapshotManager:addContentToAppView Failed to add content to hierarchy!");
    _wrapperView = nil;
    _camera = nil;
    
    return NO;
}

- (BOOL)setupGestures
{
    if (!self.wrapperView || !self.camera.view) {
        return NO;
    }
    // Create a gesture for the Wrapper View, in order to move it
    _moveWrapperGestureRecognizer = [[UIPanGestureRecognizer alloc] initWithTarget:self.appViewController
                                                                            action:@selector(handleViewMove:)];
    _moveWrapperGestureRecognizer.maximumNumberOfTouches = 1;
    _moveWrapperGestureRecognizer.enabled = NO;
    
    // Create a gesture for the Wrapper View, in order to move it
    _resizeWrapperGestureRecognizer = [[UIPinchGestureRecognizer alloc] initWithTarget:self.appViewController
                                                                                action:@selector(handleViewResize:)];
    _resizeWrapperGestureRecognizer.enabled = NO;
    
    // Create a gesture for the Wrapper View, in order to take a snapshot
    _snapPictureGestureRecognizer = [[UILongPressGestureRecognizer alloc] initWithTarget:self
                                                                                  action:@selector(handleSnap:)];
    _snapPictureGestureRecognizer.minimumPressDuration = 0.5;
    _snapPictureGestureRecognizer.enabled = NO;
    
    // Create a gesture for the Camera View, in order to cancel the snapshot
    _cancelSnapshotGestureRecognizer = [[UILongPressGestureRecognizer alloc] initWithTarget:self
                                                                                     action:@selector(handleCancel:)];
    _cancelSnapshotGestureRecognizer.minimumPressDuration = 1.0;
    _cancelSnapshotGestureRecognizer.enabled = NO;

    [_wrapperView addGestureRecognizer:_moveWrapperGestureRecognizer];
    [_wrapperView addGestureRecognizer:_resizeWrapperGestureRecognizer];
    [_wrapperView addGestureRecognizer:_snapPictureGestureRecognizer];
    [self.camera.view addGestureRecognizer:_cancelSnapshotGestureRecognizer];
    
    return YES;
}

- (InventioGradientView *)newWrapperViewWithFrame:(CGRect)frame
{
    InventioGradientView *view = [[InventioGradientView alloc] initWithFrame:frame];
    view.backgroundColor = [UIColor clearColor];
    
    // Apply gradient and rounded corners
    CAGradientLayer *gradient = (CAGradientLayer *)view.layer;
    gradient.cornerRadius = [InventioViewManager screenBounds].size.height / 40.0;
    gradient.colors = @[(id)[UIColor darkGrayColor].CGColor, (id)[UIColor blackColor].CGColor];
    gradient.startPoint = CGPointMake(1, 0.25);
    gradient.endPoint = CGPointMake(0.25, 0.75);
    gradient.opaque = YES;
    
    return view;
}

- (void)handleSnap:(UILongPressGestureRecognizer *)sender
{
    if ((sender.state == UIGestureRecognizerStateBegan) && (sender.numberOfTouches == 1)) {
        [self.appViewController snapshotFlash];
        [self hideInstruction];

        self.moveWrapperGestureRecognizer.enabled = NO;
        self.resizeWrapperGestureRecognizer.enabled = NO;
        self.snapPictureGestureRecognizer.enabled = NO;
        self.cancelSnapshotGestureRecognizer.enabled = NO;

        // Wait for the snapshots to be finished in a queue
        NSOperationQueue *queue = [NSOperationQueue new];
        InventioSnapshotUnity *snapshotUnity = [InventioSnapshotUnity new];
        if ([snapshotUnity captureUnityScreenUsingQueue:queue] &&
            [self.camera captureCameraShotUsingQueue:queue]) {
            [[TrackerNGPluginManager sharedInstance] trackSnapshotEventWithCompleted:YES];
            self.snapshotUnity = snapshotUnity;
            [NSThread detachNewThreadSelector:@selector(waitForSnapshotsInQueue:)
                                     toTarget:self
                                   withObject:queue];
        }
        else {
            // TODO: Localize
            [UIAlertView showAlertWithTitle:@"Snapshot Error"
                                    message:@"Failed to create a snapshot, please try again."
                          cancelButtonTitle:nil
                              okButtonTitle:@"Ok"
                                 completion:nil];
            [[TrackerNGPluginManager sharedInstance] trackSnapshotEventWithCompleted:NO];
            [self deactivateSnapshot];
        }
    }
}

- (void)handleCancel:(UILongPressGestureRecognizer *)sender
{
    if ((sender.state == UIGestureRecognizerStateBegan) && (sender.numberOfTouches == 1)) {
        [self hideInstruction];
        [[InventioAnalyticsManager sharedInstance] logEventWithCategory:@"Social"
                                                                  event:@"Snapshot"
                                                                  label:@"Cancel"
                                                                  value:nil];
        
        [[TrackerNGPluginManager sharedInstance] trackSnapshotEventWithCompleted:NO];
        [self deactivateSnapshot];
    }
}

- (void)waitForSnapshotsInQueue:(NSOperationQueue *)queue
{
    dispatch_async(dispatch_get_main_queue(), ^{
        [[InventioViewManager sharedInstance] displayActivityIndicator:YES];
    });

    NSDate *start = [NSDate date];
    [queue waitUntilAllOperationsAreFinished];
    NSNumber *seconds = [NSNumber numberWithDouble:-[start timeIntervalSinceNow]];
    [[InventioAnalyticsManager sharedInstance] logEventWithCategory:@"Social"
                                                              event:@"Snapshot"
                                                              label:@"Time"
                                                              value:seconds];

    [self combineCameraSnapshot:self.camera.snapshot
                  unitySnapshot:self.snapshotUnity.snapshot];
    
    self.snapshotUnity = nil;
}

- (void)combineCameraSnapshot:(UIImage *)cameraSnapshot unitySnapshot:(UIImage *)unitySnapshot
{
    // Create an UIView with cameraSnapshot
    UIImageView *cameraImageView = [[UIImageView alloc] initWithFrame:self.camera.view.bounds];
    cameraImageView.image = cameraSnapshot;
    cameraImageView.contentMode = UIViewContentModeScaleAspectFill;
    
    // Create an UIView with unitySnapshot
    UIView *unityImageWrapperView = [self newWrapperViewWithFrame:self.wrapperView.frame];
    UIImageView *unityImageView = [[UIImageView alloc] initWithFrame:self.appViewController.unityView.frame];
    unityImageView.clipsToBounds = YES;
    unityImageView.image = unitySnapshot;
    unityImageView.contentMode = UIViewContentModeScaleAspectFill;
    [unityImageWrapperView addSubview:unityImageView];
    [cameraImageView addSubview:unityImageWrapperView];
    
    // Create a snapshot
    CGRect bounds = cameraImageView.bounds;
    UIGraphicsBeginImageContextWithOptions(bounds.size, YES, 0);
    [cameraImageView.layer renderInContext:UIGraphicsGetCurrentContext()];
    UIImage *finalImage = UIGraphicsGetImageFromCurrentImageContext();
    UIGraphicsEndImageContext();
    
    __block InventioSnapshotManager *myself = self;
    dispatch_async(dispatch_get_main_queue(), ^{
        [[InventioSocialManager sharedInstance] shareImage:finalImage
                                                      text:InventioLocalizedString(@"Look at this!")
                                                   subject:[NSString stringWithFormat:@"%@ %@", InventioLocalizedString(@"Snapshot from SitSim"), [InventioConfigManager sharedInstance].appName]
                                                 fromFrame:myself.wrapperView.frame];
        // Delay deactivation to hide it behind the SocialManager
        [myself performSelector:@selector(deactivateSnapshot) withObject:nil afterDelay:0.5];
    });
}

- (void)showInstruction
{
    if (!self.instructionLabel) {
        self.instructionLabel = [[UILabel alloc] initWithFrame:CGRectMake(0, -50, self.appViewController.view.bounds.size.width, 50)];
        [self.appViewController.view addSubview:self.instructionLabel];
        self.instructionLabel.textAlignment = NSTextAlignmentCenter;
        self.instructionLabel.text = InventioLocalizedString(@"Snapshot instruction");
        self.instructionLabel.backgroundColor = [[UIColor whiteColor] colorWithAlphaComponent:0.5];
        
        __block InventioSnapshotManager *mySelf = self;
        [UIView animateWithDuration:0.5
                         animations:^{
            mySelf.instructionLabel.frame = CGRectMake(0, 0, mySelf.appViewController.view.bounds.size.width, 50);
        }];
        
        [self performSelector:@selector(hideInstruction) withObject:nil afterDelay:8.0];
    }
}

- (void)hideInstruction
{
    if (self.instructionLabel) {
        __block InventioSnapshotManager *mySelf = self;
        [UIView animateWithDuration:0.5
                         animations:^{
                             mySelf.instructionLabel.frame = CGRectMake(0, -50, mySelf.camera.view.bounds.size.width, 50);
                         } completion:^(BOOL finished) {
                             [mySelf.instructionLabel removeFromSuperview];
                             mySelf.instructionLabel = nil;
                         }];
    }
}

- (BOOL)activateSnapshot
{
    if (self.isPrepared) {
        __block InventioSnapshotManager *myself = self;
        [self.camera setupCameraPreviewWithCompletion:^(BOOL success) {
            if (success && [myself.camera startWithCompletion:^{
                CGRect screenBounds = [InventioViewManager screenBounds];
                CGRect targetWrapperFrame = CGRectMake(screenBounds.size.width * 1.0/3.0,
                                                       screenBounds.size.height  * 2.0/3.0 - 5.0,
                                                       screenBounds.size.width * 1.0/3.0,
                                                       screenBounds.size.height  * 1.0/3.0);
                [UIView animateWithDuration:0.5
                                 animations:^{
                                     myself.wrapperView.frame = targetWrapperFrame;
                                 }
                                 completion:^(BOOL finished) {
                                     myself.moveWrapperGestureRecognizer.enabled = YES;
                                     myself.resizeWrapperGestureRecognizer.enabled = YES;
                                     myself.snapPictureGestureRecognizer.enabled = YES;
                                     myself.cancelSnapshotGestureRecognizer.enabled = YES;
                                     [myself showInstruction];
                                 }];
                
                [[SpatialManager sharedInstance] updateTilt:0 animated:YES];
            }]) {
                [self.appViewController enableUnityInput:NO];
                [[InventioAnalyticsManager sharedInstance] logEventWithCategory:@"Social"
                                                                          event:@"Snapshot"
                                                                          label:@"Activate"
                                                                          value:nil];
            }
            else {
                // Access denied or failed to activate camera => disable menu item
                // TODO: Remove menu item snapshot
            }
        }];
        return YES;
    }
    return NO;
}

- (BOOL)deactivateSnapshot
{
    if (self.isPrepared && self.camera.isRunning) {
        self.moveWrapperGestureRecognizer.enabled = NO;
        self.resizeWrapperGestureRecognizer.enabled = NO;
        self.snapPictureGestureRecognizer.enabled = NO;
        self.cancelSnapshotGestureRecognizer.enabled = NO;
        __block InventioSnapshotManager *myself = self;
        [UIView animateWithDuration:0.5
                         animations:^{
                             myself.wrapperView.frame = myself.wrapperFrame;
                         }
                         completion:^(BOOL finished) {
                             [myself.camera stopWithCompletion:nil];
                             [myself.appViewController enableUnityInput:YES];
                         }];
        if ([InventioConfigManager sharedInstance].useTiltOffset) {
            [[SpatialManager sharedInstance] updateTilt:[InventioConfigManager sharedInstance].defaultTiltValue  animated:YES];
        }
    }
    return YES;
}

@end


#pragma mark - Bindings
bool _EnableSnapshot()
{
#ifdef INVENTIO_USE_UNITY_DEFAULT_VIEW_CONTROLLER
    return false;
#else
    return [[InventioSnapshotManager sharedInstance] prepareSnapshot];
#endif
}

bool _ActivateSnapshot()
{
#ifdef INVENTIO_USE_UNITY_DEFAULT_VIEW_CONTROLLER
    return false;
#else
    return [[InventioSnapshotManager sharedInstance] activateSnapshot];
#endif
}

void _SetCaptureScreenshotCallback(CaptureScreenshotCallback callback)
{
    captureCallback = callback;
}
