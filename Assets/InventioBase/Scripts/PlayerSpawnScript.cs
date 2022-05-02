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
	public class PlayerSpawnScript : MonoBehaviour {

		public bool useSpawnPosition = true;

		private GameObject spawnPositionObject = null;
		void Start () {
			if (this.useSpawnPosition) {
				GameObject spawn = GameObject.Find ("PlayerSpawnPosition");
				if (spawn != null) {
					this.spawnPositionObject = spawn;
				}
				else {
					this.spawnPositionObject = new GameObject("PlayerSpawnPosition");
					GameObject.DontDestroyOnLoad(this.spawnPositionObject);
				}
			}
		}

		void OnDestroy()
		{
			if (this.useSpawnPosition && this.spawnPositionObject != null) {
				spawnPositionObject.transform.position = transform.position;
			}
		}

		void OnLevelWasLoaded(int level) {
			if (this.useSpawnPosition) {
				GameObject spawn = GameObject.Find ("PlayerSpawnPosition");
				if (spawn != null) {
					Vector3 rayOrigin = spawn.transform.position + Vector3.up * 20f;
					RaycastHit hit;
					if (Physics.Raycast (rayOrigin, Vector3.down, out hit)) {
						transform.position = hit.point + hit.normal * .5f;
					}
					else {
						Debug.LogWarning("PlayerSpawnScript: Didn't find ground at spawn position!");
					}
				}
			}
		}

	}
}
