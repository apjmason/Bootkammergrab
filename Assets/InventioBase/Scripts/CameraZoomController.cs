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

public class CameraZoomController : MonoBehaviour, IInventioControllerObserverInterface, IGeneralGuiDelegateInterface {

	public float minFOV = 20f;

	private float defaultFOV;
	private float fovRange;
	private Camera playerCamera = null;

	private float defaultTapDistance;
	private bool balloonsEnabled = false;
	public bool BalloonsEnabledInZoom {
		set { balloonsEnabled = value; }
	}

	void Start () {
		GameObject cameraObject = GameObject.Find ("/Player/Main Camera");
		if ((cameraObject == null) || (cameraObject.GetComponent<Camera>() == null)) {
			Debug.LogWarning ("CameraZoomController " + name + " couldn't find Main Camera!");
			gameObject.SetActive (false);
			return;
		}
		if (minFOV < 0) {
			Debug.LogWarning ("CameraZoomController " + name + " has illegal minFOV value!");
			gameObject.SetActive (false);
			return;
		}
		if (minFOV >= cameraObject.GetComponent<Camera>().fieldOfView) {
			Debug.LogWarning ("CameraZoomController " + name + " has minFOV > camera's default FOV!");
			gameObject.SetActive (false);
			return;
		}

		playerCamera = cameraObject.GetComponent<Camera>();
		defaultFOV = playerCamera.fieldOfView;
		fovRange = defaultFOV - minFOV;

		enabled = false; // No updates necessary

		InventioController.SharedController().AddObserver (this);
	}

	public void ZoomMenuButtonPressed( int buttonId )
	{
		if (isInactive && GeneralGuiProxy.SharedGui ().RequestOwnership (this)) {
			InventioAnalyticsManager.logEvent("CameraZoomActivated", null);
			TrackerNGPlugin.TrackZoom();
			if (balloonsEnabled) {
				defaultTapDistance = BalloonManager.SharedBalloonManager().TapDistance;
			} else {
				TouchController.SharedTouchController ().SetBalloonsEnabled (false);
			}
			GlobalMenuProxy.SharedMenu().PushHideMenu();
		}
	}

	private float CurrentFOVValue {
		get { return (defaultFOV -  playerCamera.fieldOfView) / fovRange * 100f; }
		set { playerCamera.fieldOfView = defaultFOV - fovRange / 100f * value; }
	}

	private bool isActive = false;
	private bool isInactive = true;
	private bool hasGui = false;

	private void ZoomIn()
	{
		isInactive = false;
		isActive = false;

		LeanTween.value(gameObject, UpdateFOV, this.CurrentFOVValue, 20f, 1f).setEase(LeanTweenType.easeInOutCubic).setOnComplete(ZoomInFinished).setUseEstimatedTime(true);
	}

	private void ZoomInFinished()
	{
		isActive = true;
		if (hasGui) {
			GeneralGuiProxy.SharedGui ().EnableButtonTopLeft (LocalizationControllerProxy.Localize("Base.ExitCameraZoom"));
			GeneralGuiProxy.SharedGui ().EnableSliderRight (LocalizationControllerProxy.Localize("Base.CameraZoomLabel"), this.CurrentFOVValue);
		}
	}

	private void ZoomOut()
	{
		isActive = false;

		LeanTween.value(gameObject, UpdateFOV, this.CurrentFOVValue, 0f, 1).setEase(LeanTweenType.easeInOutCubic).setOnComplete(ZoomOutFinished).setUseEstimatedTime(true);
	}

	private void ZoomOutFinished()
	{
		if (hasGui) {
			GeneralGuiProxy.SharedGui ().DropOwnership ();
		}
		isInactive = true;
		hasGui = false;

		if (balloonsEnabled) {
			BalloonManager.SharedBalloonManager ().TapDistance = defaultTapDistance;
		} else {
			TouchController.SharedTouchController ().SetBalloonsEnabled (true);
		}
		GlobalMenuProxy.SharedMenu().PopHideMenu();
	}

	private void UpdateFOV( float val )
	{
		this.CurrentFOVValue = val;
	}


	/*
	 * IInventioControllerObserverInterface
	 */
	public void InventioSetupComplete()
	{
		GlobalMenuProxy.SharedMenu ().AddRightButton (LocalizationControllerProxy.Localize("Base.CameraZoom"), ZoomMenuButtonPressed);
	}


	/*
	 * IGeneralGuiDelegateInterface
	 */
	public bool ReceiveOwnership()
	{
		if (isInactive) {
			// Start moving
			ZoomIn ();
		} else if (isActive) {
			GeneralGuiProxy.SharedGui ().EnableButtonTopLeft (LocalizationControllerProxy.Localize("Base.ExitCameraZoom"));
			GeneralGuiProxy.SharedGui ().EnableSliderRight (LocalizationControllerProxy.Localize("Base.CameraZoomLabel"), this.CurrentFOVValue);
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
		GeneralGuiProxy.SharedGui ().EnableSliderRight (null);
		ZoomOut ();
	}

	public void ActivateButtonTopRight()
	{
		// NOP
	}

	public void ChangeValueSliderLeft(float value) // value=[0,100]
	{
		// NOP
	}

	public void ChangeValueSliderRight(float value) // value=[0,100]
	{
		this.CurrentFOVValue = value;

		if (balloonsEnabled) {
			float currentFOV = defaultFOV - fovRange / 100f * value;
			BalloonManager.SharedBalloonManager ().TapDistance = this.defaultTapDistance * (defaultFOV / currentFOV);
		}
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
