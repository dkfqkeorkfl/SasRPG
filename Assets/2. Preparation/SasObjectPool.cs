using UnityEngine;
using System.Collections.Generic;
using SPStudios.Tools;
using System;
using Sas;

public interface IPool<T> where T : class, new()
{
	T Alloc ();
	T[] Alloc (int size);
	void Dealloc (T item);

	int capacity { get; set; }
	int inactive { get;  }
}

public class SasPool<T> :Singleton< SasPool<T> > where T : class, new()
{
	private class Defulat : IPool<T>
	{
		const int DEF_DEFAULT_CAPACUTY = 20;

		private int mCapacity = DEF_DEFAULT_CAPACUTY;

		private readonly Stack<T> mInactive = new Stack<T>();

		public int  capacity { get { return mCapacity; } set { mCapacity = value; } }
		public int  inactive { get { return mInactive.Count;      } }
		public bool empty    { get { return mInactive.Count == 0; } }

		public T Alloc()
		{
			return empty ? default(T) : mInactive.Pop ();
		}

		public T[] Alloc(int size)
		{
			T[] items = new T[size];
			for (int i = 0; i < size; ++i)
				items [i] = Alloc ();

			return items;
		}

		public void Dealloc(T item)
		{
			mInactive.Push (item);
		}
	}

	private readonly Defulat DEFAULT_ADAPTER = new Defulat();

	private IPool<T> mAdapter = null;

	public event Action<T> reset, release;
	public IPool<T> adapter 
	{ 
		get { return mAdapter != null ? mAdapter : DEFAULT_ADAPTER; }
		set { mAdapter = value; }
	}

	public T Alloc()
	{
		T item = adapter.Alloc ();

		Reset (item);
		return item;
	}

	public T[] Alloc(int size)
	{
		T[] items = adapter.Alloc (size);
		Reset (items, size);
		return items;
	}

	public void Dealloc(T item)
	{
		IPool<T> a = adapter;
		Release(item);
		
		if (a.inactive < a.capacity == false)
			return;
		
		a.Dealloc (item);
	}

	public void Dealloc(T[] items)
	{
		foreach (var item in items)
			Dealloc (item);
	}



	public void Reset(T item)   
	{ 
		if (reset != null) reset (item); 
	}
	public void Reset(T[] items, int size) 
	{ 
		if (reset == null)
			return;

		for (int i = 0; i < size; ++i)
			reset (items [i]);
	}
	public void Release(T item) 
	{ 
		if( release != null ) release (item); 
	}
}

public class SasPoolObject  : Singleton<SasPoolObject>, IPool<GameObject>
{
	private readonly List<BetterObjectPool> mPools = new List<BetterObjectPool>();
	private BetterObjectPool mCached = null;

	public int capacity { get { return mCached.initialMax; } set{ mCached.initialMax = value; } }
	public int inactive { get { return mCached.inactive;} }

	static SasPoolObject()
	{
		SasPool<GameObject>.Instance.adapter = Instance;
		SasPool<GameObject>.Instance.release += (obj) => Instance.GetPool (obj);
	}
		
	public GameObject Alloc ()
	{
		return mCached.GetInstanceFromPool();
	}

	public GameObject Alloc(GameObject obj, Transform parent, Vector3 pos, Quaternion rotation)
	{
		BetterObjectPool pool = GetPool(obj);
		if ( pool == null )
			return null;

		return pool.GetPoolObject (parent, pos, rotation);
	}

	public GameObject[] Alloc (int size)
	{
		GameObject[] objs = new GameObject[size];
		for (int i = 0; i < size; ++i)
			objs [i] = Alloc ();
		return objs;
	}
		
	public void Dealloc (GameObject item)
	{
		BetterObjectPool pool = GetPool(item);
		if ( pool == null )
			return;

		pool.RemoveInstanceFromPool (item);
	}

	public bool Contain(GameObject obj)
	{
		return GetPool (obj) != null;
	}
		
	public BetterObjectPool GetPool(GameObject instance)
	{
		GameObject prefab = instance.GetPrefabOrigin ();
		if (prefab == null)
			return null;

		if (mCached != null && prefab == mCached.objectPrefab )
			return mCached;

		int index = mPools.FindIndex ((val) => val.objectPrefab == prefab);
		if ( index < mPools.Count == false) {
			BetterObjectPool pool = new BetterObjectPool ();
			pool.objectPrefab = prefab;
			mPools.Add (pool);
		} 

		mCached = mPools [index];
		return mCached;
	}
		
}
	
public static class SasPoolExtension
{
	public static BetterObjectPool Ready(this SasPool<GameObject> pool, GameObject obj)
	{
		return SasPoolObject.Instance.GetPool (obj);
	}

	public static bool Contain(this SasPool<GameObject> pool, GameObject obj)
	{
		return SasPoolObject.Instance.Contain (obj);
	}
	public static GameObject Alloc(this SasPool<GameObject> pool, GameObject obj, Transform parent)
	{
		return Alloc (pool, obj, parent, Vector3.zero, Quaternion.identity);
	}
	public static GameObject Alloc(this SasPool<GameObject> pool, GameObject obj, Transform parent, Vector3 pos)
	{
		return Alloc (obj, parent, pos, Quaternion.identity);
	}
	public static GameObject Alloc(this SasPool<GameObject> pool, GameObject obj, Transform parent, Quaternion rotation)
	{
		return Alloc (pool, obj, parent, Vector3.zero, rotation);
	}
	public static GameObject Alloc(this SasPool<GameObject> pool, GameObject obj, Transform parent, Vector3 pos, Quaternion rotation)
	{
		return Alloc(obj, parent, pos, rotation);
	}
	private static GameObject Alloc(GameObject obj, Transform parent, Vector3 pos, Quaternion rotation)
	{
		GameObject pooled = SasPoolObject.Instance.Alloc (obj, parent, pos, rotation);
		SasPool<GameObject>.Instance.Reset(pooled);
		return pooled;
	}
}