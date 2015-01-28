using UnityEngine;
using System.Collections;

/// <summary>
/// ゲーム全体の制御クラス
/// 但し入力の監視はMasterInputが行う
/// </summary>
public class MasterMove : MonoBehaviour {

	#region "定数"
	/// <summary>
	/// Playerゲームオブジェクト名
	/// </summary>
	private const string playerName = "Player";
	
	/// <summary>
	/// Levelゲームオブジェクト名
	/// </summary>
	private const string levelName = "Level";
	#endregion



	/// <summary>
	/// 目標フレームレート
	/// TODO:フレームレートに対する倍率を考慮すべき
	/// </summary>
	[SerializeField]
	private int targetFrameRate = 0;
	
	/// <summary>
	/// 垂直リフレッシュレート
	/// </summary>
	[SerializeField]
	private int vSyncCount = 0;	//0は非同期


	//各種の管理者
	/// <summary>
	/// プレイヤー管理者
	/// </summary>
	private PlayerMove playerMove = null;

	/// <summary>
	/// レベル(舞台)管理者
	/// </summary>
	private LevelManager levelManager = null;

	
	/// <summary>
	/// ゲーム全体に対する入力
	/// TODO:今は直接PlayerInputが入力を監視しているが
	/// TODO:上位にとって不要な入力を下位にたらい回しする機構があっても良いのではないだろうか？？
	/// </summary>
	[SerializeField]
	private MasterInput masterInput = null;


	/// <summary>
	/// 管理者の初期化
	/// 但し各管理者のファーストリセットはそれぞれの初期化メソッドで行っている
	/// TODO:上記の方法で管理しているため、MasterMoveのResetが呼べなくなっている
	/// TODO:それぞれの管理者のファーストリセットをAwake時やStart時には呼ばず、常にMasterMoveのResetから呼ばれるように修正すべきだ
	/// </summary>
	void Awake() {

		//管理者を取得
		this.playerMove = transform.FindChild(MasterMove.playerName).GetComponent<PlayerMove>();
		//到達レベルの更新イベントに登録
		this.playerMove.GroundedLevelChanged += new PlayerMove.PlayerValueChangeHandler(this.groundedLevelChanged);

		this.levelManager = transform.FindChild(MasterMove.levelName).GetComponent<LevelManager>();


		//this.Reset(); //管理者の初期化コードに任せる
		//QualitySettings.vSyncCount = this.vSyncCount;
		//Application.targetFrameRate = this.targetFrameRate;

	}


	/// <summary>
	/// イベントの監視
	/// TODO:イベントループはMasterInputに持たせるべきではないだろうか？
	/// TODO:少なくとも入力イベントの監視を行う限りにおいては。
	/// </summary>
	void Update () {

		//リセット
		if (this.isReset()) {
			this.Reset();
		}
	}


	#region "動作メソッド"

	//リセット共通処理
	/// <summary>
	/// リセット処理
	/// TODO：イベントハンドラから呼ばれるように変更すべきではないだろうか？
	/// </summary>
	private void resetCommon() {
		//unity全般の設定
		//速度最適化
		QualitySettings.vSyncCount = this.vSyncCount;
		Application.targetFrameRate = this.targetFrameRate;
	}


	//リセット処理を行う
	//外部から呼び出される可能性を考慮してpublicとしている

	/// <summary>
	/// リセット処理
	/// </summary>
	public void Reset() {

		this.resetCommon();

		//フォグ
		Color fogColor = Utility.RandomColorRGB();
		RenderSettings.fogColor = fogColor;
		RenderSettings.fog = true;

		//環境光
		Color ambientColor = Utility.RandomColorRGB();
		RenderSettings.ambientLight = ambientColor;

		//メインカメラ
		Color background = Color.black; //Utility.RandomColorRGB();
		//Camera.main.backgroundColor = background;

		//色をコンソール出力
		Debug.Log("Color : " + fogColor.r + "," + fogColor.g + "," + fogColor.b +
			"," + ambientColor.r + "," + ambientColor.g + "," + ambientColor.b +
			"," + background.r  + "," + background.g + "," + background.b);

		//管理者にリセットを通知(順序を維持すること)
		this.playerMove.ResetPlayer();
		this.levelManager.ResetLevel(this.playerMove.LastGroundedLevel);

	}


	//キャラクタ単独のリセットを行う
	public void resetCharacter(string characterName) {
		this.resetCommon();
		this.playerMove.ResetPlayer(characterName);
	}

	
	//レベル単独のリセットを行う
	public void resetLevel(uint levelNumber) {
		this.resetCommon();
		this.levelManager.ResetLevel(levelNumber);
	}

	#endregion


	#region "判定メソッド"

	/// <summary>
	/// リセット入力に対する有効性の判定
	/// </summary>
	/// <returns></returns>
	private bool isReset() {
		return this.masterInput.ResetInput();
	}


	/// <summary>
	/// キャラクタの変更（または新規設定）に対する有効性の判定
	/// </summary>
	/// <returns></returns>
	private bool isSetCharacter() {
		return this.masterInput.SetCharacterInput();
	}

	/// <summary>
	/// レベル変更指示に対する有効性の判定
	/// </summary>
	/// <returns></returns>
	private bool isSetLevel() {
		return this.masterInput.SetLevelInput();
	}


	/// <summary>
	/// イベントハンドラ
	/// プレイヤーが新しいレベル(舞台)に到達した
	/// 単にレベルが切り替わったことを取得
	/// 最高到達点とは無関係
	/// </summary>
	/// <param name="playerMove"></param>
	private void groundedLevelChanged(PlayerMove playerMove) {

		//TODO BGMの更新（但しLevel.Resetで実装する方が順当と思われる）
		
		//levelManagerの天気を更新
		//TODO:直接メソッドを呼ぶのではなく、プレイヤーが現在存在するレベルが変更されたことをただlevelマネージャに伝えるべきだ
		//TODO:MasterMoveを経由する必要があるだろうか？？直接levelManagerがイベントをハンドルしても良いのではなかろうか？
		this.levelManager.InitWeather();
	}

	#endregion
}
