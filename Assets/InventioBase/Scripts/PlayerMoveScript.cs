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

[RequireComponent (typeof (CharacterController))]

public class PlayerMoveScript : MonoBehaviour {
	public float speed = 5f;
	public float touchMoveDistance = 500f;
	public bool ignoreTouches = false;
	public Transform playerCamera = null;

	private Transform playerMirrorTransform = null;
	private CharacterController character = null;
		
	private float maxTouchX = 0f;
	private float touchMoveSpeed = 1.0f;

	// Use this for initialization
	void Start () {
		playerMirrorTransform = SpatialManagerPlugin.SharedMirrorObject().transform;
		character = GetComponent<CharacterController>();

		maxTouchX = Screen.width * 0.5f;
		touchMoveSpeed = 15f / Screen.height;

		BalloonManager.SharedBalloonManager ().CurrentCamera = playerCamera.gameObject.GetComponent<Camera> ();
	}

	public bool IsWithinArea {
		get {
			Vector3 distance = playerMirrorTransform.position - transform.position;
			distance.y = 0f;
			return (distance.magnitude < touchMoveDistance);
		}
	}

	private Vector3 movement = Vector3.zero;
	private bool touchMove = false;
	private int gpsAlertId = 0;
	void Update () {
		if (character.isGrounded) {
			touchMove = true;
			if (SpatialManagerPlugin.UsingTouchMove ()) {
				// NOP
			}
			else if (SpatialManagerPlugin.HasValidPosition()) {
				movement = playerMirrorTransform.position - transform.position;
				movement.y = 0f; // Move only horizontally
				if (movement.magnitude < touchMoveDistance) {
					touchMove = false;
					if (movement.magnitude < 0.1f) {
						movement = Vector3.zero;
					}
					else {
						// Move the player with a speed to get the desired location in 0.5 seconds
						movement *= 2f;
					}
				}
			}
			else if (gpsAlertId == 0) {
				gpsAlertId = GeneralGuiProxy.SharedGui().AddFlashMessage(LocalizationControllerProxy.Localize("Base.NoValidGPS"));
			}

			if (touchMove) {
				if (ignoreTouches) {
					movement = Vector3.zero;
				}
				else {
					movement = HandleTouchMove() * speed;
				}
			}
			else if (gpsAlertId != 0) {
				GeneralGuiProxy.SharedGui().RemoveFlashMessage(gpsAlertId);
				gpsAlertId = 0;
			}
		}

		movement += Physics.gravity;
		movement *= Time.unscaledDeltaTime;

		character.Move(movement);
	}

	private int fingerId = -1;
	private Vector2 fingerDownPosition = Vector2.zero;
	private Vector3 HandleTouchMove()
	{
		Vector3 touchMovement = Vector3.zero;

		if (playerCamera != null) {
			int count = Input.touchCount;
			bool handled = false;
			for (int i = 0; i < count; ++i) {
				Touch touch = Input.GetTouch(i);
				if (fingerId >= 0) {
					// Handle current touch
					if (touch.fingerId == fingerId) {
						if ((touch.phase == TouchPhase.Moved)||
						    (touch.phase == TouchPhase.Stationary)) {
							Vector2 touchDistance = (touch.position - fingerDownPosition) * touchMoveSpeed;
							Vector3 localDirection = new Vector3(touchDistance.x, 0f, touchDistance.y);
							touchMovement = playerCamera.TransformDirection(localDirection);
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
						if (touch.position.x < maxTouchX) {
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
		else {
			Debug.LogError("PlayerMoveScript has no playerCamera set!!!");
		}
		return touchMovement;
	}
}
