using UnityEngine;
using System.Collections;

public class FeetCollision : MonoBehaviour {

	//衝突時に呼び出されるメソッドのタイプ
	public delegate void FeetCollisionHandler(Collider other);

	//衝突イベント
	public event FeetCollisionHandler CollisionStepEvent;			//段差との衝突
	public event FeetCollisionHandler CollisionKicableEnvent;	//キック対象との衝突


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

  
	//段差との当たり判定
	void OnTriggerStay(Collider other){

		//対 地面
		/*
		if (other.gameObject.layer == LayerName.Ground) {
			if (this.CollisionStepEvent != null) {
				this.CollisionStepEvent(other);
			}
		}

		//対 キック対象
		if (other.gameObject.layer == LayerName.Kicable) {
			if (this.CollisionKicableEnvent != null) {
				this.CollisionKicableEnvent(other);
			}
		}
		 * */

  }
}
