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

#if (UNITY_EDITOR)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

using InventioBase;


public class SitSimConfiguration : MonoBehaviour {

		private static float DEFAULT_REFERENCE_LENGTH = 100f;

		public bool showApplicationSettings = true;
		public bool showCoreSitSimSettings = true;
		public bool showPlayerSettings = true;
		public bool showTerrainSettings = true;

		public InventioController inventioController = null;
		public LocalizationController_dfGUI localizationController = null;

		public GameObject terrain = null;
		public GameObject terrainAsset = null;
		public GameObject referenceObject = null;
		private float referenceObjectLength = 100.0f;
		public float ReferenceObjectLength {
				get { return referenceObjectLength; }
				set { 
						referenceObjectLength = value;
						if (referenceObject != null) { 
								referenceObject.transform.localScale = new Vector3 (referenceObjectLength / 3.0f, referenceObjectLength / 3.0f, 1.0f);
						} 
				}
		}
		public GameObject origo = null;

		public GameObject player = null;

		public GameObject visualRoot = null;
		public float visualsOrientation {
				get {return visualRoot.transform.eulerAngles.y;}
				set {
						Vector3 euler = visualRoot.transform.eulerAngles;
						euler.y = value;
						visualRoot.transform.eulerAngles = euler;
				}
		}
		public float visualsScale {
				get {return terrain.transform.localScale.x;}
				set { terrain.transform.localScale = Vector3.one * value; }
		}

		public GameObject balloons = null;

		/*
		 * Singleton
		 */
		private static SitSimConfiguration sInstance = null;
		public static SitSimConfiguration SharedInstance {
				get {
						if (sInstance == null) {
								sInstance = FindObjectOfType<SitSimConfiguration>();
								if (sInstance == null) {
										Debug.LogWarning ("NO SITSIM CONFIGURATION!");
								}

						}
						return sInstance;
				}
		}

		public static SitSimConfiguration CreateInstance() {
				if (sInstance == null) {
						// Add it to the scene
						GameObject configurationObject = new GameObject("SitSimConfiguration");
						Undo.RecordObject (configurationObject, "Create SitSim Configuration");
						sInstance = configurationObject.AddComponent<SitSimConfiguration> ();
						if (sInstance == null) {
								Debug.LogWarning ("COULD NOT CREATE SITSIM CONFIGURATION!");
								// TODO: Show error dialog
								GameObject.DestroyImmediate(configurationObject);
						}
				}
				return sInstance;
		}


		/*
		 * Visibility of SitSim core objects and this configuration object
		 */
		public bool sitsimObjectsVisible = false;
		public static void ToggleVisibility() {
				SitSimConfiguration config = SitSimConfiguration.SharedInstance;
				if (config != null) {
						Undo.RecordObject (config, "Toggle visibility");
						config.sitsimObjectsVisible = !config.sitsimObjectsVisible;
						UpdateVisibility (config.gameObject);
						if (config.inventioController != null) {
								UpdateVisibility (config.inventioController.gameObject);
						}
						if (config.localizationController != null) {
								UpdateVisibility (config.localizationController.gameObject);
						}
						if (config.player != null) {
								UpdateVisibility (config.player);
						}

						EditorApplication.DirtyHierarchyWindowSorting ();
				}
		}

		private static void UpdateVisibility(GameObject forObject) {
				SitSimVisibilityController visibilityController = forObject.GetComponent<SitSimVisibilityController> ();
				if (visibilityController == null) {
						visibilityController = forObject.AddComponent<SitSimVisibilityController> ();
				}
				visibilityController.SetHideFlags (Visibility);
		}

		public static HideFlags Visibility {
				get {
						SitSimConfiguration config = SitSimConfiguration.SharedInstance;
						if (config != null) {
								return (config.sitsimObjectsVisible ? HideFlags.None : HideFlags.HideInHierarchy);
						}
						return HideFlags.None;
				}
		}


		/*
		 * Editor Icons
		 */
		private static Texture sitSimEditorIcon = null;
		public static Texture SitSimEditorIcon() {
				if (sitSimEditorIcon == null) {
						Texture asset = AssetDatabase.LoadAssetAtPath("Assets/SitSimEditor/Textures/SitSimEditorIcon.png", typeof(Texture)) as Texture;
						if (asset != null) {
								sitSimEditorIcon = asset;
						}
				}
				return sitSimEditorIcon;
		}

		private static Texture sitSimBalloonEditorIcon = null;
		public static Texture SitSimBalloonEditorIcon() {
				if (sitSimBalloonEditorIcon == null) {
						Texture asset = AssetDatabase.LoadAssetAtPath("Assets/SitSimEditor/Textures/SitSimBalloonEditorIcon.png", typeof(Texture)) as Texture;
						if (asset != null) {
								sitSimBalloonEditorIcon = asset;
						}
				}
				return sitSimBalloonEditorIcon;
		}

		private static Texture validationOkIcon = null;
		public static Texture ValidationOkIcon() {
				if (validationOkIcon == null) {
						Texture asset = AssetDatabase.LoadAssetAtPath("Assets/SitSimEditor/Textures/ValidationOkIcon.png", typeof(Texture)) as Texture;
						if (asset != null) {
								validationOkIcon = asset;
						}
				}
				return validationOkIcon;
		}

		private static Texture validationFailIcon = null;
		public static Texture ValidationFailIcon() {
				if (validationFailIcon == null) {
						Texture asset = AssetDatabase.LoadAssetAtPath("Assets/SitSimEditor/Textures/ValidationFailIcon.png", typeof(Texture)) as Texture;
						if (asset != null) {
								validationFailIcon = asset;
						}
				}
				return validationFailIcon;
		}

		/*
		 * Preparing Core SitSim Functionality
		 */
		public void PrepareCoreSitSimObjects() {
				PrepareInventioController ();
				PrepareLocalizationController ();
				PreparePlayer ();
				PrepareVisualRootObject ();
				PrepareBalloons ();

				EditorSceneManager.MarkSceneDirty (EditorSceneManager.GetActiveScene ());
		}

		void PrepareInventioController() {
				if (inventioController == null) {
						// Find it in the secene
						inventioController = FindObjectOfType<InventioController>();
				}

				if (inventioController == null) {
						// Add it to the scene
						InventioController prefab = AssetDatabase.LoadAssetAtPath("Assets/InventioBase/Prefabs/InventioController.prefab", typeof(InventioController)) as InventioController;
						if (prefab != null) {
								inventioController = PrefabUtility.InstantiatePrefab (prefab) as InventioController;
								Undo.RecordObject (inventioController, "Prepare Inventio Controller");
								inventioController.name = "InventioController";
								inventioController.appName = PlayerSettings.productName;
								inventioController.requestLocationUse = true;
								inventioController.birdPerspectiveRange = new Vector2 (10.0f, 20.0f);
						}
						else {
								// TODO: Show error dialog
						}
				}

				if (inventioController == null) {
						Debug.LogWarning ("NO INVENTIO CONTROLLER!");
				}
				else {
						inventioController.gameObject.hideFlags = Visibility;
				}
		}

		void PrepareLocalizationController() {
				if (localizationController == null) {
						// Find it in the secene
						localizationController = FindObjectOfType<LocalizationController_dfGUI>();
				}

				if (localizationController == null) {
						// Add it to the scene
						LocalizationController_dfGUI prefab = AssetDatabase.LoadAssetAtPath("Assets/InventioBase/Prefabs/LocalizationController dfGUI.prefab", typeof(LocalizationController_dfGUI)) as LocalizationController_dfGUI;
						if (prefab != null) {
								localizationController = PrefabUtility.InstantiatePrefab (prefab) as LocalizationController_dfGUI;
								Undo.RecordObject (localizationController, "Prepare Localization Controller");
								localizationController.name = "LocalizationController";
						}
						else {
								Debug.Log ("Failed adding LocalizationController");
								// TODO: Show error dialog
						}
				}

				if (localizationController == null) {
						Debug.LogWarning ("NO LOCALIZATION CONTROLLER!");
				}
				else {
						localizationController.gameObject.hideFlags = Visibility;
				}
		}

		void PreparePlayer() {
				if (player == null) {
						// Find it in the scene
						PlayerMoveScript moveScript = FindObjectOfType<PlayerMoveScript>();
						if (moveScript != null) {
								player = moveScript.gameObject;
						}
				}

				if (player == null) {
						// Add it to the scene
						GameObject prefab = AssetDatabase.LoadAssetAtPath("Assets/InventioBase/Prefabs/Player.prefab", typeof(GameObject)) as GameObject;
						if (prefab != null) {
								player = PrefabUtility.InstantiatePrefab (prefab) as GameObject;
								Undo.RecordObject (player, "Prepare Player");
								player.name = "Player";
						}
						else {
								Debug.Log ("Failed adding Player");
								// TODO: Show error dialog
						}

						// Remove Main Camera if any (added automatically when creating a new scene
						Camera[] cameras = FindObjectsOfType<Camera>();
						foreach (Camera cam in cameras) {
								if (cam.gameObject.name.Equals("Main Camera") && (cam.transform.parent == null)) {
										DestroyImmediate (cam.gameObject);
										break;
								}
						}
				}

				if (player != null) {
						player.hideFlags = Visibility;
						player.AddComponent<SitSimStayOnGround>();
				}
				else {
						Debug.LogWarning ("NO PLAYER!");
				}
		}

		void PrepareVisualRootObject() {
				if (visualRoot == null) {
						// Find it in the scene
						GameObject[] allObjects = FindObjectsOfType<GameObject>();
						foreach (GameObject candidateObject in allObjects) {
								if (candidateObject.name.Equals("Visual Root") && (candidateObject.transform.parent == null)) {
										visualRoot = candidateObject;
										break;
								}
						}
				}

				if (visualRoot == null) {
						// Add it to the scene
						visualRoot = new GameObject("Visual Root");
				}
		}

		void PrepareBalloons() {
				if (balloons == null) {
						// Find it in the scene
						GameObject[] allObjects = FindObjectsOfType<GameObject>();
						foreach (GameObject candidateObject in allObjects) {
								if (candidateObject.name.Equals("Balloons") && (candidateObject.transform.parent == visualRoot.transform)) {
										balloons = candidateObject;
										break;
								}
						}
				}

				if (balloons == null) {
						// Add it to the scene
						balloons = new GameObject("Balloons");
						Undo.RecordObject (balloons, "Prepare balloons");
						balloons.transform.parent = visualRoot.transform;
				}
		}

		public void CreateReferenceObject() {
				if (referenceObject == null) {
						// Find it in the secene
						GameObject[] allObjects = FindObjectsOfType<GameObject>();
						foreach (GameObject candidateObject in allObjects) {
								if (candidateObject.name.Equals("ReferenceObject") && (candidateObject.transform.parent == null)) {
										referenceObject = candidateObject;
										break;
								}
						}
				}

				if (referenceObject == null) {
						// Add it to the scene
						GameObject prefab = AssetDatabase.LoadAssetAtPath ("Assets/SitSimEditor/SitSimReferenceObject.prefab", typeof(GameObject)) as GameObject;
						if (prefab != null) {
								referenceObject = PrefabUtility.InstantiatePrefab (prefab) as GameObject;
								referenceObject.name = "ReferenceObject";
						} else {
								Debug.Log ("Failed adding ReferenceObject");
								// TODO: Show error dialog
						}
				}

				if (referenceObject == null) {
						Debug.LogWarning ("NO REFERENCE OBJECT!");
				}
				else {
						ReferenceObjectLength = DEFAULT_REFERENCE_LENGTH;
						referenceObject.transform.GetChild (0).GetComponent<Renderer> ().enabled = false;
						referenceObject.hideFlags = Visibility;
				}

		}
				
		public void RemoveReferenceObject() {
				if (referenceObject != null) {
						GameObject.DestroyImmediate (referenceObject);
				}
				ReferenceObjectLength = DEFAULT_REFERENCE_LENGTH;
				referenceObject = null;
		}

		/*
		 * Adding and adjusting terrain
		 */
		public void AddTerrainObject() {
				terrain = PrefabUtility.InstantiatePrefab(terrainAsset) as GameObject;
				Undo.RecordObject (terrain, "Add Terrain");
				terrain.transform.parent = visualRoot.transform;
				terrain.name = "Terrain";
		}

		public void RemoveTerrainObject() {
				if (terrain != null) {
						GameObject.DestroyImmediate (terrain);
						terrain = null;
				}
		}

		/*
		 * This method will change the scale factor of the imported assets to the one set on the
		 * terrain object, reset the scale of the terrain object to one, and move the balloons 
		 * into their proper scaled position.
		 */
		public bool ConfirmVisualsScale() {
				bool completed = false;
				float scale = terrain.transform.localScale.x;
				string assetPath = AssetDatabase.GetAssetPath (terrainAsset);
				ModelImporter importer = AssetImporter.GetAtPath (assetPath) as ModelImporter;
				if (importer != null) {
						if (EditorUtility.DisplayDialog ("Update terrain asset import settings?", "This operation will update the import settings of the terrain asset and trigger a re-import which may take a while.", "Update", "Don't update")) {
								importer.globalScale *= scale;
								importer.SaveAndReimport ();
								AssetDatabase.Refresh ();
								Undo.RecordObject (terrain, "Change Terrain Scale");
								terrain.transform.localScale = Vector3.one;

								for (int idx = 0; idx < balloons.transform.childCount; ++idx) {
										Transform t = balloons.transform.GetChild (idx);
										Undo.RecordObject (t, "Adapt balloon to terrain scale");
										t.localPosition *= scale;
								}

								completed = true;
						}
				}
				return completed;
		}

		/*
		 * This method will move the visualRoot to the same position as origo (excep Y coordinate which is unchanged),
		 * it will also remove the origo object.
		 */
		public bool ConfirmOrigo() {
				bool completed = false;
				Vector3 position = origo.transform.position;
				position.y = visualRoot.transform.position.y;

				if (EditorUtility.DisplayDialog ("Change origin?", "This operation will move the terrain and balloon to align with the origin.", "Change origin", "Abort")) {
						Undo.RecordObject (visualRoot, "Change Origo");
						visualRoot.transform.position -= position;

						GameObject.DestroyImmediate (origo);
						origo = null;
						completed = true;
				}
				return completed;
		}
}
#endif