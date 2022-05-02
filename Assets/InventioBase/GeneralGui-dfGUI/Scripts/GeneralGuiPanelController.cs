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

public class GeneralGuiPanelController : MonoBehaviour, IGeneralGuiControllerInterface, IInventioControllerObserverInterface, ITouchConsumerInterface
{

	public dfButton topLeftButton = null;
	public dfButton topRightButton = null;

	public dfSlider leftSlider = null;
	public dfLabel leftSliderLabel = null;
	public dfSlider rightSlider = null;
	public dfLabel rightSliderLabel = null;

	public dfPanel progressPanel = null;
	public dfProgressBar progressBar = null;
	public dfButton leftProgressButton = null;
	public dfButton rightProgressButton = null;

	public GeneralGuiFlashPanelController flashPanel = null;

	public dfPanel imagePanel = null;

	private dfPanel panel;

	private IGeneralGuiDelegateInterface guiDelegate = null;
	private dfList<IGeneralGuiDelegateInterface> waivedDelegates = new dfList<IGeneralGuiDelegateInterface>();

	private dfGUIManager guiManager = null;

	// Use this for initialization
	void Start () {
		panel = GetComponent<dfPanel>();

		if (topLeftButton == null) {
			Debug.LogError("GeneralGuiPanelController missing topLeftButton");
		}
		if (topRightButton == null) {
			Debug.LogError("GeneralGuiPanelController missing topRightButton");
		}
		if (leftSlider == null) {
			Debug.LogError("GeneralGuiPanelController missing leftSlider");
		}
		if (leftSliderLabel == null) {
			Debug.LogError("GeneralGuiPanelController missing leftSliderLabel");
		}
		if (rightSlider == null) {
			Debug.LogError("GeneralGuiPanelController missing rightSlider");
		}
		if (rightSliderLabel == null) {
			Debug.LogError("GeneralGuiPanelController missing rightSliderLabel");
		}

		if ((progressPanel == null) ||
		    (leftProgressButton == null) ||
		    (rightProgressButton == null)) {
			Debug.LogError("GeneralGuiPanelController missing progress item(s)");
		}

		if (flashPanel == null) {
			Debug.LogError("GeneralGuiPanelController missing flashPanel");
		}

		if (imagePanel == null) {
			Debug.LogError("GeneralGuiPanelController missing imagePanel");
		}
		if (panel == null) {
			Debug.LogError("GeneralGuiPanelController missing panel");
		}
		GeneralGuiProxy.SetSharedGui(this);

		topLeftButton.Click += new MouseEventHandler(ButtonClick);
		topRightButton.Click += new MouseEventHandler(ButtonClick);
		leftSlider.ValueChanged += new PropertyChangedEventHandler<float>(SliderValueChanged);
		rightSlider.ValueChanged += new PropertyChangedEventHandler<float>(SliderValueChanged);
		leftProgressButton.Click += new MouseEventHandler(ButtonClick);
		rightProgressButton.Click += new MouseEventHandler(ButtonClick);

		if (Application.platform != RuntimePlatform.OSXEditor) {
			float scale = (float)Screen.height / 640f;
			topLeftButton.Size = scale * topLeftButton.Size;
			topRightButton.Size = scale * topRightButton.Size;
		}
		guiManager = panel.GetManager();

		this.ClearAndHidePanel();

		InventioController.SharedController ().AddObserver (this);
	}
	
	private void ClearAndHidePanel()
	{
		topLeftButton.Hide();
		topRightButton.Hide();
		leftSlider.Hide();
		rightSlider.Hide();
		progressPanel.Hide();
		leftProgressButton.Hide();
		imagePanel.Hide();
		// TODO: Hide more controls

		panel.Hide();
	}

	private bool ResumeWaivedDelegate()
	{
		this.ClearAndHidePanel();
		guiDelegate = null;
		while (waivedDelegates.Count > 0) {
			guiDelegate = waivedDelegates.Last();
			waivedDelegates.Remove(guiDelegate);
			
			if (guiDelegate.ReceiveOwnership()) {
				panel.Show();
				break;
			}
			else {
				Debug.LogWarning("GeneralGuiPanelController:DropOwnership: Waived delegate refused ownership!");
			}
			guiDelegate = null;
		}

		return (guiDelegate != null);
	}

	private void EnableAutosizeButton(dfButton button, string title)
	{
		if (title != null) {
			button.AutoSize = true;
			button.Text = title;
			button.Show();
			if (button.Size.x == button.MinimumSize.x) {
				button.AutoSize = false;
				button.TextAlignment = TextAlignment.Center;
			}
		}
		else {
			button.Hide();
		}

	}

	// UI Callbacks
	public void ButtonClick( dfControl control, dfMouseEventArgs mouseEvent )
	{
		if (guiDelegate == null) {
			Debug.LogWarning("GeneralGuiPanelController::ButtonClick: Have no delegate to notify");
			return;
		}

		if (control == topLeftButton) {
			guiDelegate.ActivateButtonTopLeft();
		}
		else if (control == topRightButton) {
			guiDelegate.ActivateButtonTopRight();
		}
		else if (control == leftProgressButton) {
			guiDelegate.ActivateProgressButtonLeft();
		}
		else if (control == rightProgressButton) {
			guiDelegate.ActivateProgressButtonRight();
		}
		else {
			Debug.LogWarning("GeneralGuiPanelController::ButtonClick: Unknown dfControl: " + control.name);
		}
	}

	public void SliderValueChanged(dfControl control, float value)
	{
		if (guiDelegate == null) {
			Debug.LogWarning("GeneralGuiPanelController::SliderValueChanged: Have no delegate to notify");
			return;
		}

		if (control == leftSlider) {
			guiDelegate.ChangeValueSliderLeft(value);
		}
		else if (control == rightSlider) {
			guiDelegate.ChangeValueSliderRight(value);
		}
		else {
			Debug.LogWarning("GeneralGuiPanelController::SliderValueChanged: Unknown dfControl: " + control.name);
		}
	}

	// GeneralGuiControllerInterface
	public bool RequestOwnership(IGeneralGuiDelegateInterface aDelegate)
	{
		if (aDelegate == null) {
			Debug.LogWarning("GeneralGuiPanelController:RequestOwnership: Provided delegate is null!");
			return false;
		}

		if (guiDelegate != null) {
			int dropResponse = guiDelegate.DropOwnershipRequest();
			switch(dropResponse) 
			{
			case 0: // Dropped
				// NOP
				break;
			case 1: // Waived
				waivedDelegates.Enqueue(guiDelegate);
				break;
			case 2: // Blocked
				Debug.LogWarning("GeneralGuiPanelController:RequestOwnership: DropOwnershipRequest received block response from delegate!");
				return false;
			default:
				Debug.LogError("GeneralGuiPanelController:RequestOwnership: DropOwnershipRequest received illegal response from delegate:" + dropResponse);
				break;
			}
		}

		this.ClearAndHidePanel();
		guiDelegate = aDelegate;
		if (aDelegate.ReceiveOwnership()) {
			guiDelegate = aDelegate;
			panel.Show();
			return true;
		}
		else {
			Debug.LogError("GeneralGuiPanelController:RequestOwnership: Requesting delegate refused ownership!");
			return (this.ResumeWaivedDelegate());
		}
	}

	public void DropOwnership()
	{
		this.ClearAndHidePanel();
		this.ResumeWaivedDelegate();
	}
	
	public void EnableButtonTopLeft(string title)
	{
		EnableAutosizeButton (topLeftButton, title);
	}

	public void EnableButtonTopRight(string title)
	{
		EnableAutosizeButton (topRightButton, title);
	}
	
	// Sliders, e.g "Zoom", "Altitude". Min value 0, max value 100 - client has to adapt scale
	public void EnableSliderLeft(string title, float initialValue = 0f)
	{
		if (title != null) {
			leftSliderLabel.Text = title;
			leftSlider.Value = initialValue;
			leftSlider.Show();
		}
		else {
			leftSlider.Hide();
		}
	}

	public void EnableSliderRight(string title, float initialValue = 0f)
	{
		if (title != null) {
			rightSliderLabel.Text = title;
			rightSlider.Value = initialValue;
			rightSlider.Show();
		}
		else {
			rightSlider.Hide();
		}
	}

	public void EnableProgressBar(string rightButtonTitle, string leftButtonTitle = null) {
		if (rightButtonTitle != null) {
			EnableAutosizeButton (rightProgressButton, rightButtonTitle);
			EnableAutosizeButton (leftProgressButton, leftButtonTitle);
			progressBar.Value = 0f;
			progressPanel.Show();
		}
		else {
			progressPanel.Hide();
		}
	}

	public void UpdateProgressBar(float value)
	{
		progressBar.Value = value;
	}

	private int pushHideFlashCount = 0;
	public void PushHideFlashPanel()
	{
		if (flashPanel) {
			pushHideFlashCount += 1;
			flashPanel.Hide();
		} else {
			Debug.LogWarning("GeneralGuiPanelController:PushHideFlashPanel: Missing flashPanel!");
		}
	}

	public void PopHideFlashPanel()
	{
		if (flashPanel) {
			pushHideFlashCount -= 1;
			if (pushHideFlashCount < 0) {
				Debug.LogWarning("GeneralGuiPanelController:PullHideFlashPanel: Too many PullHideFlashPanel!");
				pushHideFlashCount = 0;
			} else if (pushHideFlashCount == 0) {
				flashPanel.Show();
			}
		} else {
			Debug.LogWarning("GeneralGuiPanelController:PullHideFlashPanel: Missing flashPanel!");
		}
	}

	public int AddFlashMessage(string message)
	{
		if (flashPanel != null) {
			return flashPanel.AddMessage(message);
		}
		else {
			Debug.LogWarning("GeneralGuiPanelController:AddFlashMessage: Missing flashPanel!");
			return -1;
		}
	}

	public bool RemoveFlashMessage(int id)
	{
		if (flashPanel != null) {
			return flashPanel.RemoveMessage(id);
		}
		else {
			Debug.LogWarning("GeneralGuiPanelController:RemoveFlashMessage: Missing flashPanel!");
			return false;
		}
	}

	public void FadeToColor(Color fadeColor)
	{
		// TODO: FadeToColor
	}

	public void ShowImage(Texture2D texture, Color32 backgroundColor)
	{
		if (imagePanel != null) {
			dfTextureSprite imageSprite = imagePanel.Find<dfTextureSprite> ("ImageSprite");
			if (imageSprite == null) {
				Debug.LogWarning("GeneralGuiPanelController:ShowImage: Couldn't find ImageSprite!");
				return;
			}
			imageSprite.Texture = texture;

			float heightRatio = (float)texture.height / (float)imagePanel.Height;
			float widthRatio = (float)texture.width / (float)imagePanel.Width;
			if (heightRatio > widthRatio) {
				imageSprite.Height = imagePanel.Height;
				imageSprite.Width = imageSprite.Height * ((float)texture.width / (float)texture.height);
			} else {
				imageSprite.Width = imagePanel.Width;
				imageSprite.Height = imageSprite.Width * ((float)texture.height / (float)texture.width);
			}

			imagePanel.BackgroundColor = backgroundColor;

			dfTweenFloat opacityTween = imagePanel.GetComponent<dfTweenFloat>();
			if (opacityTween != null) {
				imagePanel.Opacity = 0f;
				opacityTween.StartValue = 0f;
				opacityTween.EndValue = 1f;
				opacityTween.Play ();
			} else {
				Debug.LogWarning("GeneralGuiPanelController:ShowImage: Missing opacity tween!");
				imagePanel.Opacity = 1f;
			}
			imagePanel.Show();
		} else {
			Debug.LogWarning("GeneralGuiPanelController:ShowImage: Missing imagePanel!");
		}
	}

	public void HideImage(bool dropOwnership = true)
	{
		if (imagePanel != null) {
			float delay = 0;
			dfTweenFloat opacityTween = imagePanel.GetComponent<dfTweenFloat> ();
			if (opacityTween != null) {
				opacityTween.StartValue = 1f;
				opacityTween.EndValue = 0f;
				opacityTween.Play ();
				delay = opacityTween.Length;
			} else {
				Debug.LogWarning ("GeneralGuiPanelController:HideImage: Missing opacity tween!");
			}
			if (dropOwnership) {
				Invoke ("DropOwnership", delay);
			} else {
				imagePanel.Invoke ("Hide", delay);
			}
		} else {
			Debug.LogWarning("GeneralGuiPanelController:HideImage: Missing imagePanel!");
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
