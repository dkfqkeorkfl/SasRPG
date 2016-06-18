using UnityEngine;
using System.Collections.Generic;
using Sas;
using Sas.Data;
using System;

public class SasActor : MonoBehaviour
{
	private SasActorData mData = null;
	private Transform mCachedTm = null;
	private Animator mAnimator = null;
	public uint serial { get { return null != data ? data.serial : 0; } }
	public SasLayerActor layer { get; private set; }
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
			layer = value != null ? cachedTm.GetComponentInParent<SasLayerActor> () : null;
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

public class SasActorSpawner
{
	public class SerializableData
	{
		public ushort res;
		public float  ratio;
		public ushort default_level;
		public List<Ratio> items;
		public List<Ratio> levels;
	}

	public class Ratio
	{
		public ushort idx;
		public float ratio;
		public int size;
	}
		
	public ushort idxRes { get; set; }
	public float  ratio  { get; set; }

	public ushort leveld { get; set; } // level default
	public List< Ratio > items { get; set; }
	public List< Ratio > levels { get; set; }
	public SerializableData serialize
	{
		get { 
			var rtn = SasPool<SerializableData>.Instance.Alloc();
			rtn.res = this.idxRes;
			rtn.ratio = this.ratio;
			rtn.default_level = this.leveld;
			rtn.items = this.items;
			rtn.levels = this.levels;
			return rtn;
		}

		set {
			this.idxRes = value.res;
			this.ratio = value.ratio;
			this.leveld = value.default_level;
			this.items = value.items;
			this.levels = value.levels;
			SasPool<SerializableData>.Instance.Dealloc (value);
		}
	}

	public SasActorData assigned { get; private set; }
	public bool TryAssign()
	{
		if (UnityEngine.Random.value < ratio == false)
			return false;

		assigned = SasPool<SasActorData>.Instance.Alloc();
		var instanced = new LinkedList<SasFantasyBag.InstancedItem> ();

		for (int i = 0; i < items.Count; ++i)
			if (items [i].ratio < UnityEngine.Random.value) {
				var item = SasPool<SasFantasyBag.InstancedItem>.Instance.Alloc ();
				item.data = Sas.DataSet.GetItem (items [i].idx);
				item.size = items [i].size;
				item.created = DateTime.Now;
				instanced.AddLast (item);
			}
		var level = levels.Find ((val) => { return val.ratio < UnityEngine.Random.value; });
		assigned.Init (
			SasActorData.MakeSerial (), 
			idxRes, level != null ? level.idx : leveld, 0,
			null, instanced);

		return true;
	}
}

public class SasLayerActor : MonoBehaviour, ILayerDeco<SasLayerData>
{
	public class SerializableData
	{
		public List<SasActorSpawner.SerializableData> spawners;
		public int capacity;

		public float timeSpawn;
		public float height;

		static SerializableData()
		{
			SasPool<SerializableData>.Instance.release += Release;
		}
		static void Release(SerializableData data)
		{
			data.spawners = null;
			data.timeSpawn = 0.0f;
			data.capacity = 0;
			data.height = 0;
		}
	};

	private Transform mCachedTm = null;
	private readonly LinkedList<SasActor> actors = new LinkedList<SasActor> ();

	public int   capacity  { get; set; }
	public int   count     { get { return actors.Count; } }
	public bool  full      { get { return count > capacity; } }

	public List<SasActorSpawner> spawners { get; set; }
	public float timeSpawn { get; set; }

	public float height    { get; set; }
	public Transform cachedTm { 
		get { 
			if( null == mCachedTm ) mCachedTm = transform; 
			return mCachedTm; 
		}
	}
	public SerializableData seialize {
		set {
			this.height = value.height;
			this.timeSpawn = value.timeSpawn;
			this.spawners = value.spawners.ConvertAll( (val)=> {
				var rtn = SasPool<SasActorSpawner>.Instance.Alloc();
				rtn.serialize = val;
				return rtn;
			});
			this.capacity = value.capacity;
			SasPool<SerializableData>.Instance.Dealloc (value);
		}

		get {
			var rtn = SasPool<SerializableData>.Instance.Alloc ();
			rtn.capacity = this.capacity;
			rtn.height = this.height;
			rtn.spawners = this.spawners.ConvertAll((val)=>{ return val.serialize; });
					
			rtn.timeSpawn = this.timeSpawn;
			return rtn;
		}
	}
		
	public System.Collections.IEnumerator Spawn()
	{
		while (true) {
			yield return new WaitForSeconds(timeSpawn);
			if (null == spawners )
				continue;
			
			for (int i = 0; i < spawners.Count && !full; ++i) {
				if (spawners [i].TryAssign ())
					Add (spawners [i].assigned );
			}
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
		StopCoroutine ("Spawn");
		StartCoroutine (Spawn());
		var iter = actors.First;
		while (iter != null) {
			Point origin = iter.Value.data.pt.origin;
			Vector2 pos = data.rects [origin.x, origin.y].center + iter.Value.data.offset;
			Vector3 vec = new Vector3 (pos.x, height, pos.y);
			iter.Value.cachedTm.localPosition = vec;

			iter = iter.Next;
		}
	}

	public static void Release(SasLayerActor layer)
	{
		layer.StopAllCoroutines ();
	}
}

