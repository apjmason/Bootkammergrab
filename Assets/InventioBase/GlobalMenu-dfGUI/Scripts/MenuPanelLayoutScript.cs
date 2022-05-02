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

public class MenuPanelLayoutScript : MonoBehaviour {
	private dfPanel panel;
	private dfPanel leftPanel;
	private dfPanel rightPanel;

	void Start () 
	{
		panel = GetComponent<dfPanel>();
		leftPanel = panel.Find<dfPanel>("LeftPanel");
		rightPanel = panel.Find<dfPanel>("RightPanel");

		if (panel == null) {
			Debug.LogError("MenuPanelLayout missing panel!");
			gameObject.SetActive(false);
			return;
		}
		if (leftPanel == null) {
			Debug.LogError("MenuPanelLayout missing left panel!");
		}
		if (rightPanel == null) {
			Debug.LogError("MenuPanelLayout missing right panel!");
		}
	}

	void PanelSlideInWillStart()
	{
		float splitPosition = panel.Size.x / 2f;
		if (leftPanel && rightPanel) {
			if (rightPanel.Controls.Count < 1) {
				splitPosition = panel.Size.x;
			}
			else if (rightPanel.Controls.Count < 3) {
				splitPosition = 2f/3f * panel.Size.x;
			}
			else if (leftPanel.Controls.Count < 3) {
				splitPosition = 1f/3f * panel.Size.x;
			}
		}
		else if (leftPanel) {
			splitPosition = panel.Size.x;
		}
		else if (rightPanel) {
			splitPosition = 0f;
		}

		float panelHeight = 0f;
		if (leftPanel) {
			leftPanel.Size = new Vector2(splitPosition, leftPanel.Size.y);
			leftPanel.RelativePosition = Vector2.zero;
			leftPanel.SendMessage("PerformLayout", SendMessageOptions.DontRequireReceiver);
			panelHeight = Mathf.Max(panelHeight, leftPanel.Size.y);
		}
		if (rightPanel) {
			rightPanel.Size = new Vector2(panel.Size.x - splitPosition, rightPanel.Size.y);
			rightPanel.RelativePosition = new Vector2(splitPosition, 0);
			rightPanel.SendMessage("PerformLayout", SendMessageOptions.DontRequireReceiver);
			panelHeight = Mathf.Max(panelHeight, rightPanel.Size.y);
		}
		panel.Size = new Vector2(panel.Size.x, panelHeight);
	}

	void PanelSlideInDidStart()
	{
	}
}
