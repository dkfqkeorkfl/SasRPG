using UnityEngine;
using System.Collections.Generic;
using Sas;

public class SasActor : MonoBehaviour
{
	private SasActorData mData = null;
	private Transform mCachedTm = null;
	private Animator mAnimator = null;
	private SasNavigator mNavigator = null;
	private readonly Dictionary<string,string> mStates = new Dictionary<string,string>();
	private string mStateCur = "";

	public uint serial { get { return null != data ? data.serial : 0; } }
	public SasTile<int>.Pos pos { get; set; }
	public Vector2 offset { get; set; }
	public SasLayerActor layer { get; private set; }
	public Transform cachedTm { 
		get { 
			if( null == mCachedTm ) mCachedTm = transform; 
			return mCachedTm; 
		}
	}
	public SasNavigator navigator
	{
		get {
			if (null == mNavigator)
				mNavigator = gameObject.GetComponentExactly<SasNavigator> ();
			return mNavigator;
		}
	}
	public SasActorData data
	{ 
		get { return mData; }
		set {
			mData = value;
			layer = value != null ? cachedTm.GetComponentInParent<SasLayerActor> () : null;
			navigator.tile = layer.data;
			SetState ("idle");
		}
	}

	private void Awake()
	{
		mStates.Add ("idle", "OnIdle");
		mStates.Add ("patrol", "OnPatrol");
		mStates.Add ("combat", "OnCombat");
	}


	private System.Collections.IEnumerator OnIdle()
	{
		navigator.Patrol (pos);
		yield return new WaitForEndOfFrame ();
		SetState ("partrol");
	}

	const float DEF_SPEED = 1.0f;
	private System.Collections.IEnumerator OnPatrol()
	{
		var it = navigator.points.First;
		var rects = layer.data.rects;
		while (it != null) {
			
			var from = rects[pos.pt.x,pos.pt.y].center + offset;
			var to = rects[it.Value.x,it.Value.y].center;
			var distanceTo = Vector2.Distance (from, to);
			float disatnceFrom = 0.0f;
			while( disatnceFrom > distanceTo)
			{
				disatnceFrom += DEF_SPEED * Time.deltaTime;
				var pos = Vector2.Lerp (from, to, disatnceFrom / distanceTo);
				cachedTm.localPosition = new Vector3 (pos.x, cachedTm.localPosition.y, pos.y);
				yield return new WaitForEndOfFrame ();
			}
		}

		SetState ("idle");
	}
	private System.Collections.IEnumerator OnCombat()
	{
		yield return new WaitForEndOfFrame ();
	}

	private void Dealloc()
	{
		SasPool<SasActorData>.Instance.Dealloc (data);
		this.data = null;
	}

	private void Update()
	{
		UpdatePos ();
	}

	private void UpdatePos()
	{
		var rects = layer.data.rects [pos.pt.x, pos.pt.y];
		var curV2 = cachedTm.localPosition;
		var curPos = new Vector2 (curV2.x, curV2.z);
		if (rects.Contains (curPos)) {
			offset = curPos - rects.center;
			return;
		}

		for (var i = 0; i < pos.count; ++i) {
			var posNxt = pos;
			if (pos.TryNext (i, out posNxt) != SasTile.eDirection.COUNT && 
				layer.data.rects [posNxt.pt.x, posNxt.pt.y].Contains (curPos))
				pos = posNxt;
		}

		offset = curPos - rects.center;

	}

	void SetState(string name)
	{
		string routine;
		if (!mStates.TryGetValue (name, out routine))
			return;
		
		mStateCur = name;
		//mAnimator.SetTrigger (name.GetHashCode ());
		StopAllCoroutines ();
		StartCoroutine (routine);
	}
}

public class SasActorSpawner
{
	public class SerializableData
	{
		public ushort res;
		public float  ratio;
		public List<Ratio> items;
		public List<Ratio> levels;
		public List<Point> locals;

		static SerializableData()
		{
			SasPool<SerializableData>.Instance.release += SasPool<SerializableData>.Instance.MakeRelease();
		}
	}

	public class Ratio
	{
		public ushort idx;
		public float ratio;
		public int size;
	}

	public ushort idxRes { get; set; }
	public float  ratio  { get; set; }

	public List< Ratio > items { get; set; }
	public List< Ratio > levels { get; set; } // levels Unique
	public List<Point> locals { get; set; }
	public ushort leveld { 
		get { 
			UnityEngine.Assertions.Assert.IsTrue (this.levels.Count == 0, "Make sure! it doesn't have level info!");
			return levels [0].idx; } 
	}

	public SerializableData serialize
	{
		get { 
			var rtn = SasPool<SerializableData>.Instance.Alloc();
			rtn.res = this.idxRes;
			rtn.ratio = this.ratio;
			rtn.items = this.items;
			rtn.levels = this.levels;
			rtn.locals = this.locals;
			return rtn;
		}

		set {
			this.idxRes = value.res;
			this.ratio = value.ratio;
			this.items = value.items;
			this.levels = value.levels;
			this.locals = value.locals;
			SasPool<SerializableData>.Instance.Dealloc (value);
		}
	}

	public SasActorData assigned { get; private set; }
	public bool TryAssign()
	{
		UnityEngine.Assertions.Assert.IsTrue (this.locals.Count == 0, "Why did u make it? it doesn't have locataion infos.");
		if (UnityEngine.Random.value < ratio == false || locals.Count == 0)
			return false;

		assigned = SasPool<SasActorData>.Instance.Alloc();
		var instanced = new LinkedList<SasFantasyBag.InstancedItem> ();

		for (int i = 0; i < items.Count; ++i)
			if (items [i].ratio < UnityEngine.Random.value) {
				var item = SasPool<SasFantasyBag.InstancedItem>.Instance.Alloc ();
				item.data = Sas.DataSet.GetItem (items [i].idx);
				item.size = items [i].size;
				item.created = System.DateTime.Now;
				instanced.AddLast (item);
			}

		var idxLevel = levels.FindIndex ((val) => { return val.ratio > UnityEngine.Random.value; });

		assigned.Init (
			SasActorData.MakeSerial (), 
			idxRes, idxLevel != -1 ? levels[idxLevel].idx : leveld, 0,
			null, instanced);
		assigned.apprearance = locals [UnityEngine.Random.Range (0, locals.Count)];
		return true;
	}
}