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

public class InventioViewManager : MonoBehaviour {

	#if UNITY_IOS
	[DllImport ("__Internal")]
	private static extern bool _ShowViewController(string viewControllerName);
	[DllImport ("__Internal")]
	private static extern bool _HideViewController();
	[DllImport ("__Internal")]
	private static extern void _ShowSimpleAlert(string title, string message, string buttonTitle);

	#endif
	#if UNITY_ANDROID && FALSE
	private static AndroidJavaObject viewManagerBinding = null;

	private static AndroidJavaObject GetAndroidViewManagerBinding() {
		if (viewManagerBinding == null) {
			AndroidJavaClass jc = new AndroidJavaClass("se.codegrind.inventio.inventiobase.ViewManagerBinding"); 
			viewManagerBinding = jc.CallStatic<AndroidJavaObject>("instance");
		}
		return viewManagerBinding;
	}
	#endif

	public static bool ShowViewController(string viewControllerName)
	{
		bool res = false;
		if (Application.platform == RuntimePlatform.IPhonePlayer) {
			#if UNITY_IOS
			res = _ShowViewController(viewControllerName);
			#endif
		}
		else if (Application.platform == RuntimePlatform.Android) {
			#if UNITY_ANDROID && FALSE
			AndroidJavaObject jo = GetAndroidViewManagerBinding();
			res = jo.Call<bool>("_ShowViewController", viewControllerName);
			#endif			
		}
		return res;
	}

	public static bool HideViewController()
	{
		bool res = false;
		if (Application.platform == RuntimePlatform.IPhonePlayer) {
			#if UNITY_IOS
			res = _HideViewController();
			#endif
		}
		else if (Application.platform == RuntimePlatform.Android) {
			#if UNITY_ANDROID && FALSE
			AndroidJavaObject jo = GetAndroidViewManagerBinding();
			res = jo.Call<bool>("_HideViewController");
			#endif			
		}
		return res;
	}

	public static void ShowSimpleAlert(string title, string message, string buttonTitle)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) {
			#if UNITY_IOS
			_ShowSimpleAlert(title, message, buttonTitle);
			#endif
		}
		else if (Application.platform == RuntimePlatform.Android) {
			#if UNITY_ANDROID && FALSE
			Debug.LogError("ShowSimpleAlert is not implemented on Android!");
			#endif			
		}
	}
}
