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

#import "InventioConfigViewController.h"

#import "InventioViewManager.h"
#import "InventioConfigManager.h"
#import "InventioLocalizationManager.h"

#import <SpatialManagerCore/SpatialManagerCore.h>
#import <SpatialManagerCore/SpatialManagerCore-Swift.h>

@interface InventioConfigViewController () <SpatialManagerDelegate>

@property (retain, nonatomic) IBOutlet UILabel *latitudeLabel;
@property (retain, nonatomic) IBOutlet UILabel *longitudeLabel;
@property (retain, nonatomic) IBOutlet UIActivityIndicatorView *locationActivityIndicator;
@property (retain, nonatomic) IBOutlet UIButton *getLocationButton;
@property (retain, nonatomic) IBOutlet UIButton *appLocationButton;
@property (retain, nonatomic) IBOutlet UISwitch *touchMoveSwitch;
@property (retain, nonatomic) IBOutlet UISwitch *tiltOffsetSwitch;
@property (retain, nonatomic) IBOutlet UILabel *errorLabel;

@property (assign, nonatomic) BOOL isGettingLocation;

- (void)updateLocationLabelsWithCoordinate:(CLLocationCoordinate2D)coordinate;
@end

@implementation InventioConfigViewController

- (id)initWithNibName:(NSString *)nibNameOrNil bundle:(NSBundle *)nibBundleOrNil
{
    self = [super initWithNibName:nibNameOrNil bundle:nibBundleOrNil];
    if (self) {

    }
    return self;
}

- (void)viewDidLoad
{
    [super viewDidLoad];

    SpatialManager *spatialManager = [SpatialManager sharedInstance];
    CLLocationCoordinate2D refLocation =spatialManager.referenceCoordinate;
    [self updateLocationLabelsWithCoordinate:refLocation];
    [self updateGetLocation];
    
    InventioConfigManager *configManager = [InventioConfigManager sharedInstance];
    [self.appLocationButton setTitle:configManager.appName forState:UIControlStateNormal];
    self.touchMoveSwitch.on = configManager.useTouchMove;
    self.tiltOffsetSwitch.on = configManager.useTiltOffset;
    
    CLLocation *refLoc = [[CLLocation alloc] initWithLatitude:refLocation.latitude
                                                    longitude:refLocation.longitude];
    CLLocation *appLoc = [[CLLocation alloc] initWithLatitude:configManager.appLocationCoordinate.latitude
                                                    longitude:configManager.appLocationCoordinate.longitude];
    if ([refLoc distanceFromLocation:appLoc] < 1.0) {
        self.appLocationButton.enabled = NO;
    }
}

- (void)viewDidAppear:(BOOL)animated
{
    [super viewDidAppear:animated];
    
    self.view.frame = self.view.superview.bounds;
    
    [SpatialManager sharedInstance].delegate = self;
}

- (void)viewWillDisappear:(BOOL)animated
{
    [SpatialManager sharedInstance].delegate = nil;
}

- (void)didReceiveMemoryWarning
{
    [super didReceiveMemoryWarning];
    // Dispose of any resources that can be recreated.
}

- (void)updateLocationLabelsWithCoordinate:(CLLocationCoordinate2D)coordinate
{
    self.latitudeLabel.text = [NSString stringWithFormat:@"%lf", coordinate.latitude];
    self.longitudeLabel.text = [NSString stringWithFormat:@"%lf", coordinate.longitude];
    self.errorLabel.text = @"-";
}

- (void)updateLocationLabelsWithLocation:(CLLocation *)location
{
    [self updateLocationLabelsWithCoordinate:location.coordinate];
    self.errorLabel.text = [NSString stringWithFormat:@"%.0lfm", location.horizontalAccuracy];
}

- (void)updateGetLocation
{
    if (self.isGettingLocation) {
        [self.getLocationButton setTitle:InventioLocalizedStringFromTable(@"Stop", InventioLocalizationTable) forState:UIControlStateNormal];
        [self.locationActivityIndicator startAnimating];
    }
    else {
        [self.getLocationButton setTitle:InventioLocalizedStringFromTable(@"Get Location", InventioLocalizationTable) forState:UIControlStateNormal];
        [self.locationActivityIndicator stopAnimating];
    }
}

#pragma mark - UI Callbacks
- (IBAction)getLocation:(UIButton *)sender {
    self.appLocationButton.enabled = YES;
    self.isGettingLocation = !self.isGettingLocation;

    if (self.isGettingLocation) {
        CLLocation *deviceLocation = [SpatialManager sharedInstance].deviceLocation;
        [self updateLocationLabelsWithLocation:deviceLocation];
    }
    [self updateGetLocation];
}

- (IBAction)useAppLocation:(UIButton *)sender {
    self.appLocationButton.enabled = NO;
    self.isGettingLocation = NO;

    CLLocationCoordinate2D coordinate = [InventioConfigManager sharedInstance].appLocationCoordinate;
    [self updateLocationLabelsWithCoordinate:coordinate];
    [self updateGetLocation];
}


- (IBAction)configDone:(UIButton *)sender {
    self.isGettingLocation = NO;
    
    InventioConfigManager *configManager = [InventioConfigManager sharedInstance];
    configManager.touchMove = self.touchMoveSwitch.on;
    configManager.tiltOffset = self.tiltOffsetSwitch.on;

    CLLocationCoordinate2D coordinate = CLLocationCoordinate2DMake([self.latitudeLabel.text doubleValue],
                                                                   [self.longitudeLabel.text doubleValue]);
    [[SpatialManager sharedInstance] setReferenceCoordinate:coordinate];
    
    [[InventioViewManager sharedInstance] hideViewController:self];
}

#pragma mark - InventioSpatialManagerDelegate
- (void)spatialManager:(SpatialManager *)manager
     didUpdateLocation:(CLLocation *)location
{
    if (self.isGettingLocation) {
        [self updateLocationLabelsWithLocation:location];
    }
}

- (void)spatialManager:(SpatialManager *)manager
     didUpdateAttitude:(CMAttitude *)attitude
{
    // Don't care
}

@end
