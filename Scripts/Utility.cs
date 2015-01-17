using UnityEngine;
using System.Collections;


public static class Utility{

	private const string playerName ="Player";	//プレイヤーオブジェクトのTag名 かつ オブジェクト名

	//指定名のプレハブをリソースから取得する(インスタンス化はしない)
	public static GameObject GetPrefabFromResource(string prefabName){

		//プレバブのパスを生成
		string prefabPath = "Prefabs/" + prefabName;
		GameObject prefab = (GameObject)Resources.Load (prefabPath);
		if(prefab == null){
			Debug.LogError("プレハブ("+ prefabName + ")の取得に失敗");
		}

		return prefab;
	}
	
	//指定のプレハブからゲームオブジェクトを生成して、指定ゲームオブジェクトの子とする
	//子ゲームオブジェクト(インスタンス化済)を返す
	public static GameObject GetChildFromResource(GameObject parent,string prefabName){
		//プレハブを取得
		GameObject prefab = GetPrefabFromResource(prefabName);

		//インスタンス化
		GameObject child = Object.Instantiate (prefab,parent.transform.position, Quaternion.identity) as GameObject;
		if(child == null){
			Debug.LogError("Child化(parent:" + parent.name + " child:" + prefabName + ")に失敗"); 
		}

		//子とする
		child.transform.parent = parent.transform;
		return child;
	}

	//指定のタグ名の指定名のゲームオブジェクトを取得する
	//存在しない場合はnullを通知
	public static GameObject GetGameObject(string tagName,string objectName){
		GameObject[] objects = GameObject.FindGameObjectsWithTag(tagName);
		foreach(GameObject obj in objects){
			if(obj.name == objectName){
				return obj;
			}
		}

		return null;
	}

	//プレイヤーゲームオブジェクトを検出して通知する
	//前提:Tag名 Player かつ オブジェクト名 Player
	public static GameObject GetPlayerObject(){
		return Utility.GetGameObject(Utility.playerName, Utility.playerName);
	}

} // end of class
