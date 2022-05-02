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

public class MenuPanelSlideScript : MonoBehaviour {

	private dfTweenVector3 slideTween;
	private dfPanel panel;

	// Use this for initialization
	void Start () {
		slideTween = GetComponent<dfTweenVector3>();
		panel = GetComponent<dfPanel>();
	}
	
	public void PanelSlideIn()
	{
		if (slideTween && panel) {
			// For some reason, since the update to Unity 5 the panel slides 
			// to the right for each time snapshot's been activated, set posX to zero.
			Vector3 pos = gameObject.transform.localPosition;
			pos.x = 0f;
			gameObject.transform.localPosition = pos;

			SendMessage("PanelSlideInWillStart", SendMessageOptions.DontRequireReceiver);

			slideTween.EndValue = Vector3.up * panel.Height;
			slideTween.Play();

			SendMessage("PanelSlideInDidStart", SendMessageOptions.DontRequireReceiver);
		}
	}

	public void PanelSlideOut()
	{
		if (slideTween && panel) {
			SendMessage("PanelSlideOutWillStart", SendMessageOptions.DontRequireReceiver);

			slideTween.EndValue = -Vector3.up * panel.Height;
			slideTween.Play();

			SendMessage("PanelSlideOutDidStart", SendMessageOptions.DontRequireReceiver);
		}
	}
}
