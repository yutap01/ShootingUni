using UnityEngine;
using System.Collections;


//TODO:MaterMoveのゲームサイクルから毎フレーム呼ばれる形式で良いだろうか？？
//TODO:イベントループはMasterInputが持ち、イベントを発行させた方が美しくないか？


/// <summary>
/// ゲーム全体に対する入力を監視する
/// あくまでも入力の有無のみをチェックしている
/// </summary>
public class MasterInput : MonoBehaviour {

	//リセットに相当する入力がされたか

	/// <summary>
	/// リセットコマンド相当の入力が検知されたか
	/// </summary>
	/// <returns></returns>
	public bool ResetInput(){
		return Input.GetKeyDown(KeyCode.R);
	}


	/// <summary>
	/// キャラクターの変更（新規設定）に相当する入力がされたか
	/// </summary>
	/// <returns></returns>
	public bool SetCharacterInput(){
		return false;	//dummy
	}

	/// <summary>
	/// レベルの強制変更コマンドが入力されたか
	/// </summary>
	/// <returns></returns>
	public bool SetLevelInput() {
		return false;	//dummy
	}


}	//end of class
