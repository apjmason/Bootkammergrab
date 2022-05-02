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

#import <SpatialManagerCore/SpatialManagerCore.h>
#import <SpatialManagerCore/SpatialManagerCore-Swift.h>

#import "TrackerNGPlugin.h"

typedef void (* LocationAuthorizationCallback) (bool authorized);
typedef void (* SpatialWarmupCallback) (int level);
typedef void (* OrientationCallback) (float yaw, float pitch, float roll);
typedef void (* LocationCallback) (float east, float north, float up);
typedef void (* TiltCallback) (float tiltValue, bool animated);
typedef void (* TouchMoveCallback) (bool useTouch);

static SpatialWarmupManager *warmupManager = nil;

#pragma mark - SpatialManagerPlugin Binding
void _RequestLocationAuthorization(LocationAuthorizationCallback callback)
{
    if (!warmupManager) {
        warmupManager = [[SpatialWarmupManager alloc] init];
    }
    [warmupManager setLocationAuthorizationCallback:^(BOOL authorized) {
        dispatch_async(dispatch_get_main_queue(), ^{
            callback(authorized);
        });
    }];
    [warmupManager requestAuthorization];
}

int _GetLocationAuthorizationStatus()
{
    // NotDetermined=0, Denied=1, Authorized=2
    return SpatialWarmupManager.authorizationStatus;
}

void _StartSpatialWarmup(SpatialWarmupCallback callback)
{
    if (!warmupManager) {
        warmupManager = [[SpatialWarmupManager alloc] init];
    }
    
    [warmupManager setSpatialWarmupCallback:^(NSInteger level) {
        dispatch_async(dispatch_get_main_queue(), ^{
            callback((int)level);
        });
    }];
    [warmupManager startWarmupSequence];
}

void _StopSpatialWarmup()
{
    [warmupManager stopWarmupSequence];
    warmupManager = nil;
}

bool _SetReferenceLocation(double latitude, double longitude)
{
    [SpatialManager sharedInstance].referenceLocation = [[CLLocation alloc] initWithLatitude:latitude longitude:longitude];
    return YES;
}

bool _StartUpdatingLocationUsingReferenceLocation(double latitude, double longitude)
{
    [SpatialManager sharedInstance].referenceLocation = [[CLLocation alloc] initWithLatitude:latitude longitude:longitude];
    [[SpatialManager sharedInstance] startLocationUpdates];
    return YES;
}

bool _StartUpdatingOrientation()
{
    [[SpatialManager sharedInstance] startAttitudeUpdates:1.0 / 60.0];
    return YES;
}

bool _StopUpdates()
{
    [[SpatialManager sharedInstance] stopUpdates];
    return YES;
}

void _SetOrientationCallback(OrientationCallback callback)
{
    [[SpatialManager sharedInstance] setOrientationCallback:^(float_t yaw, float_t pitch , float_t roll) {
        dispatch_async(dispatch_get_main_queue(), ^{
            callback(yaw, pitch, roll);
        });
    }];
}

void _SetLocationCallback(LocationCallback callback)
{
    [[SpatialManager sharedInstance] setLocationCallback:^(double east, double north, double altitude) {
        dispatch_async(dispatch_get_main_queue(), ^{
            callback(east, north, altitude);
        });
    }];
}

void _SetTiltCallback(TiltCallback callback)
{
    [[SpatialManager sharedInstance] setTiltCallback:^(float_t tiltValue, BOOL animated) {
        dispatch_async(dispatch_get_main_queue(), ^{
            callback(tiltValue, animated);
        });
    }];
}

void _SetTouchMoveCallback(TouchMoveCallback callback)
{
    [[SpatialManager sharedInstance] setTouchMoveCallback:^(BOOL useTouch) {
        dispatch_async(dispatch_get_main_queue(), ^{
            callback(useTouch);
        });
    }];
}

void _OpenLocationInMaps(double latitude, double longitude, const char *name)
{
    CLLocation *location = [[CLLocation alloc] initWithLatitude:latitude longitude:longitude];
    NSString *nameString = [NSString stringWithUTF8String:name];
    [[SpatialManager sharedInstance] openLocationInMaps:location withName:nameString];
}
