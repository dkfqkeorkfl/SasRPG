using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class SasLayerRes : MonoBehaviour,  Sas.ILayerDeco<SasLayerData>
{

	private GameObject[,] mObjs = null;
	private Transform mCachedTm = null;

	public Transform cachedTm {
		get { 
			if (mCachedTm == null)
				mCachedTm = transform; 
			return mCachedTm;
		}
	}

	public GameObject[,] objs {
		get { return mObjs; }
		private set {
			Clear ();
			mObjs = value;
		}
	}

	public Point        size { get; private set; }

	public SasLayerData data { get; set; }

	public IList<GameObject> objMap{ get; set; }

	public void Apply ()
	{
		if (null == data)
			return;
			
		if (size.EqualTo (data.size))
			Clear ();
		else {
			size = data.size;
			objs = new GameObject[size.x, size.y];
		}

		if (null == objMap)
			return;

		Rect[,] rects = data.rects;
		for (int x = 0; x < size.x; ++x)
			for (int y = 0; y < size.y; ++y) {
				int idxRes = data.GetIdxRes (new Point{ x = x, y = y });
				if (0 != idxRes)
					mObjs [x, y] = SasPool<GameObject>.Instance.Alloc (
						objMap [idxRes],
						cachedTm,
						ConvertAreaToWorld (rects [x, y])
					);
			}
	}

	public void Clear ()
	{
		if (mObjs == null)
			return;

		for (int x = 0; x < size.x; ++x)
			for (int y = 0; y < size.y; ++y) {
				if (mObjs [x, y] != null) {
					SasPool<GameObject>.Instance.Dealloc (mObjs [x, y]);
					mObjs [x, y] = null;
				}
			}
	}

	private void Reposition ()
	{
		Rect[,] rects = data.rects;

		for (int x = 0; x < size.x; ++x)
			for (int y = 0; y < size.y; ++y) {
				if (null != mObjs [x, y])
					mObjs [x, y].transform.localPosition = ConvertAreaToWorld (rects [x, y]);
			}
	}

	public Vector3 ConvertAreaToWorld (Rect rect)
	{
		Vector2 vec2D = rect.center;
		return new Vector3 (vec2D.x, 0.0f, vec2D.y);
	}

	public void OnDrawGizmosSelected ()
	{
		if (null == data)
			return;
		Gizmos.matrix = cachedTm.localToWorldMatrix;
		var length = data.length;
		var size = data.size;
		for (int x = 0; x < size.x + 1; ++x) {
			Vector3 from = new Vector3 (x * length.x, 0.1f, 0.0f);
			Vector3 to = new Vector3 (x * length.x, 0.1f, size.y * length.y);

			Gizmos.DrawLine (from, to);
		}

		for (int y = 0; y < size.y + 1; ++y) {
			Vector3 from = new Vector3 (0.0f, 0.1f, y * length.y);
			Vector3 to = new Vector3 (size.x * length.x, 0.1f, y * length.y);
			Gizmos.DrawLine (from, to);
		}

		Gizmos.matrix = Matrix4x4.identity;
	}
}
