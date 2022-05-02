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
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace InventioBase {
	public class TrackerNGPlugin : MonoBehaviour {

		#if UNITY_IOS
		[DllImport("__Internal")]
		private static extern void _inventioStartTrackerNGSession(string appId, int interval);
		[DllImport("__Internal")]
		private static extern void _inventioEndTrackerNGSession();
        [DllImport("__Internal")]
        private static extern void _inventioTrackBalloon(string name);
        [DllImport("__Internal")]
        private static extern void _inventioTrackBirdsView();
        [DllImport("__Internal")]
        private static extern void _inventioTrackMap();
        [DllImport("__Internal")]
        private static extern void _inventioTrackZoom();
        [DllImport("__Internal")]
        private static extern void _inventioTrackAppSpecific(string eventName, string data);
		#endif

		public static void StartTrackerNGSession(string appId, int interval)
		{
			if( Application.platform == RuntimePlatform.IPhonePlayer) {
				#if UNITY_IOS
				_inventioStartTrackerNGSession(appId, interval);
				#endif
			}
		}

		public static void EndTrackerNGSession()
		{
			if( Application.platform == RuntimePlatform.IPhonePlayer) {
				#if UNITY_IOS
				_inventioEndTrackerNGSession();
				#endif
			}
		}
        
        public static void TrackBalloon(string name)
        {
            if( Application.platform == RuntimePlatform.IPhonePlayer) {
                #if UNITY_IOS
                _inventioTrackBalloon(name);
                #endif
            }
        }
        
        public static void TrackBirdsView()
        {
            if( Application.platform == RuntimePlatform.IPhonePlayer) {
                #if UNITY_IOS
                _inventioTrackBirdsView();
                #endif
            }
        }
        
        public static void TrackMap()
        {
            if( Application.platform == RuntimePlatform.IPhonePlayer) {
                #if UNITY_IOS
                _inventioTrackMap();
                #endif
            }
        }
        
        public static void TrackZoom()
        {
            if( Application.platform == RuntimePlatform.IPhonePlayer) {
                #if UNITY_IOS
                _inventioTrackZoom();
                #endif
            }
        }
        
        public static void TrackAppSpecific(string eventName, Dictionary<string, string> data)
        {
            string dataString = "{ ";
            int count = 1;
            foreach (var pair in data)
            {
                dataString += "\"" + pair.Key + "\" : \"" + pair.Value + "\"";
                if (count++ < data.Count) {
                    dataString += " , ";
                }
            }
            dataString += " }";
            
            if( Application.platform == RuntimePlatform.IPhonePlayer) {
                #if UNITY_IOS
                _inventioTrackAppSpecific(eventName, dataString);
                #endif
            }
        }
        
        
	}
}
