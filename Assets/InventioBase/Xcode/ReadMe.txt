Dra in dessa kataloger i projektet
----------------------------------
Inventio


Ta bort onödiga filer ur projektet
----------------------------------
*.meta
etc.


Ta bort onödiga filer ur "Copy Bundle Resources"
------------------------------------------------
ReadMe-filer
etc.

Uppdatera Project Build Settings
--------------------------------
Sätt “Enable Bitcode” till NO (Google Analytics)
Sätt “Always Embed Swift Standard Libraries” till YES (TrackerNGCore)
Sätt “Enable Modules (C and Objective-C)” till YES (TrackerNGCore)

Komplettera Info.plist
----------------------
Addera text för Privacy - Motion Usage 
Addera text för Privacy - Photo Library Additions Usage

Konfigurationsfiler (appspecifika)
----------------------------------
Addera GoogleService-Info.plist (med korrekt bundle id)

Länka med följande bibliotek
----------------------------
libsqlite3.tbd (Google Analytics)
libz.tbd (Google Analytics)

Addera Other Linker Flags
-------------------------
-ObjC

Lägg till följande framework i "Embedded Binaries"
--------------------------------------------------
TrackerNGCore.framework
SpatialManagerCore.framework

Ta bort följande frameworks ur "Linked Frameworks and Libraries"
----------------------------------------------------------------
Parse.framework (är redan med i TrackerNGCore.framework) 
Bolts.framework (är redan med i TrackerNGCore.framework)


Följande länkas in mha inspector i Unity
----------------------------------------
CoreData.framework (Google Analytics)
MobileCoreServices.framework (Parse)
Security.framework (Parse)
StoreKit.framework (Parse)
