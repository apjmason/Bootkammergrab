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
	public class MapController : MonoBehaviour, IInventioControllerObserverInterface, IGeneralGuiDelegateInterface {
		public int textureSize = 512;
		public int mapSize = 512;
		public float mapAltitude = 50f;
		public float viewAltitude = 2300f;

		public Vector2 mapCenterPosition = new Vector2(0, 0);

		public Shader mapShader = null;

		public bool reuseInModes = false;
		public bool trackBalloons = true;

		public Color trackColor = Color.red;

		public GameObject startPositionMarkerPrefab = null;
		public GameObject currentPositionMarkerPrefab = null;
		public GameObject balloonMarkerPrefab = null;

		public Canvas mapUiCanvas = null;
		public RectTransform mapUiNeedle = null;

		private Texture2D mapTexture = null;
		private GameObject mapObject = null;
		private Transform player = null;
		private Transform mainCameraTransform = null;

		private Camera mainCamera = null;
		private Vector3 cameraLocalPosition;
		private float cameraNearClipping;
		private float cameraFarClipping;
		private float cameraFieldOfView;

		private const string MAP_OBJECT_NAME = "MapObject";

		private bool showingMap = false;

		private float mapPixelScale = 1f;
		private float mapPixelOffset = 0f;

		private GameObject playerMarker = null;
		private GameObject startMarker = null;
		private GameObject balloonMarkers = null;

		private Transform followUserHead = null;
		private Transform turnNorthHead = null;
		private Transform fixedHead = null;

		private bool northUp = false;

		private Vector3 startPosition = Vector3.zero;

		private MapBalloonTracker balloonTracker = null;

		private bool recordingEnabled = false;

		private float fogDensity = 0f;

		void Start ()
		{
			if (!SetReferences ()) {
				gameObject.SetActive (false);
				return;
			}

			if (this.mapUiCanvas == null || this.mapUiNeedle == null) {
				Debug.LogWarning ("MapController missing mapUiCanvas and/or mapUiNeedle!");
			}
			else if (UnityEngine.EventSystems.EventSystem.current == null) {
				Debug.LogWarning ("MapController has mapUiCanvas but no EventSystem!");
			}

			CreateMapObject ();

			if (this.reuseInModes) {
				GameObject.DontDestroyOnLoad (gameObject);
				if (this.mapUiCanvas != null) {
					GameObject.DontDestroyOnLoad (this.mapUiCanvas.gameObject);
				}
				if (UnityEngine.EventSystems.EventSystem.current != null) {
					GameObject.DontDestroyOnLoad(UnityEngine.EventSystems.EventSystem.current.gameObject);
				}
			}

			CreateHeads ();

			if (this.trackBalloons) {
				this.balloonTracker = gameObject.AddComponent<MapBalloonTracker>();
			}

			AddToGlobalMenu ();

			// Delay recording positions
			Invoke ("EnableRecording", 2f);

			this.mapObject.GetComponent<Renderer>().enabled = false;
		}
	
		void Update ()
		{
			if (this.recordingEnabled) {
				// Update texture
				int xPixel = Mathf.RoundToInt ((this.player.position.x - this.mapCenterPosition.x) * this.mapPixelScale + this.mapPixelOffset);
				int yPixel = Mathf.RoundToInt ((this.player.position.z - this.mapCenterPosition.y) * this.mapPixelScale + this.mapPixelOffset);
				if ((xPixel > 0) && (xPixel < this.mapTexture.width - 1) && (yPixel > 0) && (yPixel < this.mapTexture.height - 1)) {
					this.mapTexture.SetPixel (xPixel - 1, yPixel, this.trackColor);
					this.mapTexture.SetPixel (xPixel, yPixel, this.trackColor);
					this.mapTexture.SetPixel (xPixel + 1, yPixel, this.trackColor);
					this.mapTexture.SetPixel (xPixel, yPixel + 1, this.trackColor);
					this.mapTexture.SetPixel (xPixel, yPixel - 1, this.trackColor);
					if (this.mapObject.GetComponent<Renderer>().enabled) {
						this.mapTexture.Apply ();
					}
				}
			}

			// Update Player Marker
			if (this.playerMarker != null) {
				Vector3 markerPosition = this.player.position;
				markerPosition.y = this.mapAltitude + 1f;
				this.playerMarker.transform.position = markerPosition;
			}

			if (this.startPosition == Vector3.zero) {
				this.startPosition = this.player.position;
				this.startPosition.y = this.mapAltitude + 1f;
			}

			if (this.mapUiNeedle != null) {
				Vector3 needleRotation = Vector3.zero;
				needleRotation.z = this.mainCameraTransform.eulerAngles.y;
				this.mapUiNeedle.localEulerAngles = needleRotation;
			}
		}

		void OnLevelWasLoaded(int level) {
			GameObject mapObject = GameObject.Find (MAP_OBJECT_NAME);
			if ((mapObject != null) && (this.mapTexture == null)) {
				// We already have a mapObject and texture, remove this controller and its Canvas
				GameObject.Destroy (gameObject);
				if (this.mapUiCanvas != null) {
					GameObject.Destroy (this.mapUiCanvas.gameObject);
				}
				return;
			}

			if (!SetReferences ()) {
				gameObject.SetActive (false);
				return;
			}

			CreateHeads ();
			AddToGlobalMenu ();

			// Delay recording positions
			Invoke ("EnableRecording", 2f);
		}

		private bool SetReferences()
		{
			GameObject cameraObject = GameObject.Find ("/Player/Main Camera");
			if (cameraObject == null) {
				Debug.LogWarning ("MapController " + name + " couldn't find Main Camera Transform!");
				return false;
			}
			this.mainCamera = cameraObject.GetComponent<Camera> ();
			if (this.mainCamera == null) {
				Debug.LogWarning ("MapController " + name + " couldn't find Main Camera!");
				return false;
			}

			if (this.startPositionMarkerPrefab == null) {
				Debug.LogWarning ("MapController " + name + " missing startPositionMarkerPrefab!");
				return false;
			}
			if (this.currentPositionMarkerPrefab == null) {
				Debug.LogWarning ("MapController " + name + " missing currentPositionMarkerPrefab!");
				return false;
			}
			if (this.trackBalloons && this.balloonMarkerPrefab == null) {
				Debug.LogWarning ("MapController " + name + " missing balloonMarkerPrefab!");
				return false;
			}

			this.player = cameraObject.transform.parent;
			this.mainCameraTransform = cameraObject.transform;

			this.cameraLocalPosition = this.mainCameraTransform.localPosition;
			this.cameraNearClipping = this.mainCamera.nearClipPlane;
			this.cameraFarClipping = this.mainCamera.farClipPlane;
			this.cameraFieldOfView = this.mainCamera.fieldOfView;

			return true;
		}

		private void EnableRecording()
		{
			if (SpatialManagerPlugin.UsingTouchMove () || 
			    SpatialManagerPlugin.HasValidPosition() && this.player.GetComponent<PlayerMoveScript>().IsWithinArea) {
				this.recordingEnabled = true;
			} else {
				Invoke ("EnableRecording", 2f); // Try again
			}
		}

		private void CreateMapObject()
		{
			// Create plane
			this.mapObject = new GameObject (MAP_OBJECT_NAME);
			if (this.reuseInModes) {
				GameObject.DontDestroyOnLoad (this.mapObject);
			}
			this.mapObject.transform.position = new Vector3 (this.mapCenterPosition.x, this.mapAltitude, this.mapCenterPosition.y);
			
			MeshFilter meshFilter = this.mapObject.AddComponent<MeshFilter>();
			Mesh mesh = new Mesh ();
			meshFilter.mesh = mesh;
			
			// Plane vertices
			Vector3[] vertices = new Vector3[4];
			vertices[0] = new Vector3(-this.mapSize / 2f, 0, -this.mapSize / 2f);
			vertices[1] = new Vector3(this.mapSize / 2f, 0, -this.mapSize / 2f);
			vertices[2] = new Vector3(-this.mapSize / 2f, 0, this.mapSize / 2f);
			vertices[3] = new Vector3(this.mapSize / 2f, 0, this.mapSize / 2f);
			mesh.vertices = vertices;
			
			// Plane triangles
			int[] tri = new int[6];
			tri[0] = 0;
			tri[1] = 2;
			tri[2] = 1;
			
			tri[3] = 2;
			tri[4] = 3;
			tri[5] = 1;
			mesh.triangles = tri;
			
			// Plane (vertices) normals
			Vector3[] normals = new Vector3[4];
			normals[0] = Vector3.up;
			normals[1] = Vector3.up;
			normals[2] = Vector3.up;
			normals[3] = Vector3.up;
			mesh.normals = normals;
			
			// Plane texture coordinates
			Vector2[] uv = new Vector2[4];
			uv[0] = new Vector2(0, 0);
			uv[1] = new Vector2(1, 0);
			uv[2] = new Vector2(0, 1);
			uv[3] = new Vector2(1, 1);
			mesh.uv = uv;

			MeshRenderer renderer = this.mapObject.AddComponent<MeshRenderer>();
			renderer.castShadows = false;
			renderer.receiveShadows = false;
			
			// Create texture
			this.mapTexture = new Texture2D(this.textureSize, this.textureSize);
			this.mapTexture.wrapMode = TextureWrapMode.Clamp;
			for (int y = 0; y < this.textureSize; ++y) {
				for (int x = 0; x < this.textureSize; ++x) {
					Color defaultColor = Color.clear; // new Color(0f, 1f, 0f, .3f);
					this.mapTexture.SetPixel(x, y, defaultColor);
				}
			}
			this.mapTexture.Apply ();
			
			// Create material
			Material mapMaterial = new Material (this.mapShader);
			mapMaterial.mainTexture = this.mapTexture;
			
			renderer.material = mapMaterial;

			this.mapPixelScale = (float)this.textureSize / (float)this.mapSize;
			this.mapPixelOffset = (float)this.textureSize / 2f;

		}

		private void CreateHeads()
		{
			if (this.followUserHead != null) {
				GameObject.Destroy(this.followUserHead.gameObject);
			}
			if (this.turnNorthHead != null) {
				GameObject.Destroy(this.turnNorthHead.gameObject);
			}
			if (this.fixedHead != null) {
				GameObject.Destroy(this.fixedHead.gameObject);
			}

			GameObject head = new GameObject ("FollowUserHead");
			head.transform.parent = transform;
			head.transform.localPosition = Vector3.zero;
			head.AddComponent<CameraRotationScript> ();
			this.followUserHead = head.transform;

			head = new GameObject ("TurnNorthHead");
			head.transform.parent = transform;
			head.transform.localPosition = Vector3.zero;
			head.AddComponent<HeadingFollowCompassScript> ();
			this.turnNorthHead = head.transform;

			head = new GameObject ("FixedHead");
			head.transform.parent = transform;
			head.transform.localPosition = Vector3.zero;
			head.transform.eulerAngles = Vector3.zero;
			this.fixedHead = head.transform;
		}

		private void AddToGlobalMenu()
		{
			InventioController.SharedController().AddObserver(this);
		}

		private void ShowMap()
		{
			if (GeneralGuiProxy.SharedGui ().RequestOwnership (this)) {
				TrackerNGPlugin.TrackMap();
				TouchController.SharedTouchController ().SetBalloonsEnabled (false);
				GlobalMenuProxy.SharedMenu ().PushHideMenu ();
				
				// Re-parent the camera in order to move it
				transform.position = this.mainCameraTransform.position;
				if (this.northUp) {
					this.mainCameraTransform.parent = this.fixedHead;
				}
				else {
					this.mainCameraTransform.parent = this.turnNorthHead;
				}

				// Disable camera rotation
				CameraRotationScript rotation = this.mainCameraTransform.GetComponent<CameraRotationScript>();
				if (rotation != null) {
					rotation.enabled = false;
				}

				// Disable touch move
				PlayerMoveScript move = this.player.GetComponent<PlayerMoveScript>();
				if (move != null) {
					move.ignoreTouches = true;
				}

				LeanTween.rotateLocal(this.mainCameraTransform.gameObject, new Vector3(90f, 0f, 0f), 1f).setEase (LeanTweenType.easeInOutCubic).setOnComplete (MoveUp).setUseEstimatedTime(true);
			} else {
				Debug.LogWarning ("MapController " + name + " didn't get GeneralGui ownership!");
			}
		}

		private void MoveUp()
		{
			Vector3 targetPosition = new Vector3 (this.player.position.x, this.viewAltitude, this.player.position.z);
			LeanTween.move(gameObject, targetPosition, 1.5f).setEase (LeanTweenType.easeInOutCubic).setOnComplete (MoveUpFinished).setUseEstimatedTime(true);
			LeanTween.value(gameObject, UpdateFieldOfView, this.mainCamera.fieldOfView, 7f, .1f).setEase (LeanTweenType.easeInOutCubic).setUseEstimatedTime(true);
			LeanTween.value(gameObject, UpdateFarClipping, this.mainCamera.farClipPlane, this.viewAltitude+700f, 1.1f).setEase (LeanTweenType.easeInOutCubic).setUseEstimatedTime(true).setDelay(.3f);
			LeanTween.value(gameObject, UpdateNearClipping, this.mainCamera.nearClipPlane, this.viewAltitude-300f, 1.1f).setEase (LeanTweenType.easeInOutCubic).setUseEstimatedTime(true).setDelay(.3f);

			this.fogDensity = RenderSettings.fogDensity;
			LeanTween.value(gameObject, UpdateFog, RenderSettings.fogDensity, 0f, 1f).setEase (LeanTweenType.easeInOutCubic).setUseEstimatedTime(true);
		}

		private void MoveUpFinished()
		{
			Color tintColor = new Color (1f, 1f, 1f, 0f);
			this.mapTexture.Apply ();
			this.mapObject.GetComponent<Renderer>().enabled = true;
			this.mapObject.GetComponent<Renderer>().sharedMaterial.color = tintColor;
			LeanTween.alpha (this.mapObject, 1f, .5f).setUseEstimatedTime(true);
			if (this.hasGui) {
				GeneralGuiProxy.SharedGui().EnableButtonTopLeft(LocalizationControllerProxy.Localize("Base.CloseMapView"));
				TouchController.SharedTouchController ().SetBalloonsEnabled (true);
			}
			this.showingMap = true;

			MapPan pan = gameObject.AddComponent<MapPan> ();
			pan.MainCamera = this.mainCamera;
			pan.MapPosMin = new Vector2 (this.mapCenterPosition.x - this.mapSize / 2f, this.mapCenterPosition.y - this.mapSize / 2f);
			pan.MapPosMax = new Vector2 (this.mapCenterPosition.x + this.mapSize / 2f, this.mapCenterPosition.y + this.mapSize / 2f);

			CreateMarkers ();

			if (this.mapUiCanvas != null) {
				this.mapUiCanvas.gameObject.SetActive(true);
			}
		}

		private void CreateMarkers()
		{
			this.playerMarker = GameObject.Instantiate (this.currentPositionMarkerPrefab) as GameObject;
			this.playerMarker.name = "MapView_PlayerMarker";
			this.playerMarker.transform.position = new Vector3 (this.player.position.x, this.mapAltitude + 1f, this.player.position.z);
			this.playerMarker.GetComponent<Renderer>().sharedMaterial.renderQueue += 1;

			this.startMarker = GameObject.Instantiate (this.startPositionMarkerPrefab) as GameObject;
			this.startMarker.name = "MapView_StartMarker";
			this.startMarker.transform.position = this.startPosition;
			this.startMarker.GetComponent<Renderer>().sharedMaterial.renderQueue += 1;
			if (this.startPosition == Vector3.zero) {
				// We have no valid start position
				this.startMarker.SetActive(false);
			}

			this.balloonMarkers = new GameObject("BalloonMarkers");
			this.balloonMarkers.transform.parent = this.mapObject.transform;
			if (this.balloonTracker != null) {
				Vector3[] activatedBalloons = this.balloonTracker.GetBalloonPositions(true);
				Vector3[] nonActivatedBalloons = this.balloonTracker.GetBalloonPositions(false);
				for (int idx = 0; idx < activatedBalloons.Length; ++idx) {
					GameObject balloon = GameObject.Instantiate(this.balloonMarkerPrefab) as GameObject;
					Vector3 balloonPosition = activatedBalloons[idx];
					balloonPosition.y = this.mapAltitude + 2f;
					balloon.transform.position = balloonPosition;
					balloon.transform.parent = this.balloonMarkers.transform;
					balloon.GetComponent<Renderer>().sharedMaterial.renderQueue += 1;
					HeadingFollowObjectScript headingFollow = balloon.AddComponent<HeadingFollowObjectScript>();
					headingFollow.follow = this.mainCameraTransform;
					headingFollow.offsetDegrees = 180f;
					LeanTween.alpha(balloon, .5f, .2f).setUseEstimatedTime(true);
				}
				for (int idx = 0; idx < nonActivatedBalloons.Length; ++idx) {
					GameObject balloon = GameObject.Instantiate(this.balloonMarkerPrefab) as GameObject;
					Vector3 balloonPosition = nonActivatedBalloons[idx];
					balloonPosition.y = this.mapAltitude + 2f;
					balloon.transform.position = balloonPosition;
					balloon.transform.parent = this.balloonMarkers.transform;
					balloon.GetComponent<Renderer>().sharedMaterial.renderQueue += 1;
					HeadingFollowObjectScript headingFollow = balloon.AddComponent<HeadingFollowObjectScript>();
					headingFollow.follow = this.mainCameraTransform;
					headingFollow.offsetDegrees = 180f;
				}
			}
		}

		private void RemoveMarkers()
		{
			GameObject.Destroy (this.playerMarker);
			this.playerMarker = null;
			GameObject.Destroy (this.startMarker);
			this.startMarker = null;
			GameObject.Destroy (this.balloonMarkers);
			this.balloonMarkers = null;
		}

		private void UpdateFieldOfView(float fov)
		{
			this.mainCamera.fieldOfView = fov;
		}

		private void UpdateFarClipping(float clipping)
		{
			this.mainCamera.farClipPlane = clipping;
		}

		private void UpdateNearClipping(float clipping)
		{
			this.mainCamera.nearClipPlane = clipping;
		}

		private void UpdateFog(float fogDensity)
		{ 
			RenderSettings.fogDensity = fogDensity;
		}

		private void HideMap()
		{
			this.showingMap = false;

			if (this.mapUiCanvas != null) {
				this.mapUiCanvas.gameObject.SetActive(false);
			}

			MapPan pan = gameObject.GetComponent<MapPan> ();
			if (pan != null) {
				GameObject.Destroy(pan);
			}
			LeanTween.alpha (this.mapObject, 0f, .5f).setOnComplete(MoveDown).setUseEstimatedTime(true);
		}

		private void MoveDown()
		{
			this.mapObject.GetComponent<Renderer>().enabled = false;

			transform.parent = this.player;
			LeanTween.moveLocal(gameObject, this.cameraLocalPosition, 1.5f).setEase (LeanTweenType.easeInOutCubic).setOnComplete (MoveDownFinished).setUseEstimatedTime(true);
			LeanTween.value(gameObject, UpdateFieldOfView, this.mainCamera.fieldOfView, this.cameraFieldOfView, .1f).setEase (LeanTweenType.easeInOutCubic).setUseEstimatedTime(true).setDelay(1.4f);
			LeanTween.value(gameObject, UpdateFarClipping, this.mainCamera.farClipPlane, this.cameraFarClipping, 1f).setEase (LeanTweenType.easeInOutCubic).setUseEstimatedTime(true);
			LeanTween.value(gameObject, UpdateNearClipping, this.mainCamera.nearClipPlane, this.cameraNearClipping, 1f).setEase (LeanTweenType.easeInOutCubic).setUseEstimatedTime(true);

			RemoveMarkers ();

			LeanTween.value(gameObject, UpdateFog, RenderSettings.fogDensity, this.fogDensity, 1f).setEase (LeanTweenType.easeInOutCubic).setUseEstimatedTime(true);
		}

		private void MoveDownFinished()
		{
			this.mainCameraTransform.parent = this.followUserHead;
			LeanTween.rotateLocal(this.mainCamera.gameObject, Vector3.zero, 1f).setEase (LeanTweenType.easeInOutCubic).setOnComplete (LeaveMapView).setUseEstimatedTime(true);
		}

		private void LeaveMapView()
		{
			this.mainCameraTransform.parent = this.player;

			// Enable camera rotation
			CameraRotationScript rotation = this.mainCameraTransform.GetComponent<CameraRotationScript>();
			if (rotation != null) {
				rotation.enabled = true;
			}

			// Enable touch move
			PlayerMoveScript move = this.player.GetComponent<PlayerMoveScript>();
			if (move != null) {
				move.ignoreTouches = false;
			}

			transform.parent = null;

			if (hasGui) {
				GeneralGuiProxy.SharedGui ().DropOwnership ();
			}

			TouchController.SharedTouchController ().SetBalloonsEnabled (true);
			GlobalMenuProxy.SharedMenu ().PopHideMenu ();
		}

		public void ToggleCompassMode()
		{
			this.northUp = !this.northUp;
			if (this.northUp) {
				this.mainCameraTransform.parent = this.fixedHead;
			} else {
				this.mainCameraTransform.parent = this.turnNorthHead;
			}
			LeanTween.rotateLocal(this.mainCameraTransform.gameObject, new Vector3(90f, 0f, 0f), .5f).setEase (LeanTweenType.easeInOutCubic).setUseEstimatedTime(true);
		}

		// GlobalMenu Callbacks
		public void MapViewMenuButtonClicked( int buttonId )
		{
			ShowMap ();
		}


		// IInventioControllerObserverInterface
		public void InventioSetupComplete()
		{
			InventioController.SharedController().RemoveObserver(this);
			GlobalMenuProxy.SharedMenu().AddRightButton(LocalizationControllerProxy.Localize("Base.MapView"), new GlobalMenuButtonHandler(MapViewMenuButtonClicked));

			if (this.mapUiCanvas != null) {
				this.mapUiCanvas.gameObject.SetActive(false);
			}
		}


	 	// IGeneralGuiDelegateInterface
		private bool hasGui = false;
		public bool ReceiveOwnership()
		{
			if (this.showingMap) {
				GeneralGuiProxy.SharedGui().EnableButtonTopLeft(LocalizationControllerProxy.Localize("Base.CloseMapView"));
			}
			this.hasGui = true;
			return true;
		}
		
		public int DropOwnershipRequest() // 0=Dropped, 1=Waived, 2=Blocked
		{
			hasGui = false;
			return 1;
		}
		
		public void ActivateButtonTopLeft()
		{
			GeneralGuiProxy.SharedGui().EnableButtonTopLeft(null);

			HideMap ();
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
