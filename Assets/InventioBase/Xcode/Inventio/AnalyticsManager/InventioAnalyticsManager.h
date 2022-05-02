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

#import <Foundation/Foundation.h>


typedef NS_ENUM(NSInteger, InventioAnalyticsLogLevel) {
    kInventioAnalyticsLogLevelError,
    kInventioAnalyticsLogLevelWarning,
    kInventioAnalyticsLogLevelInfo,
    kInventioAnalyticsLogLevelVerbose
};

@protocol InventioAnalyticsManagerService <NSObject>

- (void)setLogLevel:(InventioAnalyticsLogLevel)level;

- (void)activateTrackingWithKey:(NSString *)key;
- (void)logEventWithCategory:(NSString *)category event:(NSString *)event label:(NSString *)label value:(NSNumber *)value;

@optional
- (void)activateTrackingWithKey:(NSString *)key miscString:(NSString *)misc;

@end

@interface InventioAnalyticsManager : NSObject

+ (InventioAnalyticsManager *)sharedInstance;

- (void)activateTrackingWithServiceClass:(Class)analyticsServiceClass key:(NSString *)key miscString:(NSString *)misc;
- (void)setLogLevel:(InventioAnalyticsLogLevel)level;
- (void)logEventWithCategory:(NSString *)category event:(NSString *)event label:(NSString *)label value:(NSNumber *)value;

@end
