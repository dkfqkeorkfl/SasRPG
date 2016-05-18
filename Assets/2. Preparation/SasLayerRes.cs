using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class SasLayerRes : MonoBehaviour,  Sas.ILayerDeco<SasLayerData> {

	private readonly List<GameObject> mObjMap = new List<GameObject>();
	private GameObject[,] mObjs = null;
	private Transform mCachedTm = null;

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

	public Point        size { get; private set; }
	public SasLayerData data { get; set; }
	
	public void Apply()
	{
		size = data.size;
		objs = new GameObject[size.x, size.y];
		Rect[,] rects = data.rects;
		for (int x = 0; x < size.x; ++x)
			for (int y = 0; y < size.y; ++y) {
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
				if( null != mObjs[x,y] )
					mObjs[x,y].transform.localPosition = ConvertAreaToWorld(rects[x,y]);
			}
	}

	private Vector3 ConvertAreaToWorld(Rect rect) 
	{
		Vector2 vec2D = rect.center;
		return new Vector3 (vec2D.x, 0.0f, vec2D.y);
	}
}
