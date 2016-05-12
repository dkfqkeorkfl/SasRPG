using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ActiveInfoText : MonoBehaviour {

	public BetterObjectPool currentPool;

	private Text myText;

	void Awake () {
		myText = GetComponent<Text> ();
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		myText.text = currentPool.GetInfoText ();
	}
}
