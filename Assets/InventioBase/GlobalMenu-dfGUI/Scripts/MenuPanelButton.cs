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
using System;
using System.Collections;
using System.Collections.Generic;

public class MenuPanelButton : dfButton
{
	public int panelSortOrder = 0;

	private GlobalMenuButtonHandler buttonHandler = null;
	private GlobalMenuPopupHandler popupHandler = null;

	public void SetupMenuPanelButton(string title, GlobalMenuButtonHandler handler, int sortOrder, string backgroundImage)
	{
		this.panelSortOrder = sortOrder;
		this.name = "Button_" + title;
		this.Text = title;
		this.Anchor = dfAnchorStyle.None;
		this.TextColor = new Color32(255, 255, 255, 255);
		this.DisabledTextColor = new Color32(127, 127, 127, 255);
		this.BackgroundSprite = backgroundImage;
		this.FocusSprite = backgroundImage;
		this.HoverSprite = backgroundImage;
		this.PressedSprite = backgroundImage;

		this.buttonHandler = handler;
	}

	public void SetupMenuPanelPopupButton(string title, GlobalMenuPopupHandler handler, int sortOrder, string backgroundImage)
	{
		this.panelSortOrder = sortOrder;
		this.name = "Button_" + title;
		this.Text = title;
		this.Anchor = dfAnchorStyle.None;
		this.TextColor = new Color32(255, 255, 255, 255);
		this.DisabledTextColor = new Color32(127, 127, 127, 255);
		this.BackgroundSprite = backgroundImage;
		this.FocusSprite = backgroundImage;
		this.HoverSprite = backgroundImage;
		this.PressedSprite = backgroundImage;
		
		this.popupHandler = handler;
	}

	public void SetupSizes(Vector2 buttonSize, float textScale)
	{
		this.Size = buttonSize;
		this.TextScale = textScale;
	}

	public void PanelButtonClicked()
	{
		if (buttonHandler != null) {
			buttonHandler(this.GetInstanceID());
		}
	}

	public void PanelButtonPressed()
	{
		if (popupHandler != null) {
			popupHandler(this.GetInstanceID(), this.GetScreenRect());
		}
	}
}
