using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.Text;

namespace Sas.Edit
{
	[CustomEditor(typeof (SasLayerRes))]
	public class SasLayerResEditor : Editor {

		private SasLayerRes mLayerRes = null;
		private bool mViewObj = true;

		private bool mPoolExpand = true;
		private readonly List<GameObject> mPoolObj = new List<GameObject>();

		private bool mDataEdit = false;
		private bool mDataExpand = true;
		private readonly SasLayerData mDataLayer = new SasLayerData();

		int  mIdxRes = 0;
		int  mIdxgroup;
		bool mIsBlock = false;


		private void OnEnable()
		{
			mLayerRes = target as SasLayerRes;
			mDataLayer.size = new Point{ x = 10, y = 10 };
			mPoolObj.Add (null);

			var posData = mDataLayer.posTile;
			int count = posData.count;
			var empty = posData;
			for (int i = 0; i < count; ++i) {
				Debug.LogFormat ("dir/access {0}/{1}", 
					posData.GetDir (i).ToString (), posData.GetPos (i,ref empty).ToString ());
			}
		}

		private void onDisable()
		{
		}

		public override void OnInspectorGUI()
		{
			EditorGUIUtility.labelWidth = 70.0f;

			mPoolExpand = EditorGUILayout.Foldout (mPoolExpand, "Pool");
			if (mPoolExpand) {
				int size = EditorGUILayout.IntField ("size", mPoolObj.Count);
				if( size >= 1 )
					mPoolObj.Resize (size, null);

				EditorGUI.indentLevel += 1;

				EditorGUI.BeginDisabledGroup (true);
				EditorGUILayout.ObjectField ( "obj 0", (Object)mPoolObj[0], typeof(GameObject) , false);
				EditorGUI.EndDisabledGroup ();
				for (int i = 1; i < mPoolObj.Count; ++i) {
					mPoolObj [i] = (GameObject)EditorGUILayout.ObjectField (
						string.Format ("obj {0}", i), 
						(Object)mPoolObj [i], 
						typeof(GameObject), false
					);
				}
				EditorGUI.indentLevel -= 1;
			}

			mDataExpand = EditorGUILayout.Foldout (mDataExpand, "Data");
			if (mDataExpand) {
				EditorGUI.indentLevel += 1;
				Point size = mDataLayer.size;

				EditorGUILayout.BeginHorizontal ();
				size.x = EditorGUILayout.IntField ("Width", size.x);
				size.y = EditorGUILayout.IntField ("Height", size.y);
				EditorGUILayout.EndHorizontal ();
				size.x = size.x < 1 ? 1 : size.x;
				size.y = size.y < 1 ? 1 : size.y;
				mDataLayer.size = size;

				mDataLayer.length = EditorGUILayout.Vector2Field ("Length", mDataLayer.length);

				{
					mDataEdit = EditorGUILayout.BeginToggleGroup("Data Edit", mDataEdit); 
					int[] idxs = System.Linq.Enumerable.Range (0, mPoolObj.Count).ToArray (mPoolObj.Count);
					mIdxgroup = EditorGUILayout.IntField ("group", mIdxgroup);
					{
						EditorGUILayout.BeginHorizontal ();
						mIdxRes = EditorGUILayout.IntPopup ("res", mIdxRes, System.Array.ConvertAll (idxs, (val) => val.ToString ()), idxs);
						mIsBlock = EditorGUILayout.Toggle ("block", mIsBlock);
						EditorGUILayout.EndHorizontal ();
					}
					EditorGUILayout.EndToggleGroup ();
				} 

				EditorGUI.indentLevel -= 1;
			}
		}

		public void OnSceneGUI()
		{
			if (!mDataEdit)
				return;

			Handles.BeginGUI();

			Rect rc = new Rect (0.0f, 0.0f, Screen.width, Screen.height);
			Handles.DrawSolidRectangleWithOutline (rc, Color.white, Color.white);

			Handles.color = Color.black;
			Point size = mDataLayer.size;
			float width = Screen.width / size.x;
			float height = (Screen.height-40.0f) / size.y;
			for (int x = 0; x < size.x; ++x)
				Handles.DrawLine (new Vector3 {x = x * width}, new Vector3 {x = x * width, y = Screen.height});
			for (int y = 0; y < size.y; ++y)
				Handles.DrawLine (new Vector3 {y= y * height}, new Vector3 {x = Screen.width, y = y * height});	

			Vector3 mouse = -Vector3.one;
			if (Event.current.isMouse && Event.current.button == 1 && Event.current.type == EventType.MouseUp) {
				mouse = Event.current.mousePosition;
			}


			for (int x = 0; x < size.x; ++x)
				for (int y = 0; y < size.y; ++y) {

					Point pt    = new Point { x = x, y = y };
					rc.min = new Vector2 ( x * width, y * height );
					rc.max = new Vector2 ( rc.min.x + width, rc.min.y + height );
					if (rc.Contains (mouse)) {
						mDataLayer.SetBlock (pt, mIsBlock);
						mDataLayer.SetIdxGroup (pt, mIdxgroup);
						mDataLayer.SetIdxRes (pt, mIdxRes);
					}

					if (mDataLayer.tile [x, y] == 0)
						continue;
					
					int   res   = mDataLayer.GetIdxRes (pt);
					int   group = mDataLayer.GetIdxGroup (pt);
					bool  block = mDataLayer.IsBlock (pt);

					StringBuilder b = new StringBuilder();
					b.Append (res);
					b.Append (" / ").Append (group);
					b.Append ("\n").Append (block);


					GUI.Label(rc, b.ToString(), EditorUti.GUI_CENTER);
				}
			Handles.EndGUI();
		}

		private void ActiveObj(bool enable)
		{
			if (mLayerRes.objs == null)
				return;

			foreach (var obj in mLayerRes.objs) {
				obj.SetActive (enable);
			}
		}

	}

}
