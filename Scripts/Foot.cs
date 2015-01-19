using UnityEngine;
using System.Collections;

public class Foot : MonoBehaviour {


	private GameObject playerObj = null;

	//地面にめり込んだ時の強制排出量
	private const float escapeY = 0.1f;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	//チャンクに体がめり込んだ時の救済処置
	void OnTriggerStay(Collider collider){
		
		
		Transform transform = collider.transform;
		//Groundへのめり込み判定
		if (transform.gameObject.layer == LayerName.Ground){

			if (this.playerObj == null) {
				this.playerObj = Utility.GetPlayerObject();
			}

			//離脱
			Vector3 position = this.playerObj.transform.position;
			position.y += Foot.escapeY;
			this.playerObj.transform.position = position;
		} 

	}
}
