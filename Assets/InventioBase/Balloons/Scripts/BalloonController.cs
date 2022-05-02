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
using UnityEngine.Events;
using System.Collections;

using InventioBase;

public interface IBalloonObserverInterface {
	void BalloonActivated(BalloonController balloon);
}


public class BalloonController : MonoBehaviour, IAudioCommentsNotifiable {

	public string balloonName = null;

	public Texture2D detailImage = null;
	public GameObject detailObject = null;
	public GameObject detailObjectClean = null;
	public AudioClip detailAudio = null;
	public string detailText = null;
	public string detailWeb = null;
	public string detailPDF = null;
	public bool quizBalloon = false;
	public bool allowClose = true;
    public UnityEvent onActivate;

	private string userBalloonId = null;

	private Transform balloonPlate = null;
	private Transform balloonText = null;
	private Transform balloonClosedPlate = null;

	private bool balloonClosed = false;

	private BalloonFlashScript balloonFlash = null;
	private BalloonTextFlashScript textFlash = null;

	private BalloonManager manager = null;

	private ArrayList balloonObservers = new ArrayList ();

	/*
	 * Chained|Parallel details:
	 * 
	 * detailImage|detailAudio
	 * detailObject|detailAudio
	 * detailObjectClean|detailAudio
	 * detailText|detailAudio
	 * detailImage -> detailWeb|detailPDF
	 * detailObject -> detailWeb|detailPDF
	 * detailObjectClean -> detailWeb|detailPDF
	 * detailText -> detailWeb|detailPDF
	 * detailAudio -> detailWeb|detailPDF
	 * detailWeb
	 * detailPDF
	 * +
	 * balloonObservers
	 */

	public Collider TapCollider {
		get {
			return ((balloonPlate != null) ? balloonPlate.GetComponent<Collider> () : null);
		}
	}

	private string BalloonPlateName {
		get {
			return (quizBalloon ? "BalloonPlateQuiz" : "BalloonPlate");
		}
	}

	private string UnusedPlateName {
		get {
			return (quizBalloon ? "BalloonPlate" : "BalloonPlateQuiz");
		}
	}

	private string BalloonPoleName {
		get {
			return (quizBalloon ? "BalloonPoleQuiz" : "BalloonPole");
		}
	}

	private string UnusedPoleName {
		get {
			return (quizBalloon ? "BalloonPole" : "BalloonPoleQuiz");
		}
	}

	public void AddObserver(IBalloonObserverInterface observer)
	{
		balloonObservers.Add(observer);
	}

	public void RemoveObserver(IBalloonObserverInterface observer)
	{
		balloonObservers.Remove(observer);
	}

	public void EnableBalloon(bool flag)
	{
		Collider tapCollider = this.TapCollider;
		if (tapCollider != null) {
			tapCollider.enabled = flag;
		}
	}

	public void ShowBalloon(bool flag, bool andEnable = true)
	{
		if (andEnable) {
			EnableBalloon (flag);
		}
		Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>(true);
		foreach (Renderer r in renderers) {
			r.enabled = flag;
		}
	}

	public void DeleteBalloon()
	{
		manager.RemoveBalloon (this);
		GameObject.Destroy (gameObject);
	}

	public void SetUserBalloonIdAndName(string balloonId, string balloonName)
	{
		this.userBalloonId = balloonId;
		this.balloonName = balloonName;

		// Compensate position, assume 1.3m pole
		gameObject.transform.localPosition += Vector3.up * 1.3f;

	}

	void Start () 
	{
		bool success = true;

		balloonPlate = transform.Find (this.BalloonPlateName);
		balloonText = transform.Find ("BalloonText");
		balloonClosedPlate = transform.Find ("BalloonPlateClosed");

		manager = BalloonManager.SharedBalloonManager ();

		if (balloonPlate == null) {
			Debug.LogError ("BalloonController " + gameObject.name + " missing balloon plate!");
			success = false;
		}
		if (balloonText == null) {
			Debug.LogError ("BalloonController " + gameObject.name + " missing balloon text!");
			success = false;
		}
		if (balloonName == null) {
			Debug.LogError ("BalloonController " + gameObject.name + " missing balloon name!");
			success = false;
		}
		if (balloonClosedPlate == null) {
			Debug.LogWarning("BalloonController " + gameObject.name + " missing balloon closed plate!");
		}
		if (success) {
			SetupBalloon ();
		}
		else {
			gameObject.SetActive (false);
			return;
		}
	}

	void Update () {
		HideOrCloseUpdate ();
	}

	void OnValidate()
	{
		balloonPlate = transform.Find (this.BalloonPlateName);
		balloonText = transform.Find ("BalloonText");

		if ((balloonPlate != null) && (balloonText != null)){
			TextMesh text = balloonText.GetComponent<TextMesh> ();
			if (text != null) {
				text.text = LocalizationControllerProxy.Localize(balloonName);
			}

			SetPlateSize ();
		} else {
			Debug.LogError ("BalloonController:Validate: Must have plate and text!");
		}
	}

	void SetupBalloon()
	{
		// Localize name before setting plate size
		TextMesh text = balloonText.GetComponent<TextMesh> ();
		if (text != null) {
			text.text = (quizBalloon ? "" : LocalizationControllerProxy.Localize(balloonName));
		}

		SetPlateSize ();

		balloonText.parent = balloonPlate;
		balloonPlate.gameObject.AddComponent<BalloonRotationScript>();
		balloonFlash = balloonPlate.gameObject.AddComponent<BalloonFlashScript> ();
		textFlash = balloonText.gameObject.AddComponent<BalloonTextFlashScript> ();

		Transform unused = transform.Find (this.UnusedPlateName);
		if (unused != null) {
			unused.gameObject.SetActive (false);
		}
		unused = transform.Find (this.UnusedPoleName);
		if (unused != null) {
			unused.gameObject.SetActive (false);
		}

		if (balloonClosedPlate != null) {
			if (quizBalloon || !allowClose) {
				// Quiz balloons should not open/close
				GameObject.Destroy (balloonClosedPlate.gameObject);
				balloonClosedPlate = null;
			} else {
				balloonClosedPlate.gameObject.AddComponent<BalloonRotationScript> ();
				CloseBalloon (false);
			}
		}

		manager.AddBalloon (this);
	}

	private void SetPlateSize()
	{
		if (!quizBalloon) {
			balloonPlate = transform.Find (this.BalloonPlateName);
			balloonText = transform.Find ("BalloonText");

			if ((balloonPlate != null) && (balloonText != null)) {
				/*
			 * Scale the ballon plate so that it's big enough for the text 
			 * 
			 * Assumption: Plate and text aligned with worlds xy-plane
			 */
				MeshRenderer textRenderer = balloonText.GetComponent<MeshRenderer> ();
				MeshRenderer plateRenderer = balloonPlate.GetComponent<MeshRenderer> ();
				if ((textRenderer != null) && (plateRenderer != null) && (textRenderer.bounds.size.x > 0)) {
					float factor = textRenderer.bounds.size.x / plateRenderer.bounds.size.x;
					float scaleX = balloonPlate.localScale.x * factor * 1.1f;
					scaleX = (scaleX > 0.5f ? scaleX : 0.5f);
					balloonPlate.localScale = new Vector3 (scaleX, 1f, 1f);
				}
			}
		}
	}

	private void HideOrCloseUpdate()
	{
		if (manager.CurrentCamera != null) {
			float closeDistance = manager.CloseDistance;
			float distance = Vector3.Distance (manager.CurrentCamera.transform.position, transform.position);
			if (balloonClosed && (distance < (closeDistance-1f))) {
				CloseBalloon (false);
			} else if (!balloonClosed && (distance > closeDistance)) {
				CloseBalloon (true);
			}
		}
	}

	private void CloseBalloon(bool close)
	{
		if (balloonClosedPlate != null) {
			balloonClosedPlate.gameObject.SetActive (close);
			balloonPlate.gameObject.SetActive (!close);
		}
		balloonClosed = close;
	}

	public void ActivateBalloon(Camera usingCamera)
	{
		if (balloonFlash != null) {
			balloonFlash.Flash ();
		}
		if (textFlash != null) {
			textFlash.Flash ();
		}
		if ((userBalloonId != null) && (userBalloonId.Length > 0)) {
			ActivateUserBalloon ();
		} else if (detailImage != null) {
			ActivateImageDetail ();
		} else if (detailObject != null) {
			ActivateObjectDetail ();
		} else if (detailObjectClean != null) {
			ActivateObjectDetail ();
		} else if (detailText.Length > 0) {
			ActivateTextDetail ();
		} else if (detailAudio != null) {
			ActivateAudioDetail ();
		} else if (detailWeb.Length > 0) {
			ActivateWebDetail ();
		} else if (detailPDF.Length > 0) {
			ActivatePDFDetail ();
        }
        else if ((balloonObservers.Count == 0) && (onActivate.GetPersistentEventCount() == 0))
        {
            Debug.LogWarning ("Balloon '" + balloonName + "' has no detail!");
		}

		InventioAnalyticsManager.logEvent("ActivateBalloon", balloonName);
		TrackerNGPlugin.TrackBalloon (balloonName);

        onActivate.Invoke();
		ActivateObservers();
	}

	private void ActivateUserBalloon()
	{
		// ParseUserManager remove 170402 due to switch from Parse to Firebase
		// ParseUserManager.UserShowBalloon (userBalloonId);
	}

	private void ActivateImageDetail()
	{
		// May be parallel with Audio and/or chained with Web
		Debug.Log ("ActivateImage");
		// TODO: Activate Image Detail
	}
	private void ActivateObjectDetail()
	{
		// May be parallel with Audio and/or chained with Web
		BalloonPoleScript pole = GetComponentInChildren<BalloonPoleScript> ();
		if (pole != null) {
			Transform anchor = pole.anchorTransform;
			if (anchor != null) {
				if (detailObject != null) {
					DetailController3D.ShowDetail (detailObject, anchor.position, manager.CurrentCamera, detailAudio, this);
				} else if (detailObjectClean != null) {
					DetailController3D.ShowDetailClean (detailObjectClean, anchor.position, manager.CurrentCamera, detailAudio, this);
				}
				return;
			}
		}
		Debug.LogError("Balloon " + balloonName + " ActivateObjectDetail: Missing anchor position!");
	}
	private void ActivateTextDetail()
	{
		// May be parallel with Audio and/or chained with Web
		Debug.Log ("ActivateText");
		// TODO: Display text
	}
	private void ActivateAudioDetail()
	{
		AudioCommentsController controller = AudioCommentsController.SharedController ();
		if (controller != null) {
			controller.PlayComment (detailAudio, this);
		}
	}
	private void ActivateWebDetail()
	{
		InventioWebView.ShowWebViewWithURL (detailWeb);
	}
	private void ActivatePDFDetail()
	{
		InventioWebView.ShowWebViewWithPDF (detailPDF);
	}

	private void ActivateObservers()
	{
		for (int idx = 0; idx < balloonObservers.Count; ++idx) {
			IBalloonObserverInterface observer = balloonObservers [idx] as IBalloonObserverInterface;
			if (observer != null) {
				observer.BalloonActivated (this);
			}
		}
	}


	/*
	 * IAudioCommentsNotifiable
	 */
	public bool HasDetail()
	{
		return ((detailWeb.Length > 0) || (detailPDF.Length > 0));
	}

	public void CommentDidFinish()
	{
		// NOP
	}

	public void CommentDidFinishWithDetail()
	{
		if (detailWeb.Length > 0) {
			ActivateWebDetail ();
		} else if (detailPDF.Length > 0) {
			ActivatePDFDetail ();
		} else {
			Debug.LogWarning ("Balloon " + balloonName + " received CommentDidFinishWithDetail without web or PDF detail!");
		}
	}
}
