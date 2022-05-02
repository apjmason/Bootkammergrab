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

using InventioBase;

public interface IAudioCommentsNotifiable {
	bool HasDetail();
	void CommentDidFinish();
	void CommentDidFinishWithDetail();
}

public class AudioCommentsController : MonoBehaviour, IGeneralGuiDelegateInterface {
	
	private bool hasGUI = false;
	private IAudioCommentsNotifiable notifiable = null;

	private static AudioCommentsController _sharedInstance = null;
	public static AudioCommentsController SharedController()
	{
		return _sharedInstance;
	}

	void Start () {
		gameObject.AddComponent<AudioSource>();
		if (gameObject.GetComponent<AudioSource>() != null) {
			_sharedInstance = this;
			gameObject.GetComponent<AudioSource>().loop = false;
			gameObject.GetComponent<AudioSource>().ignoreListenerVolume = true;
		} else {
			Debug.LogError ("AudioCommentsController has no AudioSource!");
			gameObject.SetActive (false);
		}
	}
	
	void Update () {
		if (hasGUI) {
			if (gameObject.GetComponent<AudioSource>().isPlaying) {
				float progress = 100f * gameObject.GetComponent<AudioSource>().time / gameObject.GetComponent<AudioSource>().clip.length;
				GeneralGuiProxy.SharedGui ().UpdateProgressBar (progress);
			} else if ((notifiable != null) && notifiable.HasDetail ()) {
				GeneralGuiProxy.SharedGui ().UpdateProgressBar (100f);
			} else {
				if (notifiable != null) {
					notifiable.CommentDidFinish ();
				}
				DropGUI ();
			}
		} else if (gameObject.GetComponent<AudioSource>().clip != null) {
			if (!gameObject.GetComponent<AudioSource>().isPlaying) {
				if (notifiable != null) {
					notifiable.CommentDidFinish ();
				}
				StopPlaying ();
			}
		}
	}

	public void PlayComment(AudioClip comment, IAudioCommentsNotifiable notifyWhenFinished, bool showGUI = true)
	{
		StopPlaying ();

		gameObject.GetComponent<AudioSource>().clip = comment;
		notifiable = notifyWhenFinished;

		if (showGUI) {
			if (!GeneralGuiProxy.SharedGui ().RequestOwnership (this)) {
				Debug.LogWarning ("AudioCommentsController did not receive General GUI ownership!");
				gameObject.GetComponent<AudioSource>().clip = null;
				notifiable = null;
			}
		} else {
			gameObject.GetComponent<AudioSource>().Play ();
		}
	}

	public void StopPlaying()
	{
		if (hasGUI) {
			DropGUI ();
		}
		gameObject.GetComponent<AudioSource>().Stop ();
		gameObject.GetComponent<AudioSource>().clip = null;
	}

	private void DropGUI()
	{
		GeneralGuiProxy.SharedGui().DropOwnership ();
		hasGUI = false;
		notifiable = null;

		gameObject.GetComponent<AudioSource>().Stop ();
		gameObject.GetComponent<AudioSource>().clip = null;

		GlobalMenuProxy.SharedMenu ().PopHideMenu ();
	}


	/*
	 * IGeneralGuiDelegateInterface
	 */
	public bool ReceiveOwnership()
	{
		hasGUI = true;
		IGeneralGuiControllerInterface gui = GeneralGuiProxy.SharedGui ();
		string detailString = null;
		if ((notifiable != null) && notifiable.HasDetail ()) {
			detailString = LocalizationControllerProxy.Localize("Base.ReadMore");
		}
		gui.EnableProgressBar (LocalizationControllerProxy.Localize("Base.StopAudio"), detailString);
		gui.UpdateProgressBar (0f);
		gameObject.GetComponent<AudioSource>().Play ();

		GlobalMenuProxy.SharedMenu ().PushHideMenu ();
		return true;
	}
	public int DropOwnershipRequest() // 0=Dropped, 1=Waived, 2=Blocked
	{
		StopPlaying ();
		return 0;
	}

	public void ActivateButtonTopLeft()
	{
		// NOP
	}
	public void ActivateButtonTopRight()
	{
		// NOP
	}

	public void ChangeValueSliderLeft(float value) // value=[0,100]
	{
		// NOP
	}
	public void ChangeValueSliderRight(float value) // value=[0,100]
	{
		// NOP
	}

	public void ActivateProgressButtonLeft()
	{
		// Details...
		if (notifiable != null) {
			notifiable.CommentDidFinishWithDetail ();
		}
		StopPlaying ();
	}
	public void ActivateProgressButtonRight()
	{
		// Stop...
		if (notifiable != null) {
			notifiable.CommentDidFinish ();
		}
		StopPlaying ();
	}
}
