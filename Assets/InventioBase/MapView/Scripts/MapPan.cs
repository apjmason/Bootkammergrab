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

namespace InventioBase {
	public class MapPan : MonoBehaviour {
		private int fingerId = -1;
		private Vector2 fingerDownPosition = Vector2.zero;

		private Camera mainCamera = null;
		public Camera MainCamera {
			set { mainCamera = value; }
		}

		private Vector2 mapPosMin;
		public Vector2 MapPosMin {
			set { mapPosMin = value; }
		}
		private Vector2 mapPosMax;
		public Vector2 MapPosMax {
			set { mapPosMax = value; }
		}

		void Update () {
			HandleTouch ();
		}

		private void HandleTouch()
		{
			if (this.mainCamera != null) {
				int count = Input.touchCount;
				bool handled = false;
				for (int i = 0; i < count; ++i) {
					Touch touch = Input.GetTouch (i);
					if (fingerId >= 0) {
						// Handle current touch
						if (touch.fingerId == fingerId) {
							if ((touch.phase == TouchPhase.Moved) ||
								(touch.phase == TouchPhase.Stationary)) {
								Vector3 lastPosition = this.mainCamera.ScreenToWorldPoint(new Vector3(this.fingerDownPosition.x, this.fingerDownPosition.y, transform.position.y));
								Vector3 currPosition = this.mainCamera.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, transform.position.y));
								Vector3 moveVector = lastPosition - currPosition;
								moveVector.y = 0;
								transform.position += moveVector;

								Vector3 limitMove = Vector3.zero;
								if (transform.position.x < this.mapPosMin.x || transform.position.x > this.mapPosMax.x) {
									limitMove.x = moveVector.x;
								}
								if (transform.position.z < this.mapPosMin.y || transform.position.z > this.mapPosMax.y) {
									limitMove.z = moveVector.z;
								}
								transform.position -= limitMove;

								fingerDownPosition = touch.position;
								handled = true;
								break;
							} else if ((touch.phase == TouchPhase.Ended) ||
								(touch.phase == TouchPhase.Canceled)) {
								fingerId = -1;
								handled = true;
								break;
							}
						} else {
							continue;
						}
					} else {
						// Find new touch
						if (touch.phase == TouchPhase.Began) {
							fingerId = touch.fingerId;
							fingerDownPosition = touch.position;
							handled = true;
							break;
						}
					}
				}
				if (!handled) {
					fingerId = -1;
				}
			} else {
				Debug.LogWarning("MapPan missing mainCamera!");
			}
		}
	}
}