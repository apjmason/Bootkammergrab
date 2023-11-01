# Terrain notes

Notes on terrain process

 1 Downloaded 1m lidar from mntopo and saved in GISVOL Carleton_Bootkammergrab
 2 Opened in ArcGIS Pro and clipped to 257 x 257 meters around Hill of 3 Oaks
 3 Used Raster calc to multiply by 100 to convert to cms and remove floats
 4 Export raster as 32 Bit Unsigned, scale pixel value checked
 5 Opened in PhotoShop and saved as 16bits/channel Grayscale, save as RAW, Mac byte order
 6 Import Raw... in Unity terrain as 257x257x14 w/Mac byte order 16 bit, flip vertical