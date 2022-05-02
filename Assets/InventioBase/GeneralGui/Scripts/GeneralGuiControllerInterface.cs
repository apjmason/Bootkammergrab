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

/*
 * IGeneralGuiControllerInterface
 * 
 * Before use, a "client" has to request ownership of the GUI, in order to not collide with other instances.
 * When requesting ownership, an instance of IGeneralGuiDelegateInterface must be provided, the controller will
 * call this delegate upon GUI events.
 * 
 */
using UnityEngine;

public interface IGeneralGuiDelegateInterface {
	bool ReceiveOwnership();
	int DropOwnershipRequest(); // 0=Dropped, 1=Waived, 2=Blocked

	void ActivateButtonTopLeft();
	void ActivateButtonTopRight();

	void ChangeValueSliderLeft(float value); // value=[0,100]
	void ChangeValueSliderRight(float value); // value=[0,100]

	void ActivateProgressButtonLeft();
	void ActivateProgressButtonRight();
}

public interface IGeneralGuiControllerInterface {
	bool RequestOwnership(IGeneralGuiDelegateInterface aDelegate);
	void DropOwnership();

	// Simple buttons, e.g "Exit", "Leave"
	void EnableButtonTopLeft(string title); // title=NULL => Hide
	void EnableButtonTopRight(string title); // title=NULL => Hide

	// Sliders, e.g "Zoom", "Altitude". Min value 0, max value 100 - client has to adapt scale
	void EnableSliderLeft(string title, float initialValue = 0f); // title=NULL => Hide, initialValue=[0,100]
	void EnableSliderRight(string title, float initialValue = 0f); // title=NULL => Hide, initialValue=[0,100]

	// Progress bar, e.g playing audio, right button mandatory, left button optional
	void EnableProgressBar(string rightButtonTitle, string leftButtonTitle = null); // rightButtonTitle=null => Hide
	void UpdateProgressBar(float value); // value=[0,100]

	void PushHideFlashPanel(); // Will hide flash panel completely
	void PopHideFlashPanel(); // Will show flash panel if as many pulls as pushes
	int AddFlashMessage(string message);
	bool RemoveFlashMessage(int id);

	void ShowImage(Texture2D texture, Color32 backgroundColor);
	void HideImage(bool dropOwnership = true);

	void FadeToColor(Color fadeColor);
}

public class GeneralGuiProxy {
	private static IGeneralGuiControllerInterface sharedGuiInstance = null;
	
	public static IGeneralGuiControllerInterface  SharedGui()
	{
		return sharedGuiInstance;
	}
	
	public static void SetSharedGui(IGeneralGuiControllerInterface gui) 
	{
		sharedGuiInstance = gui;
	}
}