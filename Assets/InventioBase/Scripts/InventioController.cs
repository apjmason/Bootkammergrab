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

using InventioBase;

public interface IInventioControllerObserverInterface {
	void InventioSetupComplete();
}

public class InventioController : MonoBehaviour {

	public string appName = "";
	public double longitude = 0;
	public double latitude = 0;
	public bool requestLocationUse = false;
	public bool initialScene = true;

	public bool snapshotEnabled = true;
	public float zoomFOV = 30f;
	public bool zoomEnabledBalloons = false;

	public Vector2 birdPerspectiveRange = Vector2.zero;
	public bool configEnabled = false;

	public GameObject globalMenuPrefab = null;
	public GameObject generalGuiPrefab = null;

	public int targetFrameRate = 60;
	public bool neverSleep = true;

	public string sharedURL = null;

	public bool limitWebViewToInitialHost = false;

	private ArrayList observers = new ArrayList();
	private bool setupComplete = false;

	private static InventioController sharedControllerInstance = null;
	public static InventioController SharedController()
	{
		return sharedControllerInstance;
	}

	void Awake() {
		InventioController.sharedControllerInstance = this;
		LeanTween.init ();
	}

	void Start () {
		Application.targetFrameRate = targetFrameRate;
		if (neverSleep) {
			Screen.sleepTimeout = SleepTimeout.NeverSleep;
		}

		if (globalMenuPrefab != null) {
			Object.Instantiate(globalMenuPrefab);
		}
		if (generalGuiPrefab != null) {
			Object.Instantiate(generalGuiPrefab);
		}

		if (appName.Length > 0) {
			InventioConfigManager.SetApplicationName (appName);
			InventioConfigManager.SetApplicationLocation (latitude, longitude);
		}

		if (initialScene) {
			if (requestLocationUse) {
				SpatialManagerPlugin.RequestLocationAuthorization (onLocationAuthorized);
			} else {
				SpatialManagerPlugin.StartUpdatingLocationUsingReferenceLocation (latitude, longitude);
			}
			SpatialManagerPlugin.StartUpdatingOrientation();
		}
		else {
			SpatialManagerPlugin.SetReferenceLocation(latitude, longitude);
		}

		if (snapshotEnabled) {
			GameObject snapshotController = new GameObject ("SnapshotController");
			snapshotController.AddComponent<SnapshotController> ();
		}

		if (zoomFOV > 0) {
			GameObject zoomObject = new GameObject ("CameraZoomController");
			CameraZoomController zoomController = zoomObject.AddComponent<CameraZoomController> ();
			zoomController.minFOV = zoomFOV;
			zoomController.BalloonsEnabledInZoom = zoomEnabledBalloons;
		}

		if (birdPerspectiveRange != Vector2.zero) {
			GameObject birdsViewObject = new GameObject ("BirdsViewController");
			BirdsViewController birdsViewController = birdsViewObject.AddComponent<BirdsViewController> ();
			birdsViewController.minAltitude = birdPerspectiveRange.x;
			birdsViewController.maxAltitude = birdPerspectiveRange.y;
		}

		TouchController.SharedTouchController().SetConfigEnabled(configEnabled);
		TouchController.SharedTouchController().SetCurrentCamera(Camera.main);

		// Create Audio Comments Controller
		GameObject audioController = new GameObject ("AudioCommentsController");
		audioController.AddComponent<AudioCommentsController>();

		// Check for LocalizationManager
		if (LocalizationControllerProxy.SharedLocalizer == null) {
			Debug.LogError ("InventioController: Couldn't find LocalizationController!");
		}

		if ((sharedURL != null) && (sharedURL.Length > 0)) {
			InventioSocialManager.SetSharedURL(sharedURL);
		}

		if (limitWebViewToInitialHost) {
			// Dont allow WebViews to navigate outside the initial host
			InventioWebView.LimitWebViewToInitialHost();
		}

		InventioConfigManager.UpdateSpatialSettings ();

		Invoke("NotifyObserversComplete", 0.5f);
	}
	void OnApplicationQuit()
	{
		SpatialManagerPlugin.StopUpdates ();
	}

	private void NotifyObserversComplete()
	{
		// Iterate over a copy of the observers list, in case any observer is removed during iteration
		ArrayList observersClone = observers.Clone () as ArrayList;
		foreach (IInventioControllerObserverInterface observer in observersClone) {
			observer.InventioSetupComplete();
		}
	}

	public void AddObserver(IInventioControllerObserverInterface observer)
	{
		if (!observers.Contains(observer)) {
			observers.Add(observer);
		}
		if (setupComplete) {
			observer.InventioSetupComplete ();
		}
	}
	public void RemoveObserver(IInventioControllerObserverInterface observer)
	{
		observers.Remove(observer);
	}

	public void onLocationAuthorized(bool authorized) 
	{
		if (authorized) {
			SpatialManagerPlugin.StartUpdatingLocationUsingReferenceLocation(latitude, longitude);
		}
	}
}
