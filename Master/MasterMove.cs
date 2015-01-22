using UnityEngine;
using System.Collections;

public class MasterMove : MonoBehaviour {

	#region "定数"
		private const string playerName = "Player";	//Playerゲームオブジェクト名
		private const string levelName = "Level";	//Levelゲームオブジェクト名
	#endregion



	[SerializeField]
	private int targetFrameRate = 0;
	[SerializeField]
	private int vSyncCount = 0;


	//各種の管理者	
	private PlayerMove playerMove = null;
	private LevelManager levelManager = null;
	//ゲーム全体に対する入力
	[SerializeField]
	private MasterInput masterInput = null;



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

	void Start () {
	
	}
	
	void Update () {

		//リセット
		if (this.isReset()) {
			this.Reset();
		}
	}


	#region "動作メソッド"

	//リセット共通処理
	private void resetCommon() {
		//unity全般の設定
		//速度最適化
		QualitySettings.vSyncCount = this.vSyncCount;
		Application.targetFrameRate = this.targetFrameRate;
	}


	//リセット処理を行う
	//外部から呼び出されるためpublicとしている
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

	//キャラクタのみリセットを行う
	public void resetCharacter(string characterName) {
		this.resetCommon();
		this.playerMove.ResetPlayer(characterName);
	}

	//レベルのみリセットを行う
	public void resetLevel(uint levelNumber) {
		this.resetCommon();
		this.levelManager.ResetLevel(levelNumber);
	}

	#endregion

	#region "判定メソッド"

	//(有効な)リセット入力されたか
	private bool isReset() {
		return this.masterInput.ResetInput();
	}

	//(有効な)外部からキャラクタの設定もしくはが変更されたか
	private bool isSetCharacter() {
		return this.masterInput.SetCharacterInput();
	}

	//(有効な)外部からレベル変更されたか
	//通常にステージを攻略した際のレベル変更は除く
	private bool isSetLevel() {
		return this.masterInput.SetLevelInput();
	}

	//[イベント]プレイヤー(キャラクター)が新規レベルに到達した
	private void groundedLevelChanged(PlayerMove playerMove) {

		//TODO BGMの更新
		
		//levelManagerの天気を更新
		this.levelManager.InitWeather();
	}

	#endregion
}
