using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/* This class is attached to a GameObject named "IcosphereGenerator" in the hierarchy.
 * 
 * It can create and destroy objects through the IcosphereObjectPool.
 */

[RequireComponent (typeof(BoxCollider))]
public class IcosphereGenerator : MonoBehaviour {

	private BoxCollider myCollider;
	private List<GameObject> createdInstances = new List<GameObject>();

	void Awake () {
		myCollider = GetComponent<BoxCollider> ();
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey (KeyCode.A)) {
			CreateOne ();
		}

		if (Input.GetKey (KeyCode.S)) {
			DestroyOne ();
		}
	}

	// Gets a random point within the bounds of the box collider
	Vector3 GetRandomPoint () {
		Vector3 randomPoint = transform.position + myCollider.center;

		randomPoint += new Vector3 (myCollider.size.x * Random.Range (-0.5f, 0.5f),
				myCollider.size.y * Random.Range (-0.5f, 0.5f),
				myCollider.size.z * Random.Range (-0.5f, 0.5f));

		return randomPoint;
	}

	public void CreateOne () {
		GameObject currentInstance = IcosphereObjectPool.current.GetInstanceFromPool(GetRandomPoint(), Quaternion.identity);
		// Do anything else to currentInstance here after it has been created.
		createdInstances.Add(currentInstance);
	}

	public void DestroyOne () {
		// Remove the last element in the createdInstances list.
		if (createdInstances.Count > 0) {
			int lastIndex = createdInstances.Count - 1;
			IcosphereObjectPool.current.RemoveInstanceFromPool (createdInstances [lastIndex]);
			createdInstances.RemoveAt (lastIndex);
		}
	}
}
