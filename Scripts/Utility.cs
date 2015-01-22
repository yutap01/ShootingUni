using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Utility{

	private const string playerName ="Player";	//プレイヤーゲームオブジェクトのTag名 かつ オブジェクト名
	private const string masterName = "Master";	//マスターゲームオブジェクトのTag名かつオブジェクト名

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
		GameObject child = Object.Instantiate(prefab) as GameObject;
		//GameObject child = Object.Instantiate (prefab,parent.transform.position, Quaternion.identity) as GameObject;
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

	//プレイヤー制御コンポーネントを取得する
	public static PlayerMove GetPlayerMoveComponent() {
		GameObject playerObject = Utility.GetPlayerObject();
		return Utility.GetSafeComponent<PlayerMove>(playerObject);
	}

	//Masterゲームオブジェクトを通知する
	public static GameObject GetMasterObject() {
		return Utility.GetGameObject(Utility.masterName, Utility.masterName);
	}

	//マスター制御コンポーネントを取得する
	public static MasterMove GetMasterMoveComponent() {
		GameObject masterObject = Utility.GetMasterObject();
		return Utility.GetSafeComponent<MasterMove>(masterObject);
	}


	//指定ゲームオブジェクトの子ゲームオブジェクトのうち、タグが指定名のものを通知する
	public static List<GameObject> GetChildObjectsByTag(GameObject parent, string tagName) {

		List<GameObject> res = new List<GameObject>();

		foreach (Transform transform in parent.transform) {
			if (transform.gameObject.tag == tagName) {
				res.Add(transform.gameObject);
			}
		}

		return res;
	}


	//GetComponentの取得失敗時にエラーとなるバージョン
	public static T GetSafeComponent<T>(this GameObject obj) where T : MonoBehaviour {
		T component = obj.GetComponent<T>();

		if (component == null) {
			Debug.LogError("Expected to find component of type "
				 + typeof(T) + " but found none", obj);
		}

		return component;
	}

	//Colliderの主体が指定のタグ名を持つか否かを判定する
	public static bool isColliderOwnerHasTag(Collider collider,string tagName) {
		return (collider.gameObject.tag == tagName);
	}


	//ランダムに色を作る(Aは1固定)
	public static Color RandomColorRGB() {
		float r = Random.Range(0.0f, 1.0f);
		float g = Random.Range(0.0f, 1.0f);
		float b = Random.Range(0.0f, 1.0f);
		return new Color(r, g, b);
	}
} // end of class
