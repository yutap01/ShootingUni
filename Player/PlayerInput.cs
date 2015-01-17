using UnityEngine;
using System.Collections;

public class PlayerInput : MonoBehaviour {


	private Vector3 defaultQuatanion = new Vector3(0,0,0);

 
	//現在の角度をデフォルト値とする
	public void SetDefaultQuatanion(){
#if !UNITY_EDITOR && UNITY_IPHONE
		this.defaultQuatanion = Input.gyro.gravity;
#endif
	}

	//入力値を水平方向に対する値として通知
	public float Horizontal{
		get {
#if !UNITY_EDITOR && UNITY_IPHONE
			return this.RotationY;
#else
			return Input.GetAxis(InputName.Horizontal);
#endif
		}
	}


	//入力値をを垂直方向に対する値として通知
	public float Virtical {
		get {
#if !UNITY_EDITOR && UNITY_IPHONE
			return this.RotationX;
#else
			return Input.GetAxis(InputName.Vertical);
#endif
		}
	}

	
	//入力値を水平方向をX(左が負 右が正) 奥手前方向をZ(手前が負 奥が正)として通知
	public Vector3 Axis {
		get {
#if !UNITY_EDITOR && UNITY_IPHONE
			return new Vector3(this.RotationY, 0, this.RotationX);
#else
			return new Vector3(this.Horizontal, 0, this.Virtical);
#endif
		}
	}


	//ジャンプボタンを押下している最中であることを通知
	public bool JumpingInput {
		get {
#if !UNITY_EDITOR && UNITY_IPHONE
			return (Input.touchCount>0 && Input.touches[0].phase == TouchPhase.Stationary);
#else
			return Input.GetButton(InputName.Jump);
#endif
		}
	}

	//ジャンプアクションに相当する入力があることを通知
	//入力した瞬間
	public bool JumpInput {

		get {
#if !UNITY_EDITOR && UNITY_IPHONE
			return (Input.touchCount>0 && Input.touches[0].phase == TouchPhase.Began);
#else
			return Input.GetButtonDown(InputName.Jump);
#endif

		}
	}


	//スクロール速度変更アクションに相当する入力があることを通知
	//-n スクロール速度をn段階下げる, nスクロール速度をn段階上げる
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
	// ホームボタンを下にして縦持ちした際の
	//縦軸をY(正:右を下方向倒す　負:左を下方向へ倒す)
	//横軸をX(正:下を下方向へ倒す　負:上を下方向へ倒す)
	//それぞれに直行する軸をZとする(正:時計回り、負:反時計回り)
	//return new Vector3(this.HorizontalQuatanion, 0, this.VirticalQuatanion);
	//回転係数
	private float rotationFactor = 2.0f;
	public float RotationFactor{
		get{
			return this.rotationFactor;
		}
		set{
			this.rotationFactor = value;
		}
	}

	//デバイス固有の値を得る
	public float RotationX{
		get{
			float value = (Input.gyro.gravity.y - defaultQuatanion.y) * this.rotationFactor;
			return Mathf.Clamp(value,-1.0f,1.0f);
		}
	}

	public float RotationY{
		get{
			float value = (Input.gyro.gravity.x - this.defaultQuatanion.x) * this.rotationFactor;
			return Mathf.Clamp(value,-1.0f,1.0f);
		}
	}

	public float RotationZ{
		get{
			float value = (Input.gyro.gravity.z - this.defaultQuatanion.z) * this.rotationFactor;
			return Mathf.Clamp(value,-1.0f,1.0f);
		}
	}

	#endregion
}	//end of class
