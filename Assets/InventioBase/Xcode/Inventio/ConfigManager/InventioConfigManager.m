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

#import "InventioConfigManager.h"

#import <SpatialManagerCore/SpatialManagerCore.h>
#import <SpatialManagerCore/SpatialManagerCore-Swift.h>

@interface InventioConfigManager ()
@property (strong, nonatomic) CLLocation *appLocation;
@end


@implementation InventioConfigManager

- (void)setAppLocationCoordinate:(CLLocationCoordinate2D)appLocationCoordinate
{
    _appLocationCoordinate = appLocationCoordinate;
    _appLocation = [[CLLocation alloc] initWithCoordinate:_appLocationCoordinate
                                                 altitude:0
                                       horizontalAccuracy:0
                                         verticalAccuracy:0
                                                timestamp:[NSDate date]];
}

- (void)setTouchMove:(BOOL)touchMove
{
    [[SpatialManager sharedInstance] forceTouchMove:touchMove];
    _touchMove = touchMove;
}

- (void)setTiltOffset:(BOOL)tiltOffset
{
    if (tiltOffset) {
        [[SpatialManager sharedInstance] updateTilt:self.defaultTiltValue animated:NO];
    }
    else {
        [[SpatialManager sharedInstance] updateTilt:0 animated:NO];
    }
    _tiltOffset = tiltOffset;
}

- (float)defaultTiltValue
{
    return 30;
}

#pragma mark - Singleton
+ (InventioConfigManager *)sharedInstance
{
    static dispatch_once_t pred;
    static InventioConfigManager *shared = nil;
    
    dispatch_once(&pred, ^{
        shared = [[InventioConfigManager alloc] init];
    });
    return shared;
}

- (id)init
{
    self = [super init];
    if (self) {
        _touchMove = NO;
        _tiltOffset = YES;
    }
    return self;
}

#pragma mark - Bindings
bool _SetAppName(const char *name)
{
    if (name) {
        [InventioConfigManager sharedInstance].appName = [NSString stringWithUTF8String:name];
        return true;
    }
    return false;
}

bool _SetAppLocation(double latitude, double longitude)
{
    CLLocationCoordinate2D coordinate = CLLocationCoordinate2DMake(latitude, longitude);
    if (CLLocationCoordinate2DIsValid(coordinate)) {
        [InventioConfigManager sharedInstance].appLocationCoordinate = coordinate;
        return YES;
    }
    return NO;
}

void _UpdateSpatialSettings()
{
    InventioConfigManager *config = [InventioConfigManager sharedInstance];
    [[SpatialManager sharedInstance] updateTilt:config.defaultTiltValue animated:NO];
    [[SpatialManager sharedInstance] forceTouchMove:config.useTouchMove];
    
}

void _OpenAppSettings()
{
    NSURL *url = [NSURL URLWithString:UIApplicationOpenSettingsURLString];
    if ([[UIApplication sharedApplication] canOpenURL:url]) {
        [[UIApplication sharedApplication] openURL:url];
    }
}

@end
