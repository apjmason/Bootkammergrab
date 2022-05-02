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

public class InventioConfigManager : MonoBehaviour {
	#if UNITY_IOS
	[DllImport ("__Internal")]
	private static extern bool _SetAppName(string name);
	[DllImport ("__Internal")]
	private static extern bool _SetAppLocation(double latitude, double longitude);
	[DllImport ("__Internal")]
	private static extern void _UpdateSpatialSettings();
	[DllImport ("__Internal")]
	private static extern void _OpenAppSettings();
	#endif
	#if UNITY_ANDROID
	#endif

	public static bool SetApplicationName(string name)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) {
			#if UNITY_IOS
			return _SetAppName(name);
			#endif
		}
		else if (Application.platform == RuntimePlatform.Android) {
			#if UNITY_ANDROID
			Debug.LogError("InventioConfigManager not implemented for Android!!!");
			#endif			
		}
		return false;
	}

	public static bool SetApplicationLocation(double latitude, double longitude)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) {
			#if UNITY_IOS
			return _SetAppLocation(latitude, longitude);
			#endif
		}
		else if (Application.platform == RuntimePlatform.Android) {
			#if UNITY_ANDROID
			Debug.LogError("InventioConfigManager not implemented for Android!!!");
			#endif
		}
		return false;
	}

	public static void UpdateSpatialSettings()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) {
			#if UNITY_IOS
			_UpdateSpatialSettings();
			#endif
		}
		else if (Application.platform == RuntimePlatform.Android) {
			#if UNITY_ANDROID
			Debug.LogError("InventioConfigManager not implemented for Android!!!");
			#endif
		}
	}

	public static bool CanOpenAppSettings()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) {
			return true;
		}
		return false;
	}

	public static void OpenAppSettings()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) {
			#if UNITY_IOS
			_OpenAppSettings ();
			#endif
		} else if (Application.platform == RuntimePlatform.Android) {
			#if UNITY_ANDROID
			Debug.LogError("InventioConfigManager not implemented for Android!!!");
			#endif
		}
	}
}
