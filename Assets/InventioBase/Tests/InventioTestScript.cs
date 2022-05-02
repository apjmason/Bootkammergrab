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
using System.Collections.Generic;

using InventioBase;

public class InventioTestScript : MonoBehaviour, IInventioControllerObserverInterface {

	public Texture2D testTexture = null;

	void Start () {
		InventioController.SharedController().AddObserver(this);
	}


	private bool viewControllerShowed = false;
	private float startTime = 0f;
	void Update () {
		if ( !viewControllerShowed && (Time.unscaledTime - startTime > 15f)) {
			InventioViewManager.ShowViewController("ViewManagerTestViewController");
			viewControllerShowed = true;
		}
	}

	public void GlobalMenuButtonClicked( int buttonId )
	{
		Debug.Log("==>GlobalMenuClicked InventioTestScript Button");
		//GlobalMenuProxy.SharedMenu().RemoveButton(buttonId);
		GlobalMenuProxy.SharedMenu().EnableButton(buttonId, false);
	}

	public void ActivateGUITest( int buttonId )
	{
		GeneralGuiProxy.SharedGui().RequestOwnership(new InventioGUITestScript());
	}

	public void ActivateTextureTest( int buttonId )
	{
		if (testTexture) {
			GeneralGuiProxy.SharedGui ().RequestOwnership (new InventioTextureTestScript (testTexture));
		} else {
			Debug.LogWarning ("InventioTestScript:ActivateTextureTest: Missing testTexture!");
		}
	}

	public void ActivatePopupTest( int buttonId, Rect rect )
	{
		Debug.Log ("ActivatePopupTest with rect " + rect);
	}

	private int alert1, alert2;
	private void ShowAlert1()
	{
		alert1 = GeneralGuiProxy.SharedGui().AddFlashMessage("Alert 1");
		Invoke("HideAlert1", 15);
	}
	private void HideAlert1()
	{
		GeneralGuiProxy.SharedGui().RemoveFlashMessage(alert1);
	}

	private void ShowAlert2()
	{
		alert2 = GeneralGuiProxy.SharedGui().AddFlashMessage("Alert 2");
		Invoke("HideAlert2", 15);
	}
	private void HideAlert2()
	{
		GeneralGuiProxy.SharedGui().RemoveFlashMessage(alert2);
	}
	private void HideFlash()
	{
		GeneralGuiProxy.SharedGui().PushHideFlashPanel();
		Invoke("ShowFlash", 5);
	}
	private void ShowFlash()
	{
		GeneralGuiProxy.SharedGui().PopHideFlashPanel();
	}

	// IInventioControllerObserverInterface
	public void InventioSetupComplete()
	{
		startTime = Time.unscaledTime;
		
		GlobalMenuProxy.SharedMenu().AddRightButton("R_Ett", new GlobalMenuButtonHandler(GlobalMenuButtonClicked), 10);
		GlobalMenuProxy.SharedMenu().AddRightButton("R_Två", new GlobalMenuButtonHandler(GlobalMenuButtonClicked), 20);
		GlobalMenuProxy.SharedMenu().AddRightButton("R_Tre", new GlobalMenuButtonHandler(GlobalMenuButtonClicked), 30);

		GlobalMenuProxy.SharedMenu().AddLeftButton("L_Ett", new GlobalMenuButtonHandler(GlobalMenuButtonClicked), 10);
		GlobalMenuProxy.SharedMenu().AddLeftButton("L_Två", new GlobalMenuButtonHandler(GlobalMenuButtonClicked), 20);
		GlobalMenuProxy.SharedMenu().AddLeftButton("L_Tre", new GlobalMenuButtonHandler(GlobalMenuButtonClicked), 30);

		GlobalMenuProxy.SharedMenu().AddLeftButton("GUItest", new GlobalMenuButtonHandler(ActivateGUITest), 0);
		GlobalMenuProxy.SharedMenu().AddLeftButton("TextureTest", new GlobalMenuButtonHandler(ActivateTextureTest), 0);

		GlobalMenuProxy.SharedMenu().AddLeftPopupButton("Popup", new GlobalMenuPopupHandler(ActivatePopupTest));
		Invoke("ShowAlert1", 1);
		Invoke("ShowAlert2", 5);
		Invoke("HideFlash", 10);

		// We're not interested in the controller any more
		InventioController.SharedController().RemoveObserver(this);
	}
}

public class InventioGUITestScript : IGeneralGuiDelegateInterface
{
	private int depth = 0;
	private float leftSliderValue = 25f;
	private float rightSliderValue = 75f;
	private float progressValue = 0f;

	public InventioGUITestScript(int aDepth = 0)
	{
		depth = aDepth;
	}

	public bool ReceiveOwnership()
	{
		IGeneralGuiControllerInterface gui = GeneralGuiProxy.SharedGui();
		gui.EnableButtonTopLeft(LocalizationControllerProxy.Localize("Base.Close"));
		gui.EnableButtonTopRight(LocalizationControllerProxy.Localize("Test.Depth") + " " + depth);
		gui.EnableSliderLeft(LocalizationControllerProxy.Localize("Test.Left"), leftSliderValue);
		gui.EnableSliderRight(LocalizationControllerProxy.Localize("Test.Right"), rightSliderValue);
		gui.EnableProgressBar(LocalizationControllerProxy.Localize("Base.Close"), LocalizationControllerProxy.Localize("Test.Increase"));

		// TODO: Enable other elements...

		gui.UpdateProgressBar(progressValue);

		GlobalMenuProxy.SharedMenu().PushHideMenu();
		return true;
	}

	public int DropOwnershipRequest() // 0=Dropped, 1=Waived, 2=Blocked
	{
		GlobalMenuProxy.SharedMenu().PopHideMenu();

		return 1; // Waived
	}
	
	public void ActivateButtonTopLeft()
	{
		Debug.Log("Left Button");
		TrackerNGPlugin.TrackAppSpecific ("testButtonActivated", new Dictionary<string, string> () { {"position", "left"} });
		GeneralGuiProxy.SharedGui().DropOwnership();
		GlobalMenuProxy.SharedMenu().PopHideMenu();
	}

	public void ActivateButtonTopRight()
	{
		Debug.Log("Right Button");
		TrackerNGPlugin.TrackAppSpecific ("testButtonActivated", new Dictionary<string, string> () { {"position", "right"} });
		GeneralGuiProxy.SharedGui().RequestOwnership(new InventioGUITestScript(depth + 1));
	}
	
	public void ChangeValueSliderLeft(float value)
	{
		Debug.Log("Left Slider Value: " + value);
		leftSliderValue = value;
	}

	public void ChangeValueSliderRight(float value)
	{
		Debug.Log("Right Slider Value: " + value);
		rightSliderValue = value;
	}

	public void ActivateProgressButtonLeft()
	{
		progressValue += 10f;
		if (progressValue > 100f) {
			progressValue = 0f;
		}
		GeneralGuiProxy.SharedGui().UpdateProgressBar(progressValue);
	}

	public void ActivateProgressButtonRight()
	{
		GeneralGuiProxy.SharedGui().EnableProgressBar(null);
	}

}

public class InventioTextureTestScript : IGeneralGuiDelegateInterface
{
	private Texture2D texture = null;

	public InventioTextureTestScript(Texture2D aTexture)
	{
		texture = aTexture;
	}
	public bool ReceiveOwnership()
	{
		if (texture == null) {
			Debug.LogWarning ("InventioTextureTestScript: Missing texture!");
			return false;
		}
		IGeneralGuiControllerInterface gui = GeneralGuiProxy.SharedGui();
		gui.ShowImage(texture, new Color32(255, 0, 0, 255));
		gui.EnableButtonTopLeft(LocalizationControllerProxy.Localize("Base.Close"));
		gui.EnableButtonTopRight (LocalizationControllerProxy.Localize("Base.Share"));

		gui.PushHideFlashPanel();
		GlobalMenuProxy.SharedMenu().PushHideMenu();
		return true;
	}
	public int DropOwnershipRequest() // 0=Dropped, 1=Waived, 2=Blocked
	{
		GeneralGuiProxy.SharedGui().PopHideFlashPanel();
		GlobalMenuProxy.SharedMenu().PopHideMenu();
		return 0;
	}

	public void ActivateButtonTopLeft()
	{
		GeneralGuiProxy.SharedGui().HideImage();
		GeneralGuiProxy.SharedGui().EnableButtonTopLeft(null);
		GeneralGuiProxy.SharedGui().PopHideFlashPanel();
		GlobalMenuProxy.SharedMenu().PopHideMenu();
	}
	public void ActivateButtonTopRight()
	{
		InventioSocialManager.ShareImageData (texture.EncodeToPNG (), "Testing image share.", "Look here!");
		GeneralGuiProxy.SharedGui().HideImage();
		GeneralGuiProxy.SharedGui().EnableButtonTopLeft(null);
		GeneralGuiProxy.SharedGui().PopHideFlashPanel();
		GlobalMenuProxy.SharedMenu().PopHideMenu();
	}

	public void ChangeValueSliderLeft(float value) // value=[0,100]
	{
	}
	public void ChangeValueSliderRight(float value) // value=[0,100]
	{
	}

	public void ActivateProgressButtonLeft()
	{
	}
	public void ActivateProgressButtonRight()
	{
	}

}