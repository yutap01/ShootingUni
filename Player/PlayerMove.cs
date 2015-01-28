using UnityEngine;
using System.Collections;

public class PlayerMove : MonoBehaviour {

	#region "プロジェクタ"

	/// <summary>
	/// Brob Shadow Projectorのキャラクタからの高さ
	/// TODO:廃止予定
	/// </summary>
	private const float shadowProjectorY = 2;	//Brob Shadow Projectorのキャラクタからの高さ
	#endregion

	#region "接地判定用"
	/// <summary>
	/// 下向きに飛ばすレイの距離
	/// </summary>
	private const float groundLayY = 0.04f;
	
	/// <summary>
	/// transform.Position+groundLayOffsetによりレイの原点を算出する
	/// </summary>
	private static readonly Vector3 groundLayOffset = new Vector3(0, 0.2f, 0);

	/// <summary>
	/// レイの半径
	/// </summary>
	private const float groundLayR = 1.0f;

	/// <summary>
	/// ジャンプ開始時点から、limitGround 回のフレームは、着地判定を行わない
	/// 着地判定をすぐ行ってしまうと、上昇中に着地と判定されてしまうことを回避する目的
	/// </summary>
	private const int limitGround = 20;

	/// <summary>
	/// 最後にジャンプ入力した時のフレーム数(接地判定が可能になるまでの待ち時間をカウントする)
	/// </summary>
	private int lastJumpedTime = 0;


	#endregion

	#region "段差判定用"
	/// <summary>
	/// 前向きに飛ばすレイの距離の基本値
	/// </summary>
	private const float stepLayZ = 0.4f;	
	
	/// <summary>
	/// スクロール速度に対する係数
	/// レイを飛ばす距離はstepLayZ + (speed * speedFactor)として算出する
	/// </summary>
	private const float speedFactorZ = 16.0f;

	/// <summary>
	/// レイの半径
	/// </summary>
	private const float stepLayR = 0.8f;

	/// <summary>
	/// キャラクター位置に以下のオフセット値を加算してレイの原点とする
	/// </summary>
	private static readonly Vector3 stepLayOffset = new Vector3(0,1.1f,0);

	/// <summary>
	/// キャラクターが段差にあたった際に受ける力
	/// TODO 速度により変動するべきではなかろうか？
	/// </summary>
	private static readonly Vector3 stepPower = new Vector3(0, 20, -30);
	#endregion

	#region "落下判定用"

	/// <summary>
	/// キャラクターが落下したと判定されるY位置
	/// </summary>
	private const float dropedLine = -10.0f;


	/// <summary>
	/// キャラクターが前後に移動できる範囲
	/// </summary>
	private static readonly ValueRange moveLimitZ = new ValueRange(1,Chunk.SIZE_Z);

	#endregion

	#region "プレイヤー状態の遷移"
	/// <summary>
	/// Plyerゲームオブジェクトの状態変更通知を受けるハンドラ型
	/// </summary>
	/// <param name="playerMove">状態はPlayerMoveで受け取る</param>
	public delegate void PlayerValueChangeHandler(PlayerMove playerMove);

	/// <summary>
	/// スクロール速度変更イベント
	/// </summary>
	public event PlayerValueChangeHandler ScrollSpeedChanged;


	/// <summary>
	/// キャラクターがいる レベル（大地）の更新イベント
	/// </summary>
	public event PlayerValueChangeHandler GroundedLevelChanged;

	/// <summary>
	/// 最後にキャラクターが接地したレベルのレベル番号
	/// </summary>
	private uint lastGroundedLevel = 0;
	public uint LastGroundedLevel {
		get {
			return this.lastGroundedLevel;
		}
	}

	#endregion

	#region "多段ジャンプ"
	/// <summary>
	/// キャラクターが接地一回ごとにジャンプ可能な回数(地上から空中＋空中でのジャンプ）
	/// </summary>
	[SerializeField]
	private int maxJumpCapacity = 1;

	/// <summary>
	/// 最終接地状態からのジャンプ可能数
	/// つまり、現在の状態であと何回ジャンプができるか
	/// </summary>
	private int jumpCapacity = 1;

	#endregion


	/// <summary>
	/// キャラクターゲームオブジェクト
	/// </summary>
	private GameObject characterObj;

	#region "キャラクター固有値"
	//成長との関係は未定

	/// <summary>
	/// スクロール速度
	/// </summary>
	[SerializeField]
	private float scrollSpeed = 0.0f;
	public float ScrollSpeed {
		get {
			return scrollSpeed;
		}
		set {
			if (value == this.scrollSpeed) {
				return;
			}
			scrollSpeed = value;
			//スクロールスピード変更イベントを発行する
			if (this.ScrollSpeedChanged != null) {
				this.ScrollSpeedChanged(this);
			}
		}
	}

	/// <summary>
	/// 前後左右移動の速度(スクロール速度とは別)
	/// </summary>
	[SerializeField]
	private float footwork = 0.0f;

	/// <summary>
	/// ジャンプ力
	/// </summary>
	[SerializeField]
	private float jumpPower = 0.0f;

	/// <summary>
	/// 重力
	/// </summary>
	[SerializeField]
	private float gravity = 0.0f;	//重力の代わりとなる下向きの力


	/// <summary>
	/// 重力に対する抵抗力
	/// ジャンプボタンを押し続けている間効果を発揮する
	/// </summary>
	[SerializeField]
	private float endureGravity = 0.0f;

	/// <summary>
	/// キャラクター(プレハブ)に設定されているアニメータ
	/// </summary>
	private Animator characterAnimator;

	/// <summary>
	/// 現在のキャラクター名
	/// 現時点では、リソース内のプレハブ名と同一であることを前提としている
	/// </summary>
	[SerializeField]
	private string characterName = "BoxUnityChan";	//キャラクタとなるGameObject名
	public string CharacterName {
		get {
			return this.characterName;
		}
	}

	#endregion

	/// <summary>
	/// 現在のキャラクターが現在持っている武器
	/// </summary>
	[SerializeField]
	private string weaponName = "PlayerWeapon";	//デフォルト
	public string WeaponName {
		get {
			return this.weaponName;
		}
	}


	/// <summary>
	/// プレイヤーの初期位置
	/// </summary>
	[SerializeField]
	private Vector3 startPosition;

	/// <summary>
	/// 入力管理者
	/// </summary>
	[SerializeField]
	private PlayerInput playerInput = null;

	
	/// <summary>
	/// プレイヤーオブジェクトの初期化
	/// </summary>
	void Awake(){

		this.ResetPlayer();

		//Blob Shadow Projectorを得る
		//TODO:即値を撤廃
		//TODO:Shadowのリセットコードを別にする
		//TODO:Shadowはキャッシュする
		GameObject shadow = Utility.GetChildFromResource(this.characterObj, "Blob Shadow Projector");
		shadow.transform.localPosition = new Vector3(0, PlayerMove.shadowProjectorY,0);
		shadow.transform.rotation = Quaternion.Euler(90, 0, 0);

 
		//武器を取得
		Utility.GetChildFromResource(this.gameObject,this.WeaponName);
	}
	

	/// <summary>
	/// Start時
	/// </summary>
	void Start () {
		//武器を取得する
	}
	

	/// <summary>
	/// Update時
	/// TODO デバイス入力によって発生処理はイベントハンドラ経由にする
	/// TODO 物理挙動によって発生する処理はfixedUpdateに移動する
	/// </summary>
	void Update () {

		//ジャンプ瞬間
		if (this.isJumped()) {
			this.jumpProc();
			return;
		}

		//つまづき中ずっと
		if (this.isStep()) {
			this.stepProc();
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

	/// <summary>
	/// FixedUpdate時
	/// </summary>
	void FixedUpdate(){

		//アニメーションの接地状態制御
		this.characterAnimator.SetBool("IsGrounded",this.isGrounded());

	}



	#region "動作メソッド"
	/// <summary>
	/// ジャンプした瞬間に行う処理
	/// </summary>
	private void jumpProc() {
		--this.jumpCapacity; //TODO:ジャンプアニメーションを再起動しなければいけない？？
		//ジャンプの瞬間はY速度を0にする
		Vector3 velocity = this.rigidbody.velocity;
		velocity.y = 0;
		this.rigidbody.velocity = velocity;

		//ジャンプ開始時刻を覚える(接地判定が有効になるまでの時間をカウントするため)
		this.lastJumpedTime = Time.frameCount;

		this.rigidbody.AddForce(0, this.jumpPower, 0);
	}

	/// <summary>
	/// 空中にいる間、毎フレーム行う処理
	/// </summary>
	private void inAirProc() {
		//今現在は何も行っていない
	}

	/// <summary>
	/// つまづいている間、毎フレーム行う処理
	/// </summary>
	private void stepProc() {
		this.rigidbody.AddForce(PlayerMove.stepPower);
	}

	/// <summary>
	/// 通常状態において、毎フレーム行う処理
	/// </summary>
	private void moveProc() {
		Vector3 axis = this.playerInput.Axis;
		this.characterObj.transform.position += axis * this.footwork * Time.deltaTime;
	}

	/// <summary>
	/// キック対象との衝突時に行う処理
	/// </summary>
	/// <param name="other"></param>
	private void kickProc(Collider other) {
		Debug.Log("hittedByKicable");
	}

	/// <summary>
	/// 落下した際に行う処理
	/// </summary>
	private void droppedProc(){
		//Application.LoadLevel(Application.loadedLevelName);
		
		//落下時につき処理効率は無視
		MasterMove masterMove = Utility.GetMasterMoveComponent();
		masterMove.Reset();		
	}


	/// <summary>
	/// 武器をセットまたは交換する
	/// </summary>
	/// <param name="name"></param>
	private void setWeapon(string name) {
		//未実装
	}
	
	
	/// <summary>
	/// キャラクターをセットもしくは交換する
	/// </summary>
	/// <param name="name"></param>
	private void setCharacter(string name) {
		if (this.characterObj != null) {
			GameObject.Destroy(this.characterObj);
		}
		this.characterObj = Utility.GetChildFromResource(this.gameObject, this.CharacterName);	//キャラクターを設定 Playerの子にする
		this.characterObj.transform.position = this.transform.position;
	
		//TODO キャラクターステータスをプレイヤーにコピー
	}


	/// <summary>
	/// プレイヤー状態のリセット
	/// キャラクターが変更されていないことを前提としている
	/// </summary>
	public void ResetPlayer() {
		this.transform.position = this.startPosition;
		this.setCharacter(this.characterName);

		//アニメーター
		this.characterAnimator = this.characterObj.GetComponent<Animator>();

		//TODO お金や取得アイテムの情報をデータファイルから再設定する
	}


	/// <summary>
	/// プレイヤー状態のリセット
	/// 指定名のキャラクターに変更した後、リセットを行う
	/// </summary>
	public void ResetPlayer(string characterName) {
		this.characterName = characterName;
		this.ResetPlayer();
	}

	#endregion


	#region "状況判定メソッド"

	
	/// <summary>
	/// 有効なジャンプ入力が行われたか
	/// </summary>
	/// <returns></returns>
	private bool isJumped() {
		return this.playerInput.JumpInput && (this.jumpCapacity > 0);
	}


	/// <summary>
	/// 接地判定(引数なしバージョン)
	/// </summary>
	/// <returns></returns>
	private bool isGrounded() {
		//テスト用
		//return true;
		//TODO: 即値撤廃
		return this.isGrounded("Ground", PlayerMove.groundLayY);
	}

	/// <summary>
	/// 接地判定
	/// </summary>
	/// <param name="tagName">タグ名が指定値のものを地面として認識する</param>
	/// <param name="rayDepth">検索用のレイを下方に飛ばす距離</param>
	/// <returns></returns>
	private bool isGrounded(string tagName, float rayDepth) {
		//接地判定が可能になるまでの時間を経過しているかチェック
		if ((Time.frameCount - this.lastJumpedTime) < PlayerMove.limitGround) {
			return false;
		} 

		int mask = 1 << LayerMask.NameToLayer(tagName); // Groundレイヤーにのみを対象

		RaycastHit hit;
		Transform transform = this.characterObj.transform;
		Vector3 layOrigin = transform.position + PlayerMove.groundLayOffset;

		bool grounded = Physics.SphereCast (layOrigin,PlayerMove.groundLayR, Vector3.down, out hit, rayDepth,mask);

		//衝突相手(ground)からチャンクを得る
		if (grounded) {
			Chunk chunk = hit.transform.gameObject.GetComponent<Chunk>();
			if (chunk != null) {

				//キャラクタが新規レベルへ到達した場合イベント発行
				bool levelUpdate = this.lastGroundedLevel != chunk.LevelNumber;
				this.lastGroundedLevel = chunk.LevelNumber;
				if (levelUpdate) {
					this.GroundedLevelChanged(this);
				}
			}
		}
		return grounded;
	}


	/// <summary>
	/// つまづき判定
	/// </summary>
	/// <returns></returns>
	private bool isStep() {
		//isGroundedを前提とする
		if (!this.isGrounded()) {
			return false;
		}

		int mask = 1 << LayerMask.NameToLayer("Ground"); // Groundレイヤーにのみを対象
		
		RaycastHit hit;
		Transform transform = this.characterObj.transform;
		Vector3 layOrigin = transform.position + PlayerMove.stepLayOffset;
		float distance = PlayerMove.stepLayZ + this.ScrollSpeed * PlayerMove.speedFactorZ;
		return (Physics.SphereCast(layOrigin,PlayerMove.stepLayR, Vector3.forward, out hit, distance, mask));
	}

	/// <summary>
	/// 落下判定
	/// </summary>
	/// <returns></returns>
	private bool isDropped() {
		return (this.characterObj.transform.position.y < PlayerMove.dropedLine);
	}

	#endregion


	/// <summary>
	/// 重力を受ける
	/// 重力の抵抗力を考慮している
	/// </summary>
	private void getGravity() {
		//チャンプボタンを押しているときは下向きの力が減少する
		//減少値はキャラクター固有とする
		float g = this.gravity ; //要修正：高い場所ほど重力が小さくなるように（ちょっと変な話ではあるが）
		if (this.playerInput.JumpingInput) {
			g -= this.endureGravity;
		}
		this.rigidbody.AddForce(0, -g, 0);
	}

	/// <summary>
	/// 移動範囲制限
	/// Z値のみを考慮している
	/// </summary>
	private void limitMoveZ() {
		Vector3 position = this.characterObj.transform.position;
		position.z = Mathf.Clamp(position.z,(float)PlayerMove.moveLimitZ.Min,(float)PlayerMove.moveLimitZ.Max);
		this.characterObj.transform.position = position;
	}

}	//end of class
