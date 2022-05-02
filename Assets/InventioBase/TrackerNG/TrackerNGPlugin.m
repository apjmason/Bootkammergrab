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

#import <TrackerNGCore/TrackerNGCore.h>
#import <TrackerNGCore/TrackerNGCore-Swift.h>

#import "TrackerNGPlugin.h"

#import <SpatialManagerCore/SpatialManagerCore.h>
#import <SpatialManagerCore/SpatialManagerCore-Swift.h>

@interface TrackerNGPluginManager ()
@property (strong, nonatomic) TrackerDevice *currentDevice;
@property (strong, nonatomic, readwrite) TrackerSession *currentSession;
@property (retain, nonatomic) NSTimer *trackerTimer;
@property (retain, nonatomic) CLLocation *lastLocation;
@property (assign, nonatomic) NSTimeInterval trackingInterval;

- (void)setupTrackerDevice;
- (void)applicationWillTerminate:(NSNotification *)notification;
- (CLLocation *)getLocationIfValid;
- (void)trackerTimerFired:(NSTimer *)timer;
@end

@implementation TrackerNGPluginManager

- (BOOL)isActive
{
    return ((self.currentSession != nil) && ([TrackerManager sharedManager].currentAppDataBase != nil));
}

+ (TrackerNGPluginManager *)sharedInstance
{
    static TrackerNGPluginManager *_instance = nil;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        _instance = [TrackerNGPluginManager new];
    });
    return _instance;
}

- (void)startSessionWithAppId:(NSString *)appId
                     interval:(int)interval
{
    if (self.isActive) {
        NSLog(@"%s: Already running a session!", __PRETTY_FUNCTION__);
        return;
    }
    
    self.trackingInterval = interval;
    [[TrackerManager sharedManager] enterApp:appId];

    [self setupTrackerDevice];
}

- (void)completeSession
{
    dispatch_async(dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_DEFAULT, 0), ^{
        UIBackgroundTaskIdentifier bgTask = [[UIApplication sharedApplication] beginBackgroundTaskWithExpirationHandler:^{
            NSLog(@"%s: Complete session expired!", __PRETTY_FUNCTION__);
        }];
        if (bgTask != UIBackgroundTaskInvalid) {
            [self.currentSession complete];
            [[UIApplication sharedApplication] endBackgroundTask:bgTask];
        }
        else {
            NSLog(@"%s: Complete session got invalid task!!!", __PRETTY_FUNCTION__);
        }
    });
}


- (void)endSession
{
    dispatch_async(dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_DEFAULT, 0), ^{
        UIBackgroundTaskIdentifier bgTask = [[UIApplication sharedApplication] beginBackgroundTaskWithExpirationHandler:^{
            NSLog(@"%s: End session expired!", __PRETTY_FUNCTION__);
        }];
        if (bgTask != UIBackgroundTaskInvalid) {
            [[TrackerManager sharedManager] leaveApp];
            self.currentSession = nil;
            [[UIApplication sharedApplication] endBackgroundTask:bgTask];
        }
        else {
            NSLog(@"%s: End session got invalid task!!!", __PRETTY_FUNCTION__);
        }
    });
    
    [[NSNotificationCenter defaultCenter] removeObserver:self];
}

- (void)setupTrackerDevice
{
    if (self.currentDevice) {
        [self setupTrackerSession];
    }
    else {
        [TrackerDevice fetchCurrentDevice:^(TrackerDevice * _Nullable device, NSError * _Nullable error) {
            if (device) {
                self.currentDevice = device;
            }
            else {
                NSLog(@"%s: Failed fetching device: %@", __PRETTY_FUNCTION__, error.localizedDescription);
                NSString *identifier = TrackerDevice.currentDeviceId;
                if (identifier) {
                    // TODO: Should be able to create device without deviceId (get deviceId in create-method)
                    self.currentDevice = [TrackerDevice create:identifier];
                }
            }
            [self setupTrackerSession];
        }];
    }
}

- (void)setupTrackerSession {
    if (!self.currentSession && self.currentDevice) {
        self.currentSession = [TrackerSession create:self.currentDevice];
    }
    
    if (!self.trackerTimer && self.currentSession) {
        self.trackerTimer = [NSTimer scheduledTimerWithTimeInterval:self.trackingInterval
                                                             target:self
                                                           selector:@selector(trackerTimerFired:)
                                                           userInfo:nil
                                                            repeats:YES];
        [[NSNotificationCenter defaultCenter] addObserver:self
                                                 selector:@selector(applicationDidEnterBackground:)
                                                     name:UIApplicationDidEnterBackgroundNotification
                                                   object:nil];
        [[NSNotificationCenter defaultCenter] addObserver:self
                                                 selector:@selector(applicationWillTerminate:)
                                                     name:UIApplicationWillTerminateNotification
                                                   object:nil];
    }
}

- (void)applicationDidEnterBackground:(NSNotification *)notification
{
    [self completeSession];
}

- (void)applicationWillTerminate:(NSNotification *)notification
{
    [self endSession];
}

- (CLLocation *)getLocationIfValid
{
    SpatialManager *spatialManager = [SpatialManager sharedInstance];
    CLLocation *location = spatialManager.deviceLocation;
    if (location && ([location distanceFromLocation:spatialManager.referenceLocation] < 1000)) {
        return location;
    }
    return nil;
}

- (void)trackLocation:(CLLocation *)location
{
    if (self.isActive) {
        [TrackPoint createWithSession:self.currentSession location:location.coordinate];
    }
}

- (void)trackerTimerFired:(NSTimer *)timer
{
    CLLocation *location = [self getLocationIfValid];
    if (location) {
        if (!self.lastLocation || ([location distanceFromLocation:self.lastLocation] > 3.0)) {
            [self trackLocation:location];
        }
    }
}

- (void)trackBalloonEventWithName:(NSString *)name {
    CLLocation *location = [self getLocationIfValid];
    if (self.isActive && location) {
        [TrackEvent balloonEventWithName:name session:self.currentSession location:location.coordinate];
    }
}

- (void)trackBirdsViewEvent {
    CLLocation *location = [self getLocationIfValid];
    if (self.isActive && location) {
        [TrackEvent birdsViewEventWithSession:self.currentSession location:location.coordinate];
    }
}

- (void)trackMapEvent {
    CLLocation *location = [self getLocationIfValid];
    if (self.isActive && location) {
        [TrackEvent mapEventWithSession:self.currentSession location:location.coordinate];
    }
}

- (void)trackZoomEvent {
    CLLocation *location = [self getLocationIfValid];
    if (self.isActive && location) {
        [TrackEvent zoomEventWithSession:self.currentSession location:location.coordinate];
    }
}

- (void)trackSocialEventWithName:(NSString *)name {
    CLLocation *location = [self getLocationIfValid];
    if (self.isActive && location) {
        [TrackEvent socialEventWithName:name session:self.currentSession location:location.coordinate];
    }
}

- (void)trackSnapshotEventWithCompleted:(BOOL)completed {
    CLLocation *location = [self getLocationIfValid];
    if (self.isActive && location) {
        [TrackEvent snapshotEventWithCompleted:completed session:self.currentSession location:location.coordinate];
    }
}

- (void)trackAppSpecificEventWithName:(NSString *)name data:(NSDictionary *)data {
    CLLocation *location = [self getLocationIfValid];
    if (self.isActive && location) {
        [TrackEvent appSpecificEventWithName:name data:data session:self.currentSession location:location.coordinate];
    }
}

@end


#pragma mark - TrackerNGPlugin Binding
void _inventioStartTrackerNGSession(const char * appId, int interval)
{
    NSLog(@"%s", __PRETTY_FUNCTION__);
    assert(appId);
    [[TrackerNGPluginManager sharedInstance] startSessionWithAppId:[NSString stringWithUTF8String:appId]
                                                          interval:interval];
    NSLog(@"Started new Tracker session!");
    
}

void _inventioEndTrackerNGSession()
{
    NSLog(@"%s", __PRETTY_FUNCTION__);
    [[TrackerNGPluginManager sharedInstance] endSession];
    NSLog(@"Ended Tracker session!");
}

void _inventioTrackBalloon(const char *name)
{
    assert(name);
    [[TrackerNGPluginManager sharedInstance] trackBalloonEventWithName:[NSString stringWithUTF8String:name]];
}

void _inventioTrackBirdsView()
{
    [[TrackerNGPluginManager sharedInstance] trackBirdsViewEvent];
}

void _inventioTrackMap()
{
    [[TrackerNGPluginManager sharedInstance] trackMapEvent];
}

void _inventioTrackZoom()
{
    [[TrackerNGPluginManager sharedInstance] trackZoomEvent];
}

/*
 * data json text format: { "key" : "value", "key" : "value", ...}
 */
void _inventioTrackAppSpecific(const char *eventName, const char *data)
{
    assert(eventName);
    NSString *dataString = [NSString stringWithUTF8String:data];
    NSError *error = nil;
    NSDictionary *dataDictionary = [NSJSONSerialization JSONObjectWithData:[dataString dataUsingEncoding:NSUTF8StringEncoding]
                                                                   options:0
                                                                     error:&error];
    if (!dataDictionary) {
        NSLog(@"%s: Illegal json data (%@), expected dictionary, error: %@",
              __PRETTY_FUNCTION__, dataString, error.localizedDescription);
        return;
    }
    
    [[TrackerNGPluginManager sharedInstance] trackAppSpecificEventWithName:[NSString stringWithUTF8String:eventName]
                                                                      data:dataDictionary];
}
