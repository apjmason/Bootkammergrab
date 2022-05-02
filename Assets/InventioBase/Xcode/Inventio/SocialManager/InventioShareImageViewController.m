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

#import "InventioShareImageViewController.h"

#import "InventioActivityItemSource.h"
#import "InventioViewManager.h"
#import "InventioAppController.h"
#import "InventioAnalyticsManager.h"
#import "InventioSocialManager.h"

#import "TrackerNGPlugin.h"

#import <SpatialManagerCore/SpatialManagerCore.h>

@interface InventioShareImageViewController ()
@property (retain, nonatomic) IBOutlet UIImageView *imageView;

@property (retain) UIImage *image;
@property (retain) NSString *text;
@property (retain) NSString *subject;
@property (assign) CGRect fromFrame;
@property (assign) BOOL shared;
- (void)presentShareAction;
@end

@implementation InventioShareImageViewController

- (id)initWithNibName:(NSString *)nibNameOrNil bundle:(NSBundle *)nibBundleOrNil
                image:(UIImage *)image
                 text:(NSString *)text
              subject:(NSString *)subject
            fromFrame:(CGRect)frame;
{
    self = [super initWithNibName:nibNameOrNil bundle:nibBundleOrNil];
    if (self) {
        _image = image;
        _text = text;
        _subject = subject;
        _fromFrame = frame;
        _imageContentMode = UIViewContentModeScaleAspectFill;
        _shared = NO;
    }
    return self;
}

#pragma mark - UIView
- (void)viewDidLoad
{
    [super viewDidLoad];

    self.imageView.contentMode = self.imageContentMode;
    self.imageView.image = self.image;
}

- (void)viewDidAppear:(BOOL)animated
{
    [super viewDidAppear:animated];
    
    self.view.frame = self.view.superview.bounds;
    
    // Invoke later to not mess with the presentation of the view...
    [self performSelector:@selector(presentShareAction)
               withObject:nil afterDelay:0.1];

}

- (void)didReceiveMemoryWarning
{
    [super didReceiveMemoryWarning];
    // Dispose of any resources that can be recreated.
}

#pragma mark - Private Methods
- (void)presentShareAction
{
    if (self.shared) {
        return;
    }
    self.shared = YES;
    InventioActivityItemSource *textItem = [InventioActivityItemSource itemWithText:self.text];
    InventioActivityItemSource *imageItem = [InventioActivityItemSource itemWithImage:self.image];
    imageItem.subject = self.subject;
    NSArray *items = @[textItem, imageItem];
    
    NSURL *sharedURL = [InventioSocialManager sharedInstance].sharedURL;
    InventioActivityItemSource *urlItem = (sharedURL ? [InventioActivityItemSource itemWithURL:sharedURL] : nil);
    if (urlItem) {
        items = [items arrayByAddingObject:urlItem];
    }
    
    UIActivityViewController *controller = [[UIActivityViewController alloc] initWithActivityItems:items
                                                                             applicationActivities:nil];
    controller.excludedActivityTypes = [InventioActivityItemSource excludedActivities];
    controller.completionHandler = ^(NSString *activityType, BOOL completed) {
        if (completed) {
            [[InventioAnalyticsManager sharedInstance] logEventWithCategory:@"Social"
                                                                      event:@"Share image"
                                                                      label:activityType
                                                                      value:nil];
            NSString *trackerEventName = [NSString stringWithFormat:@"Share image - %@", activityType];
            [[TrackerNGPluginManager sharedInstance] trackSocialEventWithName:trackerEventName];
        }
        else {
            [[InventioAnalyticsManager sharedInstance] logEventWithCategory:@"Social"
                                                                      event:@"Share image"
                                                                      label:@"Cancel"
                                                                      value:nil];
            [[TrackerNGPluginManager sharedInstance] trackSocialEventWithName:@"Share image - Cancelled"];
        }
        [[InventioViewManager sharedInstance] hideViewController:self];
    };
    
    if (CGRectIsEmpty(self.fromFrame)) {
        self.fromFrame = CGRectInset(self.imageView.bounds,
                                     CGRectGetWidth(self.imageView.bounds) / 2.0 - 10,
                                     CGRectGetHeight(self.imageView.bounds) / 2.0 - 10);
    }
    if ([UIDevice currentDevice].userInterfaceIdiom == UIUserInterfaceIdiomPad) {
        UIPopoverController *popover = [[UIPopoverController alloc] initWithContentViewController:controller];
        [popover presentPopoverFromRect:self.fromFrame
                                 inView:self.imageView
               permittedArrowDirections:UIPopoverArrowDirectionAny
                               animated:YES];
    }
    else {
        UIViewController *parentVC = inventioGetUnityVC();
        [parentVC presentViewController:controller
                           animated:YES
                         completion:nil];
    }
}

@end
