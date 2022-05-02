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

#import "InventioAppViewController+Snapshot.h"

@implementation InventioAppViewController (Snapshot)
- (BOOL)placeUnityInWrapperView:(UIView *)view frameInset:(float)inset
{
    if (view && self.unityView) {
        [self.view addSubview:view];
        [view addSubview:self.unityView];
        self.unityView.frame = CGRectInset(view.bounds, inset, inset);
        return YES;
    }
    return NO;
}

- (BOOL)addCameraView:(UIView *)view
{
    [self.view addSubview:view];
    [self.view sendSubviewToBack:view];
    return YES;
}

- (void)handleViewMove:(UIPanGestureRecognizer *)sender
{
    if (sender.state == UIGestureRecognizerStateChanged){
        CGPoint translation = [sender translationInView:self.view];
        CGRect frame = sender.view.frame;// self.unityView.frame;
        frame.origin.x += translation.x;
        frame.origin.y += translation.y;
        sender.view.frame = frame;
        
        // Reset the translation
        [sender setTranslation:CGPointZero inView:self.view];
    }
}

- (void)handleViewResize:(UIPinchGestureRecognizer *)sender
{
    if ((sender.state == UIGestureRecognizerStateChanged) && (sender.numberOfTouches == 2)){
        CGFloat scale = sender.scale;
        if (isnan(scale)) {
            return;
        }
        
        // Calculate a new frame for unityView
        CGRect oldFrame = sender.view.frame;
        
        CGPoint touchLocation = [sender locationInView:sender.view];
        touchLocation.x /= oldFrame.size.height;
        touchLocation.y /= oldFrame.size.width;
        
        CGSize size = CGSizeMake(oldFrame.size.width * scale, oldFrame.size.height * scale);
        CGPoint origin = CGPointMake(oldFrame.origin.x - (size.width - oldFrame.size.width) * touchLocation.y,
                                     oldFrame.origin.y - (size.height - oldFrame.size.height) * (1-touchLocation.x));
        CGRect frame = CGRectMake(origin.x, origin.y, size.width, size.height);
        
        sender.view.frame = frame;
        
        // Reset the scale
        [sender setScale:1];
    }
}

- (void)snapshotFlash
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

@end
