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

public class GeneralGuiFlashPanelController : MonoBehaviour {

	private dfPanel panel = null;
	private Hashtable flashTable = new Hashtable();

	void Start () {
		panel = GetComponent<dfPanel>();
		if (!panel) {
			Debug.LogError("GeneralGuiFlashPanelController missing panel!!!");
			return;
		}
		dfTweenColor32 transparencyTween = gameObject.AddComponent<dfTweenColor32>();
		if (transparencyTween != null) {
			transparencyTween.StartValue = new Color32(255, 255, 255, 255);
			transparencyTween.EndValue = new Color32(255, 255, 255, 127);
			transparencyTween.LoopType = dfTweenLoopType.PingPong;
			transparencyTween.Function = dfEasingType.CubicEaseInOut;
			transparencyTween.Length = 0.7f;
			transparencyTween.Target = new dfComponentMemberInfo()
			{
				Component = panel,
				MemberName = "Color"
			};
			transparencyTween.Play();
		}
	}

	public void Show()
	{
		if (panel != null) {
			panel.Show();
		}
		else {
			Debug.LogError("GeneralGuiFlashPanelController:Show: missing panel!!!");
		}
	}

	public void Hide()
	{
		if (panel != null) {
			panel.Hide();
		}
		else {
			Debug.LogError("GeneralGuiFlashPanelController:Hide: missing panel!!!");
		}
	}

	private int maxNumMessages = 5;
	public int AddMessage(string message)
	{
		int id = 0;
		if ((message != null) && (panel != null)) {
			if (flashTable.Count < 5) {
				dfLabel label = panel.AddControl<dfLabel>();
				label.name = "Flash_" + message;
				label.Text = message;
				label.Anchor = dfAnchorStyle.Top | dfAnchorStyle.CenterHorizontal;
				label.Color = new Color32(255, 0, 0, 255);
				label.DisabledColor = new Color32(127, 0, 0, 255);
				label.Size = new Vector2(panel.Size.x, panel.Size.y / maxNumMessages);
				label.TextScale = 0.8f;
				label.TextAlignment = TextAlignment.Center;
				label.VerticalAlignment = dfVerticalAlignment.Middle;
				label.IsInteractive = false;

				id = label.GetInstanceID();
				flashTable.Add(id, label);
			}
			else {
				Debug.LogError("GeneralGuiFlashPanelController:AddMessage: Too many messages!!!");
			}
		}
		else if (message == null) {
			Debug.LogError("GeneralGuiFlashPanelController:AddMessage: Can't add NULL message!!!");
		}
		else {
			Debug.LogError("GeneralGuiFlashPanelController:AddMessage: missing panel!!!");
		}
		return id;
	}

	public bool RemoveMessage(int id)
	{
		bool success = false;
		if (flashTable.ContainsKey(id)) {
			dfLabel label = flashTable[id] as dfLabel;
			flashTable.Remove(id);
			if (label) {
				GameObject.Destroy(label.gameObject);
				success = true;
			}
		}
		else {
			Debug.LogError("GeneralGuiFlashPanelController:RemoveMessage: Can't find message!!!");
		}
		return success;
	}
}
