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

public class MenuPanelController : MonoBehaviour, IGlobalMenuControllerInterface, IInventioControllerObserverInterface, ITouchConsumerInterface 
{
	public DrawerButtonScript drawer = null;

	public Vector2 buttonSize = new Vector2(160, 50);
	public float buttonTextScale = 0.8f;

	public string backgroundImage = "MenuButtonBG";

	private dfPanel panel;
	private dfPanel leftPanel;
	private dfPanel rightPanel;

	private dfGUIManager guiManager = null;

	private Hashtable buttonTable = new Hashtable();

	private bool holdMenu = false; // Used by popup to hold the menu until finished

	private float buttonScale = -1f;
	private float ButtonScale {
		get {
			/*
			 * Set ButtonScale depending on ScreenSize
			 * We want to fit at least six buttons in a row
			 * NB! Screen height is fixed to device portrait, hence use height as width in landscape
			 */
			if (buttonScale < 0) {
				buttonScale = 1f; // Default
				Vector2 screenSize = guiManager.GetScreenSize();
				if ((screenSize.x / buttonSize.x) < 6f) {
					// Ideal width fits six buttons and half button width space between left and right
					float idealWidth = buttonSize.x * 6.5f;
					buttonScale = screenSize.x / idealWidth;
				}
			}
			return buttonScale;
		}
	}

	void Start () {
		panel = GetComponent<dfPanel>();
		leftPanel = panel.Find<dfPanel>("LeftPanel");
		rightPanel = panel.Find<dfPanel>("RightPanel");
		
		if (panel == null) {
			Debug.LogError("MenuPanelController missing panel!");
			gameObject.SetActive(false);
			return;
		}
		if (leftPanel == null) {
			Debug.LogError("MenuPanelController missing left panel!");
		}
		if (rightPanel == null) {
			Debug.LogError("MenuPanelController missing right panel!");
		}

		guiManager = panel.GetManager();

		// Set maximum width to match an aspect ratio of 0.5 (keep away from corners of iPhone X)
		Vector2 screenSize = guiManager.GetScreenSize();
		panel.MaximumSize = new Vector2 (screenSize.y / 0.5f, 0f);

		GlobalMenuProxy.SetSharedMenu(this);

		InventioController.SharedController ().AddObserver (this);
	}
	
	public void buttonClicked( dfControl control, dfMouseEventArgs mouseEvent )
	{
		if (!holdMenu && control.GetType() == typeof(MenuPanelButton)) {
			MenuPanelButton button = (MenuPanelButton)control;
			button.PanelButtonClicked();
		}
	}

	public void buttonPressed( dfControl control, dfMouseEventArgs mouseEvent )
	{
		if (!holdMenu && control.GetType() == typeof(MenuPanelButton)) {
			holdMenu = true;
			CancelInvoke("HideMenu");
			MenuPanelButton button = (MenuPanelButton)control;
			button.PanelButtonPressed();
		}
	}

	private int AddButtonToPanel(string title, GlobalMenuButtonHandler handler, int sortOrder, dfPanel target)
	{
		HideMenu();
		int id = -1;
		if ((target != null) && (handler != null)) {
			MenuPanelButton button = target.AddControl<MenuPanelButton>();
			button.SetupMenuPanelButton(title, handler, sortOrder, backgroundImage);
			button.SetupSizes(buttonSize * this.ButtonScale, buttonTextScale * this.ButtonScale);

			id = button.GetInstanceID();
			buttonTable.Add(id, button);
			button.Click += new MouseEventHandler(buttonClicked);
		}
		return id;
	}

	private int AddPopupButtonToPanel(string title, GlobalMenuPopupHandler handler, int sortOrder, dfPanel target)
	{
		HideMenu();
		int id = -1;
		if ((target != null) && (handler != null)) {
			MenuPanelButton button = target.AddControl<MenuPanelButton>();
			button.SetupMenuPanelPopupButton(title, handler, sortOrder, backgroundImage);
			button.SetupSizes(buttonSize * this.ButtonScale, buttonTextScale * this.ButtonScale);
			
			id = button.GetInstanceID();
			buttonTable.Add(id, button);
			button.MouseDown += new MouseEventHandler(buttonPressed);
		}
		return id;
	}

	// UI Callbacks
	public void OnClick( dfControl control, dfMouseEventArgs mouseEvent )
	{
		if (!holdMenu) {
			HideMenu ();
		}
	}

	// GlobalMenuControllerInterface
	public void ShowMenu()
	{
		if (drawer) {
			drawer.ActivateDrawerButton();
			
			Invoke("HideMenu", 5f);
		}
	}

	public void HideMenu()
	{
		holdMenu = false;
		if (drawer) {
			drawer.ResetDrawerButton();
		}

		CancelInvoke("HideMenu");
	}

	private int pushHideCount = 0;
	public void PushHideMenu()
	{
		pushHideCount += 1;
		panel.Hide();
		drawer.HideDrawerButton();
	}

	public void PopHideMenu()
	{
		pushHideCount -= 1;
		if (pushHideCount < 0) {
			Debug.LogWarning("MenuPanelController:PullHideMenu: Too many PullHideMenu!");
			pushHideCount = 0;
		}
		else if (pushHideCount == 0) {
			panel.Show();
			drawer.ShowDrawerButton();
		}
	}

	public int AddLeftButton(string title, GlobalMenuButtonHandler handler, int sortOrder = 999)
	{
		return AddButtonToPanel(title, handler, sortOrder, leftPanel);
	}

	public int AddLeftPopupButton(string title, GlobalMenuPopupHandler handler, int sortOrder = 999)
	{
		return AddPopupButtonToPanel(title, handler, sortOrder, leftPanel);
	}

	public int AddRightButton(string title, GlobalMenuButtonHandler handler, int sortOrder = 999)
	{
		return AddButtonToPanel(title, handler, sortOrder, rightPanel);
	}

	public int AddRightPopupButton(string title, GlobalMenuPopupHandler handler, int sortOrder = 999)
	{
		return AddPopupButtonToPanel(title, handler, sortOrder, rightPanel);
	}

	public void RemoveButton(int id)
	{
		if (buttonTable.ContainsKey(id)) {
			dfControl button = buttonTable[id] as dfControl;
			buttonTable.Remove(id);
			if (button) {
				GameObject.Destroy(button.gameObject);
			}
		}
	}

	public void EnableButton(int id, bool enableFlag)
	{
		if (buttonTable.ContainsKey(id)) {
			dfControl button = buttonTable[id] as dfControl;
			if (button) {
				if (enableFlag) {
					button.Enable();
				}
				else {
					button.Disable();
				}
			}
		}
	}


	// IInventioControllerObserverInterface
	public void InventioSetupComplete()
	{
		TouchController.SharedTouchController ().AddTouchConsumer (this);
	}


	// ITouchConsumerInterface
	public bool ConsumesTouchEventAtPosition(float xPos, float yPos)
	{
		return (guiManager.HitTest (new Vector2 (xPos, yPos)) != null);
	}
}
