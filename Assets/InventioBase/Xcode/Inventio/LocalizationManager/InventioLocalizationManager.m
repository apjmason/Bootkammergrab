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

#import "InventioLocalizationManager.h"

#import "CABDefines.h"
#import "InventioAnalyticsManager.h"

NSString * const InventioLocalizationTable = @"InventioBase";

@interface InventioLocalizationManager ()
+ (void)setLanguage:(NSString *)language;
+ (NSString *)currentLanguage;
+ (void)setApplicationTable:(NSString *)table;
+ (NSString *)applicationTable;
@end


#pragma mark - Localization Binding
void _inventioLocalizationInitializeLanguage(const char *languageName, const char *appLocalizedTableName)
{
    [InventioLocalizationManager setLanguage:(languageName ? [@(languageName) lowercaseString] : nil)];
    [InventioLocalizationManager setApplicationTable:(appLocalizedTableName ? @(appLocalizedTableName) : nil)];
}

bool _inventioLocalizationTestLanguage(const char *languageName, const char *key, const char *value)
{
    bool success = false;
    if (languageName && key && value) {
        NSString *currentLanguage = [InventioLocalizationManager currentLanguage];
        if ([currentLanguage isEqualToString:[@(languageName) lowercaseString]]) {
            if ([InventioLocalizedString(@(key)) isEqualToString:@(value)]) {
                success = true;
            }
        }
        else {
            NSLog(@"Native locale (%@) different from Unity locale (%@)", currentLanguage, @(languageName));
        }
    }
    return success;
}


@implementation InventioLocalizationManager

static NSString *appTableName = nil;
static NSBundle *bundle = nil;
static NSString *currentLanguage = nil;
+ (void)setLanguage:(NSString *)language
{
    currentLanguage = nil;
    if (language) {
        NSArray *languages = [[NSBundle mainBundle] localizations];
        NSString *current = [languages objectAtIndex:0];
        NSUInteger languageIndex = [languages indexOfObject:[language lowercaseString]];
        if (languageIndex != NSNotFound) {
            current = [languages objectAtIndex:languageIndex];
        }
        NSString *path = [[ NSBundle mainBundle] pathForResource:current ofType:@"lproj" ];
        bundle = [NSBundle bundleWithPath:path];
        currentLanguage = current;
    }
    else {
        bundle = [NSBundle mainBundle];
    }
}

+ (NSString *)currentLanguage
{
    return currentLanguage;
}

+ (void)setApplicationTable:(NSString *)table
{
    appTableName = [table copy];
}

+ (NSString *)applicationTable
{
    return appTableName;
}

+ (NSString *)stringForKey:(NSString *)key table:(NSString *)table
{
    return NSLocalizedStringFromTableInBundle(key, table, bundle, nil);
}

+ (NSString *)stringForKey:(NSString *)key
{
    NSString *checkFailure = @"LOCALIZATION_FAILED!";
    NSString *value = NSLocalizedStringWithDefaultValue(key,
                                                        InventioLocalizationTable,
                                                        bundle,
                                                        checkFailure,
                                                        nil);
    if ([value isEqualToString:checkFailure]) {
        NSLog(@"Missing localization for %@ in table %@.", key, InventioLocalizationTable);
        value = key;
        if (appTableName != nil) {
            value = NSLocalizedStringWithDefaultValue(key,
                                                      appTableName,
                                                      bundle,
                                                      checkFailure,
                                                      nil);
            if ([value isEqualToString:checkFailure]) {
                NSLog(@"Missing localization for %@ in table %@.", key, appTableName);
                value = key;
            }
        }
    }
    return value;
}

@end
