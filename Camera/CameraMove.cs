using UnityEngine;
using System.Collections;

/// <summary>
/// カメラの移動制御
/// </summary>
public class CameraMove : MonoBehaviour {

	/// <summary>
	/// ローカルZ座標最小値
	/// </summary>
	public float MinZ = 0.0f;
	/// <summary>
	/// ローカルZ座標最大値
	/// </summary>
	public float MaxZ = 0.0f;
	/// <summary>
	/// ローカルY座標最小値
	/// </summary>
	public float MinY = 0.0f;
	/// <summary>
	/// ローカルY座標最大値
	/// </summary>
	public float MaxY = 0.0f;

	/// <summary>
	/// ターゲットが行きうると想定される最大の高さ(TODO:ターゲットから取得すべきかもしれない)
	/// </summary>
	[SerializeField]
	private float TargetMaxHeight = 100.0f;	//想定するターゲットの最大の高さ


	//プレイヤーのy座標でカメラのz座標が決まる
	//yが高いほどzはプレイヤーに近づく

	/// <summary>
	/// カメラの座標はPlayerゲームオブジェクトのワールド座標に依存している
	/// Playerが高いワールド地点にいるほどカメラはYが大きくZが小さくなる
	/// 逆にPlayerが低いワールド地点にいるほどカメラはYが小さくZが大きくなる
	/// </summary>
	void Update () {
		this.transform.localPosition = this.cameraPosition();
		this.transform.LookAt(this.transform.parent);	//ターゲットの方を向く
	}

	/// <summary>
	/// プレイヤーのワールド座標からカメラのローカル位置を決定する
	/// </summary>
	/// <returns>算出されたローカル位置</returns>
	private Vector3 cameraPosition(){

		float z = 0.0f;
		float y = 0.0f;

		//cameraの親はターゲット。その親がプレイヤー
		//TODO:プレイヤーはGameObjectもしくはTransformから得てキャッシュするように修正せよ
		//TODO:もしくは、カメラのメンバにプレイヤーオブジェクトをアタッチすること
		//TODO:一時停止中にカメラが自由に動けるモードを作成するべきかもしれない
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
