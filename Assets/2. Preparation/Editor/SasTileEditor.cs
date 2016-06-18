using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System;

namespace Sas.Edit
{
	public class SpawnWindow : EditorWindow {

		public int mSelSpawner = 0;
		private int mSelLevel = 0, mSelItem = 0;

		public SasLayerActor.SerializableData data { get; set; }


		private void Awake()
		{
		}

		private void OnSelectionChange()
		{
			this.Close ();
		}

		private void OnGUI()
		{
			if (null == data)
				return;

			EditorGUIUtility.labelWidth = 65.0f;
			var spawners = data.spawners;

			data.height = EditorGUILayout.FloatField ("Height", data.height);
			data.timeSpawn = EditorGUILayout.FloatField ("Spawn time", data.timeSpawn);

			data.capacity = EditorGUILayout.IntField ("Capacity", data.capacity);

			{
				EditorGUILayout.BeginHorizontal ();
				int index = 0;
				mSelSpawner = EditorGUILayout.Popup( 
					"Spawners", mSelSpawner, 
					spawners.ConvertAll(val => string.Format("{0}:Resource{1}", ++index, val.res)).ToArray()
				);

				if (GUILayout.Button ("<<")) {
					mSelSpawner = spawners.Count;
					spawners.Add ( new SasActorSpawner.SerializableData {
						levels = SasPool<List<SasActorSpawner.Ratio>>.Instance.Alloc(),
						items = SasPool<List<SasActorSpawner.Ratio>>.Instance.Alloc()
						});
				}

				if (GUILayout.Button (">>") && mSelSpawner < spawners.Count)
					spawners.RemoveAt (mSelSpawner);
				EditorGUILayout.EndHorizontal ();
			}


			var spawner = mSelSpawner < spawners.Count ? spawners [mSelSpawner] : null;
			if (null == spawner)
				return;

			SasActorSpawner.Ratio selItem = mSelItem < spawner.items.Count ? spawner.items [mSelItem] : null;
			SasActorSpawner.Ratio selLevel = mSelLevel < spawner.levels.Count ? spawner.levels [mSelLevel] : null;

			spawner.res = (ushort)EditorGUILayout.IntField ("Resource", (int)spawner.res);
			spawner.ratio = EditorGUILayout.Slider ("Ratio", spawner.ratio, 0.0f, 1.0f);
			spawner.default_level = (ushort)EditorGUILayout.IntField ("Level", (int)spawner.default_level);

			{
				// Levels
				EditorGUILayout.BeginHorizontal ();
				int index = 0;
				mSelLevel = EditorGUILayout.Popup (
					"Levels", mSelLevel, 
					spawner.levels.ConvertAll (
						val => string.Format ("{0}:level:{1}, ratio:{2}", ++index, val.idx, val.ratio)).ToArray ()
				);

				{
					EditorGUILayout.BeginVertical ();
					{
						EditorGUILayout.BeginHorizontal ();
						if (GUILayout.Button ("<<")) {
							mSelLevel = spawner.levels.Count;
							spawner.levels.Add (SasPool<SasActorSpawner.Ratio>.Instance.Alloc ());
						}
						if (GUILayout.Button (">>") && mSelLevel < spawner.levels.Count)
							spawner.levels.Remove (spawner.levels [mSelLevel]);
						EditorGUILayout.EndHorizontal ();
					}

					if (null != selLevel) {
						selLevel.idx = (ushort)EditorGUILayout.IntField ("Level", (int)selLevel.idx);
						selLevel.ratio = EditorGUILayout.Slider ("Ratio", selLevel.ratio, 0.0f, 1.0f);
					}
					EditorGUILayout.EndVertical ();
				}
				EditorGUILayout.EndHorizontal ();
			}

			{
				// Items
				EditorGUILayout.BeginHorizontal ();
				int index = 0;
				mSelItem = EditorGUILayout.Popup (
					"Items",mSelItem, 
					spawner.items.ConvertAll (
						val => string.Format ("{0}:level:{1}, ratio:{2}, size:{3}", ++index, val.idx, val.ratio, val.size)).ToArray ()
				);

				{
					EditorGUILayout.BeginVertical ();
					{
						EditorGUILayout.BeginHorizontal ();
						if (GUILayout.Button ("<<")) {
							mSelItem = spawner.items.Count;
							spawner.items.Add (SasPool<SasActorSpawner.Ratio>.Instance.Alloc ());
						}
						if (GUILayout.Button (">>") && mSelItem < spawner.items.Count)
							spawner.items.Remove (spawner.items [mSelItem]);
						EditorGUILayout.EndHorizontal ();
					}

					if (null != selItem) {
						selItem.idx = (ushort)EditorGUILayout.IntField ("Item", (int)selItem.idx);
						selItem.ratio = EditorGUILayout.Slider ("Ratio", selItem.ratio, 0.0f, 1.0f);
						selItem.size = EditorGUILayout.IntField ("Count", selItem.size);
					}

					EditorGUILayout.EndVertical ();
				}
			}
			EditorGUILayout.EndHorizontal ();
		}
	}
}
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
