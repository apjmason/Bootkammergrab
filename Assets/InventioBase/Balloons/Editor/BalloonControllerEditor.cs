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
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(BalloonController))]
[CanEditMultipleObjects]
public class BalloonControllerEditor : Editor
{
	SerializedProperty balloonNameProperty;
	SerializedProperty detailImageProperty;
	SerializedProperty detailObjectProperty;
	SerializedProperty detailObjectCleanProperty;
	SerializedProperty detailAudioProperty;
	SerializedProperty detailTextProperty;
	SerializedProperty detailWebProperty;
	SerializedProperty detailPDFProperty;
	SerializedProperty quizBalloonProperty;
	SerializedProperty allowCloseProperty;
    SerializedProperty onActivateProperty;

	public void OnEnable()
	{
		balloonNameProperty = serializedObject.FindProperty ("balloonName");
		detailImageProperty = serializedObject.FindProperty ("detailImage");
		detailObjectProperty = serializedObject.FindProperty ("detailObject");
		detailObjectCleanProperty = serializedObject.FindProperty ("detailObjectClean");
		detailAudioProperty = serializedObject.FindProperty ("detailAudio");
		detailTextProperty = serializedObject.FindProperty ("detailText");
		detailWebProperty = serializedObject.FindProperty ("detailWeb");
		detailPDFProperty = serializedObject.FindProperty ("detailPDF");
		quizBalloonProperty = serializedObject.FindProperty ("quizBalloon");
		allowCloseProperty = serializedObject.FindProperty ("allowClose");
        onActivateProperty = serializedObject.FindProperty("onActivate");
    }

	public override void OnInspectorGUI()
	{
		serializedObject.Update ();

		EditorGUILayout.Space ();
		balloonNameProperty.stringValue = EditorGUILayout.TextField ("Balloon Name", balloonNameProperty.stringValue);
		EditorGUILayout.Space ();
		EditorGUILayout.HelpBox ("\t Chained and Parallel details:\n" +
			"\n" +
			"\t detailImage|detailAudio\n" +
			"\t detailObject|detailAudio\n" +
		    "\t detailObjectClean|detailAudio\n" +
		    "\t detailText|detailAudio\n" +
			"\t detailImage -> detailWeb|detailPDF\n" +
			"\t detailObject -> detailWeb|detailPDF\n" +
			"\t detailObjectClean -> detailWeb|detailPDF\n" +
			"\t detailText -> detailWeb|detailPDF\n" +
			"\t detailAudio -> detailWeb|detailPDF\n" +
			"\t detailWeb\n" +
			"\t detailPDF\n" +
			"\t +\n" +
			"\t balloonObservers", 
			MessageType.None);
		EditorGUILayout.Space ();

		detailImageProperty.objectReferenceValue = EditorGUILayout.ObjectField ("Detail Image", detailImageProperty.objectReferenceValue, typeof(Texture2D), false);
		detailObjectProperty.objectReferenceValue = EditorGUILayout.ObjectField ("Detail Object", detailObjectProperty.objectReferenceValue, typeof(GameObject), false);
		detailObjectCleanProperty.objectReferenceValue = EditorGUILayout.ObjectField ("Detail Object (Clean)", detailObjectCleanProperty.objectReferenceValue, typeof(GameObject), false);
		detailAudioProperty.objectReferenceValue = EditorGUILayout.ObjectField ("Detail Audio", detailAudioProperty.objectReferenceValue, typeof(AudioClip), false);
		detailTextProperty.stringValue = EditorGUILayout.TextField ("Detail Text", detailTextProperty.stringValue);
		detailWebProperty.stringValue = EditorGUILayout.TextField ("Detail Web", detailWebProperty.stringValue);
		detailPDFProperty.stringValue = EditorGUILayout.TextField ("Detail PDF", detailPDFProperty.stringValue);
		quizBalloonProperty.boolValue = EditorGUILayout.Toggle ("Quiz Balloon", quizBalloonProperty.boolValue);
		allowCloseProperty.boolValue = EditorGUILayout.Toggle ("Allow Close", allowCloseProperty.boolValue);
        EditorGUILayout.PropertyField(onActivateProperty);

        serializedObject.ApplyModifiedProperties ();
	}
}
