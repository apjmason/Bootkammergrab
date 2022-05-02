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

namespace InventioBase {
	public class ObjectMarkerScript : MonoBehaviour, IBalloonObserverInterface, IGeneralGuiDelegateInterface, IAudioCommentsNotifiable {

		public BalloonController activationBalloon = null;

		public AudioClip markerAudio = null;

		// Use this for initialization
		void Start () {
			bool success = true;

			if (this.markerAudio == null) {
				success = false;
				Debug.LogWarning("ObjectMarkerScript: Missing markerAudio on " + gameObject.name);
			}
			if (!success) {
				Debug.LogWarning("ObjectMarkerScript: Deactivating object " + gameObject.name);
				gameObject.SetActive(false);
				return;
			}

			gameObject.GetComponent<Renderer>().enabled = false;

			if (this.activationBalloon != null) {
				this.activationBalloon.AddObserver(this);
			}
		}
		
		public void ShowMarker()
		{
			if (GeneralGuiProxy.SharedGui ().RequestOwnership (this)) {
				GlobalMenuProxy.SharedMenu ().PushHideMenu ();
				TouchController.SharedTouchController ().SetBalloonsEnabled (false);

				Color markerColor = gameObject.GetComponent<Renderer>().sharedMaterial.color;
				markerColor.a = .1f;
				gameObject.GetComponent<Renderer>().sharedMaterial.color = markerColor;
				gameObject.GetComponent<Renderer>().enabled = true;

				LeanTween.alpha (gameObject, .2f, .6f).setEase (LeanTweenType.easeInOutCubic).setLoopPingPong ().setUseEstimatedTime(true);
			}
		}

		public void HideMarker()
		{
			if (markerAudio != null) {
				AudioCommentsController.SharedController ().StopPlaying ();
			}

			LeanTween.cancel (gameObject);
			gameObject.GetComponent<Renderer>().enabled = false;

			GlobalMenuProxy.SharedMenu().PopHideMenu ();
			TouchController.SharedTouchController().SetBalloonsEnabled (true);

			GeneralGuiProxy.SharedGui ().DropOwnership ();
		}


		// IBalloonObserverInterface
		public void BalloonActivated(BalloonController balloon)
		{
			ShowMarker ();
		}


		// IAudioCommentsNotifiable
		public bool HasDetail()
		{
			return false;
		}

		public void CommentDidFinish()
		{
			HideMarker ();
		}

		public void CommentDidFinishWithDetail()
		{
			// NOP
		}


		// IGeneralGuiDelegateInterface
		public bool ReceiveOwnership()
		{
			GeneralGuiProxy.SharedGui ().EnableButtonTopLeft(LocalizationControllerProxy.Localize("Base.ExitDetailView"));

			if (markerAudio != null) {
				AudioCommentsController.SharedController ().PlayComment (markerAudio, this, false);
			}

			return true;
		}
		public int DropOwnershipRequest() // 0=Dropped, 1=Waived, 2=Blocked
		{
			return 2;
		}
		
		public void ActivateButtonTopLeft()
		{
			GeneralGuiProxy.SharedGui ().EnableButtonTopLeft (null);
			HideMarker ();
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
			// NOP
		}
		public void ActivateProgressButtonRight()
		{
			// NOP
		}
	}
}
