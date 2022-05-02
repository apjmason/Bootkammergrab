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

public class CameraRotationScript : MonoBehaviour {

	public bool ignoreTouches = false;

	private Transform playerMirrorTransform = null;

	private float minTouchX = 0f;
	private float touchRotateSpeed = 1.0f;

	void Start () {
		playerMirrorTransform = SpatialManagerPlugin.SharedMirrorObject().transform;

		minTouchX = Screen.width * 0.5f;
		touchRotateSpeed = 180f / Screen.height;
	}
	
	void Update () {
		if (SpatialManagerPlugin.HasValidAttitude()) {
			transform.rotation = playerMirrorTransform.rotation;
		}
		else if (!ignoreTouches) {
			HandleTouchRotation();
		}
	}

	private int fingerId = -1;
	private Vector2 fingerDownPosition = Vector2.zero;
	private void HandleTouchRotation()
	{
		int count = Input.touchCount;
		bool handled = false;
		for (int i = 0; i < count; ++i) {
			Touch touch = Input.GetTouch(i);
			if (fingerId >= 0) {
				// Handle current touch
				if (touch.fingerId == fingerId) {
					if ((touch.phase == TouchPhase.Moved)||
					    (touch.phase == TouchPhase.Stationary)) {
						Vector2 rotateDistance = (touch.position - fingerDownPosition) * touchRotateSpeed * Time.unscaledDeltaTime;
						transform.Rotate(0f, rotateDistance.x, 0f, Space.World);
						transform.Rotate(-rotateDistance.y, 0f, 0f);
						handled = true;
						break;
					}
					else if ((touch.phase == TouchPhase.Ended) ||
					         (touch.phase == TouchPhase.Canceled)) {
						fingerId = -1;
						handled = true;
						break;
					}
				}
				else {
					continue;
				}
			}
			else {
				// Find new touch
				if (touch.phase == TouchPhase.Began) {
					if (touch.position.x > minTouchX) {
						fingerId = touch.fingerId;
						fingerDownPosition = touch.position;
						handled = true;
						break;
					}
				}
			}
		}
		if (!handled) {
			fingerId = -1;
		}
	}
}
