using UnityEngine;
using System.Collections;

public class CharacterHit : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	}

	void OnCollisionEnter(Collision col){
		//Debug.Log (this.gameObject.name + " Collision to " + col.gameObject.name);
	}

	void OnTriggerEnter(Collider col){
		//Debug.Log (this.gameObject.name + " Trigger to " + col.gameObject.name);
	}
}
