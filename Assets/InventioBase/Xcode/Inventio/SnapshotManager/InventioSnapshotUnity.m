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

#import "InventioSnapshotUnity.h"

CaptureScreenshotCallback captureCallback = NULL; // Set by InventioSnapshotManager

#define UNITY_SNAPSHOT_FILENAME @"unityscreen.png"

@interface InventioSnapshotUnity ()
- (void)clearSnapshots;

@property (strong, readonly) NSString *unitySnapshotPath;
@end


@implementation InventioSnapshotUnity

#pragma mark - Properties
- (NSString *)unitySnapshotPath
{
    NSURL *documentsURL = [[NSFileManager defaultManager] URLForDirectory:NSDocumentDirectory
                                                                 inDomain:NSAllDomainsMask
                                                        appropriateForURL:nil
                                                                   create:NO
                                                                    error:nil];
    return [documentsURL URLByAppendingPathComponent:UNITY_SNAPSHOT_FILENAME].path;
}


#pragma mark - Public methods
- (BOOL)captureUnityScreenUsingQueue:(NSOperationQueue *)queue
{
    BOOL success = false;
    
    [self clearSnapshots];
    
    // Invoke Unity callback in InventioSnapshotManager
    if (captureCallback) {
        captureCallback([UNITY_SNAPSHOT_FILENAME cStringUsingEncoding:NSUTF8StringEncoding]);
        success = YES;
        NSString *snapshotPath = self.unitySnapshotPath;
        [queue addOperationWithBlock:^{
            NSFileManager *fm = [NSFileManager defaultManager];
            BOOL haveUnityScreenShot = NO;
            NSDate *startDate = [NSDate date];
            
            // Wait for snapshot to complete, maximum 10s
            unsigned long long lastFileSize = 0;
            while (!haveUnityScreenShot && ([startDate timeIntervalSinceNow] > -10)) {
                if ([fm fileExistsAtPath:snapshotPath]) {
                    unsigned long long fileSize = [fm attributesOfItemAtPath:snapshotPath
                                                                       error:nil].fileSize;
                    if ((fileSize > 0) && (fileSize == lastFileSize)) {
                        haveUnityScreenShot = YES;
                        usleep(500000); // Still, hold on for another 0.5s...
                    }
                    else {
                        lastFileSize = fileSize;
                        usleep(100000);
                    }
                }
            }
            if (haveUnityScreenShot) {
                _snapshot = [UIImage imageWithContentsOfFile:snapshotPath];
            }
        }];
    }
    else {
        NSLog(@"Got no captureCallback!!!");
    }
    return success;
}


#pragma mark - Private methods
- (void)clearSnapshots
{
    [[NSFileManager defaultManager] removeItemAtPath:self.unitySnapshotPath error:nil];
    _snapshot = nil;
    
}

@end
