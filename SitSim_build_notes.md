# Terrain notes

Notes on terrain process

 1 Downloaded 1m lidar from mntopo and saved in GISVOL Carleton_Bootkammergrab
 2 Opened in ArcGIS Pro and clipped to 257 x 257 meters around Hill of 3 Oaks
 3 Used Raster calc to multiply by 100 to convert to cms and remove floats
 4 Export raster as 32 Bit Unsigned, scale pixel value checked
 5 Opened in PhotoShop and saved as 16bits/channel Grayscale, save as RAW, Mac byte order
 6 Import Raw... in Unity terrain as 257x257x14 w/Mac byte order 16 bit, flip vertical
 
 

# XCode Fix Notes

To build to new ios device (in my case 15.5) from XCode 10.1, Swift 4.2

## One time setup

1. adding iOS 15.5 device support file inside the Xcode app (Show Package Contents) in path contents/Developer/platform/iPhoneOS.platform/DeviceSupport 
    You can download it from [GitHub](https://github.com/iGhibli/iOS-DeviceSupport/tree/master/DeviceSupport) or you can download the new beta version and grab the directory from it.
    SOURCE: Xcode unsupported iOS version after beta update 14.5 error 18e5140k
    https://developer.apple.com/forums/thread/673131?answerId=661007022#661007022
2. go to ~/Library/Developer/Xcode/iOS DeviceSupport/15.5/
    delete contents of folder and copy in from working version, e.g. 13.3.1
    SOURCE: dyld_shared_cache_extract_dylibs failed
    https://developer.apple.com/forums/thread/108917?answerId=618095022#618095022
    
    
## Each time before build

1.  Add `--generate-entitlement-der` to `OTHER_CODE_SIGN_FLAGS` in XCode Build settings
     Select your app (not project) target, Build Settings, All (not Basic), Signing, Other Code Signing Flags. Add the specified flag to both Debug and Release. 
     SOURCE: The code signature version is no longer supported
     https://stackoverflow.com/questions/68467306/the-code-signature-version-is-no-longer-supported
     
     
