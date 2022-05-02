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

public class InventioAnalytics : MonoBehaviour {

	public bool useAnalytics = true;
	public string analyticsServiceClassName = null;
	public string analyticsKey = null;
	public string miscString = null;

	public bool useTracking = true;
	public string locationTrackingApplicationId = null;
	public int locationTrackingInterval = 30; // [s]

	void Start () {
		if (useAnalytics || useTracking) {
			Invoke("ActivateAnalyticsAndTracking", 5);
		}
	}

	private void ActivateAnalyticsAndTracking()
	{
		if (useAnalytics && (analyticsServiceClassName != null) && (analyticsKey != null)) {
			InventioAnalyticsManager.activateAnalytics(analyticsServiceClassName, analyticsKey, miscString);
		}

		if (useTracking && (locationTrackingApplicationId != null)) {
			InventioBase.TrackerNGPlugin.StartTrackerNGSession(locationTrackingApplicationId, locationTrackingInterval);
		}

		gameObject.SetActive(false);
	}
}
