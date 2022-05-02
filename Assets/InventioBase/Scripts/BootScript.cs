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

public class BootScript : MonoBehaviour {

	public dfPanel locationPanel = null;
	public dfButton startButton = null;

	public dfPanel deniedPanel = null;
	public dfButton settingsButton = null;

	public dfPanel warmupPanel = null;
	public dfLabel warmupLabel = null;
	public dfProgressBar warmupProgress = null;

	public string startSceneName = null;

	void Start () {
		if (Application.isEditor) {
			gameObject.SetActive(false);
			LoadStartScene();
			return;
		}
		bool success = true;


		if ((this.locationPanel == null) || (this.startButton == null)) {
			Debug.LogWarning("BootScript: Needs locationPanel and startButton!");
			success = false;
		}
		if ((this.deniedPanel == null) || (this.settingsButton == null)) {
			Debug.LogWarning("BootScript: Needs deniedPanel and settingsButton!");
			success = false;
		}
		if ((this.warmupPanel == null) || (this.warmupLabel == null) || (this.warmupProgress == null)) {
			Debug.LogWarning("BootScript: Needs warmupPanel, warmupLabel and warmupProgress!");
			success = false;
		}
		if ((this.startSceneName == null) || (this.startSceneName.Length == 0)) {
			Debug.LogWarning("BootScript: Needs startSceneName!");
			success = false;
		}

		if (!success) {
			gameObject.SetActive(false);
			return;
		}

		this.locationPanel.Hide ();
		this.deniedPanel.Hide ();
		this.warmupPanel.Hide ();

		SpatialManagerPlugin.SpatialLocationAuthorization auth = SpatialManagerPlugin.GetLocationAuthorizationStatus ();
		switch (auth) {
		case SpatialManagerPlugin.SpatialLocationAuthorization.NotDetermined:
			SetupAuthorizationNotDetermined();
			break;
		case SpatialManagerPlugin.SpatialLocationAuthorization.Denied:
			SetupAuthorizationDenied();
			break;
		case SpatialManagerPlugin.SpatialLocationAuthorization.Granted:
			// Wait for the splash to disappear...
			Invoke ("SetupAuthorizationGranted", 5f);
			break;
		default:
			Debug.LogError("BootScript.Start: Illegal SpatialLocationAuthorization " + auth);
			gameObject.SetActive(false);
			return;
			break;
		}

		Screen.sleepTimeout = SleepTimeout.NeverSleep;
	}
	
	private void SetupAuthorizationNotDetermined()
	{
		this.locationPanel.Show ();
		this.deniedPanel.Hide ();
		this.warmupPanel.Hide ();
		this.startButton.Click += new MouseEventHandler(StartBootActivated);
	}

	private void SetupAuthorizationDenied()
	{
		this.locationPanel.Hide ();
		this.deniedPanel.Show ();
		this.warmupPanel.Hide ();
		if (InventioConfigManager.CanOpenAppSettings ()) {
			this.settingsButton.Click += new MouseEventHandler (OpenAppSettings);
		} else {
			this.settingsButton.Hide();
		}
	}

	private void SetupAuthorizationGranted()
	{
		this.locationPanel.Hide ();
		this.deniedPanel.Hide ();
		this.warmupPanel.Show ();

		this.warmupProgress.Value = 0f;
		SpatialManagerPlugin.StartSpatialWarmup (SpatialWarmupUpdated);
	}


	// Authorization Not Determined
	public void StartBootActivated(dfControl control, dfMouseEventArgs mouseEvent)
	{
		SpatialManagerPlugin.RequestLocationAuthorization (LocationAuthorizationResponse);
	}

	public void LocationAuthorizationResponse( bool authorized )
	{
		if (authorized) {
			Invoke("SetupAuthorizationGranted", .1f);
		} else {
			SetupAuthorizationDenied();
			Invoke("SetupAuthorizationDenied", .1f);
		}
	}

	// Denied
	public void OpenAppSettings(dfControl control, dfMouseEventArgs mouseEvent)
	{
		InventioConfigManager.OpenAppSettings ();
	}

	// Warmup
	private float warmupLevel = -1f;
	public void SpatialWarmupUpdated( int level )
	{
		if (warmupLevel < 0) {
			// First update, configure progress
			this.warmupProgress.MinValue = 0f;
			this.warmupProgress.MaxValue = level;
		}
		warmupLevel = this.warmupProgress.MaxValue - level;
		this.warmupProgress.Value = warmupLevel;

		if (level == 0) {
			SpatialManagerPlugin.StopSpatialWarmup();
			this.warmupLabel.Text = "Loading...";
			Invoke ("LoadStartScene", 1f);
		}
	}

	private void LoadStartScene()
	{
		Application.LoadLevel(this.startSceneName);
	}
}
