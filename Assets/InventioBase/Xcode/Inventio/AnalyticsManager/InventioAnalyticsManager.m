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

#import "InventioAnalyticsManager.h"

#if 0 // Something similar to InventioToolkit to access user preferences
#define INVENTIO_HAS_TOOLKIT
#import "InventioToolkit.h"
#endif
#import "InventioConfigViewController.h"

#import <objc/runtime.h> // objc_getClass

#pragma mark - Analytics Binding
void _inventioAnalyticsActivate(const char *serviceClassName, const char *key, const char *misc)
{
    assert(serviceClassName);
    assert(key);
    NSString *miscStr = nil;
    if (misc && strlen(misc)) {
        miscStr = [NSString stringWithUTF8String:misc];
    }
    [[InventioAnalyticsManager sharedInstance] activateTrackingWithServiceClass:objc_getClass(serviceClassName)
                                                                            key:[NSString stringWithUTF8String:key]
                                                                     miscString:miscStr];
}

void _inventioAnalyticsLogEvent(const char *event, const char *label)
{
    if (event) {
        NSString *labelString = (label ?
                                 [NSString stringWithUTF8String:label] :
                                 nil);
        [[InventioAnalyticsManager sharedInstance] logEventWithCategory:@"Unity"
                                                                  event:[NSString stringWithUTF8String:event]
                                                                  label:labelString
                                                                  value:nil];
    }
    else {
        NSLog(@"InventioAnalyticsManager binding: Log Event needs an Event!");
    }
}

#pragma mark - Inventio Analytics Manager
NSString *kInventioAnalyticsConsent = @"InventioAnalyticsConsent";
NSString *kInventioAnalyticsConsentCount = @"InventioAnalyticsConsentCount";

@interface InventioAnalyticsManager () <UIActionSheetDelegate>
- (void)activateService;
@property (retain, nonatomic) id<InventioAnalyticsManagerService> analyticsService;
@property (retain, nonatomic) NSString *serviceKey;
@property (retain, nonatomic) NSString *miscString;
@property (assign, nonatomic) InventioAnalyticsLogLevel logLevel;
@end

@implementation InventioAnalyticsManager

+ (InventioAnalyticsManager *)sharedInstance
{
    static id sharedInstance = nil;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        sharedInstance = [[InventioAnalyticsManager alloc] init];
    });
    return sharedInstance;
}

- (id)init
{
    self = [super init];
    if (self) {
        _analyticsService = nil;
        _serviceKey = nil;
        _logLevel = kInventioAnalyticsLogLevelError;
    }
    return self;
}

- (void)activateService
{
    if (self.miscString) {
        if ([self.analyticsService respondsToSelector:@selector(activateTrackingWithKey:miscString:)]) {
            [self.analyticsService activateTrackingWithKey:self.serviceKey miscString:self.miscString];
        }
        else {
            NSLog(@"InventioAnalyticsManager: %@ does not use miscString for activation!",
                  [self.analyticsService class]);
            [self.analyticsService activateTrackingWithKey:self.serviceKey];
        }
    }
    else {
        [self.analyticsService activateTrackingWithKey:self.serviceKey];
    }
    NSLog(@"Analytics activated");
}

- (void)activateTrackingWithServiceClass:(Class)analyticsServiceClass
                                     key:(NSString *)key
                              miscString:(NSString *)misc;
{
    NSAssert(analyticsServiceClass && key, @"InventioAnalyticsManager: service class and key must not be nil!");
    
    if (![analyticsServiceClass conformsToProtocol:@protocol(InventioAnalyticsManagerService)]) {
        NSLog(@"InventioAnalyticsManager: %@ is not an InventioAnalyticsManagerService!",
              NSStringFromClass(analyticsServiceClass));
        return;
    }

    if (_analyticsService) {
        NSLog(@"InventioAnalyticsManager: Tracking already active!");
        return;
    }

    self.serviceKey = key;
    self.miscString = misc;
    _analyticsService = [[analyticsServiceClass alloc] init];
    [self.analyticsService setLogLevel:self.logLevel];
    
#ifdef INVENTIO_HAS_TOOLKIT
    if ([InventioToolkit prefsHasKey:kInventioAnalyticsConsent]) {
        [self activateService];
    }
    else {
        // Nag three times, then give up
        int countDown = 3;
        if ([InventioToolkit prefsHasKey:kInventioAnalyticsConsentCount]) {
            countDown = [InventioToolkit prefsGetIntForKey:kInventioAnalyticsConsentCount];
        }
        if (countDown > 0) {
            // TODO: Better consent message
            NSString *message = @"This application is part of an ongoing research project at the University of Oslo, Norway.\n\nDo you give us your consent to collecting non-personal data on how this application is used, for research purposes?";
            
            UIActionSheet *sheet = [[UIActionSheet alloc] initWithTitle:message
                                                               delegate:self
                                                      cancelButtonTitle:nil
                                                 destructiveButtonTitle:@"No"
                                                      otherButtonTitles:@"Agree", nil];
            
            [sheet showInView:[[UIApplication sharedApplication] keyWindow]];
            [InventioToolkit prefsSetInt:countDown-1 forKey:kInventioAnalyticsConsentCount];
        }
    }
#else
    [self activateService];
#endif
}

- (void)setLogLevel:(InventioAnalyticsLogLevel)level
{
    _logLevel = level;
    [self.analyticsService setLogLevel:level];
}

- (void)logEventWithCategory:(NSString *)category
                       event:(NSString *)event
                       label:(NSString *)label
                       value:(NSNumber *)value
{
    [self.analyticsService logEventWithCategory:category
                                          event:event
                                          label:label
                                          value:value];
}

#pragma mark - UIActionSheet Delegate
- (void)actionSheet:(UIActionSheet *)actionSheet didDismissWithButtonIndex:(NSInteger)buttonIndex
{
#ifdef INVENTIO_HAS_TOOLKIT
    if ((buttonIndex != actionSheet.cancelButtonIndex) &&
        (buttonIndex != actionSheet.destructiveButtonIndex)) {
        [InventioToolkit prefsSetInt:1 forKey:kInventioAnalyticsConsent];
        [self activateService];
    }
    else {
        NSLog(@"Analytics denied");
    }
#endif
}
@end
