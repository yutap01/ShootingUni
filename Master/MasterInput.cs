using UnityEngine;
using System.Collections;

public class MasterInput : MonoBehaviour {

	//リセットに相当する入力がされたか
	public bool ResetInput(){
		return Input.GetKeyDown(KeyCode.R);
	}

	//キャラクタの設定もしくはが変更に相当する入力がされたか
	public bool SetCharacterInput(){
		return false;	//dummy
	}

	//レベル変更されたかに相当する入力がされたか
	public bool SetLevelInput() {
		return false;	//dummy
	}


}	//end of class
