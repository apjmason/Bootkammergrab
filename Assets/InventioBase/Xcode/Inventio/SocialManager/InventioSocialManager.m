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

#import "InventioSocialManager.h"

#import "InventioViewManager.h"
#import "InventioShareImageViewController.h"
#import "InventioActivityItemSource.h"

@implementation InventioSocialManager

#pragma mark - Singleton
+ (InventioSocialManager *)sharedInstance
{
    static dispatch_once_t onceToken;
    static InventioSocialManager *shared = nil;
    
    dispatch_once(&onceToken, ^{
        shared = [[InventioSocialManager alloc] init];
    });
    return shared;

}

#pragma mark - Sharing
- (void)shareImage:(UIImage *)image text:(NSString *)text subject:(NSString *)subject fromFrame:(CGRect)fromFrame fitOnScreen:(BOOL)fit
{
    if (!image || !text || !subject) {
        return;
    }
    
    [[InventioViewManager sharedInstance] displayActivityIndicator:YES];
    InventioShareImageViewController *controller = [[InventioShareImageViewController alloc] initWithNibName:@"InventioShareImageViewController" bundle:nil
                                                                                                       image:image
                                                                                                        text:text
                                                                                                     subject:subject
                                                                                                   fromFrame:fromFrame];
    if (fit) {
        controller.imageContentMode = UIViewContentModeScaleAspectFit;
    }
    [[InventioViewManager sharedInstance] showViewController:controller
                                             completionBlock:^{
                                                 [[InventioViewManager sharedInstance] displayActivityIndicator:NO];
                                             }];
}

- (void)shareImage:(UIImage *)image text:(NSString *)text subject:(NSString *)subject fromFrame:(CGRect)fromFrame
{
    [self shareImage:image text:text subject:subject fromFrame:fromFrame fitOnScreen:NO];
}

@end


#pragma mark - Bindings
bool _ShareImageData(const char *imageData, int size, const char *text, const char *subject)
{
    NSData *iData = [NSData dataWithBytes:imageData length:size];
    if (iData) {
        UIImage *image = [UIImage imageWithData:iData];
        NSString *textStr = (text ? [NSString stringWithUTF8String:text] : nil);
        NSString *subjectStr = (subject ? [NSString stringWithUTF8String:subject] : nil);
        if (image && textStr && subjectStr) {
            [[InventioSocialManager sharedInstance] shareImage:image
                                                          text:textStr
                                                       subject:subjectStr
                                                     fromFrame:CGRectZero
                                                   fitOnScreen:YES];
            return true;
        }
    }
    return false;
}

bool _SetSharedURL(const char *url)
{
    NSString *urlStr = (url ? [NSString stringWithUTF8String:url] : nil);
    NSURL *realURL = [NSURL URLWithString:urlStr];
    if (realURL) {
        [InventioSocialManager sharedInstance].sharedURL = realURL;
        return true;
    }
    return false;
}
