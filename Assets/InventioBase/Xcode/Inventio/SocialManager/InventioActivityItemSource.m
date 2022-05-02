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

#import "InventioActivityItemSource.h"

#import "InventioConfigManager.h"

@interface InventioActivityImageItemSource : InventioActivityItemSource
@property (strong) UIImage *image;
@end

@interface InventioActivityTextItemSource : InventioActivityItemSource
@property (strong) NSString *text;
@end

@interface InventioActivityURLItemSource : InventioActivityItemSource
@property (strong) NSURL *url;
@end


#pragma mark -
@implementation InventioActivityItemSource
+ (NSArray *)excludedActivities
{
    // Some of these activities doesn't exist on iOS 6
    if ((&UIActivityTypeAddToReadingList != NULL) && (&UIActivityTypePostToVimeo != NULL)) {
        return @[UIActivityTypePrint,
                 UIActivityTypeCopyToPasteboard,
                 UIActivityTypeAssignToContact,
                 UIActivityTypeAddToReadingList, // iOS 7+
                 UIActivityTypePostToVimeo]; // iOS 7+
    }
    else {
        return @[UIActivityTypePrint,
                 UIActivityTypeCopyToPasteboard,
                 UIActivityTypeAssignToContact];
    }
}

#pragma mark Factory methods
+ (InventioActivityItemSource *)itemWithImage:(UIImage *)image
{
    InventioActivityImageItemSource *item = [InventioActivityImageItemSource new];
    item.image = image;
    return item;
}

+ (InventioActivityItemSource *)itemWithText:(NSString *)text
{
    InventioActivityTextItemSource *item = [InventioActivityTextItemSource new];
    item.text = text;
    return item;
}

+ (InventioActivityItemSource *)itemWithURL:(NSURL *)url
{
    InventioActivityURLItemSource *item = [InventioActivityURLItemSource new];
    item.url = url;
    return item;
}


#pragma mark ActivityItemSource Protocol
- (id)activityViewController:(UIActivityViewController *)activityViewController
         itemForActivityType:(NSString *)activityType
{
    return nil;
}

- (id)activityViewControllerPlaceholderItem:(UIActivityViewController *)activityViewController
{
    return nil;
}

- (NSString *)activityViewController:(UIActivityViewController *)activityViewController
              subjectForActivityType:(NSString *)activityType
{
    return self.subject;
}

@end


#pragma mark -
@implementation InventioActivityImageItemSource
- (id)activityViewController:(UIActivityViewController *)activityViewController
         itemForActivityType:(NSString *)activityType
{
    return self.image;
}

- (UIImage *)activityViewController:(UIActivityViewController *)activityViewController
      thumbnailImageForActivityType:(NSString *)activityType
                      suggestedSize:(CGSize)size
{
    UIImage *image = image;
    if (image) {
        UIGraphicsBeginImageContextWithOptions(size, NO, 0.0);
        [self.image drawInRect:CGRectMake(0, 0, size.width, size.height)];
        image = UIGraphicsGetImageFromCurrentImageContext();
        UIGraphicsEndImageContext();
    }
    
    return image;
}

- (id)activityViewControllerPlaceholderItem:(UIActivityViewController *)activityViewController
{
    return self.image;
}
@end


#pragma mark -
@implementation InventioActivityTextItemSource
- (id)activityViewController:(UIActivityViewController *)activityViewController
         itemForActivityType:(NSString *)activityType
{
    return self.text;
}

- (id)activityViewControllerPlaceholderItem:(UIActivityViewController *)activityViewController
{
    return self.text;
}
@end


#pragma mark -
@implementation InventioActivityURLItemSource
- (id)activityViewController:(UIActivityViewController *)activityViewController
         itemForActivityType:(NSString *)activityType
{
    return self.url;
}

- (id)activityViewControllerPlaceholderItem:(UIActivityViewController *)activityViewController
{
    return self.url;
}
@end
