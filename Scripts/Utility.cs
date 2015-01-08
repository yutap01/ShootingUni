using UnityEngine;
using System.Collections;


public static class Utility{

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

}
