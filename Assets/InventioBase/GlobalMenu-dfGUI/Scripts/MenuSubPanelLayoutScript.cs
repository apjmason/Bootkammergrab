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
using System; // Array.Sort
using System.Linq; // IList.ToArray()
using System.Collections;
using System.Collections.Generic;

public class MenuSubPanelLayoutScript : MonoBehaviour {
	public enum MenuSubPanelButtonAlignment {
		Left,
		Right
	};

	public MenuSubPanelButtonAlignment alignment = MenuSubPanelButtonAlignment.Left;
	public float borderPadding = 0f;
	public float itemSpacing = 0f;

	private dfPanel panel;

	void Start () {
		panel = GetComponent<dfPanel>();
	}

	public void PerformLayout()
	{
		Vector2 currentPosition = Vector2.zero;

		bool firstInRow = true;
		float maxHeight = 0f;

		dfControl[] controls = panel.Controls.ToArray();
		Array.Sort(controls, delegate(dfControl x, dfControl y) {
			MenuPanelButton buttonX = (MenuPanelButton)x;
			MenuPanelButton buttonY = (MenuPanelButton)y;
			return buttonX.panelSortOrder-buttonY.panelSortOrder;
		});
		for(int idx = 0; idx < controls.Length; ++idx) {
			dfControl control = controls[idx];
			if (firstInRow) {
				if (alignment == MenuSubPanelButtonAlignment.Left) {
					currentPosition = new Vector2(0f + borderPadding, currentPosition.y + maxHeight);
				}
				else {
					currentPosition = new Vector2(panel.Size.x - borderPadding, currentPosition.y + maxHeight);
				}
				maxHeight = 0f;
				firstInRow = false;
			}

			if (alignment == MenuSubPanelButtonAlignment.Left) {
			    if ((currentPosition.x + control.Size.x) > (panel.Size.x - borderPadding)) {
					// Restart new row
					idx -= 1;
					firstInRow = true;
					continue;
				}
			}
			else {
				currentPosition.x -= control.Size.x;// - itemSpacing);
				if (currentPosition.x < (0 + borderPadding)) {
					// Restart new row
					idx -= 1;
					firstInRow = true;
					continue;
				}
			}

			control.RelativePosition = currentPosition;
			maxHeight = Mathf.Max(maxHeight, control.Size.y);
			panel.Size = new Vector2(panel.Size.x, currentPosition.y + maxHeight);

			if (alignment == MenuSubPanelButtonAlignment.Left) {
				currentPosition.x += control.Size.x + itemSpacing;
			}
			else {
				currentPosition.x -= itemSpacing;
			}
		}
	}
}
