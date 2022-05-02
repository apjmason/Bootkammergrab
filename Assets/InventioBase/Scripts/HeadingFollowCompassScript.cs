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

public class HeadingFollowCompassScript : MonoBehaviour {
	public float offset = 0f;

	private Transform playerMirrorTransform = null;

	void Start () {
		playerMirrorTransform = SpatialManagerPlugin.SharedMirrorObject().transform;
	}

	void OnLevelWasLoaded(int level) {
		playerMirrorTransform = SpatialManagerPlugin.SharedMirrorObject().transform;
	}

	void Update () {
		if (SpatialManagerPlugin.HasValidAttitude()) {

			transform.eulerAngles = new Vector3(0f, playerMirrorTransform.eulerAngles.y + this.offset, 0f);
		}
	}
}
