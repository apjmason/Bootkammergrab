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

#import "UIGestureRecognizer+Inventio.h"

#import <objc/runtime.h>

@interface InventioGestureRecognizer : NSObject

@property (copy) void(^handlerBlock)(UIGestureRecognizer *recognizer);

@end


@implementation InventioGestureRecognizer

// Called when a recognizer triggers
- (void)handleRecognizer:(UIGestureRecognizer *)recognizer
{
    if (self.handlerBlock)
        self.handlerBlock(recognizer);
}

@end


static const char kInventioGestureRecognizer;
@implementation UIGestureRecognizer (Inventio)
+ (instancetype)recognizerWithClass:(Class)recognizerClass
                            handler:(void(^)(UIGestureRecognizer *))handler
{
    InventioGestureRecognizer *inventioRecognizer = [[InventioGestureRecognizer alloc] init];
    inventioRecognizer.handlerBlock = handler;
    
    id recognizer = [[recognizerClass alloc] init];
    NSAssert([recognizer isKindOfClass:[UIGestureRecognizer class]], @"%s: Illegal recognizerClass (%s)", __PRETTY_FUNCTION__, class_getName(recognizerClass));
    
    recognizer = [recognizer initWithTarget:inventioRecognizer action:@selector(handleRecognizer:)];

    // Set the wrapper as an associated object
    objc_setAssociatedObject(recognizer, &kInventioGestureRecognizer, inventioRecognizer, OBJC_ASSOCIATION_RETAIN_NONATOMIC);
    
    return recognizer;
}

@end
