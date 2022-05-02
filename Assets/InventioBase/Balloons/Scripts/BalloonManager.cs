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

public class BalloonManager : MonoBehaviour {
	private ArrayList balloons = new ArrayList();

	private Camera currentCamera = null;
	public Camera CurrentCamera {
		get { return currentCamera; }
		set { currentCamera = value; }
	}

	private float tapDistance = 20f;
	public float TapDistance {
		get { return tapDistance; }
		set { tapDistance = value; }
	}
	public float CloseDistance {
		get { return tapDistance+1f; }
	}

	private static BalloonManager sharedInstance = null;
	public static BalloonManager SharedBalloonManager()
	{
		if (sharedInstance == null) {
			GameObject balloonManagerObject = new GameObject("BalloonManager");
			sharedInstance = balloonManagerObject.AddComponent<BalloonManager>();
		}
		return sharedInstance;
	}

	public void AddBalloon(BalloonController balloon)
	{
		if (balloon) {
			balloons.Add (balloon);
		} else {
			Debug.LogWarning ("BalloonManager:AddBalloon: Can't add <null> balloon!");
		}
	}
	public void RemoveBalloon(BalloonController balloon)
	{
		if (balloon) {
			balloons.Remove (balloon);
		} else {
			Debug.LogWarning ("BalloonManager:AddBalloon: Can't remove <null> balloon!");
		}
	}

	private float mouseDownTime = 0f;
	private float lastTapTime = 0f;
	private BalloonController tappedBalloon = null;
	private float tappedBalloonDistance = 0f;
	public void CheckForBalloonTap(Camera usingCamera)
	{
		currentCamera = usingCamera;

		bool touchDown = false;
		bool touchUp = false;
		#if UNITY_EDITOR
		touchDown = Input.GetMouseButtonDown (0);
		touchUp =  Input.GetMouseButtonUp (0);
		#else
		touchDown = (Input.touchCount > 0 && (Input.GetTouch (0).phase == TouchPhase.Began));
		touchUp = (Input.touchCount > 0 && (Input.GetTouch (0).phase == TouchPhase.Ended));
		#endif

		if (touchDown) {
			mouseDownTime = Time.unscaledTime;
		} else if (touchUp) {
			if (Time.unscaledTime - mouseDownTime < 0.2) {
				if (Time.unscaledTime - lastTapTime > 2.0) {
					Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
					tappedBalloon = null;
					for (int idx = 0; idx < balloons.Count; ++idx) {
						BalloonController balloon = balloons [idx] as BalloonController;
						Collider tapCollider = balloon.TapCollider;
						if ((tapCollider != null) && balloon.enabled) {
							RaycastHit hit = new RaycastHit ();
							if (tapCollider.Raycast (ray, out hit, tapDistance)) {
								if ((tappedBalloon == null) || (hit.distance < tappedBalloonDistance)) {
									tappedBalloon = balloon;
									tappedBalloonDistance = hit.distance;
								}
							}
						}
					}
					if (tappedBalloon != null) {
						lastTapTime = Time.unscaledTime;

						AudioCommentsController controller = AudioCommentsController.SharedController ();
						if (controller != null) {
							controller.StopPlaying ();
						}

						tappedBalloon.ActivateBalloon (currentCamera);
					}
				}
			}
		}
	}
}
