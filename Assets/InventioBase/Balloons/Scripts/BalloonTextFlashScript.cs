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

public class BalloonTextFlashScript : MonoBehaviour {

	private Color defaultColor = Color.white;
	private Color activatedColor = Color.white;

	void Start () {
		defaultColor = gameObject.GetComponent<Renderer>().sharedMaterial.color;
		activatedColor = new Color (defaultColor.r * 0.3f, defaultColor.g * 0.3f, defaultColor.b * 0.3f);
	}

	private const float flashDuration = 0.2f;

	public void Flash()
	{
		gameObject.GetComponent<Renderer>().material.color = defaultColor;
		LeanTween.color (gameObject, activatedColor, flashDuration).setUseEstimatedTime(true);
	}
}
