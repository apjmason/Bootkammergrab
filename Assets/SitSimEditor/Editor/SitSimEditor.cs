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

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using InventioBase;

public class SitSimEditor : EditorWindow
{
    const string sSitSimEditorVersion = "0.6";

    const string sCameraUsageDescription = "Camera access is necessary for snapshot functionality";
    const string sLocationUsageDescription = "A Situated Simulation as this app requires access to the current location to work properly.";
    const string sMinimumiOSVersion = "9.1";

    SitSimConfiguration mSitSimConfiguration = null;

    const float sEditorWindowWidth = 450.0f;
    const float sEditorWindowMinHeight = 450.0f;
    bool mProjectPrepared = false;
    string mSigningTeamIdentifier = "";

    bool mSettingsValid = false;

    [MenuItem("Sitsim AR/Editor", false, 99)]
    public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
        Rect windowRect = new Rect(0, 0, sEditorWindowWidth, sEditorWindowMinHeight);
        EditorWindow window = EditorWindow.GetWindowWithRect(typeof(SitSimEditor), windowRect, false, "Sitsim AR Editor");
        window.minSize = new Vector2(sEditorWindowWidth, sEditorWindowMinHeight);
        window.maxSize = new Vector2(2 * sEditorWindowWidth, 2 * sEditorWindowMinHeight);
    }

    [MenuItem("Sitsim AR/Version " + sSitSimEditorVersion, true, 100)]
    static bool DummyVersionValidator() { return false; }
    [MenuItem("Sitsim AR/Version " + sSitSimEditorVersion, false, 100)]
    static void DummyVersion() { }

    [MenuItem("Sitsim AR/Advanced/Toggle Sitsim AR objects visibility", false, 120)]
    static void ToggleSitSimObjectsVisibility()
    {
        SitSimConfiguration.ToggleVisibility();
    }

    void OnEnable()
    {
        mSitSimConfiguration = SitSimConfiguration.SharedInstance;
    }

    void OnFocus()
    {
        mSitSimConfiguration = SitSimConfiguration.SharedInstance;
    }

    void OnInspectorUpdate()
    {
        // Validate player settings;
        BuildTarget activeBuildTarget = EditorUserBuildSettings.activeBuildTarget;
        int architecture = PlayerSettings.GetArchitecture(BuildTargetGroup.iOS);
        UIOrientation defaultOrientation = PlayerSettings.defaultInterfaceOrientation;
        iOSAppInBackgroundBehavior backgroundBehavior = PlayerSettings.iOS.appInBackgroundBehavior;
        bool automaticSigning = PlayerSettings.iOS.appleEnableAutomaticSigning;
        string cameraUsageDescription = PlayerSettings.iOS.cameraUsageDescription;
        string locationUsageDescription = PlayerSettings.iOS.locationUsageDescription;
        bool requiresFullScreen = PlayerSettings.iOS.requiresFullScreen;
        iOSSdkVersion sdkVersion = PlayerSettings.iOS.sdkVersion;
        iOSTargetDevice device = PlayerSettings.iOS.targetDevice;
        string osVersion = PlayerSettings.iOS.targetOSVersionString;

        bool projectPrepared = (
                (activeBuildTarget == BuildTarget.iOS) &&
                (architecture == 2) && // 2 = Universal
                (defaultOrientation == UIOrientation.LandscapeLeft) &&
                (backgroundBehavior == iOSAppInBackgroundBehavior.Exit) &&
                (automaticSigning) &&
                (cameraUsageDescription.Equals(sCameraUsageDescription)) &&
                (locationUsageDescription.Equals(sLocationUsageDescription)) &&
                (requiresFullScreen) &&
                (sdkVersion == iOSSdkVersion.DeviceSDK) &&
                (device == iOSTargetDevice.iPhoneAndiPad) &&
                (osVersion.Equals(sMinimumiOSVersion)));
        if (projectPrepared != mProjectPrepared)
        {
            mProjectPrepared = projectPrepared;
        }
        Repaint();
    }


    /*
     * GUI Updates
     */
    Vector2 scrollPosition;
    void OnGUI()
    {
        if (EditorApplication.isPlaying)
        {
            OnGUIIsPlaying();
            return;
        }

        mSitSimConfiguration = SitSimConfiguration.SharedInstance;

        if (mSitSimConfiguration == null)
        {
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Get started"))
            {
                mSitSimConfiguration = SitSimConfiguration.CreateInstance();
            }
        }

        if (mSitSimConfiguration == null)
        {
            GUILayout.Label("Sitsim AR Editor failed to prepare Sitsim scene configuration data storage, please contact vendor for help.");
            return;
        }

        if (!mProjectPrepared)
        {
            OnGUIPrepareConfiguration();
            return;
        }

        OnGUIHeader();

        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        OnGUIApplicationSettings();

        if (mProjectPrepared)
        {
            if (CoreSitSimObjectsAdded())
            {
                OnGUICoreSitSimSection();
                OnGUITerrainSection();
                OnGUIPlayerSection();
                OnGUIBalloonSection();
            }

            else
            {
                OnGUIAddCoreSitSimObjects();
            }
        }

        OnGUIBuildSection();
        GUILayout.EndScrollView();
    }

    void OnGUIIsPlaying()
    {
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Sitsim AR Editor is disabled while playing");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
    }

    void OnGUIPrepareConfiguration()
    {
        GUI.skin.label.wordWrap = true;

        EditorGUILayout.Space();
        GUILayout.Label("The project needs to be configured with default Sitsim settings, press the button below to get started.",
                GUILayout.Width(sEditorWindowWidth));
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Prepare Sitsim AR project"))
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
            PlayerSettings.SetArchitecture(BuildTargetGroup.iOS, 2); // 2 = Universal
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.iOS.appInBackgroundBehavior = iOSAppInBackgroundBehavior.Exit;
            PlayerSettings.iOS.appleEnableAutomaticSigning = true;
            PlayerSettings.iOS.cameraUsageDescription = sCameraUsageDescription;
            PlayerSettings.iOS.locationUsageDescription = sLocationUsageDescription;
            PlayerSettings.iOS.requiresFullScreen = true;
            PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
            PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
            PlayerSettings.iOS.targetOSVersionString = sMinimumiOSVersion;
            PlayerSettings.iOS.hideHomeButton = true;

            mProjectPrepared = true;
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    void OnGUIHeader()
    {
        GUILayout.BeginVertical("box");
        GUILayout.Box(SitSimConfiguration.SitSimEditorIcon(), GUILayout.ExpandWidth(true));

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Version " + sSitSimEditorVersion);
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
    }

    void OnGUIApplicationSettings()
    {
        string applicationIdentifier = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS);

        GUILayout.BeginVertical("box");
        mSitSimConfiguration.showApplicationSettings = OnGUIFoldoutValidated(mSitSimConfiguration.showApplicationSettings, "Application Settings", ValidApplicationSettings());

        if (mSitSimConfiguration.showApplicationSettings)
        {
            EditorGUI.indentLevel++;
            PlayerSettings.productName = EditorGUILayout.TextField("Application Name", PlayerSettings.productName);
            applicationIdentifier = EditorGUILayout.TextField("Bundle Identifier", applicationIdentifier);
            PlayerSettings.companyName = EditorGUILayout.TextField("Company/Organisation", PlayerSettings.companyName);
            PlayerSettings.iOS.appleDeveloperTeamID = EditorGUILayout.TextField("Signing Team Identifier", PlayerSettings.iOS.appleDeveloperTeamID);
            EditorGUI.indentLevel--;
        }
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, applicationIdentifier);

        UpdateInventioController();
        GUILayout.EndVertical();
    }

    void OnGUIAddCoreSitSimObjects()
    {
        EditorGUILayout.Space();
        if (GUILayout.Button("Add Core Sitsim AR objects"))
        {
            mSitSimConfiguration.PrepareCoreSitSimObjects();
        }
    }

    void OnGUICoreSitSimSection()
    {
        EditorGUILayout.Space();

        GUILayout.BeginVertical("box");
        mSitSimConfiguration.showCoreSitSimSettings = OnGUIFoldoutValidated(mSitSimConfiguration.showCoreSitSimSettings, "Core Sitsim AR Settings", CoreSitSimConfigured());

        if (mSitSimConfiguration.showCoreSitSimSettings && (mSitSimConfiguration.inventioController != null))
        {
            EditorGUI.indentLevel++;
            double longitude = EditorGUILayout.DoubleField("Longitude", mSitSimConfiguration.inventioController.longitude);
            if (!Mathf.Approximately((float)longitude, (float)mSitSimConfiguration.inventioController.longitude))
            {
                Undo.RecordObject(mSitSimConfiguration.inventioController, "Longitude");
                mSitSimConfiguration.inventioController.longitude = longitude;
            }
            double latitude = EditorGUILayout.DoubleField("Latitude", mSitSimConfiguration.inventioController.latitude);
            if (!Mathf.Approximately((float)latitude, (float)mSitSimConfiguration.inventioController.latitude))
            {
                Undo.RecordObject(mSitSimConfiguration.inventioController, "Latitude");
                mSitSimConfiguration.inventioController.latitude = latitude;
            }
            EditorGUI.indentLevel--;
        }
        GUILayout.EndVertical();
    }

    void OnGUITerrainSection()
    {
        EditorGUILayout.Space();

        GUILayout.BeginVertical("box");
        mSitSimConfiguration.showTerrainSettings = OnGUIFoldoutValidated(mSitSimConfiguration.showTerrainSettings, "Terrain Settings", TerrainConfigured());

        if (mSitSimConfiguration.showTerrainSettings)
        {
            EditorGUI.indentLevel++;
            if (mSitSimConfiguration.terrain != null)
            {
                // Add object to compare scale and orientation
                if (mSitSimConfiguration.referenceObject != null)
                {
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Comparison object");
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Select"))
                    {
                        Selection.activeTransform = mSitSimConfiguration.referenceObject.transform;
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUI.indentLevel++;
                    mSitSimConfiguration.ReferenceObjectLength = EditorGUILayout.Slider("Length (m)", mSitSimConfiguration.ReferenceObjectLength, 10f, 200f);

                    Vector3 euler = mSitSimConfiguration.referenceObject.transform.eulerAngles;
                    euler.y = EditorGUILayout.Slider("Orientation", euler.y, 0f, 360f);
                    mSitSimConfiguration.referenceObject.transform.eulerAngles = euler;

                    Vector3 referencePosition = mSitSimConfiguration.referenceObject.transform.position;
                    referencePosition.y = EditorGUILayout.Slider("Altitude (m)", referencePosition.y, -50f, 200f);
                    mSitSimConfiguration.referenceObject.transform.position = referencePosition;

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Remove"))
                    {
                        mSitSimConfiguration.RemoveReferenceObject();
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUI.indentLevel--;

                    EditorGUILayout.LabelField("Terrain orientation and scale");
                    EditorGUI.indentLevel++;
                    mSitSimConfiguration.visualsScale = EditorGUILayout.Slider("Scale", mSitSimConfiguration.visualsScale, 0.1f, 200f);
                    mSitSimConfiguration.visualsOrientation = EditorGUILayout.Slider("Orientation", mSitSimConfiguration.visualsOrientation, 0f, 360f);
                    GUI.enabled = (mSitSimConfiguration.visualsScale != 1f);
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Confirm scale") && mSitSimConfiguration.ConfirmVisualsScale())
                    {
                        Repaint();
                    }
                    EditorGUILayout.EndHorizontal();
                    GUI.enabled = true;
                    EditorGUI.indentLevel--;
                    EditorGUILayout.EndVertical();
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Add reference object to resize or rotate"))
                    {
                        EditorUtility.DisplayDialog("Comparison object", "An object will be created in the scene to help find the correct scale of the terrain. " +
                                "Adjust the length and orientation of the comparison object in the Sitsim AR Editor window and move it around in the scene editor to place it in a suitable position for measurement. " +
                                "When the scale and orientation of the terrain asset has been configured, you can easily remove the comparison object using the Sitsim AR Editor window.", "Ok");
                        mSitSimConfiguration.CreateReferenceObject();
                        FocusTopDownOnTransform(mSitSimConfiguration.referenceObject.transform);
                    }
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical("box");
                if (mSitSimConfiguration.origo != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Origin");
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Select and Focus"))
                    {
                        FocusTopDownOnTransform(mSitSimConfiguration.origo.transform);
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUI.indentLevel++;
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Abort"))
                    {
                        GameObject.DestroyImmediate(mSitSimConfiguration.origo);
                        mSitSimConfiguration.origo = null;
                        Repaint();
                    }
                    GUI.enabled = (mSitSimConfiguration.origo != null) && (mSitSimConfiguration.origo.transform.position.magnitude > 0f);
                    if (GUILayout.Button("Confirm") && mSitSimConfiguration.ConfirmOrigo())
                    {
                        Repaint();
                    }
                    GUI.enabled = true;
                    EditorGUILayout.EndHorizontal();
                    EditorGUI.indentLevel--;
                }
                else
                {
                    EditorGUILayout.LabelField("Origin");
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Place origin"))
                    {
                        EditorUtility.DisplayDialog("Origin", "An object will be created in the scene, place that object where you want the origin. " +
                                "Tap Confirm in the Sitsim AR Editor window to apply your changes, or tap Abort to cancel the change and remove the origin object.", "Ok");
                        mSitSimConfiguration.origo = new GameObject("Origin");
                        FocusTopDownOnTransform(mSitSimConfiguration.origo.transform);
                        /*
                        * TODO: 
                        * Open a window/dialog that keeps updating SceneView.lastActiveSceneView
                        * according to above, until user closes window. 
                        * 
                        * Using notification temporarily
                        */
                        string notification = EditorGUILayout.TextField("Move the origin into desired location");
                        SceneView.lastActiveSceneView.ShowNotification(new GUIContent(notification));
                    }
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Drop terrain"))
                {
                    if (EditorUtility.DisplayDialog("Drop terrain?", "Are you sure you want to drop the connection to the terrain object? The terrain object will still be in the scene, but unknown to the Sitsim AR Editor", "Drop", "Cancel"))
                    {
                        mSitSimConfiguration.terrain = null;
                    }
                }
                if (GUILayout.Button("Remove terrain"))
                {
                    if (EditorUtility.DisplayDialog("Remove terrain?", "Are you sure you want to remove the terrain from the scene?", "Remove", "Cancel"))
                    {
                        mSitSimConfiguration.RemoveTerrainObject();
                    }
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                OnGUITerrainSectionNoAsset();
            }
            EditorGUI.indentLevel--;
        }
        GUILayout.EndVertical();
    }

    void OnGUITerrainSectionNoAsset()
    {
        // We need to select terrain asset
        GUILayout.BeginVertical();
        mSitSimConfiguration.terrainAsset = EditorGUILayout.ObjectField("Terrain Asset", mSitSimConfiguration.terrainAsset, typeof(GameObject), false, null) as GameObject;
        if (mSitSimConfiguration.terrainAsset != null)
        {
            string assetPath = AssetDatabase.GetAssetPath(mSitSimConfiguration.terrainAsset);
            ModelImporter importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            if (importer != null)
            {
                if (!importer.addCollider && GUILayout.Button("Add collider"))
                {
                    importer.addCollider = true;
                    importer.SaveAndReimport();
                    AssetDatabase.Refresh();
                }
            }

            EditorGUILayout.Space();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Add terrain to scene"))
            {
                mSitSimConfiguration.AddTerrainObject();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("- OR select terrain object -");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GameObject terrain = mSitSimConfiguration.terrain;
            terrain = EditorGUILayout.ObjectField("Terrain Object", terrain, typeof(GameObject), true, null) as GameObject;
            if (terrain != null)
            {
                if (!terrain.scene.IsValid())
                {
                    EditorUtility.DisplayDialog("Error!", "Select an object from the scene, not a prefab!", "Ok");
                }
                else if (terrain.transform.IsChildOf(mSitSimConfiguration.visualRoot.transform))
                {
                    mSitSimConfiguration.terrain = terrain;
                }
                else
                {
                    if (EditorUtility.DisplayDialog("Change terrain hierarchy?", "The terrain has to be a child object of VisualRoot, should we fix that?", "Fix", "Cancel"))
                    {
                        terrain.transform.parent = mSitSimConfiguration.visualRoot.transform;
                        mSitSimConfiguration.terrain = terrain;
                    }
                }
            }
        }
        GUILayout.EndVertical();
    }

    void OnGUIPlayerSection()
    {
        EditorGUILayout.Space();

        GUILayout.BeginVertical("box");
        mSitSimConfiguration.showPlayerSettings = OnGUIFoldout(mSitSimConfiguration.showPlayerSettings, "Player Settings");

        if (mSitSimConfiguration.showPlayerSettings && (mSitSimConfiguration.player != null))
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Set Player Position"))
            {
                FocusTopDownOnTransform(mSitSimConfiguration.player.transform);

                /*
                 * TODO: 
                 * Open a window/dialog that keeps updating SceneView.lastActiveSceneView
                 * according to above, until user closes window. 
                 * 
                 * Using notification temporarily
                 */
                string notification = EditorGUILayout.TextField("Move the player into desired location");
                SceneView.lastActiveSceneView.ShowNotification(new GUIContent(notification));
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
    }

    void OnGUIBalloonSection()
    {
        if (mSitSimConfiguration.balloons == null)
        {
            return;
        }
        EditorGUILayout.Space();
        GUILayout.BeginVertical("box");
        GUILayout.Label("Balloons", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        //				if (mSitSimConfiguration.balloons.transform.childCount > 0) {
        if (GUILayout.Button("Show Balloon Editor"))
        {
            SitSimBalloonEditor.ShowWindow();
        }
        //				}
        //				else {
        //						if (GUILayout.Button("Add Balloon") && SitSimBalloonEditor.AddBalloon(mSitSimConfiguration.balloons)) {
        //								SitSimBalloonEditor.ShowWindow ();
        //						}
        //				}
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    void OnGUIBuildSection()
    {
        EditorGUILayout.Space();

        bool projectReady = ProjectReadyForBuild();
        GUILayout.Label("Build Xcode project", EditorStyles.boldLabel);

        GUI.enabled = projectReady && !BuildPipeline.isBuildingPlayer;
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (projectReady)
        {
            GUILayout.Label("Configuration done, ready to build!");
        }
        else
        {
            GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
            style.normal.textColor = Color.red;
            GUILayout.Label("Configuration incomplete", style);
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Build & Run", GUILayout.MinWidth(100), GUILayout.MinHeight(40)))
        {
            BuildAndRunProject();
        }
        EditorGUILayout.Space();
        if (GUILayout.Button("Build", GUILayout.Width(100), GUILayout.Height(40)))
        {
            BuildProject();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        GUI.enabled = true;
    }


    /*
     * Core SitSim objects
     */
    void UpdateInventioController()
    {
        if (mSitSimConfiguration.inventioController != null)
        {
            mSitSimConfiguration.inventioController.appName = PlayerSettings.productName;
        }
    }


    /*
     * Project validation
     */
    bool ValidApplicationSettings()
    {
        return (
                PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS).Length > 0 &&
                PlayerSettings.productName.Length > 0 &&
                PlayerSettings.iOS.appleDeveloperTeamID.Length > 0);
    }

    bool CoreSitSimObjectsAdded()
    {
        return ((mSitSimConfiguration.inventioController != null) &&
                (mSitSimConfiguration.localizationController != null) &&
                (mSitSimConfiguration.player != null));
    }

    bool CoreSitSimConfigured()
    {
        bool configured = true;
        if ((mSitSimConfiguration.inventioController.latitude < -90.0) || (mSitSimConfiguration.inventioController.latitude > 90.0))
        {
            configured = false;
        }
        if ((mSitSimConfiguration.inventioController.longitude < -180.0) || (mSitSimConfiguration.inventioController.longitude > 180.0))
        {
            configured = false;
        }
        return configured;
    }

    bool TerrainConfigured()
    {
        return (mSitSimConfiguration.referenceObject == null) && (mSitSimConfiguration.origo == null) && (mSitSimConfiguration.terrain != null);
    }

    bool ProjectReadyForBuild()
    {
        bool projectReady = false;
        if (mProjectPrepared)
        {
            projectReady = ValidApplicationSettings() && CoreSitSimObjectsAdded() && CoreSitSimConfigured() && TerrainConfigured();
        }
        return projectReady;
    }


    /*
     * Build project
     */
    void BuildProject()
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new string[] { };
        buildPlayerOptions.locationPathName = PlayerSettings.productName;
        buildPlayerOptions.targetGroup = BuildTargetGroup.iOS;
        buildPlayerOptions.target = BuildTarget.iOS;
        /* 
         * egoX doesn't support BuildOptions.AcceptExternalModificationsToPlayer
         * Embedded libraries (SpatialManager and TrackerNG) are added multiple times,
         * once egoX is updated to support this, inclulde this option
         */
        buildPlayerOptions.options = BuildOptions.ShowBuiltPlayer;
        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }

    void BuildAndRunProject()
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new string[] { };
        buildPlayerOptions.locationPathName = PlayerSettings.productName;
        buildPlayerOptions.targetGroup = BuildTargetGroup.iOS;
        buildPlayerOptions.target = BuildTarget.iOS;
        /* 
         * egoX doesn't support BuildOptions.AcceptExternalModificationsToPlayer
         * Embedded libraries (SpatialManager and TrackerNG) are added multiple times,
         * once egoX is updated to support this, inclulde this option
         */
        buildPlayerOptions.options = BuildOptions.AutoRunPlayer;
        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }


    /*
     * Helpers
     */
    public static void FocusTopDownOnTransform(Transform transform)
    {
        Selection.activeTransform = transform;
        SceneView.lastActiveSceneView.orthographic = true;
        SceneView.lastActiveSceneView.pivot = transform.position;
        SceneView.lastActiveSceneView.rotation = Quaternion.Euler(new Vector3(90f, 0f, 0f));
        SceneView.lastActiveSceneView.Repaint();
        //	EditorUtility.FocusProjectWindow (); // TODO; FocusProjectWindow seems to trigger an error in Unity, skip this until investigated
    }

    private bool OnGUIFoldoutValidated(bool foldout, string text, bool valid)
    {
        Texture icon = SitSimConfiguration.ValidationFailIcon();
        if (valid)
        {
            icon = SitSimConfiguration.ValidationOkIcon();
        }
        GUIContent content = new GUIContent(text, icon);

        return EditorGUILayout.Foldout(foldout, content, true);
    }

    private bool OnGUIFoldout(bool foldout, string text)
    {
        return EditorGUILayout.Foldout(foldout, text, true);
    }

}
