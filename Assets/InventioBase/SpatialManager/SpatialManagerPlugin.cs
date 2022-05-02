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

using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using AOT;

public delegate void LocationAuthorizationHandler( bool authorized );
public delegate void SpatialWarmupHandler( int level );

namespace InventioBase {
    public class SpatialManagerPlugin : MonoBehaviour {
        private static GameObject mirrorObject = null;
        private static bool validPosition = false;
        private static bool validAttitude = false;
        private static float tiltValue = 0f;
        private static bool forceTouchMove = false;
        
        private static LocationAuthorizationHandler authorizationHandler = null;
        private static SpatialWarmupHandler warmupHandler = null;
        
        public enum SpatialLocationAuthorization {
            NotDetermined,
            Denied,
            Granted
        };
        
        #if UNITY_IOS
        public delegate void LocationAuthorizationCallback(bool authorized);
        public delegate void SpatialWarmupCallback(int level);
        public delegate void LocationCallback(float east, float north, float up);
        public delegate void AttitudeCallback(float yaw, float pitch, float roll);
        public delegate void TiltCallback(float tiltValue, bool animated);
        public delegate void TouchMoveCallback(bool useTouch);
        
        [DllImport ("__Internal")]
        private static extern void _RequestLocationAuthorization(LocationAuthorizationCallback callback);
        [DllImport ("__Internal")]
        private static extern int _GetLocationAuthorizationStatus(); // Cast to SpatialLocationAuthorization
        [DllImport ("__Internal")]
        private static extern bool _StartSpatialWarmup(SpatialWarmupCallback callback);
        [DllImport ("__Internal")]
        private static extern bool _StopSpatialWarmup();
        [DllImport ("__Internal")]
        private static extern bool _SetReferenceLocation(double latitude, double longitude);
        [DllImport ("__Internal")]
        private static extern bool _StartUpdatingLocationUsingReferenceLocation(double latitude, double longitude);
        [DllImport ("__Internal")]
        private static extern bool _StartUpdatingOrientation();
        [DllImport ("__Internal")]
        private static extern bool _StopUpdates();
        
        [DllImport ("__Internal")]
        private static extern void _SetLocationCallback(LocationCallback callback);
        [DllImport ("__Internal")]
        private static extern void _SetOrientationCallback(AttitudeCallback callback);
        [DllImport ("__Internal")]
        private static extern void _SetTiltCallback(TiltCallback callback);
        [DllImport ("__Internal")]
        private static extern void _SetTouchMoveCallback(TouchMoveCallback callback);
        [DllImport ("__Internal")]
        private static extern bool _OpenLocationInMaps(double latitude, double longitude, string name);
        #endif
        
        public static SpatialLocationAuthorization GetLocationAuthorizationStatus()
        {
            SpatialLocationAuthorization res = SpatialLocationAuthorization.NotDetermined;
            if (Application.platform == RuntimePlatform.IPhonePlayer) {
                #if UNITY_IOS
                res = (SpatialLocationAuthorization)_GetLocationAuthorizationStatus();
                #endif
            }
            else if (Application.platform == RuntimePlatform.Android) {
                #if UNITY_ANDROID
                Debug.LogError("SpatialManagerPlugin.GetLocationAuthorizationStatus: Not implemented on Android!");
                res = 2;
                #endif
            }
            return res;
        }
        
        public static void RequestLocationAuthorization(LocationAuthorizationHandler handler)
        {
            SpatialManagerPlugin.authorizationHandler = handler;
            
            if (Application.platform == RuntimePlatform.IPhonePlayer) {
                #if UNITY_IOS
                _RequestLocationAuthorization (LocationAuthorization);
                #endif
            }
            else if (Application.platform == RuntimePlatform.Android) {
                #if UNITY_ANDROID
                Debug.LogError("SpatialManagerPlugin.RequestLocationAuthorization: Not implemented on Android!");
                #endif
            }
        }
        
        public static bool StartSpatialWarmup(SpatialWarmupHandler handler)
        {
            SpatialManagerPlugin.warmupHandler = handler;
            bool res = false;
            if (Application.platform == RuntimePlatform.IPhonePlayer) {
                #if UNITY_IOS
                res = _StartSpatialWarmup(UpdateSpatialWarmup);
                #endif
            }
            else if (Application.platform == RuntimePlatform.Android) {
                #if UNITY_ANDROID
                Debug.LogError("SpatialManagerPlugin.StartSpatialWarmup: Not implemented on Android!");
                #endif
            }
            return res;
        }
        
        public static bool StopSpatialWarmup()
        {
            SpatialManagerPlugin.warmupHandler = null;
            bool res = false;
            if (Application.platform == RuntimePlatform.IPhonePlayer) {
                #if UNITY_IOS
                res = _StopSpatialWarmup();
                #endif
            }
            else if (Application.platform == RuntimePlatform.Android) {
                #if UNITY_ANDROID
                Debug.LogError("SpatialManagerPlugin.StopSpatialWarmup: Not implemented on Android!");
                #endif
            }
            return res;
        }
        
        public static bool SetReferenceLocation(double latitude, double longitude)
        {
            validPosition = false;
            
            bool res = false;
            if (Application.platform == RuntimePlatform.IPhonePlayer) {
                #if UNITY_IOS
                res = _SetReferenceLocation(latitude, longitude);
                #endif
            }
            else if (Application.platform == RuntimePlatform.Android) {
                #if UNITY_ANDROID
                AndroidJavaObject jo = GetAndroidSpatialBinding();
                res = jo.Call<bool>("_SetReferenceLocation", latitude, longitude);
                #endif
            }
            return res;
        }
        
        public static bool StartUpdatingLocationUsingReferenceLocation(double latitude, double longitude)
        {
            validPosition = false;
            
            bool res = false;
            if (Application.platform == RuntimePlatform.IPhonePlayer) {
                #if UNITY_IOS
                _SetTouchMoveCallback(UpdateTouchMove);
                _SetLocationCallback(UpdateLocation);
                res = _StartUpdatingLocationUsingReferenceLocation(latitude, longitude);
                #endif
            }
            else if (Application.platform == RuntimePlatform.Android) {
                #if UNITY_ANDROID
                AndroidJavaObject jo = GetAndroidSpatialBinding();
                res = jo.Call<bool>("_StartUpdatingLocationUsingReferenceLocation", latitude, longitude);
                #endif
            }
            return res;
        }
        
        public static bool OpenLocationInMaps(double latitude, double longitude, string name)
        {
            validPosition = false;
            
            bool res = false;
            if (Application.platform == RuntimePlatform.IPhonePlayer) {
                #if UNITY_IOS
                res = _OpenLocationInMaps(latitude, longitude, name);
                #endif
            }
            else if (Application.platform == RuntimePlatform.Android) {
                #if UNITY_ANDROID
                Debug.LogError("SpatialManagerPlugin.OpenLocationInMaps: Not implemented on Android!");
                #endif
            }
            return res;
        }

        public static bool StartUpdatingOrientation()
        {
            validAttitude = false;
            
            bool res = false;
            
            if (Application.platform == RuntimePlatform.IPhonePlayer) {
                #if UNITY_IOS
                _SetTiltCallback(UpdateTilt);
                _SetOrientationCallback(UpdateOrientation);
                res = _StartUpdatingOrientation();
                #endif
            }
            else if (Application.platform == RuntimePlatform.Android) {
                #if UNITY_ANDROID
                AndroidJavaObject jo = GetAndroidSpatialBinding();
                res = jo.Call<bool>("_StartUpdatingOrientation");
                #endif
            }
            return res;
        }
        
        public static bool StopUpdates()
        {
            bool res = false;
            if (Application.platform == RuntimePlatform.IPhonePlayer) {
                #if UNITY_IOS
                res = _StopUpdates();
                #endif
            }
            else if (Application.platform == RuntimePlatform.Android) {
                #if UNITY_ANDROID
                Debug.LogError("SpatialManagerPlugin.StopUpdates: Not implemented on Android!");
                #endif
            }
            return res;
        }
        
        public static GameObject SharedMirrorObject()
        {
            if (mirrorObject == null) {
                mirrorObject = new GameObject("PlayerMirror");
            }
            return mirrorObject;
        }
        
        public static Vector3 Position()
        {
            return SharedMirrorObject().transform.position;
        }
        
        public static Quaternion Rotation()
        {
            return SharedMirrorObject().transform.rotation;
        }
        
        public static bool HasValidPosition()
        {
            return validPosition;
        }
        
        public static bool HasValidAttitude()
        {
            return validAttitude;
        }
        
        public static bool UsingTiltOffset()
        {
            return (tiltValue > Mathf.Epsilon);
        }
        
        public static bool UsingTouchMove()
        {
            return (forceTouchMove || Application.isEditor);
        }
        
        // Callbacks from Native code
        public static void tiltUpdated(float tilt)
        {
            tiltValue = tilt;
        }
        
        public static void tiltUpdatedAnimated(float tilt, bool animated)
        {
            if (animated) {
                LeanTween.value(mirrorObject, tiltUpdated, tiltValue, tilt, 0.5f).setUseEstimatedTime(true);
            } else {
                tiltValue = tilt;
            }
        }
        
        public static void touchMoveUpdated(bool useTouch)
        {
            forceTouchMove = useTouch;
        }
        
        public static void locationUpdated(float east, float north, float up)
        {
            SharedMirrorObject().transform.position = new Vector3(east, up, north);
            
            validPosition = true;
        }
        
        public static void attitudeUpdated(float yaw, float pitch, float roll)
        {
            SharedMirrorObject().transform.rotation = Quaternion.Euler(90f + roll - tiltValue, yaw, -pitch);
            validAttitude = true;
        }
        
        [MonoPInvokeCallback (typeof (LocationAuthorizationCallback))]
        private static void LocationAuthorization(bool authorized) {
            if (SpatialManagerPlugin.authorizationHandler != null) {
                SpatialManagerPlugin.authorizationHandler (authorized);
            } else {
                Debug.LogError("SpatialManagerPlugin.LocationAuthorization callback: Got no authorizationHandler!");
            }
            SpatialManagerPlugin.authorizationHandler = null;
        }
        
        [MonoPInvokeCallback (typeof (SpatialWarmupCallback))]
        private static void UpdateSpatialWarmup(int level) {
            if (SpatialManagerPlugin.warmupHandler != null) {
                SpatialManagerPlugin.warmupHandler (level);
            } else {
                Debug.LogError("SpatialManagerPlugin.SpatialWarmup callback: Got no warmupHandler!");
            }
        }
        
        [MonoPInvokeCallback (typeof (LocationCallback))]
        private static void UpdateLocation(float east, float north, float up) {
            locationUpdated (east, north, up); // Tidigare använd i Mono-bryggan
        }
        
        [MonoPInvokeCallback (typeof (AttitudeCallback))]
        private static void UpdateOrientation(float yaw, float pitch, float roll) {
            attitudeUpdated (yaw, pitch, roll); // Tidigare använd i Mono-bryggan
        }
        
        [MonoPInvokeCallback (typeof (TiltCallback))]
        private static void UpdateTilt(float tiltValue, bool animated) {
            tiltUpdatedAnimated(tiltValue, animated); // Tidigare använd i Mono-bryggan
        }
        
        [MonoPInvokeCallback (typeof (TouchMoveCallback))]
        private static void UpdateTouchMove(bool useTouch) {
            touchMoveUpdated (useTouch); // Tidigare använd i Mono-bryggan
        }
    }
    
    public class SpatialManagerPluginAndroidProxy : MonoBehaviour {
        
        public void locationUpdated(string locationString) //float east;float north:float up
        {
            char[] separatorArray = {';'};
            string[] locationStringArray = locationString.Split (separatorArray, System.StringSplitOptions.None);
            
            float east = 0f, north = 0f, up = 0f;
            if (locationStringArray.Length == 3) {
                east = float.Parse(locationStringArray[0]);
                north = float.Parse(locationStringArray[1]);
                up = float.Parse(locationStringArray[2]);
                
                SpatialManagerPlugin.locationUpdated(east, north, up);
            }
            else {
                Debug.LogError("SpatialManagerPluginAndroidProxy received illegal locationUpdate: " + locationString);
            }
        }
        
        public void attitudeUpdated(string attitudeString) //float yaw;float pitch:float roll
        {
            char[] separatorArray = {';'};
            string[] attitudeStringArray = attitudeString.Split (separatorArray, System.StringSplitOptions.None);
            
            float yaw = 0f, pitch = 0f, roll = 0f;
            if (attitudeStringArray.Length == 3) {
                yaw = float.Parse(attitudeStringArray[0]);
                pitch = float.Parse(attitudeStringArray[1]);
                roll = float.Parse(attitudeStringArray[2]);
                
                SpatialManagerPlugin.attitudeUpdated(yaw, pitch, roll);
            }
            else {
                Debug.LogError("SpatialManagerPluginAndroidProxy received illegal attitudeUpdate: " + attitudeString);
            }
        }
    }
    
}
