using UnityEngine;
using System.Collections;

public class PlayerInput : MonoBehaviour {


	private Vector3 defaultQuanion = new Vector3(0,0,0);

 
	//現在の角度をデフォルト値とする
	public void SetDefaultQuatanion(){
#if !UNITY_EDITOR && UNITY_IPHONE
		this.defaultQuanion = Input.gyro.gravity;
#endif
	}

	//水平方向に対する傾きを-1 から 1の値として通知する
	//TODO 縦持ち 横持ち判定 //横方向判定
	public float HorizontalQuatanion {
		get {
	#if !UNITY_EDITOR && UNITY_IPHONE
			//とりあえず右にホームボタンがある横持ち状態を前提
			return Mathf.Clamp(Input.gyro.gravity.x-this.defaultQuatanion.y,-1.0f,1.0f);
	#else
			return 0;
	#endif
		}
	}


	//垂直方向
	//TODO 縦持ち 横持ち判定 //横方向判定
	public float VirticalQuatanion {
		get {
#if !UNITY_EDITOR && UNITY_IPHONE
			//とりあえず右にホームボタンがある横持ち状態を前提
			return Mathf.Clamp(Input.gyro.gravity.y-defaultQuatanion.x,-1.0f,1.0f);
#else
			return 0;
#endif
		}
	}


	//入力値を水平方向に対する値として通知
	public float Horizontal{
		get {
#if !UNITY_EDITOR && UNITY_IPHONE
			return this.HorziontalQuatanion;
#else
			return Input.GetAxis(InputName.Horizontal);
#endif
		}
	}


	//入力値をを垂直方向に対する値として通知
	public float Virtical {
		get {
#if !UNITY_EDITOR && UNITY_IPHONE
			return this.VirticalQuatanion;
#else
			return Input.GetAxis(InputName.Vertical);
#endif
		}
	}

	
	//入力値を水平方向をX(左が負 右が正) 奥手前方向をZ(手前が負 奥が正)として通知
	public Vector3 Axis {
		get {
#if !UNITY_EDITOR && UNITY_IPHONE
			return new Vector3(this.HorizontalQuatanion, 0, this.VirticalQuatanion);
#else
			return new Vector3(this.Horizontal, 0, this.Virtical);
#endif
		}
	}


	//ジャンプアクションに相当する入力があることを通知
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


}	//end of class
