using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class SasLayerRes : MonoBehaviour {

	public enum Axis
	{
		X, Y, Z
	};

	private readonly List<GameObject> mObjMap = new List<GameObject>();
	private SasLayerData mData = null;
	private GameObject[,] mObjs = null;
	private Transform mCachedTm = null;
	private Axis mAxis = Axis.Y;

	public Axis axis
	{
		get { return mAxis; }
		set { 
			if (mAxis == value) return;
			mAxis = value;
			Reposition ();
		}
	}

	public Transform cachedTm{
		get 
		{ 
			if (mCachedTm == null) mCachedTm = transform; 
			return mCachedTm;
		}
	}

	public GameObject[,] objs
	{
		get { return mObjs; }
		private set { RelaseObj (); mObjs = value; }
	}

	public Point size { get; private set; }
	public SasLayerData data 
	{
		get { return mData; }
		set { 
			if (mData == value) return; 
			mData = value;
			size = value.size;
			objs = new GameObject[size.x, size.y];
			ApplyRes ();
		}
	}
	
	private void ApplyRes()
	{
		Rect[,] rects = data.rects;
		for (int x = 0; x < size.x; ++x)
			for (int y = 0; y < size.y; ++y) {
				if (mObjs [x, y] != null)
					SasPool<GameObject>.Instance.Dealloc (mObjs [x, y]);

				int idxRes = data.GetIdxRes (new Point{ x = x, y = y });
				mObjs[x,y] = SasPool<GameObject>.Instance.Alloc(
					mObjMap[idxRes],
					cachedTm,
					ConvertAreaToWorld(rects[x,y])
				);
			}
	}

	private void RelaseObj()
	{
		if ( mObjs == null )
			return;

		for (int x = 0; x < size.x; ++x)
			for (int y = 0; y < size.y; ++y) {
				if (mObjs [x, y] != null) {
					SasPool<GameObject>.Instance.Dealloc (mObjs [x, y]);
					mObjs [x, y] = null;
				}
			}
	}

	private void Reposition()
	{
		Rect[,] rects = data.rects;

		for (int x = 0; x < size.x; ++x)
			for (int y = 0; y < size.y; ++y) {
				mObjs[x,y].transform.localPosition = ConvertAreaToWorld(rects[x,y]);
			}
	}

	private Vector3 ConvertAreaToWorld(Rect rect) 
	{
		Vector2 vec2D = rect.center;
		return axis == Axis.Y ? new Vector3 (vec2D.x, 0.0f, vec2D.y) :
			axis == Axis.X ? new Vector3 (0.0f, vec2D.y, vec2D.x) : 
			new Vector3(vec2D.x, vec2D.y);
	}
}
