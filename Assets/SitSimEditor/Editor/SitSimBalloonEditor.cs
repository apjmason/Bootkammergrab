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
using UnityEngine;
using UnityEditor;
using InventioBase;

public class SitSimBalloonEditor : EditorWindow
{
    SitSimConfiguration mSitSimConfiguration = null;
    Vector2 mScrollPosition;

    void OnEnable()
    {
        mSitSimConfiguration = GetSimSimConfiguration();
    }

    void OnFocus()
    {
        mSitSimConfiguration = GetSimSimConfiguration();
    }

    SitSimConfiguration GetSimSimConfiguration()
    {
        SitSimConfiguration configuration = mSitSimConfiguration;
        if (configuration == null)
        {
            // Find it in the secene
            configuration = FindObjectOfType<SitSimConfiguration>();
        }

        if (configuration == null)
        {
            Debug.LogWarning("NO SITSIM AR CONFIGURATION!");
        }

        return configuration;
    }

    public static void ShowWindow()
    {
        SitSimBalloonEditor window = (SitSimBalloonEditor)EditorWindow.GetWindow(typeof(SitSimBalloonEditor), false, "Sitsim AR Balloon Editor");
        window.Show();
    }

    public static bool AddBalloon(GameObject parentObject)
    {
        bool success = false;
        if (parentObject != null)
        {
            //BalloonController prefab = AssetDatabase.LoadAssetAtPath("Assets/InventioBase/Prefabs/Balloon.prefab", typeof(BalloonController)) as BalloonController;
            GameObject prefab = AssetDatabase.LoadAssetAtPath("Assets/InventioBase/Prefabs/Balloon.prefab", typeof(GameObject)) as GameObject;
            if (prefab != null)
            {
                Undo.RegisterFullObjectHierarchyUndo(parentObject, "Add Balloon");

                GameObject balloonGameObject = Instantiate(prefab, parentObject.transform);
                Undo.RegisterCreatedObjectUndo(balloonGameObject, "Add Balloon");

                Undo.RecordObject(balloonGameObject, "Add Balloon");
                balloonGameObject.name = "Balloon";
                SitSimBalloonConfiguration balloonConfiguration = balloonGameObject.AddComponent<SitSimBalloonConfiguration>();
                SitSimStayOnGround balloonStayOnGround = balloonGameObject.AddComponent<SitSimStayOnGround>();
                balloonStayOnGround.offset = 1.2f; // Pole is 1.2m

                SitSimEditor.FocusTopDownOnTransform(balloonGameObject.transform);
                success = true;
            }
        }
        else
        {
            Debug.Log("AddBalloon needs parent!!!");
        }
        return success;
    }


    /*
     * GUI Updates
     */
    void OnGUI()
    {
        if (EditorApplication.isPlaying)
        {
            OnGUIIsPlaying();
            return;
        }

        mSitSimConfiguration = SitSimConfiguration.SharedInstance;

        OnGUIHeader();

        if (mSitSimConfiguration == null)
        {
            GUILayout.Label("Sitsim AR Balloon Editor failed to find Sitsim AR scene configuration data storage, please prepare the Sitsim using the Sitsim AR Editor.");
            if (GUILayout.Button("Open Sitsim AR Editor"))
            {
                SitSimEditor.ShowWindow();
                Close();
            }
            return;
        }

        if (mSitSimConfiguration.balloons == null)
        {
            GUILayout.Label("Sitsim AR scene configuration data storage has no balloon container, please contact vendor for help.");
            return;
        }

        mScrollPosition = EditorGUILayout.BeginScrollView(mScrollPosition);
        if (mSitSimConfiguration.balloons.transform.childCount == 0)
        {
            GUILayout.Label("No balloons added to the scene.");
        }
        for (int balloonIndex = 0; balloonIndex < mSitSimConfiguration.balloons.transform.childCount; ++balloonIndex)
        {
            Transform balloonTransform = mSitSimConfiguration.balloons.transform.GetChild(balloonIndex);
            BalloonController balloon = balloonTransform.GetComponent<BalloonController>();
            if (balloon != null)
            {
                EditorGUILayout.Space();
                OnGUIBalloonSettings(balloon);
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Add balloon", GUILayout.MinWidth(100), GUILayout.MinHeight(40)))
        {
            AddBalloon(mSitSimConfiguration.balloons);
            mScrollPosition.y += 9999f;
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();

    }

    void OnGUIIsPlaying()
    {
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Sitsim AR Balloon Editor is disabled while playing");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
    }

    void OnGUIHeader()
    {
        GUILayout.BeginVertical("box");
        GUILayout.Box(SitSimConfiguration.SitSimBalloonEditorIcon(), GUILayout.ExpandWidth(true));
        GUILayout.EndVertical();
    }


    void OnGUIBalloonSettings(BalloonController balloon)
    {
        SitSimBalloonConfiguration balloonConfiguration = balloon.GetComponent<SitSimBalloonConfiguration>();
        if (balloonConfiguration == null)
        {
            balloonConfiguration = balloon.gameObject.AddComponent<SitSimBalloonConfiguration>();
        }
        SitSimStayOnGround balloonStayOnGround = balloon.GetComponent<SitSimStayOnGround>();
        if (balloonStayOnGround == null)
        {
            balloonStayOnGround = balloon.gameObject.AddComponent<SitSimStayOnGround>();
        }
        balloonStayOnGround.offset = 1.2f; // Pole is 1.2m

        GUILayout.BeginVertical("box");
        GUILayout.BeginHorizontal("box");
        balloonConfiguration.showInSitSimBalloonEditor = EditorGUILayout.Foldout(balloonConfiguration.showInSitSimBalloonEditor, balloon.name);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Select"))
        {
            SitSimEditor.FocusTopDownOnTransform(balloon.transform);
        }
        GUILayout.EndHorizontal();
        if (balloonConfiguration.showInSitSimBalloonEditor)
        {
            EditorGUI.indentLevel++;

            string name = TextField("Name", balloon.name);
            if (name != null)
            {
                Undo.RecordObject(balloon, "Balloon name");
                balloon.name = name;
            }

            string label = TextField("Label", balloon.balloonName);
            if (label != null)
            {
                Undo.RecordObject(balloon, "Balloon label");
                balloon.balloonName = label;
                balloon.Invoke("OnValidate", 0);
            }

            Object detailObject = EditorGUILayout.ObjectField("Detail Object", balloon.detailObject, typeof(GameObject), true, null);
            if ((detailObject == null) || !detailObject.Equals(balloon.detailObject))
            {
                Undo.RecordObject(balloon, "Balloon detail object");
                balloon.detailObject = detailObject as GameObject;
            }

            Object detailObjectClean = EditorGUILayout.ObjectField("Detail Object (Clean)", balloon.detailObjectClean, typeof(GameObject), true, null);
            if ((detailObjectClean == null) || !detailObjectClean.Equals(balloon.detailObjectClean))
            {
                Undo.RecordObject(balloon, "Balloon detail object (clean)");
                balloon.detailObjectClean = detailObjectClean as GameObject;
            }

            Object detailAudio = EditorGUILayout.ObjectField("Detail Audio", balloon.detailAudio, typeof(AudioClip), true, null);
            if ((detailAudio == null) || !detailAudio.Equals(balloon.detailAudio)) {
                Undo.RecordObject(balloon, "Balloon detail audio");
                balloon.detailAudio = detailAudio as AudioClip;
            }

            string detailWeb = TextField("Detail Web", balloon.detailWeb);
            if (detailWeb != null)
            {
                Undo.RecordObject(balloon, "Balloon web detail");
                balloon.detailWeb = detailWeb;
            }

            SerializedObject serializedObject = new SerializedObject(balloon);
            SerializedProperty serializedProperty = serializedObject.FindProperty("onActivate");
            EditorGUILayout.PropertyField(serializedProperty);
            serializedObject.ApplyModifiedProperties();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Delete"))
            {
                if (EditorUtility.DisplayDialog("Delete?", "Delete balloon with name " + balloon.name + "?", "Delete", "Cancel"))
                {
                    GameObject.DestroyImmediate(balloon.gameObject);
                    Repaint();
                }
            }
            GUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
        }
        GUILayout.EndVertical();
    }

    /*
     * Helper functions
     */

    /*
     * TextField
     * Returns a new value only if different from provided value
     */
    string TextField(string label, string value)
    {
        string result = EditorGUILayout.TextField(label, value);
        if (result.Equals(value))
        {
            result = null;
        }
        return result;
    }
}
