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


public class LocalizationManager : MonoBehaviour {

	#if UNITY_IOS
	[DllImport ("__Internal")]
	private static extern void _inventioLocalizationInitializeLanguage(string languageName, string appLocalizedTableName);
	[DllImport ("__Internal")]
	private static extern bool _inventioLocalizationTestLanguage(string languageName, string key, string value);
	#endif
	#if UNITY_ANDROID
	#endif

	public static void InitializeLanguage(string languageName, string localizedTableName)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) {
			#if UNITY_IOS
			_inventioLocalizationInitializeLanguage(languageName, localizedTableName);
			#endif
		}
		else if (Application.platform == RuntimePlatform.Android) {
			#if UNITY_ANDROID
			Debug.LogError("LocalizationManager:InitializeLanguage is not implemented on Android!");
			#endif			
		}
	}

	public static bool TestLanguage(string languageName, string key, string value)
	{
		bool result = false;
		if (Application.platform == RuntimePlatform.IPhonePlayer) {
			#if UNITY_IOS
			result = _inventioLocalizationTestLanguage(languageName, key, value);
			#endif
		}
		else if (Application.platform == RuntimePlatform.Android) {
			#if UNITY_ANDROID
			Debug.LogError("LocalizationManager:TestLanguage is not implemented on Android!");
			#endif			
		}

		#if UNITY_EDITOR
		Debug.Log("LocalizationManager: No native locale in editor");
		result = true;
		#endif

		return result;
	}

}
