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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SitSimStayOnGround : MonoBehaviour {
	
		public float offset = 0f;

		void OnValidate() {
				hideFlags = HideFlags.HideInInspector;
		}

		#if UNITY_EDITOR
		void Update () {
				if (!Application.isPlaying) {
						Vector3 position = transform.position;
						position.y += 200f;

						RaycastHit[] hits = Physics.RaycastAll (position, Vector3.down);
						float shortestDistance = -1f;
						foreach (RaycastHit hit in hits) {
								if (((shortestDistance < 0) || (hit.distance < shortestDistance)) && !hit.transform.IsChildOf(transform)) {
										shortestDistance = hit.distance;
										position.y = hit.point.y + offset;
								}
						}
						if (shortestDistance >= 0) {
								transform.position = position;
						}
				}
		}
		#endif
}
