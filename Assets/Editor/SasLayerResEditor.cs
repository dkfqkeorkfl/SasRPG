using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.Text;
using Sas;

namespace Sas.Edit
{
	[CustomEditor(typeof (SasLayerRes))]
	public class SasLayerResEditor : Editor {
		
		private SasLayerRes mLayerRes = null;

		private bool mPoolExpand = true;
		private readonly List<GameObject> mPoolObj = SasPool<List<GameObject>>.Instance.Alloc();

		private bool mDataEdit = false;
		private bool mDataExpand = true;
		private readonly SasLayerData mLayerData = SasPool<SasLayerData>.Instance.Alloc();

		private SasLayerActor.SerializableData mSerializedActor = SasPool<SasLayerActor.SerializableData>.Instance.Alloc();

		int  mIdxRes = 0;
		int  mIdxgroup;
		bool mIsBlock = false;
		string mFilepath = "unnamed.txt";

		private void OnEnable()
		{
			mLayerRes = target as SasLayerRes;
			mLayerData.size = new Point{ x = 10, y = 10 };
			mPoolObj.Add (null);

			mLayerRes.objMap = mPoolObj;
			mLayerRes.data = mLayerData;

			mSerializedActor.spawners = SasPool<List<SasActorSpawner.SerializableData>>.Instance.Alloc();
		}

		private void onDisable()
		{
		}

		public override void OnInspectorGUI()
		{
			EditorGUIUtility.labelWidth = 55.0f;
			EditorGUILayout.HelpBox ( 
				"If you're starting in playing, The datas will appear as models", 
				MessageType.Info);
			var file = EditorGUILayout.ObjectField ("Load", null, typeof(TextAsset), false) as TextAsset;
			if (null != file) {
				Load (file);
				if( Application.isPlaying ) mLayerRes.Apply ();
			}
 
			mPoolExpand = EditorGUILayout.Foldout (mPoolExpand, "Pool (Only can load files in Resources folder)");
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
				Point size = mLayerData.size;

				EditorGUILayout.BeginHorizontal ();
				size.x = EditorGUILayout.IntField ("Width", size.x);
				size.y = EditorGUILayout.IntField ("Height", size.y);
				EditorGUILayout.EndHorizontal ();
				size.x = size.x < 1 ? 1 : size.x;
				size.y = size.y < 1 ? 1 : size.y;
				mLayerData.size = size;

				mLayerData.length = EditorGUILayout.Vector2Field ("Length", mLayerData.length);

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

			EditorGUILayout.HelpBox ("The above button is Spawn Datas for Actor", MessageType.Info);
			if (GUILayout.Button ("Open")) {
				var window = EditorWindow.GetWindow(typeof(Sas.Edit.SpawnWindow)) as Sas.Edit.SpawnWindow;
				window.data = mSerializedActor;
			}

			EditorGUILayout.HelpBox (
				"File Path is Added To 'Resources/'. " +
				"if you write File path - 'unnamed', it will be Resources/unnamed", 
				MessageType.Info);
			mFilepath = EditorGUILayout.TextField ("File Path", mFilepath);
			if (GUILayout.Button ("Save Current Datas"))
				Save("Resources/");
		}

		public void OnSceneGUI()
		{
			if (mDataEdit)
				EditData ();
		}

		private void EditData()
		{
			Handles.BeginGUI();

			Rect rc = new Rect (0.0f, 0.0f, Screen.width, Screen.height);
			Handles.DrawSolidRectangleWithOutline (rc, Color.white, Color.white);

			Handles.color = Color.black;
			Point size = mLayerData.size;
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
						mLayerData.SetBlock (pt, mIsBlock);
						mLayerData.SetIdxGroup (pt, mIdxgroup);
						mLayerData.SetIdxRes (pt, mIdxRes);
					}

					if (mLayerData.tile [x, y] == 0)
						continue;

					int   res   = mLayerData.GetIdxRes (pt);
					int   group = mLayerData.GetIdxGroup (pt);
					bool  block = mLayerData.IsBlock (pt);

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

		private void Load(TextAsset file)
		{
			mFilepath = AssetDatabase.GetAssetPath (file).Replace("Assets/Resources/","");
			var data = JsonFx.Json.JsonReader.Deserialize<Sas.Protocols.Map> (file.text);
			mLayerData.serialization = data.tile;
			mPoolObj.Clear ();
			var objs = data.bgs.ConvertAll( (path) => (GameObject)Resources.Load (path) );
			mPoolObj.AddRange ( objs );
			mSerializedActor = data.actor;
		}

		private void Save(string prefix)
		{
			var regex = new System.Text.RegularExpressions.Regex (@"Assets/Resources/(\w+).prefab");

			var data = new Sas.Protocols.Map {
				bgs = mPoolObj.ConvertAll ((obj) => regex.Match(AssetDatabase.GetAssetPath (obj)).Groups[1].Value ),
				tile = mLayerData.serialization,
				actor = mSerializedActor
			};
			Uti.SaveJson (prefix+mFilepath, data);
		}

	}

}
