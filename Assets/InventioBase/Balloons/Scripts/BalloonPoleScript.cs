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

public class BalloonPoleScript : MonoBehaviour {

	public Transform anchorTransform = null;

	void Update () {
		if (anchorTransform != null) {
			Vector3 vecFromAnchor = transform.position - anchorTransform.position;
			transform.localScale = new Vector3(1f, vecFromAnchor.magnitude, 1f);
			transform.up = vecFromAnchor;
			GetComponent<Renderer>().material.mainTextureScale = new Vector2 (1f, vecFromAnchor.magnitude * 10f);
		}
	}
}
