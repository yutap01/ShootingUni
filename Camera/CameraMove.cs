using UnityEngine;
using System.Collections;

public class CameraMove : MonoBehaviour {
	public float MinZ = 0.0f;	//ローカルz最小値
	public float MaxZ = 0.0f;	//ローカルz最大値
	public float MinY = 0.0f;	//ローカルy最小値
	public float MaxY = 0.0f;	//ローカルy最大値
	public float TargetMaxHeight = 100.0f;	//想定するターゲットの最大の高さ

	void Awake(){
	}

	// Use this for initialization
	void Start () {
	
	}
	
	//プレイヤーのy座標でカメラのz座標が決まる
	//yが高いほどzはプレイヤーに近づく
	void Update () {
		//this.transform.localPosition = this.cameraPosition();
		this.transform.LookAt(this.transform.parent);	//ターゲットの方を向く
	}

	//ターゲットの高さからカメラのローカル位置を決定する
	private Vector3 cameraPosition(){

		float z = 0.0f;
		float y = 0.0f;

		//cameraの親はターゲット。その親がプレイヤー
		float	targetY = this.transform.parent.parent.position.y;
		float rate = (targetY-0)/(TargetMaxHeight-0); 	//ターゲットの上昇率

		z = this.MinZ + (1.0f - rate) * (this.MaxZ - this.MinZ);
		y = this.MinY + rate *  (this.MaxY - this.MinY);

		return new Vector3(
			0.0f,
			Mathf.Clamp(y,this.MinY,this.MaxY),
			Mathf.Clamp(z,this.MinZ,this.MaxZ));
	}
}
