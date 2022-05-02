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
using System;

using InventioBase;

public class DetailController3D : MonoBehaviour, ITouchConsumerInterface, IGeneralGuiDelegateInterface
{
	private bool isClean = false;
	private IAudioCommentsNotifiable notifiable = null;

	public static void ShowDetail(GameObject detail, Vector3 detailPosition, Camera fromCamera, AudioClip detailAudio, IAudioCommentsNotifiable notifyWhenFinished = null)
	{
		GameObject detailObject = new GameObject ("Detail");
		detailObject.transform.position = detailPosition;
		DetailController3D controller = detailObject.AddComponent<DetailController3D> ();
		controller.Detail = detail;
		controller.DetailAudio = detailAudio;
		controller.FromCamera = fromCamera;
		controller.notifiable = notifyWhenFinished;
	}

	// ShowDetailClean: Same as above, but with the surroundings visible instead of a homogenous background
	public static void ShowDetailClean(GameObject detail, Vector3 detailPosition, Camera fromCamera, AudioClip detailAudio, IAudioCommentsNotifiable notifyWhenFinished = null)
	{
		GameObject detailObject = new GameObject ("Detail");
		detailObject.transform.position = detailPosition;
		DetailController3D controller = detailObject.AddComponent<DetailController3D> ();
		controller.Detail = detail;
		controller.DetailAudio = detailAudio;
		controller.FromCamera = fromCamera;
		controller.isClean = true;
	}

	private GameObject detail = null;
	public GameObject Detail {
		set {
			detail = value;
		}
	}

	private AudioClip detailAudio = null;
	public AudioClip DetailAudio {
		set {
			detailAudio = value;
		}
	}

	private Camera fromCamera = null;
	public Camera FromCamera {
		set {
			fromCamera = value;
		}
	}

	private float fadeTime = 1f;
	private Camera detailCamera = null;
	private GameObject theDetail = null;
	private GameObject detailLight = null;
	private Vector3 originalPosition;
	void Start () {
		originalPosition = gameObject.transform.position;

		SetPlayerMoveIgnoreTouch (false);

		if (isClean) {
			Invoke("StartDetailViewClean", .1f);
		} else {
			GameObject cameraObject = new GameObject ("DetailCamera");
			detailCamera = cameraObject.AddComponent<Camera> ();
			detailCamera.CopyFrom (fromCamera);
			detailCamera.transform.parent = fromCamera.transform;
			fromCamera.enabled = false;

			FadeToColor (Color.black, fadeTime, StartDetailView);
		}

		GlobalMenuProxy.SharedMenu ().PushHideMenu ();
		TouchController.SharedTouchController().AddTouchConsumer (this);
		GeneralGuiProxy.SharedGui ().RequestOwnership (this);
	}

	private bool defaultIgnoreTouches = false;
	private void SetPlayerMoveIgnoreTouch(bool useDefault)
	{
		GameObject player = GameObject.Find ("Player");
		if (player != null) {
			PlayerMoveScript move = player.GetComponent<PlayerMoveScript>();
			if (useDefault) {
				move.ignoreTouches = defaultIgnoreTouches;
			} else {
				defaultIgnoreTouches = move.ignoreTouches;
				move.ignoreTouches = true;
			}
		}
	}

	private GUITexture faderGUITexture = null;
	private Color fadeColor = Color.black;
	private void FadeToColor(Color color, float fadeTime, Action onComplete = null)
	{
		fadeColor = color;
		if (faderGUITexture == null) {
			detailCamera.gameObject.AddComponent<GUILayer> (); // Fader needs a GUILayer...

			GameObject faderObject = new GameObject ("DetailFader");
			faderObject.transform.position = Vector3.zero;
			faderObject.transform.localScale = new Vector3 (2f, 2f, 0f);

			// Texture for the fader
			Texture2D faderTexture = new Texture2D (1, 1);
			faderTexture.SetPixel (0, 0, color);
			faderTexture.Apply ();

			faderGUITexture = faderObject.AddComponent<GUITexture> ();
			faderGUITexture.texture = faderTexture;
			faderGUITexture.color = new Color (1f, 1f, 1f, 0f);
		}
		LeanTween.value (faderGUITexture.gameObject, UpdateFaderAlpha, 0f, 1f, fadeTime).setEase (LeanTweenType.easeInOutQuad).setOnComplete(onComplete).setUseEstimatedTime(true);
	}

	private void FadeBack(float fadeTime, Action onComplete = null)
	{
		if (faderGUITexture != null) {
			LeanTween.value (faderGUITexture.gameObject, UpdateFaderAlpha, 1f, 0f, fadeTime).setEase (LeanTweenType.easeInOutQuad).setOnComplete(onComplete).setUseEstimatedTime(true);
		}
	}

	private void UpdateFaderAlpha(float value)
	{
		faderGUITexture.color = new Color (1f, 1f, 1f, value);
	}

	DetailTouchController3D detailTouchController = null;
	private void StartDetailView()
	{
		int detailLayer = LayerMask.NameToLayer ("DetailLayer");
		if (detailLayer < 0) {
			detailLayer = 30; // Just start looking for a available layer...
			string layerName = LayerMask.LayerToName(detailLayer);
			while ((detailLayer > 0) && (layerName != null) && (layerName.Length > 0)) {
				detailLayer -= 1;
				layerName = LayerMask.LayerToName(detailLayer);
			}
		}

		detailCamera.farClipPlane = 10f;
		detailCamera.backgroundColor = fadeColor;
		detailCamera.clearFlags = CameraClearFlags.SolidColor;
		detailCamera.cullingMask = 1 << detailLayer;

		theDetail = Instantiate (detail, gameObject.transform.position, Quaternion.identity) as GameObject;
		theDetail.name = "Model";
		theDetail.transform.parent = gameObject.transform;
		SetLayerRecursively (theDetail, detailLayer);

		// Place the object in the center of the view, independent of its local pivot position
		Renderer detailRenderer = theDetail.GetComponent<Renderer> ();
		if (detailRenderer != null) {
			GameObject detailPivot = new GameObject ("ModelPivot");
			detailPivot.transform.parent = gameObject.transform;
			detailPivot.transform.localPosition = Vector3.zero;
			theDetail.transform.parent = detailPivot.transform;
			theDetail.transform.localPosition = theDetail.transform.position - detailRenderer.bounds.center;
			theDetail = detailPivot;
		}

		detailTouchController = theDetail.AddComponent<DetailTouchController3D> ();
		detailTouchController.CameraTransform = detailCamera.transform;
		detailTouchController.MaximumScale = 2f - detailCamera.nearClipPlane - .1f; // Camera set to maxDistance*2, make room for clipping...
		detailTouchController.OriginalScale = theDetail.transform.localScale;
		detailTouchController.TouchEnabled = false;

		detailCamera.nearClipPlane = .1f;

		// Find the bounds of the detailObject
		float maxSize = 0;
		Renderer[] detailRenderers = theDetail.GetComponentsInChildren<Renderer>();
		foreach (Renderer r in detailRenderers) {
			float size = r.bounds.extents.magnitude;
			maxSize = (size > maxSize ? size : maxSize);
		}

		gameObject.transform.parent = detailCamera.transform;

		// Add some light
		detailLight = new GameObject ("Light");
		detailLight.AddComponent<Light>();
		detailLight.GetComponent<Light>().color = Color.white;
		detailLight.GetComponent<Light>().intensity = .7f;
		detailLight.GetComponent<Light>().cullingMask = detailCamera.cullingMask;
		detailLight.transform.parent = detailCamera.transform;
		detailLight.transform.localPosition = new Vector3(maxSize, maxSize, 0f);

		// Scale up the object from "small"
		theDetail.transform.localScale *= 0.01f;
		LeanTween.scale (theDetail, detailTouchController.OriginalScale, fadeTime).setEase (LeanTweenType.easeInOutQuad).setUseEstimatedTime(true);

		// Move the object in front of the camera, at maxSize*2 distance
		LeanTween.moveLocal (gameObject, Vector3.forward * maxSize * 2f, fadeTime).setEase (LeanTweenType.easeInOutQuad).setOnComplete(RunDetailView).setUseEstimatedTime(true);
		FadeBack (.5f);
	}

	private void SetLayerRecursively(GameObject obj, int newLayer)
	{
		obj.layer = newLayer;

		foreach ( Transform child in obj.transform )
		{
			SetLayerRecursively( child.gameObject, newLayer );
		}
	}

	private void StartDetailViewClean()
	{
		int detailLayer = LayerMask.NameToLayer ("DetailLayer");
		if (detailLayer < 0) {
            // Just use default layer...
            detailLayer = LayerMask.NameToLayer("Default");
		}

		theDetail = Instantiate (detail, gameObject.transform.position, Quaternion.identity) as GameObject;
		theDetail.name = "Model";
		theDetail.transform.parent = gameObject.transform;
		theDetail.layer = detailLayer;

		// Place the object in the center of the view, independent of its local pivot position
		Renderer detailRenderer = theDetail.GetComponent<Renderer> ();
		if (detailRenderer != null) {
			GameObject detailPivot = new GameObject ("ModelPivot");
			detailPivot.transform.parent = gameObject.transform;
			detailPivot.transform.localPosition = Vector3.zero;
			theDetail.transform.parent = detailPivot.transform;
			theDetail.transform.localPosition = theDetail.transform.position - detailRenderer.bounds.center;
			theDetail = detailPivot;
		}
		
		detailTouchController = theDetail.AddComponent<DetailTouchController3D> ();
		detailTouchController.CameraTransform = fromCamera.transform;
		detailTouchController.MaximumScale = 2f - fromCamera.nearClipPlane - .1f; // Camera set to maxDistance*2, make room for clipping...
		detailTouchController.OriginalScale = theDetail.transform.localScale;
		detailTouchController.TouchEnabled = false;
		
//		detailCamera.nearClipPlane = .1f;
		
		// Find the bounds of the detailObject
		float maxSize = 0;
		Renderer[] detailRenderers = theDetail.GetComponentsInChildren<Renderer>();
		foreach (Renderer r in detailRenderers) {
			float size = r.bounds.extents.magnitude;
			maxSize = (size > maxSize ? size : maxSize);
		}
		
		gameObject.transform.parent = fromCamera.transform;
		
		// Add some light
		detailLight = new GameObject ("Light");
		detailLight.AddComponent<Light>();
		detailLight.GetComponent<Light>().color = Color.white;
		detailLight.GetComponent<Light>().intensity = .7f;
		detailLight.GetComponent<Light>().cullingMask = 1 << detailLayer;
		detailLight.transform.parent = fromCamera.transform;
		detailLight.transform.localPosition = new Vector3(maxSize, maxSize, 0f);

		// Scale up the object from "small"
		theDetail.transform.localScale *= 0.01f;
		LeanTween.scale (theDetail, detailTouchController.OriginalScale, fadeTime).setEase (LeanTweenType.easeInOutQuad).setUseEstimatedTime(true);
		
		// Move the object in front of the camera, at maxSize*2 distance
		LeanTween.moveLocal (gameObject, Vector3.forward * maxSize * 2f, fadeTime).setEase (LeanTweenType.easeInOutQuad).setOnComplete(RunDetailView).setUseEstimatedTime(true);
		FadeBack (.5f);
	}

	private void RunDetailView()
	{
		detailTouchController.TouchEnabled = true;
		GeneralGuiProxy.SharedGui ().EnableButtonTopLeft(LocalizationControllerProxy.Localize("Base.ExitDetailView"));
		if ((notifiable != null) && notifiable.HasDetail()) {
			GeneralGuiProxy.SharedGui ().EnableButtonTopRight (LocalizationControllerProxy.Localize ("Base.ReadMore"));
		}
		// TODO: Provide hint for zoom and rotate details

		if (detailAudio != null) {
			AudioCommentsController.SharedController ().PlayComment (detailAudio, null, false);
		}
	}

	private void DismissDetailView ()
	{
		detailTouchController.TouchEnabled = false;
		LeanTween.move (gameObject, originalPosition, fadeTime).setEase (LeanTweenType.easeInOutQuad).setOnComplete(EndDetailView).setUseEstimatedTime(true);
		LeanTween.scale (theDetail, detailTouchController.OriginalScale * .01f, fadeTime).setEase (LeanTweenType.easeInOutQuad).setUseEstimatedTime(true);
		if (!isClean) {
			FadeToColor (Color.black, fadeTime);
		}
		if (detailAudio != null) {
			AudioCommentsController.SharedController ().StopPlaying ();
		}
	}

	private void EndDetailView()
	{
		GameObject.Destroy (theDetail);
		theDetail = null;

		if (isClean) {
			CleanupDetailView();
		}
		else {
			detailCamera.backgroundColor = fromCamera.backgroundColor;
			detailCamera.clearFlags = fromCamera.clearFlags;
			detailCamera.cullingMask = fromCamera.cullingMask;
			detailCamera.farClipPlane = fromCamera.farClipPlane;
			detailCamera.nearClipPlane = fromCamera.nearClipPlane;

			FadeBack (fadeTime, CleanupDetailView);
		}
	}
	private void CleanupDetailView()
	{
		GameObject.Destroy (detailLight);
		if (!isClean) {
			detailCamera.GetComponent<Camera>().enabled = false;
			fromCamera.enabled = true;
			GameObject.Destroy (faderGUITexture.gameObject);
			GameObject.Destroy (detailCamera.gameObject);
		}

		TouchController.SharedTouchController().RemoveTouchConsumer (this);
		GlobalMenuProxy.SharedMenu ().PopHideMenu ();
		GeneralGuiProxy.SharedGui ().DropOwnership ();

		SetPlayerMoveIgnoreTouch (true);
	}


	/*
	 * ITouchConsumerInterface
	 */
	public bool ConsumesTouchEventAtPosition(float xPos, float yPos)
	{
		return true;
	}


	/*
	 * IGeneralGuiDelegateInterface
	 */
	public bool ReceiveOwnership()
	{
		return true;
	}
	public int DropOwnershipRequest() // 0=Dropped, 1=Waived, 2=Blocked
	{
		return 2;
	}

	public void ActivateButtonTopLeft()
	{
		GeneralGuiProxy.SharedGui ().EnableButtonTopLeft (null);
		GeneralGuiProxy.SharedGui ().EnableButtonTopRight (null);
		DismissDetailView ();
	}
	public void ActivateButtonTopRight()
	{
		GeneralGuiProxy.SharedGui ().EnableButtonTopLeft (null);
		GeneralGuiProxy.SharedGui ().EnableButtonTopRight (null);
		if ((notifiable != null) && notifiable.HasDetail()) {
			notifiable.CommentDidFinishWithDetail ();
		}
		DismissDetailView ();
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
