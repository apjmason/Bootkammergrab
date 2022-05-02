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

public delegate void GlobalMenuButtonHandler( int buttonId );
public delegate void GlobalMenuPopupHandler( int buttonId, Rect rect );

public interface IGlobalMenuControllerInterface {
	void ShowMenu(); // Slide drawer out, menu in
	void HideMenu(); // Slide drawer in, menu out

	void PushHideMenu(); // Will hide menu completely
	void PopHideMenu(); // Will show menu if as many pops as pushes

	int AddLeftButton(string title, GlobalMenuButtonHandler handler, int sortOrder = 999);
	int AddLeftPopupButton(string title, GlobalMenuPopupHandler handler, int sortOrder = 999);
	int AddRightButton(string title, GlobalMenuButtonHandler handler, int sortOrder = 999);
	int AddRightPopupButton(string title, GlobalMenuPopupHandler handler, int sortOrder = 999);

	void RemoveButton(int id);
	void EnableButton(int id, bool enableFlag);
}

public class GlobalMenuProxy {
	private static IGlobalMenuControllerInterface sharedMenuInstance = null;

	public static IGlobalMenuControllerInterface SharedMenu()
	{
		return sharedMenuInstance;
	}

	public static void SetSharedMenu(IGlobalMenuControllerInterface menu) 
	{
		sharedMenuInstance = menu;
	}
}