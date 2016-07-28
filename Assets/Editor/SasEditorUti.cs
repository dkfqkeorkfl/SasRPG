using UnityEngine;
using System.Collections;
using UnityEditor;
using System;

namespace Sas.Edit
{
	public static class EditorUti {

		public static readonly GUIStyle GUI_CENTER = new GUIStyle("label");

		static EditorUti()
		{
			GUI_CENTER.alignment = TextAnchor.MiddleCenter;
		}


		public static void PropertyField(string label, SerializedObject obj, string serialized, GUILayoutOption option = null)
		{
			SerializedProperty prop = obj.FindProperty (serialized);
			if (prop == null)
				return;
			
			EditorGUILayout.PropertyField (prop, new GUIContent(label));
		}

		public static void DrawGroup(string label, Action a)
		{
			GUILayoutUtility.BeginGroup(label);
			a();
			GUILayoutUtility.EndGroup(label);
		}
	}

	public static class EditorExt
	{
		public static void GroupContents( string label, bool disabled, Action action)
		{
			if(EditorGUILayout.BeginToggleGroup (label, disabled) ) 
				action ();
			EditorGUILayout.EndToggleGroup ();
		}
	}

	
}
