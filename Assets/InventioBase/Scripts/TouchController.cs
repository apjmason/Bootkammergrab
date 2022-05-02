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

public interface ITouchConsumerInterface {
	bool ConsumesTouchEventAtPosition(float xPos, float yPos);
}

public class TouchController : MonoBehaviour {

	private static TouchController sharedInstance = null;
	public static TouchController SharedTouchController()
	{
		if (sharedInstance == null) {
			GameObject touchControllerObject = new GameObject("TouchController");
			sharedInstance = touchControllerObject.AddComponent<TouchController>();
		}
		return sharedInstance;
	}

	private ArrayList touchConsumers = new ArrayList();

	private bool configEnabled = false;
	private bool balloonsEnabled = true;
	private Camera currentCamera = null;

	void Update ()
	{
		bool validTapTouch = false;
		#if UNITY_EDITOR
		validTapTouch = (Input.GetMouseButtonDown (0) || Input.GetMouseButtonUp (0));
		#else
		validTapTouch = (Input.touchCount > 0 && ((Input.GetTouch (0).phase == TouchPhase.Began) || (Input.GetTouch (0).phase == TouchPhase.Ended)));
		#endif

		if (configEnabled && (Input.touchCount == 4) && (Input.GetTouch (3).phase == TouchPhase.Ended)) {
			// TODO: Doesn't detect four finger touch on android...
			InventioViewManager.ShowViewController ("InventioConfigViewController");
		} else if (validTapTouch) {
			if (this.ConsumesTouchEventAtPosition (Input.mousePosition.x, Input.mousePosition.y)) {
				// NOP - Just filter
			} else if (balloonsEnabled) {
				BalloonManager.SharedBalloonManager ().CheckForBalloonTap (currentCamera);
			}
		}
	}

	public void AddTouchConsumer(ITouchConsumerInterface consumer)
	{
		if (!touchConsumers.Contains(consumer)) {
			touchConsumers.Add(consumer);
		}
	}
	public void RemoveTouchConsumer(ITouchConsumerInterface consumer)
	{
		touchConsumers.Remove(consumer);
	}
	private bool ConsumesTouchEventAtPosition(float xPos, float yPos)
	{
		// Iterate over a copy of the observers list, in case any observer is removed during iteration
		ArrayList consumersClone = touchConsumers.Clone () as ArrayList;
		foreach (ITouchConsumerInterface consumer in consumersClone) {
			if (consumer.ConsumesTouchEventAtPosition (xPos, yPos)) {
				return true;
			}
		}
		return false;
	}


	public void SetConfigEnabled(bool isEnabled)
	{
		configEnabled = isEnabled;
	}

	public void SetBalloonsEnabled(bool isEnabled)
	{
		balloonsEnabled = isEnabled;
	}

	public void SetCurrentCamera(Camera newCamera)
	{
		currentCamera = newCamera;
	}

	public Camera GetCurrentCamera()
	{
		return currentCamera;
	}
}
