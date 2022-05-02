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

/*
 Set target compiler flag -DINVENTIO_USE_UNITY_DEFAULT_VIEW_CONTROLLER to use UnityDefaultViewController
 instead of InventioAppViewController.
 
 NB! This will disable the snapshot functionality though...
 */

#import "InventioAppController.h"

#import "UnityAppController+ViewHandling.h"
#import "UnityView.h"
#import "UnityViewControllerBase.h"

static const float SPLASH_MIN_DURATION = 5;

#ifdef INVENTIO_USE_UNITY_DEFAULT_VIEW_CONTROLLER
#else
#import "InventioAppViewController.h"
#endif

@interface InventioAppController ()

#ifdef INVENTIO_USE_UNITY_DEFAULT_VIEW_CONTROLLER
#else
@property (retain, nonatomic) InventioAppViewController *inventioViewController;
#endif

@end

IMPL_APP_CONTROLLER_SUBCLASS(InventioAppController)
@implementation InventioAppController

- (id)init
{
    self = [super init];
    if (self) {
    }
    return self;
}

- (void)willStartWithViewController:(UIViewController *)controller
{
    _unityView.contentScaleFactor	= UnityScreenScaleFactor([UIScreen mainScreen]);
    _unityView.autoresizingMask		= UIViewAutoresizingFlexibleWidth | UIViewAutoresizingFlexibleHeight;
    
#ifdef INVENTIO_USE_UNITY_DEFAULT_VIEW_CONTROLLER
    _rootController.view = _rootView = _unityView;
#else
    _rootView = self.inventioViewController.view;
#endif
}

#ifndef INVENTIO_USE_UNITY_DEFAULT_VIEW_CONTROLLER
- (UIViewController*)createAutorotatingUnityViewController
{
    _inventioViewController = [[InventioAppViewController alloc] initWithNibName:@"InventioAppViewController"
                                                       bundle:nil
                                                    unityView:_unityView];
    return _inventioViewController;
}

- (UIViewController*)createUnityViewControllerForOrientation:(UIInterfaceOrientation)orient
{
    _inventioViewController = [[InventioAppViewController alloc] initWithNibName:@"InventioAppViewController"
                                                                                bundle:nil
                                                                             unityView:_unityView];
    return _inventioViewController;
}
#endif

- (void)startUnity:(UIApplication*)application
{
    if (SPLASH_MIN_DURATION > 1.0) {
        NSBundle *bundle = [NSBundle mainBundle];
        NSString *xibName = @"LaunchScreen-iPhone";
        if ([UIDevice currentDevice].userInterfaceIdiom == UIUserInterfaceIdiomPad) {
            xibName = @"LaunchScreen-iPad";
        }
        UIView *launchView = [bundle loadNibNamed:xibName owner:nil options:nil].firstObject;
        if (launchView) {
            launchView.frame = self.unityView.bounds;
            
            [self.unityView addSubview:launchView];
            dispatch_time_t removeAt = dispatch_time(DISPATCH_TIME_NOW, SPLASH_MIN_DURATION * NSEC_PER_SEC);
            dispatch_after(removeAt, dispatch_get_main_queue(), ^{
                [launchView removeFromSuperview];
            });
        }
    }
    
    [super startUnity:application];
}
@end


#if 1 // INVENTIO
#ifdef __cplusplus
extern "C" {
#endif
    UIViewController *inventioGetUnityVC()
    {
        return UnityGetGLViewController();
    }
    
    UINavigationController *inventioGetNavigationController()
    {
        UIViewController *unityVC = UnityGetGLViewController();
        
        return unityVC.navigationController;
    }
    
#ifdef __cplusplus
}
#endif
#endif
