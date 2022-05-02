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
using AOT;

public class InventioSnapshotManager : MonoBehaviour {
	#if UNITY_IOS
	[DllImport ("__Internal")]
	private static extern bool _EnableSnapshot();
	[DllImport ("__Internal")]
	private static extern bool _ActivateSnapshot();

	public delegate void CaptureScreenshotCallback(string filename);

	[DllImport ("__Internal")]
	private static extern void _SetCaptureScreenshotCallback(CaptureScreenshotCallback callback);

	#endif
	#if UNITY_ANDROID
	#endif

	public static bool EnableSnapshot()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) {
			#if UNITY_IOS
			_SetCaptureScreenshotCallback(Capture);
			return _EnableSnapshot();
			#endif
		}
		else if (Application.platform == RuntimePlatform.Android) {
			#if UNITY_ANDROID
			Debug.LogError("InventioSnapshotManager not implemented for Android!!!");
			#endif			
		}

		return false;
	}

	public static bool ActivateSnapshot()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) {
			#if UNITY_IOS
			return _ActivateSnapshot();
			#endif
		}
		else if (Application.platform == RuntimePlatform.Android) {
			#if UNITY_ANDROID
			Debug.LogError("InventioSnapshotManager not implemented for Android!!!");
			#endif			
		}
		return false;
	}


	// Callbacks from Native code
	public static void CaptureScreenshot(string filename)
	{
		ScreenCapture.CaptureScreenshot(filename);
	}

	#if UNITY_IOS
	[MonoPInvokeCallback (typeof (CaptureScreenshotCallback))]
	private static void Capture(string filename) {
		CaptureScreenshot (filename); // Tidigare använd i Mono-bryggan
	}
	#endif
}
