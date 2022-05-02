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

#import "InventioViewManager.h"

#import "InventioAppController.h"
#import "UIGestureRecognizer+Inventio.h"
#import "UIAlertView+Inventio.h"

@interface InventioViewManager ()
@property (strong, nonatomic) NSMutableArray *viewControllers;
@property (strong, nonatomic) NSMutableArray *viewControllersInProgress;
@property (strong, nonatomic) UIView *inventioView;
@property (strong, nonatomic) UIView *inventioRootView;
@property (strong, nonatomic) UIActivityIndicatorView *activityIndicator;
@property (strong, nonatomic) NSMutableArray *banners;

- (UIView *)bannerWithText:(NSString *)text;
- (void)presentNextBanner;
- (void)setup;
@end


@implementation InventioViewManager

#pragma mark - Properties
- (UIActivityIndicatorView *)activityIndicator
{
    if (!_activityIndicator) {
        _activityIndicator = [[UIActivityIndicatorView alloc] initWithActivityIndicatorStyle:UIActivityIndicatorViewStyleWhiteLarge];
        _activityIndicator.layer.position = CGPointMake(CGRectGetMidX(self.inventioRootView.layer.bounds),
                                                        CGRectGetMidY(self.inventioRootView.layer.bounds));
        [_activityIndicator startAnimating];
    }
    return _activityIndicator;
}

- (NSMutableArray *)banners
{
    if (!_banners) {
        _banners = [NSMutableArray new];
    }
    return _banners;
}


#pragma mark Singleton
+ (InventioViewManager *)sharedInstance
{
    static InventioViewManager *instance;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        instance = [[InventioViewManager alloc] init];
        [instance setup];
    });
    return instance;
}

#pragma mark - Private methods
- (id)init
{
    self = [super init];
    if (self) {
        _viewControllers = [[NSMutableArray alloc] init];
        _viewControllersInProgress = [[NSMutableArray alloc] init];
        
        _inventioRootView = inventioGetUnityVC().view;
    }
    return self;
}

- (void)setup
{
    if (!self.inventioRootView) {
        NSLog(@"InventioViewManager:setup: No Inventio Root View!");
        return;
    }
    _inventioView = [[UIButton alloc] initWithFrame:self.inventioRootView.bounds]; // Use a button to consume touches...
    _inventioView.backgroundColor = [UIColor clearColor];
    _inventioView.hidden = YES;
    [self.inventioRootView addSubview:_inventioView];
    _inventioView.autoresizingMask = UIViewAutoresizingFlexibleWidth | UIViewAutoresizingFlexibleHeight;
}

- (UIView *)bannerWithText:(NSString *)text
{
    CGRect bannerFrame =  self.inventioRootView.bounds;
    bannerFrame.size.height *= 0.2;
    bannerFrame.origin.y -= CGRectGetHeight(bannerFrame);

    UILabel *banner = [[UILabel alloc] initWithFrame:bannerFrame];
    banner.text = text;
    banner.numberOfLines = 3;
    banner.adjustsFontSizeToFitWidth = YES;
    banner.backgroundColor = [UIColor lightGrayColor];
    banner.textAlignment = NSTextAlignmentCenter;

    // Recalculate banner height from text
    bannerFrame.size.height = [banner textRectForBounds:banner.bounds limitedToNumberOfLines:3].size.height;
    bannerFrame.origin.y = -bannerFrame.size.height;
    banner.frame = bannerFrame;
    
    return banner;
}

- (void)presentNextBanner
{
    UIView *banner = [self.banners firstObject];
    if (banner) {
        CGRect hideFrame = banner.frame;
        CGRect showFrame = CGRectOffset(hideFrame, 0, CGRectGetHeight(hideFrame));
        [self.inventioRootView addSubview:banner];

        __block InventioViewManager *myself = self;
        [UIView animateWithDuration:0.3
                              delay:0.0
                            options:UIViewAnimationCurveEaseOut
                         animations:^{
                             banner.frame = showFrame;
                         } completion:^(BOOL finished) {
                             [UIView animateWithDuration:0.3
                                                   delay:5.0
                                                 options:UIViewAnimationCurveEaseIn
                                              animations:^{
                                                  banner.frame = hideFrame;
                                              } completion:^(BOOL finished) {
                                                  [banner removeFromSuperview];
                                                  [myself.banners removeObject:banner];
                                                  [myself presentNextBanner];
                                              }];
                         }];

    }
}

# pragma mark Public methods
+ (CGRect)screenBounds
{
    if ([[UIScreen mainScreen] respondsToSelector:@selector(coordinateSpace)]) {
        // iOS 8+ : screen bounds according to actual orientation
        return [UIScreen mainScreen].bounds;
    }
    else {
        // Screen bounds always according to portrait orientation
        CGRect screenBounds = [UIScreen mainScreen].bounds;
        return CGRectMake(0, 0, screenBounds.size.height, screenBounds.size.width);
    }
}

- (UIViewController *)showViewController:(UIViewController *)vc
{
    return [self showViewController:vc completionBlock:nil];
}

- (UIViewController *)showViewController:(UIViewController *)vc
                         completionBlock:(void (^)())block
{
    if (vc == nil) {
        NSLog(@"InventioViewManager:showViewController: No UIViewController provided!!");
        return nil;
    }

    if (!self.inventioView) {
        NSLog(@"InventioViewManager:showViewController: No inventioView!");
        return nil;
    }
    
    if (![self.viewControllers containsObject:vc]) {
        self.inventioView.hidden = NO;
        vc.view.alpha = 0.0;
        [self.inventioView addSubview:vc.view];
        vc.view.center = self.inventioView.center;
        [self.viewControllersInProgress addObject:vc];
        [UIView animateWithDuration:0.2
                         animations:^{vc.view.alpha = 1.0;}
                         completion:^(BOOL finished){
                             if ([self.viewControllersInProgress containsObject:vc]) {
                                 [self.viewControllers addObject:vc];
                                 [self.viewControllersInProgress removeObject:vc];
                                 if (block) block();
                             }
                         }];
    }
    return vc;
}

- (UIViewController *)hideViewController:(UIViewController *)vc
{
    if ([self.viewControllers containsObject:vc]) {
        [UIView animateWithDuration:0.2
                         animations:^{vc.view.alpha = 0.0;}
                         completion:^(BOOL finished){
                             [vc.view removeFromSuperview];
                             [self.viewControllers removeObject:vc];
                             if ([self.viewControllers count] == 0) {
                                 self.inventioView.hidden = YES;
                             }
                             // TODO: Delay release? Was delayed before ARC
                         }];
    }
    else if ([self.viewControllersInProgress containsObject:vc]) {
        if (vc.view.superview && (vc.view.superview == self.inventioView)) {
            [vc.view removeFromSuperview];
            [self.viewControllersInProgress removeObject:vc];
            if ([self.viewControllers count] == 0) {
                self.inventioView.hidden = YES;
            }
        }
    }
    return vc;
}

- (UIViewController *)hideViewController
{
    return [self hideViewController:nil];
}

- (void)displayActivityIndicator:(BOOL)flag
{
    if (flag) {
        if (!self.activityIndicator.layer.superlayer) {
            [self.inventioRootView.layer addSublayer:self.activityIndicator.layer];
        }
    }
    else {
        if (self.activityIndicator.layer.superlayer) {
            [self.activityIndicator.layer removeFromSuperlayer];
        }
    }
}

- (void)showBannerWithText:(NSString *)text
{
    UIView *banner = [self bannerWithText:text];
    [self.banners addObject:banner];
    if (self.banners.count == 1) {
        [self presentNextBanner];
    }
}

@end


#pragma mark - Bindings
bool _ShowViewController(const char *viewControllerName)
{
    bool res = NO;
    
    NSString *controllerName = [NSString stringWithUTF8String:viewControllerName];
    
    // Grab the controller from the given class name. Early out if we dont have it available
	Class controllerClass = NSClassFromString( controllerName );
	if( !controllerClass )
	{
		NSLog( @"InventioViewManager Binding - View controller with class name %@ does not exist", controllerName );
		return NO;
	}
    
	// if we have a nibName and we are an iPad see if the file exists and use it if it does
	if( UI_USER_INTERFACE_IDIOM() == UIUserInterfaceIdiomPad )
	{
	    // check for iPad nib
		NSString *iPadNib = [controllerName stringByAppendingString:@"-Pad"];
		
		// if the file exists, we are an iPad so load it up
		if( [[NSBundle mainBundle] pathForResource:iPadNib ofType:@"nib"] ) {
			controllerName = iPadNib;
        }
	}
	
	// Instantiate the controller and show it
	UIViewController *controller = [[controllerClass alloc] initWithNibName:controllerName bundle:nil];
    
    [[InventioViewManager sharedInstance] showViewController:controller
                                             completionBlock:nil];
    
    return res;
}

bool _HideViewController()
{
    return ([[InventioViewManager sharedInstance] hideViewController] != nil);
}

void _ShowSimpleAlert(const char *title, const char *message, const char *buttonTitle)
{
    if (!title || !buttonTitle) {
        NSLog(@"InventioViewManager Binding - Simple alert missing title or button title");
        return;
    }
    NSString *titleStr = [NSString stringWithUTF8String:title];
    NSString *messageStr = (message ? [NSString stringWithUTF8String:message] : nil);
    NSString *buttonTitleStr = [NSString stringWithUTF8String:buttonTitle];
    
    [UIAlertView showAlertWithTitle:titleStr
                            message:messageStr
                  cancelButtonTitle:nil
                      okButtonTitle:buttonTitleStr
                         completion:nil];
}
