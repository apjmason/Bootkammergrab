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

#import "InventioAnalyticsManagerGoogle.h"

#import "GAI.h"
#import "GAIDictionaryBuilder.h"

@interface InventioAnalyticsManagerGoogle ()
@property (retain, nonatomic) id<GAITracker> tracker;
@end

@implementation InventioAnalyticsManagerGoogle

#pragma mark - InventioAnalyticsManagerService
- (void)setLogLevel:(InventioAnalyticsLogLevel)level
{
    switch (level) {
        case kInventioAnalyticsLogLevelError:
            [[GAI sharedInstance].logger setLogLevel:kGAILogLevelError];
            break;
        case kInventioAnalyticsLogLevelWarning:
            [[GAI sharedInstance].logger setLogLevel:kGAILogLevelWarning];
            break;
        case kInventioAnalyticsLogLevelInfo:
            [[GAI sharedInstance].logger setLogLevel:kGAILogLevelInfo];
            break;
        case kInventioAnalyticsLogLevelVerbose:
            [[GAI sharedInstance].logger setLogLevel:kGAILogLevelVerbose];
            break;
            
        default:
            NSAssert(0, @"InventioAnalyticsManagerGoogle: Implement handling of log level %d!", level);
            break;
    }
}

- (void)activateTrackingWithKey:(NSString *)key
{
    self.tracker = [[GAI sharedInstance] trackerWithTrackingId:key];
}

- (void)logEventWithCategory:(NSString *)category
                       event:(NSString *)event
                       label:(NSString *)label
                       value:(NSNumber *)value
{
    if (self.tracker) {
        GAIDictionaryBuilder *dictionaryBuilder = [GAIDictionaryBuilder createEventWithCategory:category
                                                                                         action:event
                                                                                          label:label
                                                                                          value:value];
        
        [self.tracker send:[dictionaryBuilder build]];
        
        [[GAI sharedInstance] dispatch];
    }
    else {
        NSLog(@"InventioAnalyticsManagerGoogle: No tracker!");
    }
}

@end
