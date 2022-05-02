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

#import <TrackerNGCore/TrackerNGCore-Swift.h>

@class TrackerSession;
@class CLLocation;

@interface TrackerNGPluginManager : NSObject
@property (strong, nonatomic, readonly) TrackerSession *currentSession;
@property (readonly) BOOL isActive;
+ (TrackerNGPluginManager *)sharedInstance;
- (void)startSessionWithAppId:(NSString *)appId clientId:(NSString *)clientId interval:(int)interval;
- (void)saveSession;
- (void)endSession;

- (void)trackLocation:(CLLocation *)location;

- (void)trackBalloonEventWithName:(NSString *)name;
- (void)trackBirdsViewEvent;
- (void)trackMapEvent;
- (void)trackZoomEvent;
- (void)trackSocialEventWithName:(NSString *)name;
- (void)trackSnapshotEventWithCompleted:(BOOL)completed;
- (void)trackAppSpecificEventWithName:(NSString *)name data:(NSDictionary *)data;
@end
