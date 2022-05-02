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

#import "InventioSnapshotCamera.h"

#import <AVFoundation/AVFoundation.h>

@interface InventioSnapshotCamera ()
@property (retain, nonatomic) AVCaptureDeviceInput *cameraInput;
@property (retain, nonatomic) AVCaptureVideoPreviewLayer *cameraPreviewLayer;
@property (retain, nonatomic) AVCaptureSession *cameraSession;
@property (retain, nonatomic) AVCaptureStillImageOutput *imageOutput;

@property(copy)void(^startCompletion)();
@property(copy)void(^stopCompletion)();

- (void)setupCameraPreviewWithCompletion:(void (^)(BOOL success))completion;
@end

@implementation InventioSnapshotCamera

#pragma mark - Properties
- (BOOL)isReady
{
    return (self.view &&
            self.cameraInput &&
            self.cameraPreviewLayer &&
            self.cameraSession &&
            self.imageOutput);
}

- (BOOL)isRunning
{
    return self.cameraSession.isRunning;
}

#pragma mark -
+ (BOOL)isAuthorized
{
    BOOL flag = true;
    if ([[AVCaptureDevice class] respondsToSelector:@selector(authorizationStatusForMediaType:)]) {
        // Available in iOS 7+
        AVAuthorizationStatus status = [AVCaptureDevice authorizationStatusForMediaType:AVMediaTypeVideo];
        flag = ((status == AVAuthorizationStatusNotDetermined) || (status == AVAuthorizationStatusAuthorized));
    }
    return flag;
}

- (id)initWithViewFrame:(CGRect)frame
{
    self = [super init];
    if (self) {
        _view = [[UIView alloc] initWithFrame:frame];
    }
    return self;
}

- (void)dealloc
{
    [_view removeFromSuperview];
}

- (void)setupCameraPreviewWithCompletion:(void (^)(BOOL success))completion
{
    if (self.isReady) {
        if (completion) {
            completion(YES);
        }
        return;
    }
    
    __block InventioSnapshotCamera *myself = self;
    void(^setupBlock)(BOOL) = ^(BOOL granted){
        BOOL success = granted && [myself setupCameraSession];
        dispatch_async(dispatch_get_main_queue(), ^{
            myself.cameraPreviewLayer = [AVCaptureVideoPreviewLayer layerWithSession:myself.cameraSession];
            myself.cameraPreviewLayer.videoGravity = AVLayerVideoGravityResizeAspectFill;
            myself.cameraPreviewLayer.frame = myself.view.bounds;
            myself.cameraPreviewLayer.connection.videoOrientation = AVCaptureVideoOrientationLandscapeRight;
            myself.cameraPreviewLayer.opaque = YES;
            
            [myself.view.layer addSublayer:myself.cameraPreviewLayer];
            
            myself.view.opaque = YES;
            myself.view.hidden = YES;
            [[NSNotificationCenter defaultCenter] addObserver:myself
                                                     selector:@selector(cameraSessionDidStartNotification:)
                                                         name:AVCaptureSessionDidStartRunningNotification
                                                       object:myself.cameraSession];
            
            [[NSNotificationCenter defaultCenter] addObserver:myself
                                                     selector:@selector(cameraSessionDidStopNotification:)
                                                         name:AVCaptureSessionDidStopRunningNotification
                                                       object:myself.cameraSession];
            
            if (completion) {
                completion(success);
            }
        });
    };
    
    if ([[AVCaptureDevice class] respondsToSelector:@selector(requestAccessForMediaType:completionHandler:)]) {
        // Available in iOS 7+
        [AVCaptureDevice requestAccessForMediaType:AVMediaTypeVideo completionHandler:setupBlock];
    }
    else {
        setupBlock(YES);
    }
}

- (BOOL)setupCameraSession
{
    NSError *error = nil;
    AVCaptureDevice *device = [AVCaptureDevice defaultDeviceWithMediaType:AVMediaTypeVideo];
    if ([device lockForConfiguration:&error]) {
        [device unlockForConfiguration];
    }
    
    self.cameraInput = [AVCaptureDeviceInput deviceInputWithDevice:device error:&error];
    if (!self.cameraInput) {
        NSLog(@"InventioSnapshotCamera: AVCaptureDeviceInput Error: %@", [error localizedDescription]);
        // TODO: Handle the error creating deviceInput appropriately.
        _view = nil;
        return NO;
    }
    
    _cameraSession = [[AVCaptureSession alloc] init];
    
    [self.cameraSession beginConfiguration];
    [self.cameraSession addInput:self.cameraInput];
    [self.cameraSession setSessionPreset:AVCaptureSessionPresetiFrame1280x720];
    
    _imageOutput = [[AVCaptureStillImageOutput alloc] init];
    NSDictionary *outputSettings = [[NSDictionary alloc] initWithObjectsAndKeys:
                                    AVVideoCodecJPEG, AVVideoCodecKey, nil];
    [self.imageOutput setOutputSettings:outputSettings];
    if ([self.cameraSession canAddOutput:self.imageOutput]) {
        [self.cameraSession addOutput:self.imageOutput];
    }
    else {
        NSLog(@"Camera Session Can't add image output!");
        // TODO: Handle the error appropriately.
        self.imageOutput = nil;
    }
    
    [self.cameraSession commitConfiguration];
    return YES;
}

#pragma mark - Public interface
- (BOOL)startWithCompletion:(void (^)(void))completion
{
    if (!self.cameraSession) {
        NSLog(@"InventioSnapshotCamera:startCamera has no cameraSession!");
        return NO;
    }
    if (self.cameraSession.isRunning) {
        NSLog(@"InventioSnapshotManager:activateSnapshot: Camera is already started!");
        return YES;
    }
    
    self.view.hidden = NO;
    
    self.startCompletion = completion;
    __block InventioSnapshotCamera *myself = self;
    dispatch_async(dispatch_queue_create(NULL, NULL), ^{
        [myself.cameraSession startRunning];
    });
    return YES;
}

- (BOOL)stopWithCompletion:(void (^)(void))completion
{
    if (!self.cameraSession) {
        NSLog(@"InventioSnapshotCamera:startCamera has no cameraSession!");
        return NO;
    }
    if (!self.cameraSession.isRunning) {
        NSLog(@"InventioSnapshotManager:activateSnapshot: Camera is not started!");
        return YES;
    }

    self.view.hidden = YES;
    
    self.stopCompletion = completion;
    [self.cameraSession stopRunning];
    return YES;
}

- (BOOL)captureCameraShotUsingQueue:(NSOperationQueue *)queue
{
    BOOL success = false;
    
    _snapshot = nil;
    
    // Assumption: Always in LandscapeRight...
    AVCaptureConnection *connection = [self.imageOutput connectionWithMediaType:AVMediaTypeVideo];
    connection.videoOrientation = AVCaptureVideoOrientationLandscapeRight;
    
    AVCaptureConnection *videoConnection = nil;
    for (AVCaptureConnection *connection in self.imageOutput.connections) {
        for (AVCaptureInputPort *port in [connection inputPorts]) {
            if ([[port mediaType] isEqual:AVMediaTypeVideo] ) {
                videoConnection = connection;
                break;
            }
        }
        if (videoConnection) { break; }
    }

    if (videoConnection) {
        success = YES;
        
        // Wait in queue for the snapshot, maximum 10s
        __block InventioSnapshotCamera *myself = self;
        [queue addOperationWithBlock:^{
            NSDate *startDate = [NSDate date];
            while (!myself.snapshot && ([startDate timeIntervalSinceNow] > -10)) {
                usleep(100000);
            }
        }];
        
        [self.imageOutput captureStillImageAsynchronouslyFromConnection:videoConnection
                                                      completionHandler:^(CMSampleBufferRef imageSampleBuffer, NSError *error)
        {
             if (imageSampleBuffer) {
                 NSData *imgData = [AVCaptureStillImageOutput jpegStillImageNSDataRepresentation:imageSampleBuffer];
                 _snapshot = [UIImage imageWithData:imgData];
             }
         }];
    }
    return success;
}

- (void)flash
{
    UIView *flashView = [[UIView alloc] initWithFrame:self.view.bounds];
    flashView.backgroundColor = [UIColor whiteColor];
    flashView.opaque = NO;
    flashView.alpha = 1.0;
    [self.view addSubview:flashView];
    [UIView animateWithDuration:0.9
                          delay:0
                        options:UIViewAnimationOptionCurveEaseOut
                     animations:^{
                         flashView.alpha = 0;
                     }
                     completion:^(BOOL finished) {
                         [flashView removeFromSuperview];
                     }];
}


#pragma mark - Notifications
- (void)cameraSessionDidStartNotification:(NSNotification *)notification
{
    if (self.startCompletion) {
        // Call completion block on main thread
        __block InventioSnapshotCamera *myself = self;
        dispatch_async(dispatch_get_main_queue(), ^{
            myself.startCompletion();
            myself.startCompletion = nil;
        });
    }
}

- (void)cameraSessionDidStopNotification:(NSNotification *)notification
{
    if (self.stopCompletion) {
        // Call completion block on main thread
        __block InventioSnapshotCamera *myself = self;
        dispatch_async(dispatch_get_main_queue(), ^{
            myself.stopCompletion();
            myself.stopCompletion = nil;
        });
    }
}

@end
