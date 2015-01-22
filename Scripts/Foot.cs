using UnityEngine;
using System.Collections;

public class Foot : MonoBehaviour {


	//キャラクター救済時の補正値
	private const float recoverY = 0.5f;

	//キャラクターTransform
	Transform characterTransform = null;
	//プレイヤーのTransform
	Transform playerTransform = null;

	//地面にめり込んだ時の強制排出量
	private const float escapeY = 0.1f;


	void Awake() {
		this.characterTransform = this.transform.parent;	//Footはキャラクターの配下
		this.playerTransform = Utility.GetPlayerObject().transform;
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	//TODO
	//救済の際にダメージを受ける
	//ダメージアニメーション

	//チャンクに足がめり込んだ瞬間(非常救済措置)
	private void OnTriggerEnter(Collider collider) {
		this.recoverCharacter(collider);
	}
	private void OnTriggerStay(Collider collider) {
		this.recoverCharacter(collider);
	}


	//チャンクにめり込んだ体を地上へ戻す
	private void recoverCharacter(Collider collider) {
		//めり込んだ相手がチャンクであれば
		if (Utility.isColliderOwnerHasTag(collider, TagName.Ground)) {
			Chunk chunk = collider.gameObject.GetComponent<Chunk>();
			//チャンクから高さ情報を得る
			if (chunk != null) {
				Vector3 characterPos = this.characterTransform.position;
				int height = chunk.HeightWithGlobalPos(characterPos);
				//高さの修正
				//調整値を要する可能性がある
				if (height != 0) {
					//位置の補正はPlayerに対して行う
					Vector3 correctionPosition = this.playerTransform.position;
					correctionPosition.y = height + Foot.recoverY;
					this.playerTransform.position = correctionPosition;

				}
			}
		}
	}
}
