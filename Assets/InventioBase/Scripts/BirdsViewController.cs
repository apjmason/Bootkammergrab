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

public class BirdsViewController : MonoBehaviour, IInventioControllerObserverInterface, IGeneralGuiDelegateInterface {

	public float minAltitude = 10f;
	public float maxAltitude = 100f;
	public float flyTime = 1f;

	private Transform player = null;
	private Transform mainCamera = null;
	private Vector3 cameraLocalPosition;
	private float altitudeRange;

	private CameraZoomController zoomController = null;

	void Start () {
		GameObject cameraObject = GameObject.Find ("/Player/Main Camera");
		if (cameraObject == null) {
			Debug.LogWarning ("BirdsViewController " + name + " couldn't find Main Camera!");
			gameObject.SetActive (false);
			return;
		}
		if ((minAltitude < 0) || (minAltitude >= maxAltitude)) {
			Debug.LogWarning ("BirdsViewController " + name + " has illegal min/max altitudes!");
			gameObject.SetActive (false);
			return;
		}
		if (flyTime < 0) {
			Debug.LogWarning ("BirdsViewController " + name + " has illegal Fly Time!");
			gameObject.SetActive (false);
			return;
		}
		altitudeRange = maxAltitude - minAltitude;
		mainCamera = cameraObject.transform;
		player = cameraObject.transform.parent;
		cameraLocalPosition = cameraObject.transform.localPosition;

		// Convert min/max altitudes to local altitudes above cameraLocalPosition
		minAltitude += cameraLocalPosition.y;
		maxAltitude += cameraLocalPosition.y;

		enabled = false; // No updates necessary

		InventioController.SharedController().AddObserver (this);
	}
	
	public void BirdsViewMenuButtonPressed( int buttonId )
	{
		if (isHome && GeneralGuiProxy.SharedGui ().RequestOwnership (this)) {
			InventioAnalyticsManager.logEvent("BirdsViewActivated", null);
			TrackerNGPlugin.TrackBirdsView();
			TouchController.SharedTouchController ().SetBalloonsEnabled (false);
			GlobalMenuProxy.SharedMenu().PushHideMenu();
		}
	}

	private float CurrentAltitudeValue {
		get { return (transform.localPosition.y - minAltitude) / altitudeRange * 100f; }
		set { LeanTween.moveLocal (gameObject, Vector3.up * (minAltitude + (value / 100f) * altitudeRange), .1f).setUseEstimatedTime(true); }
	}

	private bool isHome = true;
	private bool isOut = false;
	private bool hasGui = false;

	private void FlyUp()
	{
		isHome = false;
		isOut = false;

		// Re-parent the camera in order to move it
		transform.position = mainCamera.position;
		transform.parent = player;
		mainCamera.parent = transform;

		LeanTween.moveLocal(gameObject, Vector3.up * (minAltitude + altitudeRange / 2f), flyTime).setEase(LeanTweenType.easeInOutCubic).setOnComplete(FlyUpFinished).setUseEstimatedTime(true);
	}

	private void FlyUpFinished()
	{
		isOut = true;
		if (hasGui) {
			GeneralGuiProxy.SharedGui ().EnableButtonTopLeft (LocalizationControllerProxy.Localize("Base.ExitBirdsView"));
			if (zoomController != null) {
				GeneralGuiProxy.SharedGui ().EnableButtonTopRight (LocalizationControllerProxy.Localize("Base.CameraZoom"));
			}
			GeneralGuiProxy.SharedGui ().EnableSliderRight (LocalizationControllerProxy.Localize("Base.BirdsViewLabel"), this.CurrentAltitudeValue);
		}
	}

	private void FlyDown()
	{
		isOut = false;

		LeanTween.moveLocal(gameObject, cameraLocalPosition, flyTime).setEase(LeanTweenType.easeInOutCubic).setOnComplete(FlyDownFinished).setUseEstimatedTime(true);
	}

	private void FlyDownFinished()
	{
		if (hasGui) {
			GeneralGuiProxy.SharedGui ().DropOwnership ();
		}
		isHome = true;
		hasGui = false;
		mainCamera.parent = player;
		transform.parent = null;

		TouchController.SharedTouchController ().SetBalloonsEnabled (true);
		GlobalMenuProxy.SharedMenu().PopHideMenu();
	}


	/*
	 * IInventioControllerObserverInterface
	 */
	public void InventioSetupComplete()
	{
		GlobalMenuProxy.SharedMenu ().AddRightButton (LocalizationControllerProxy.Localize("Base.BirdsView"), BirdsViewMenuButtonPressed);

		GameObject zoomObject = GameObject.Find ("CameraZoomController");
		if (zoomObject != null) {
			zoomController = zoomObject.GetComponent<CameraZoomController> ();
		}
	}


	/*
	 * IGeneralGuiDelegateInterface
	 */
	public bool ReceiveOwnership()
	{
		if (isHome) {
			// Start moving
			FlyUp ();
		} else if (isOut) {
			GeneralGuiProxy.SharedGui ().EnableButtonTopLeft (LocalizationControllerProxy.Localize("Base.ExitBirdsView"));
			if (zoomController != null) {
				GeneralGuiProxy.SharedGui ().EnableButtonTopRight (LocalizationControllerProxy.Localize("Base.CameraZoom"));
			}
			GeneralGuiProxy.SharedGui ().EnableSliderRight (LocalizationControllerProxy.Localize("Base.BirdsViewLabel"), this.CurrentAltitudeValue);
		}
		hasGui = true;
		return true;
	}

	public int DropOwnershipRequest() // 0=Dropped, 1=Waived, 2=Blocked
	{
		hasGui = false;
		return 1;
	}

	public void ActivateButtonTopLeft()
	{
		GeneralGuiProxy.SharedGui ().EnableButtonTopLeft (null);
		GeneralGuiProxy.SharedGui ().EnableButtonTopRight (null);
		GeneralGuiProxy.SharedGui ().EnableSliderRight (null);
		FlyDown ();
	}

	public void ActivateButtonTopRight()
	{
		if (zoomController != null) {
			zoomController.ZoomMenuButtonPressed (0); // Just set a dummy value for buttonId
		}
	}

	public void ChangeValueSliderLeft(float value) // value=[0,100]
	{
		// NOP
	}

	public void ChangeValueSliderRight(float value) // value=[0,100]
	{
		this.CurrentAltitudeValue = value;
	}

	public void ActivateProgressButtonLeft()
	{
		// NOP
	}

	public void ActivateProgressButtonRight()
	{
		// NOP
	}

}
