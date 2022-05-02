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

public class DetailTouchController3D : MonoBehaviour {

	private Transform cameraTransform = null;
	public Transform CameraTransform {
		set { cameraTransform = value; }
	}
	private float maximumScale = 2f;
	public float MaximumScale {
		set { maximumScale = value; }
	}

	private Vector3 originalScale = Vector3.one;
	public Vector3 OriginalScale {
		set { originalScale = value; }
		get { return originalScale; }
	}

	private bool touchEnabled = false;
	public bool TouchEnabled {
		set { touchEnabled = value; }
	}

	private float steadyRotation = 60f; // Initial steady rotation [degrees/s]

	void Start () 
	{
		if (cameraTransform == null) {
			Debug.LogError ("DetailTouchController3D missing cameraTransform!!!");
			gameObject.SetActive (false);
		}
	}
	
	void Update()
	{
		if (touchEnabled) {
			HandleRotation ();
			HandleScale ();
		}
		PerformRotation ();
	}

	private float rotationRate = .3f;
	private Vector2 currentRotationSpeed = Vector2.zero;
	private void HandleRotation() {
		if (Input.touchCount == 1) {
			steadyRotation = 0f;
			Touch theTouch = Input.GetTouch (0);
			if ((theTouch.phase == TouchPhase.Began) || (theTouch.phase == TouchPhase.Stationary)) {
				// No rotation when steady
				currentRotationSpeed = Vector2.zero;
			} else if (theTouch.phase == TouchPhase.Moved) {
				currentRotationSpeed.x = -theTouch.deltaPosition.x * rotationRate;
				currentRotationSpeed.y = theTouch.deltaPosition.y * rotationRate;
			}
		} else if (Input.touchCount > 1) {
			// Stop rotation if multi touch
			currentRotationSpeed = Vector2.zero;
		}

	}

	private void PerformRotation() {
		if (steadyRotation > 0f) {
			gameObject.transform.RotateAround (gameObject.transform.position, cameraTransform.up, steadyRotation * Time.unscaledDeltaTime);
		} else {
			if (currentRotationSpeed.magnitude > 0.1) {
				gameObject.transform.RotateAround (gameObject.transform.position, cameraTransform.up, currentRotationSpeed.x);
				gameObject.transform.RotateAround (gameObject.transform.position, cameraTransform.right, currentRotationSpeed.y);
				currentRotationSpeed *= 0.99f;
			}
			else {
				currentRotationSpeed = Vector2.zero;
			}
		}
	}

	private float currentScaleFactor = 1f;
	private float currentTouchDistance = -1f;
	private void HandleScale() {
		if (Input.touchCount == 2) {
			steadyRotation = 0f;
			Touch touch1 = Input.GetTouch (0);
			Touch touch2 = Input.GetTouch (1);
			float touchDistance = Vector2.Distance (touch1.position, touch2.position);
			float scale = 1f;
			if (currentTouchDistance > 0) {
				scale = touchDistance / currentTouchDistance;
			}
			currentTouchDistance = touchDistance;
			if ((touch1.phase == TouchPhase.Moved) || (touch2.phase == TouchPhase.Moved)) {
				currentScaleFactor *= scale;
				if (currentScaleFactor > maximumScale) {
					currentScaleFactor = maximumScale;
				}
				else if (currentScaleFactor < .5f) {
					currentScaleFactor = .5f;
				}
				gameObject.transform.localScale = originalScale * currentScaleFactor;
			}
		} else {
			currentTouchDistance = -1f;
		}
	}
}
