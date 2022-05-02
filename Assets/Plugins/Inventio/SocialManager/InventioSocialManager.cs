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


public class InventioSocialManager : MonoBehaviour {

	#if UNITY_IOS
	[DllImport ("__Internal")]
	private static extern bool _ShareImageData(byte[] imageData, int size, string text, string subject);
	[DllImport ("__Internal")]
	private static extern bool _SetSharedURL(string url);
	#endif
	#if UNITY_ANDROID
	#endif

	public static bool ShareImageData(byte[] imageData, string text, string subject)
	{
		bool res = false;
		if (Application.platform == RuntimePlatform.IPhonePlayer) {
			#if UNITY_IOS
			res = _ShareImageData(imageData, imageData.Length, text, subject);
			#endif
		}
		else if (Application.platform == RuntimePlatform.Android) {
			#if UNITY_ANDROID
			Debug.LogError("InventioSocialManager not implemented for Android!!!");
			#endif			
		}
		return res;
	}

	public static bool SetSharedURL(string url)
	{
		bool res = false;
		if (Application.platform == RuntimePlatform.IPhonePlayer) {
			#if UNITY_IOS
			res = _SetSharedURL(url);
			#endif
		}
		else if (Application.platform == RuntimePlatform.Android) {
			#if UNITY_ANDROID
			Debug.LogError("InventioSocialManager not implemented for Android!!!");
			#endif
		}
		return res;
	}
}
