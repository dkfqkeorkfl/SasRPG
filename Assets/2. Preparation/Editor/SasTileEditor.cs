//using UnityEngine;
//using System.Collections;
//using UnityEditor;
//using System;
//
//namespace Sas.Edit
//{
//	[CustomEditor(typeof (SasTile))]
//	public class SasTileEditor : Editor {
//
//		public enum eLength
//		{
//			Fixable,
//			Each
//		}
//			
//		private SasTile mTile = null;
//		private bool mEditMode = false;
//		private eLength mLengthType = eLength.Fixable;
//		private Vector2[,] mTileGui = null;
//
//		private void OnEnable()
//		{
//			mTile = target as SasTile;
//			mTileGui = new Vector2[mTile.size.x, mTile.size.y];
//		}
//
//		public override void OnInspectorGUI()
//		{
//			serializedObject.Update ();
//			//EditorGUILayout.
//			//GUILayoutUtility.BeginGroup(
//
//			Sas.Edit.EditorUti.PropertyField("Grid", serializedObject, "mIsGizmo");
//
//			mEditMode = EditorGUILayout.BeginToggleGroup ("Edit mode", mEditMode);
//
//			if (mEditMode) {
//				Sas.Edit.EditorUti.DrawGroup("Edit", OnEditMode);
//				//OnEditMode ();
//			}
//			EditorGUILayout.EndToggleGroup();
//			//EditorUti.GroupContents ("EditMode", mEditMode, new Action (OnEditMode));
//
//			//base.PropertyField ("Length", "mLength", options);
////			mEditMode = EditorGUILayout.ToggleLeft ("Edit", mEditMode, null);
////			if (EditorGUILayout.BeginToggleGroup ("Edit Mode", mEditMode)) {
////				GameObject group = new GameObject ();
////				group.name = "Editor";
////				group.transform.parent = mTile;
////
////				mTile.
////
////				EditorGUILayout.EndToggleGroup ();
////			} else {
////			}
//
//
//			serializedObject.ApplyModifiedProperties ();
//		}
//
//		public void OnEditMode()
//		{
//			Point size = mTile.size;
//			EditorGUIUtility.labelWidth = 40.0f;
//			//EditorGUIUtility.fieldWidth (4.0f);
//			EditorGUILayout.BeginHorizontal ();
//			size.x = EditorGUILayout.IntField ("Width", size.x);
//			size.y = EditorGUILayout.IntField ("Height", size.y);
//			EditorGUILayout.EndHorizontal ();
//
//			if (!mTile.size.EqualTo (size)) {
//				mTileGui = new Vector2[size.x, size.y];
//				mTile.size = size;
//			}
//			
//			mLengthType = (eLength)EditorGUILayout.EnumPopup ("Length Type", mLengthType);
//
//			if (mLengthType == eLength.Fixable) {
//				Vector2 length = mTile.length;
//				length = EditorGUILayout.Vector2Field ("Length", 
//					new Vector2 { x = length.x * mTile.size.x, y = length.y * mTile.size.y });
//
//				length.x /= mTile.size.x;
//				length.y /= mTile.size.y;
//				mTile.length = length;
//			} 
//			else {
//				mTile.length = EditorGUILayout.Vector2Field ("Length", mTile.length);
//			}
//
//		}
//			
//		public void OnSceneGUI()
//		{
//			if (!mEditMode)
//				return;
//
//			if (Event.current.isMouse && Event.current.delta != Vector2.zero && Event.current.button == 1) {
//				Vector2 posMouse = Event.current.mousePosition;
//
//				Point size = mTile.size;
//				for (int y = 0; y < size.y; ++y)
//					for (int x = 0; x < size.x; ++x) {
//						Vector3 pos = mTile.ConvertPointToPos (new Point{ x = x, y = y });
//						mTileGui [x, y] = HandleUtility.WorldToGUIPoint (pos);
//					}
//
//				//Debug.LogFormat ("Pos x:{0}, y:{1}", guiPosition.x, guiPosition.y);
//				//Ray ray = HandleUtility.GUIPointToWorldRay(guiPosition);
//				//HandleUtility.WorldToGUIPoint
//				//Vector3 standard = mTile.axis == SasTile.Axis.Y 
//				//ray.GetPoint(
//				//HandleUtility.WorldToGUIPoint()
//				//HandleUtility.WorldToGUIPoint()
//
//			}
//
//
//		}
//	} // end class
//
//} // end namespace
