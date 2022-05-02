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

public class InventioWebView : MonoBehaviour {
	#if UNITY_IOS
	[DllImport ("__Internal")]
	private static extern void _ShowWebViewWithURL(string url);
	[DllImport ("__Internal")]
	private static extern void _ShowWebViewWithPDF(string path);
	[DllImport ("__Internal")]
	private static extern void _LimitWebViewToInitialHost();
	#endif

	public static void ShowWebViewWithURL(string url)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) {
			#if UNITY_IOS
			_ShowWebViewWithURL(url);
			#endif
		}
		else if (Application.platform == RuntimePlatform.Android) {
			#if UNITY_ANDROID
			#endif			
		}
	}

	public static void ShowWebViewWithPDF(string path)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) {
			#if UNITY_IOS
			_ShowWebViewWithPDF(Application.dataPath + "/Raw/" + path);
			#endif
		}
		else if (Application.platform == RuntimePlatform.Android) {
			#if UNITY_ANDROID
			#endif			
		}
	}

	public static void LimitWebViewToInitialHost()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) {
			#if UNITY_IOS
			_LimitWebViewToInitialHost();
			#endif
		}
		else if (Application.platform == RuntimePlatform.Android) {
			#if UNITY_ANDROID
			#endif			
		}
	}
}
