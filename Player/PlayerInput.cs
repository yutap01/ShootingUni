using UnityEngine;
using System.Collections;


/// <summary>
/// プレイヤーに対する入力を監視
/// TODO:現在は直接デバイスを監視しているが、MasterInputから呼び出される形式の方が良いのではないだろうか？
/// </summary>
public class PlayerInput : MonoBehaviour {

	/// <summary>
	/// ジャイロセンサーのデフォルト位置
	/// TODO:オイラ角で管理して良いのだろうか？？
	/// </summary>
	private Vector3 defaultQuatanion = new Vector3(0,0,0);

 
	/// <summary>
	/// 現在のジャイロ状態をデフォルトとする
	/// ジャイロ搭載デバイスでない場合は何もしない
	/// </summary>
	public void SetDefaultQuatanion(){
#if !UNITY_EDITOR && UNITY_IPHONE
		this.defaultQuatanion = Input.gyro.gravity;
#endif
	}


	/// <summary>
	/// デバイスから水平方向に対する値を取得
	/// </summary>
	public float Horizontal{
		get {
#if !UNITY_EDITOR && UNITY_IPHONE
			return this.RotationY;
#else
			return Input.GetAxis(InputName.Horizontal);
#endif
		}
	}


	/// <summary>
	/// デバイスから垂直方向に対する値を取得
	/// </summary>
	public float Virtical {
		get {
#if !UNITY_EDITOR && UNITY_IPHONE
			return this.RotationX;
#else
			return Input.GetAxis(InputName.Vertical);
#endif
		}
	}

	
	/// <summary>
	/// デバイスから 水平方向(X)と奥手間方向(Z)の値を取得
	/// </summary>
	public Vector3 Axis {
		get {
#if !UNITY_EDITOR && UNITY_IPHONE
			return new Vector3(this.RotationY, 0, this.RotationX);
#else
			return new Vector3(this.Horizontal, 0, this.Virtical);
#endif
		}
	}


	/// <summary>
	/// ジャンプボタンを押下し続けている状態であることを取得判定
	/// </summary>
	public bool JumpingInput {
		get {
#if !UNITY_EDITOR && UNITY_IPHONE
			return (Input.touchCount>0 && Input.touches[0].phase == TouchPhase.Stationary);
#else
			return Input.GetButton(InputName.Jump);
#endif
		}
	}


	/// <summary>
	/// ジャンプボタンが押下された瞬間であることを取得判定
	/// </summary>
	public bool JumpInput {

		get {
#if !UNITY_EDITOR && UNITY_IPHONE
			return (Input.touchCount>0 && Input.touches[0].phase == TouchPhase.Began);
#else
			return Input.GetButtonDown(InputName.Jump);
#endif

		}
	}


	/// <summary>
	/// スクロール速度の変更アクションが入力されたことを取得判定
	/// 返却値：+n:n段階加速　-n:n段階減速 
	/// </summary>
	public int ScrollSpeedInput {
		get{
			if (Input.GetButtonDown(InputName.Fire1) && !Input.GetButtonDown(InputName.Fire2)) {
				return 1;
			}else if(!Input.GetButtonDown(InputName.Fire1) && Input.GetButtonDown(InputName.Fire2)){
				return -1;
			}else{
				return 0;
			}
		}
	}


	#region "回転の取得"
	
	/// <summary>
	/// 回転係数（ジャイロ以外の入力とのバランスを取るための係数)
	/// </summary>
	private float rotationFactor = 2.0f;
	
	/// <summary>
	/// ホームボタンを下にして縦持ちした際の
	/// 縦軸をY(正:右を下方向倒す　負:左を下方向へ倒す)
	/// 横軸をX(正:下を下方向へ倒す　負:上を下方向へ倒す)
	/// それぞれに直行する軸をZとする(正:時計回り、負:反時計回り)
	/// </summary>
	public float RotationFactor{
		get{
			return this.rotationFactor;
		}
		set{
			this.rotationFactor = value;
		}
	}


	/// <summary>
	/// ジャイロ状態X値を係数と上限下限によりゲーム内尺度へ変換
	/// </summary>
	public float RotationX{
		get{
			float value = (Input.gyro.gravity.y - defaultQuatanion.y) * this.rotationFactor;
			return Mathf.Clamp(value,-1.0f,1.0f);
		}
	}

	/// <summary>
	/// ジャイロ状態 Y値を係数と上限下限によりゲーム内尺度へ変換
	/// </summary>
	public float RotationY{
		get{
			float value = (Input.gyro.gravity.x - this.defaultQuatanion.x) * this.rotationFactor;
			return Mathf.Clamp(value,-1.0f,1.0f);
		}
	}

	/// <summary>
	/// ジャイロ状態 Z値を係数と上限下限によりゲーム内尺度へ変換
	/// </summary>
	public float RotationZ{
		get{
			float value = (Input.gyro.gravity.z - this.defaultQuatanion.z) * this.rotationFactor;
			return Mathf.Clamp(value,-1.0f,1.0f);
		}
	}

	#endregion
}	//end of class
