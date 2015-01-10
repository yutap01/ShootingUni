using UnityEngine;
using System.Collections;

public class PlayerMove : MonoBehaviour {
	private GameObject characterObj;	//キャラクター

	public float Speed = 0.0f;
	public float Footwork = 0.0f;	//左右移動の速度
	public float JumpPower = 0.0f;	//ジャンプ力

	[SerializeField]
	private string weaponName = "PlayerWeapon";	//デフォルト
	public string WeaponName{
		get{
			return this.weaponName;
		}
	}


	[SerializeField]
	private string characterName = "BoxUnityChan";	//キャラクタとなるGameObject名
	public string CharacterName{
		get{
			return this.characterName;
		}
	}

	private bool isJumped;	//入力でジャンプ処理を受けつけたか

	private Animator characterAnimator;	//キャラクターに設定されているアニメータ

	void Awake(){
		//キャラクターを取得
		this.characterObj = Utility.GetChildFromResource(this.gameObject,this.CharacterName);	//キャラクターを設定 Playerの子にする
		this.characterAnimator = this.characterObj.GetComponent<Animator>();	//キャラクターに設定されているアニメーターを取得


		//キャラクターからFeetゲームオブジェクトを取得する
		//Feetゲームオブジェクトはキャラクターの子オブジェクトである
		//GameObject feet = this.characterObj.transform.FindChild("Feet").gameObject;
		//feetからFeetCollisionスクリプトを得る
		//FeetCollision feetCollision = feet.GetComponent<FeetCollision>();
		
		//イベントへの登録
		//feetCollision.CollisionStepEvent += new FeetCollision.FeetCollisionHandler(this.hittedByStep);
		//feetCollision.CollisionKicableEnvent += new FeetCollision.FeetCollisionHandler(this.hittedByKicable);


		//武器を取得
		Utility.GetChildFromResource(this.gameObject,this.WeaponName);
	}
	
	// Use this for initialization
	void Start () {
		//武器を取得する
	}
	
	// Update is called once per frame
	void Update () {

		//つまづき判定
		if (this.isStep()) {
			float force = 40.0f;
			this.rigidbody.AddForce(0, force, -force);
			return;
		}

		//ジャンプ処理
		//なぜかAddForceだとうまくいかない FixedUpdate内で力を消してしまう様子
		if(Input.GetButtonDown("Jump") && this.isGrounded("Ground",0.3f)){
			/*
			this.rigidbody.velocity = new Vector3(
				this.rigidbody.velocity.x,
				this.JumpPower,
				this.rigidbody.velocity.z);*/
			this.rigidbody.AddForce(0, 400, 0);
			return;
		}

		//左右移動(Characterに対して行う)
		float vx = Input.GetAxis("Horizontal") * this.Footwork * Time.deltaTime;
		if(vx !=0.0f){
			Vector3 characterPosition = this.characterObj.transform.localPosition;
			characterPosition.x += vx;
			this.characterObj.transform.localPosition = characterPosition;
		}

		//キャラクター前後移動
		float vz = Input.GetAxis("Vertical") * this.Footwork * Time.deltaTime;
		if(vz !=0.0f){
			Vector3 characterPosition = this.characterObj.transform.localPosition;
			characterPosition.z += vz;
			this.characterObj.transform.localPosition = characterPosition;
		}


	}

	void FixedUpdate(){

		//接地判定
		this.characterAnimator.SetBool("IsGrounded",this.isGrounded("Ground",0.3f));

	}

	//武器をセットする
	private void setWeapon(string name){
	}



	//タグ名tagNameのオブジェクトに接地しているか否か(検知する距離をrayDepthで指定)
	private bool isGrounded(string tagName,float rayDepth){
		//テスト用
		//return true;


		int mask = 1 << LayerMask.NameToLayer(tagName); // Groundレイヤーにのみを対象

		RaycastHit hit;
		Transform transform = this.characterObj.transform;

		Vector3 layOffset = new Vector3(0, 0.2f, 0);
		Vector3 layOrigin = transform.position + layOffset;
		return Physics.SphereCast (layOrigin,1f, Vector3.down, out hit, rayDepth,mask); 
	}

	//段差につまづいているか
	private bool isStep() {
		int mask = 1 << LayerMask.NameToLayer("Ground"); // Groundレイヤーにのみを対象
		RaycastHit hit;
		Transform transform = this.characterObj.transform;

		Vector3 layOffset = new Vector3(0, 1.0f, 0);
		Vector3 layOrigin = transform.position + layOffset;
		float distance = 1.5f + this.Speed*2;
		return (Physics.SphereCast(layOrigin, 0.5f, Vector3.forward, out hit, distance, mask));
}




	//[イベントハンドラ]段差とのヒット時に呼ばれる処理
	void hittedByStep(Collider other) {
		Debug.Log("hittedByStep");
		this.rigidbody.AddForce(0,30, 0);
	}

	//[イベントハンドラ]キック対象とのヒット時に呼ばれる処理
	void hittedByKicable(Collider other) {
		Debug.Log("hittedByKicable");
	}
}
