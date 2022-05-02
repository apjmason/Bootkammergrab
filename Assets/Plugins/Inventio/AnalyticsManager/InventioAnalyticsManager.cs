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
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
//

using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class InventioAnalyticsManager : MonoBehaviour {

	#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void _inventioAnalyticsActivate(string serviceClassName, string key, string misc);
	[DllImport("__Internal")]
	private static extern void _inventioAnalyticsLogEvent(string analyticsEvent, string label);
	#endif
	#if UNITY_ANDROID
	#endif

	public static void activateAnalytics(string serviceClassName, string key, string misc)
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer) {
			#if UNITY_IOS
			_inventioAnalyticsActivate(serviceClassName, key, misc);
			#endif
		}
		else if( Application.platform == RuntimePlatform.Android) {
#if UNITY_ANDROID
			Debug.LogError("InventioAnalyticsManager not implemented for Android!!!");
#endif
		}		
    }
	
	public static void logEvent(string analyticsEvent, string label)
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer) {
			#if UNITY_IOS
			_inventioAnalyticsLogEvent(analyticsEvent, label);
			#endif
		}
		else if( Application.platform == RuntimePlatform.Android) {
#if UNITY_ANDROID
			Debug.LogError("InventioAnalytics not implemented for Android!!!");
#endif
		}		
    }
}
