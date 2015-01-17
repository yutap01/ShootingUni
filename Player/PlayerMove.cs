using UnityEngine;
using System.Collections;

public class PlayerMove : MonoBehaviour {

	#region "定数"

	private const float shadowProjectorY = 2;	//Brob Shadow Projectorのキャラクタからの高さ
	//接地判定用
	private const float groundLayY = 0.1f;					//下向きに飛ばすレイの距離
	private static readonly Vector3 groundLayOffset = new Vector3(0, 0.2f, 0);	//transform.Positionに加算することでレイの原点を決定する
	private const float groundLayR = 1.0f;							//レイの半径
	//段差用
	private const float stepLayZ = 0.8f;	//前向きに飛ばすレイの距離の基本値
	private const float speedFactorZ = 12.0f;	//速度に対する係数　//距離はstepLayZ + (speed * speedFactor);
	private const float stepLayR = 0.8f;
	private static readonly Vector3 stepLayOffset = new Vector3(0,1.1f,0);
	private static readonly Vector3 stepPower = new Vector3(0, 30, -30);	//段差にあたったときにかかる、上向き兼後ろ向きの力
	//落下判定用
	private const float dropedLine = -10.0f;
	//移動範囲制限
	private static readonly ValueRange moveLimitZ = new ValueRange(1,Chunk.SIZE_Z);


	#endregion



	//変更値を受け入れるハンドラデリゲート型
	public delegate void PlayerValueChangeHandler(PlayerMove playerMove);

	//スクロール速度の変更イベント
	public event PlayerValueChangeHandler ScrollSpeedChanged;

	private GameObject characterObj;	//キャラクター

	//キャラクターが接地ごとにジャンプ可能な回数
	[SerializeField]
	private int maxJumpCapacity = 1;
	//最終接地状態からのジャンプ可能数
	private int jumpCapacity = 1;


	[SerializeField]
	private float scrollSpeed = 0.0f;
	public float ScrollSpeed {
		get {
			return scrollSpeed;
		}
		set {
			if(value == this.scrollSpeed){
				return;
			}
			scrollSpeed = value;
			//スクロールスピード変更イベントを発行する
			if (this.ScrollSpeedChanged != null) {
				this.ScrollSpeedChanged(this);
			}
		}
	}

	[SerializeField]
	private float footwork = 0.0f;	//左右移動の速度
	
	[SerializeField]
	private float jumpPower = 0.0f;	//ジャンプ力

	[SerializeField]
	private float gravity = 0.0f;	//重力の代わりとなる下向きの力
	[SerializeField]
	private float endureGravity = 0.0f;	//ジャンプボタンを押し続けることで重力に抗う力

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

	private Animator characterAnimator;	//キャラクターに設定されているアニメータ


	//入力
	[SerializeField]
	private PlayerInput playerInput = null;
	
	void Awake(){
		//unity全般の設定
		//速度最適化
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = 60;
		

		//キャラクターを取得
		this.characterObj = Utility.GetChildFromResource(this.gameObject,this.CharacterName);	//キャラクターを設定 Playerの子にする
		
		//Blob Shadow Projectorを得る
		GameObject shadow = Utility.GetChildFromResource(this.characterObj, "Blob Shadow Projector");
		shadow.transform.localPosition = new Vector3(0, PlayerMove.shadowProjectorY,0);	//何か基準となる値はないか？？
		shadow.transform.rotation = Quaternion.Euler(90, 0, 0);
 
		
			this.characterAnimator = this.characterObj.GetComponent<Animator>();	//キャラクターに設定されているアニメーターを取得

		//武器を取得
		Utility.GetChildFromResource(this.gameObject,this.WeaponName);
	}
	
	// Use this for initialization
	void Start () {
		//武器を取得する
	}
	
	// Update is called once per frame
	void Update () {

		//つまづき中ずっと
		if (this.isStep()) {
			this.stepProc();
			return;
		}

		//ジャンプ瞬間
		if(this.isJumped()){
			this.jumpProc();
			return;
		}

		//陸上ずっと
		if (this.isGrounded()) {
			this.jumpCapacity = this.maxJumpCapacity;
		} else {
			this.inAirProc();	//現在は何も行っていない
		}

		//常に行う処理
		if (this.playerInput) {
			this.getGravity();
			this.moveProc();
			this.limitMoveZ();//移動範囲制限(チェックはZのみ行う)
		}


		//落下判定
		if (this.isDropped()){
			this.droppedProc();
		}

		//スクロール速度の変化をテスト
		this.ScrollSpeed += this.playerInput.ScrollSpeedInput *0.05f;
	}


	void FixedUpdate(){

		//接地判定(アニメーション制御のため)
		this.characterAnimator.SetBool("IsGrounded",this.isGrounded());

	}

	//武器をセットする
	private void setWeapon(string name){
		//未実装
	}


	#region "動作メソッド"
	//ジャンプの瞬間
	private void jumpProc() {
		//ジャンプの瞬間はY速度を0にする
		Vector3 velocity = this.rigidbody.velocity;
		velocity.y = 0;
		this.rigidbody.velocity = velocity;


		this.rigidbody.AddForce(0, this.jumpPower, 0);
		this.jumpCapacity--; //ジャンプアニメーションを再起動しなければいけない
	}

	//空中ずっと
	private void inAirProc() {
		//今現在は何も行っていない
	}

	//つまづき中ずっと
	private void stepProc() {
		this.rigidbody.AddForce(PlayerMove.stepPower);
	}

	//常に
	private void moveProc() {
		Vector3 axis = this.playerInput.Axis;
		this.characterObj.transform.position += axis * this.footwork * Time.deltaTime;
	}

	//キック対象とのヒット瞬間
	private void kickProc(Collider other) {
		Debug.Log("hittedByKicable");
	}

	//落下時
	private void droppedProc(){
		Application.LoadLevel(Application.loadedLevelName);
	}

	#endregion



	#region "状況判定メソッド"

	//有効なジャンプ入力が行われたか
	private bool isJumped() {
		return this.playerInput.JumpInput && this.jumpCapacity > 0;
	}


	//接地判定(引数なしバージョン)
	private bool isGrounded() {
		//テスト用
		//return true;
		return this.isGrounded("Ground", PlayerMove.groundLayY);
	}
	//接地判定(地面のタグ名と検索距離を指定)
	private bool isGrounded(string tagName, float rayDepth) {
		int mask = 1 << LayerMask.NameToLayer(tagName); // Groundレイヤーにのみを対象

		RaycastHit hit;
		Transform transform = this.characterObj.transform;
		Vector3 layOrigin = transform.position + PlayerMove.groundLayOffset;
		return Physics.SphereCast (layOrigin,PlayerMove.groundLayR, Vector3.down, out hit, rayDepth,mask); 
	}



	//段差につまづいているか
	private bool isStep() {
		int mask = 1 << LayerMask.NameToLayer("Ground"); // Groundレイヤーにのみを対象
		
		RaycastHit hit;
		Transform transform = this.characterObj.transform;
		Vector3 layOrigin = transform.position + PlayerMove.stepLayOffset;
		float distance = PlayerMove.stepLayZ + this.ScrollSpeed * PlayerMove.speedFactorZ;
		return (Physics.SphereCast(layOrigin,PlayerMove.stepLayR, Vector3.forward, out hit, distance, mask));
	}

	//落下判定
	private bool isDropped() {
		return (this.characterObj.transform.position.y < PlayerMove.dropedLine);
	}

	#endregion


	//重力を受ける
	private void getGravity() {
		//チャンプボタンを押しているときは下向きの力が減少する
		//減少値はキャラクター固有とする
		float g = this.gravity ; //要修正：高い場所ほど重力が小さくなるように（ちょっと変な話ではあるが）
		if (this.playerInput.JumpingInput) {
			g -= this.endureGravity;
		}
		this.rigidbody.AddForce(0, -g, 0);
	}

	//Z範囲のみ制限をうける
	private void limitMoveZ() {
		Vector3 position = this.characterObj.transform.position;
		position.z = Mathf.Clamp(position.z,(float)PlayerMove.moveLimitZ.Min,(float)PlayerMove.moveLimitZ.Max);
		this.characterObj.transform.position = position;
	}


}
