using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// By Anish Dhesikan
public class BetterObjectPool : MonoBehaviour {

	public GameObject objectPrefab;
	public GameObject ObjectPrefab { get { return objectPrefab; } set { objectPrefab = value; Init (); } }
	public Object PrefabType { get; private set; }
	// initialMax: How many objects should the pool initially contain?
	public int initialMax = 100;
	// minInPool: What is the smallest number of items this pool should contain at any point?
	public int minInPool = 20;

	/*
	 * -- How does it work? --
	 * You can set 3 thresholds: lowerBound, middleBound, and upperBound. 
	 * At any time, if the percent of active objects from the pool hits 
	 * the lowerBound or upperBound thresholds, the object pool will begin to dynamically 
	 * create or destroy objects to get the number back to the middle threshold. 
	 * Try out the demo in ExampleScenes for more info.
	 */
	public float upperBound = 0.75f; // between 0 and 1
	public float middleBound = 0.5f; // between 0 and 1
	public float lowerBound = 0.25f; // between 0 and 1


	// maxInstantiatesPerFrame: How many times can Instantiate() be called in a single frame?
	// For mobile and for best performance in general, keep this number low.
	// Currently, numbers less than 1 are not supported.
	public int maxInstantiatesPerFrame = 1;

	// maxDestroysPerFrame: How many times can Destroy() be called in a single frame?
	// For mobile and for best performance in general, keep this number low.
	// Currently, numbers less than 1 are not supported.
	public int maxDestroysPerFrame = 1;

	protected HashSet<GameObject> activeObjectPool = new HashSet<GameObject>();
	protected Transform activeObjectPoolParent;
	protected List<GameObject> inactiveObjectPool = new List<GameObject>();
	protected Transform inactiveObjectPoolParent;

	private int totalCount = 0;

	private float percentActive = 1;

	public int inactive { get { return inactiveObjectPool.Count; } }

	// Use this for initialization
	public virtual void Awake () {
		if (null != ObjectPrefab)
			Init ();
	}

	void Init()
	{
		activeObjectPoolParent = new GameObject ("ActiveObjectPool " + objectPrefab.name).transform;
		inactiveObjectPoolParent = new GameObject ("InactiveObjectPool" + objectPrefab.name).transform;
		activeObjectPoolParent.parent = transform;
		inactiveObjectPoolParent.parent = transform;
		inactiveObjectPoolParent.gameObject.SetActive (false);

		InstantiateMultiple (Mathf.FloorToInt(initialMax * middleBound));
		PrefabType = UnityEditor.PrefabUtility.GetPrefabObject (objectPrefab);
	}

	// Update is called once per frame
	public virtual void Update () {
		if (null == ObjectPrefab)
			return;
		
		UpdatePercentActive ();
		if (percentActive > upperBound) {
			StopAllCoroutines ();
			StartCoroutine (ScaleUp());
		} else if (percentActive < lowerBound) {
			StopAllCoroutines ();
			StartCoroutine (ScaleDown());
		}
	}

	IEnumerator ScaleUp () {
		while (percentActive > middleBound) {
			//Mathf.FloorToInt (Mathf.Pow (totalCount, 0.5f))
			int numToInstantiate = Mathf.Min (maxInstantiatesPerFrame, Mathf.CeilToInt(totalCount / 200f));
			InstantiateMultiple (numToInstantiate);
			UpdatePercentActive ();
			yield return 0;
		}
	}

	IEnumerator ScaleDown () {
		if (percentActive > 0 && totalCount > minInPool) {
			while (percentActive < middleBound) {
				DestroyMultiple (maxDestroysPerFrame);
				UpdatePercentActive ();
				yield return 0;
			}
		}
	}

	public virtual bool ObjectIsActive (GameObject instance) {
		return activeObjectPool.Contains (instance);
	}

	public virtual void RemoveInstanceFromPool (GameObject instance) {
		StopAllCoroutines ();
		activeObjectPool.Remove (instance);
		inactiveObjectPool.Add (instance);
		instance.transform.parent = inactiveObjectPoolParent;
	}

	public virtual GameObject GetInstanceFromPool () {
		StopAllCoroutines ();
		GameObject curObject = null;
		if (inactiveObjectPool.Count <= 0) {
			InstantiateOne ();
		}

		curObject = inactiveObjectPool [0] as GameObject;
//		curObject.SetActive (true);
		inactiveObjectPool.Remove (curObject);
		activeObjectPool.Add (curObject);
		curObject.transform.parent = activeObjectPoolParent;

		return curObject;
	}

	public virtual GameObject GetInstanceFromPool (Vector3 position, Quaternion rotation) {
		GameObject curObject = GetInstanceFromPool ();
		if (curObject != null) {
			curObject.transform.position = position;
			curObject.transform.rotation = rotation;
		}
		return curObject;
	}

	// Instantiates, deactivates, and adds one to the pool
	private void InstantiateOne () {
		GameObject newObject = Instantiate (objectPrefab) as GameObject;
//		newObject.SetActive (false);
		inactiveObjectPool.Add (newObject);
		newObject.transform.parent = inactiveObjectPoolParent;
		totalCount++;
	}

	private void InstantiateMultiple (int count) {
		for (int i = 0; i < count; i++) {
			InstantiateOne ();
		}
	}

	private void DestroyOne () {
		if (inactiveObjectPool.Count > 0) {
			GameObject curObject = inactiveObjectPool [0] as GameObject;
			inactiveObjectPool.Remove (curObject);
			DestroyImmediate (curObject);
			totalCount--;
		}
	}

	private void DestroyMultiple (int count) {
		for (int i = 0; i < count; i++) {
			DestroyOne ();
		}
	}

	public string GetInfoText () {
		return "Percent Active:   " + percentActive * 100 + "%" + "\n" +
			"Number Active:   " + activeObjectPool.Count + "\n" +
			"Number Inactive: " + inactiveObjectPool.Count;
	}

	private void UpdatePercentActive () {
		percentActive = (float) activeObjectPool.Count / totalCount;


//		if (percentActive < lowerBound) {
//			Debug.Log ("Percent Active: " + percentActive);
//			Debug.Log ("TotalCount: " + totalCount);
//		}
	}

	// custom interface
	public GameObject GetPoolObject(Transform parent)
	{
		return GetPoolObject (parent, Vector3.zero, Quaternion.identity);
	}
	public GameObject GetPoolObject(Transform parent, Vector3 pos)
	{
		return GetPoolObject (parent, pos, Quaternion.identity);
	}
	public GameObject GetPoolObject(Transform parent, Quaternion rotation)
	{
		return GetPoolObject (parent, Vector3.zero, rotation);
	}
	public GameObject GetPoolObject(Transform parent, Vector3 pos, Quaternion rotation)
	{
		GameObject obj = GetInstanceFromPool();
		Transform tm = obj.transform;
		tm.parent = parent;
		tm.localPosition = pos;
		tm.localRotation = rotation;

		return obj;
	}

	public static BetterObjectPool MakePool(GameObject obj)
	{
		BetterObjectPool pool = new BetterObjectPool();
		pool.objectPrefab = obj;
		return pool;
	}
}