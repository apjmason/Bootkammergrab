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

public class DrawerButtonScript : MonoBehaviour {

	public GameObject menuPanelObject;
	public dfButton backgroundButton = null;

	private dfTweenVector3 showTween;
	private dfTweenVector3 hideTween;

	private MenuPanelSlideScript menuPanel;

	private bool isShowing = true;
	private bool isHidden = false;

	void Start () {
		dfTweenVector3[] tweens;

		dfButton button = GetComponent<dfButton>();
		if (button) {
			tweens = GetComponents<dfTweenVector3>();
			showTween = GetTweenInArray("SlideIn", tweens);
			hideTween = GetTweenInArray("SlideOut", tweens);
		}

		if (menuPanelObject) {
			menuPanel = menuPanelObject.GetComponent<MenuPanelSlideScript>();
		}

		if (backgroundButton) {
			backgroundButton.Click += new MouseEventHandler(backgroundClicked);
			backgroundButton.Disable();
		}
	}

	public void HideDrawerButton() {
		dfButton button = GetComponent<dfButton>();
		if (button) {
			button.IsVisible = false;
		}
	}

	public void ShowDrawerButton() {
		dfButton button = GetComponent<dfButton>();
		if (button) {
			button.IsVisible = true;
		}
	}

	public void ResetDrawerButton() {
		if (!isHidden) {
			return;
		}

		if (showTween) {
			showTween.Play();
		}

		if (menuPanel) {
			menuPanel.PanelSlideOut();
		}
	}

	public void ActivateDrawerButton() {
		if (!isShowing) {
			return;
		}
		if (hideTween) {
			hideTween.Play();
		}
		if (menuPanel) {
			menuPanel.PanelSlideIn();
		}
	}

	public void backgroundClicked( dfControl control, dfMouseEventArgs mouseEvent )
	{
		GlobalMenuProxy.SharedMenu().HideMenu();
	}

	private dfTweenVector3 GetTweenInArray(string tweenName, dfTweenVector3[] tweenArray)
	{
		for (int idx = 0; idx < tweenArray.Length; ++idx) {
			dfTweenVector3 tween = tweenArray[idx];
			if (tween.TweenName == tweenName) {
				return tween;
			}
		}
		return null;
	}

	// UI Callbacks
	public void OnClick( dfControl control, dfMouseEventArgs mouseEvent )
	{
		GlobalMenuProxy.SharedMenu().ShowMenu();
	}

	// Tween callbacks
	public void TweenStarted(dfTweenVector3 tween)
	{
		isShowing = false;
		isHidden = false;
	}

	public void TweenCompleted(dfTweenVector3 tween)
	{
		if (tween == showTween) {
			isShowing = true;
			backgroundButton.Disable();
		}
		else if (tween == hideTween) {
			isHidden = true;
			backgroundButton.Enable();
		}
	}
}
