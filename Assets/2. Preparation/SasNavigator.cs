using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SasNavigator : MonoBehaviour {

	private class Progress
	{
		public SasTile<int>.Pos mPos;
		public int mDir = 0;

		public Point pt { get { return mPos.pt; } }

		static Progress()
		{
			SasPool<Progress>.Instance.release += Realase;
		}
		static void Realase(Progress p)
		{
			p.mDir = 0;
		}
	}

	private LinkedList<Point> mPoints = new LinkedList<Point> ();
	private Stack<Progress> mProgresses = new Stack<Progress>();
	private SasLayerData mTile = null;
	private bool[,] mMask = null;

	public LinkedList<Point> points { get { return mPoints; } }
	public Point size { get { return mTile.size; } }
	public SasLayerData tile { 
		set {
			mTile = value;
			mMask = new bool[mTile.size.x, mTile.size.y];
		}
	}

	private IEnumerator ClearProg()
	{
		for (int x = 0; x < size.x; ++x) {
			for (int y = 0; y < size.y; ++y)
				mMask [x, y] = false;
			yield return new WaitForEndOfFrame ();
		}
			
		while (mProgresses.Count > 0) {
			var p = mProgresses.Peek ();
			SasPool<Progress>.Instance.Dealloc (p);
			mProgresses.Pop ();
			yield return new WaitForEndOfFrame ();
		}
		mPoints.Clear ();
	}
		
	public void Patrol(SasTile<int>.Pos from)
	{
		StopAllCoroutines ();
		StartCoroutine (OnPatrol(from));
	}

	private IEnumerator OnPatrol(SasTile<int>.Pos from)
	{
		StartCoroutine ("ClearProg");
		yield return new WaitUntil (()=> mPoints.Count == 0 && mProgresses.Count == 0);

		mProgresses.Push (SasPool<Progress>.Instance.Alloc ());
		mProgresses.Peek ().mPos = from;
		mPoints.AddLast (from.pt);
		bool isForward = true;
		while (mProgresses.Count > 0) 
		{
			var cur = mProgresses.Peek();
			var nxt = cur.mPos;
			while(cur.mDir++ < cur.mPos.count)
			{
				if(cur.mPos.TryNext(cur.mDir,out nxt) == SasTile.eDirection.COUNT || mTile.IsBlock(nxt.pt))
					continue;
				isForward = true;
				mMask [nxt.pt.x, nxt.pt.y] = true;
				mProgresses.Push (SasPool<Progress>.Instance.Alloc ());
				mProgresses.Peek ().mPos = nxt;
				if (nxt.pt.x != mPoints.Last.Value.x && nxt.pt.y != mPoints.Last.Value.y)
					mPoints.AddLast (cur.pt);
				break;
			}

			if (cur.mPos.pt.EqualTo(nxt.pt)) 
			{
				mProgresses.Pop ();
				var last = mProgresses.Peek ();
				if(isForward)
					mPoints.AddLast (cur.pt);
				else if (last.pt.x != mPoints.Last.Value.x && last.pt.y != mPoints.Last.Value.y)
					mPoints.AddLast (cur.pt);
				SasPool<Progress>.Instance.Dealloc (cur);
				isForward = false;
			}

			yield return new WaitForEndOfFrame ();
		} // </while>

	}
}
