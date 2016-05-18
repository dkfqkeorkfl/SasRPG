using UnityEngine;
using System.Collections.Generic;
using Sas;
using Sas.Data;
using System;

public class SasActor : MonoBehaviour
{
	private SasActorData mData = null;
	private SasLayerActor mLayer = null;

	private Transform mCachedTm = null;

	public int serial { get { return data.serial; } }
	public SasLayerActor layer { get { return mLayer; } }
	public Transform cachedTm { 
		get { 
			if( null == mCachedTm ) mCachedTm = transform; 
			return mCachedTm; 
		}
	}

	public SasActorData data 
	{ 
		get { return mData; }
		set {
			mData = value;
			mLayer = value != null ? cachedTm.GetComponentInParent<SasLayerActor> () : null;
		}
	}

	static SasActor()
	{
		SasPool<GameObject>.Instance.release += Release;
	}

	public static void Release(GameObject obj)
	{
		SasActor actor = obj.GetComponent<SasActor> ();
		if (actor == null)
			return;

		if (null != actor.data) {
			SasPool<SasActorData>.Instance.Dealloc (actor.data);
			actor.data = null;
		}

		obj.gameObject.SetActive (false);
	}
}

public class SasLayerActor : MonoBehaviour, ILayerDeco<SasLayerData>
{
	private Transform mCachedTm = null;
	private readonly LinkedList<SasActor> actors = new LinkedList<SasActor> ();

	public int   capacity { get; set; }
	public float height   { get; set; }
	public int   count    { get { return actors.Count; } }
	public bool  full     { get { return count > capacity; } }

	public Transform cachedTm { 
		get { 
			if( null == mCachedTm ) mCachedTm = transform; 
			return mCachedTm; 
		}
	}

	public SasActor Add(SasActorData adata)
	{
		if (Contain ((val) => val.data == adata || val.data.serial == adata.serial)) {
			UnityEngine.Assertions.Assert.IsTrue (Contain ((val) => val.data == adata || val.data.serial == adata.serial),
				"[SasLayerActor] it already has the ActorData");
			return null;
		}
		if (full) {
			return null;
		}

		Vector2 pos = data.rects [adata.pt.origin.x, adata.pt.origin.y].center + adata.offset;
		Vector3 vec = new Vector3 (pos.x, height, pos.y);
		GameObject prefab = (GameObject)Resources.Load (adata.resource);
		GameObject instanced = SasPool<GameObject>.Instance.Alloc (prefab, cachedTm, vec);
		instanced.SetActive (true);
		SasActor actor = instanced.GetComponentExactly<SasActor> ();
		actor.data = adata;
		actors.AddLast (actor);
		return actor;
	}
		
	public void Remove(int serial)  { Remove ((val) => val.data.serial == serial); }
	public void Remove(Predicate<SasActor> pred)  
	{ 
		var iter = actors.Find (pred);
		if (null == iter)
			return;

		SasPool<GameObject>.Instance.Dealloc (iter.Value.gameObject);
		actors.Remove (iter); 
	}

	public bool Contain(int serial) { return Find (serial) != null; }
	public bool Contain(Predicate<SasActor> pred) { return Find (pred) != null; }
	public SasActor Find(int serial) { return Find((val) => val.data.serial == serial); }
	public SasActor Find(Predicate<SasActor> pred)
	{
		var iter = actors.Find (pred);
		return iter != null ? iter.Value : null;
	}

	public SasLayerData data { set; get; }
	public void Apply ()
	{
	}
}

