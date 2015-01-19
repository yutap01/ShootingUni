using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[AddComponentMenu("Map/MapGenerator")]
public class LevelManager : MonoBehaviour {

	//private TerrainGenerator terrainGenerator;
	//private TreeGenerator treeGenerator;


	//レベル列
	[SerializeField]
	private Level[] levels = null;

	//天気エフェクト
	private const string rainName = "Rain";
	private GameObject rain = null;
	private const string thunderName = "Lightning";
	private GameObject thunder = null;
	private const string snowName = "Snowfall";
	private GameObject snow = null;


	//チャンクの生成位置
	private Vector3 createPosition = new Vector3(0,0,0);
	
	//スクロール速度(PlayerのScrollSpeedに依存)
	private float scrollSpeed = 0;

	[SerializeField]
	private int startChunks = 0;	//スタート時点で生成するチャンク数
		

	//最終プレイにおける走行距離
	//あくまでも最後に生成したチャンクの最後尾位置を基準としている点に注意
	private uint lastPlayDistance = 0;


	//現在のサイクル番号(最小は0)
	//サイクル番号を設定するとサイクル距離が自動的にゼロにリセットされる
	private uint levelNumber = 0;


	//現在のレベル
	private Level currentLevel = null;

	//現在の天気
	private bool isRain = false;
	private bool isSnow = false;
	private bool isThunder = false;
	private int brightness = 0;

	//次のレベルへ移行する
	private void toNextLevel() {
		this.levelNumber++;
		int idxLevel = (int)this.levelNumber % this.levels.Length;
		this.currentLevel = this.levels[idxLevel];
		this.currentLevel.Reset(this.levelNumber);		//レベルの初期化


		//レベルから得られる状態を反映
		float weatherRate = Random.Range(0.0f, 1.0f);

		//天気
		this.isRain = (this.currentLevel.RainRate > weatherRate);
 		this.isSnow = (!this.isRain && (this.currentLevel.SnowRate > weatherRate));			//雨のとき雪は降らない
		this.isThunder = (this.isRain && (this.currentLevel.ThunderRate > weatherRate));	//雨でないと雷は降らない

		//Debug.Log("rain is" + this.isRain + " thunder is " + this.isThunder);

		//ライティング
		this.brightness = this.currentLevel.Brightness;
	}

	//全長
	public ulong FullDistance {
		get {
			return Level.FullDistance * this.levelNumber + this.currentLevel.CurrentDistance;
		}
	}

	void Awake() {

		//コンポーネント取得
		Level.Map = GetComponent<Map>();
		this.rain = this.transform.FindChild(LevelManager.rainName).gameObject;
		this.rain.SetActive(false);

		this.thunder = this.transform.FindChild(LevelManager.thunderName).gameObject;
		this.thunder.SetActive(false);

		this.snow = this.transform.FindChild(LevelManager.snowName).gameObject;
		this.snow.SetActive(false);

		GameObject objPlayer = GameObject.FindGameObjectWithTag(TagName.Player);
		PlayerMove playerMove = objPlayer.GetComponent<PlayerMove>();
		
	
		//チャンク生成サイクルの決定（TODO:指定レベルで)
		this.levelNumber = 0;
		this.currentLevel = this.levels[this.levelNumber];
		this.currentLevel.Reset(levelNumber);


		this.scrollSpeed = playerMove.ScrollSpeed;
		
		//スクロール速度の変更イベントへ登録
		playerMove.ScrollSpeedChanged += new PlayerMove.PlayerValueChangeHandler(this.scrollSpeedChanged);			

		//terrainGenerator = new TerrainGenerator(map);
		//treeGenerator = new TreeGenerator(map);
	}


	void Start() {
		//スタート地点
		for (int i = 0; i < this.startChunks; i++) {
			Chunk chunk = this.currentLevel.NextChunk();
			if (chunk == null) {
				this.levelFinished();
				return;
			}

			this.initChunk(chunk);
			this.createPosition.z += Chunk.SIZE_Z;			
		}
		this.createPosition.z -= Chunk.SIZE_Z;	//以降は上記の最後の一つと同じ位置に作る
		this.lastPlayDistance = (uint)this.startChunks * Chunk.SIZE_Z;
	
	}


	private float chunkOffset = 0;	//最後に生成したチャンクが生成箇所から移動した距離
	void Update() {
		Vector3 position = this.createPosition;
		chunkOffset += this.scrollSpeed;

		//チャンク生成タイミング
		if (chunkOffset > Chunk.SIZE_Z) {

			//天気を反映
			this.rain.SetActive(this.isRain);
			this.thunder.SetActive(this.isThunder);
			this.snow.SetActive(this.isSnow);

			//チャンク生成場所を補正する(移動しすぎた分、生成位置を補正する)
			//この方式だと速度を変える度に生成箇所が0に近づいていってしまう・・。
			//遠くなっていく方がマシなのだが・・・。
			//しかもこの方式は速くなっていく分には良いが遅くなっていく場合には不適合
			this.createPosition.z -= (this.chunkOffset - Chunk.SIZE_Z);

			Chunk chunk = this.currentLevel.NextChunk();
			if (chunk == null) {
				this.levelFinished();
				return;
			}

			this.initChunk(chunk);


			//走行距離
			this.lastPlayDistance += Chunk.SIZE_Z;

			//立て看板
			//ここから



			//手前に戻り過ぎたら一回分余計に作って生成位置を後方へ移動する
			//最初の生成ポイントからチャンク一つ分手前の生成ポイント
			float z = (this.startChunks - 2) * Chunk.SIZE_Z;
			if (this.createPosition.z < z) {	//元々の生成ポイントの一つ手前よりも現在の生成ポイントが近かったら
				//生成ポイントを1チャンク分後ろへ下げてすぐ生成する
				this.createPosition.z += Chunk.SIZE_Z;
				
				//補正用のチャンク
				Chunk correctionChunk = this.currentLevel.NextChunk();
				if (correctionChunk == null) {
					this.levelFinished();
					return;
				}

				this.initChunk(correctionChunk);
			}

			this.chunkOffset = 0;
		}
	}

	//取得したチャンクの座標情報を初期化
	private void initChunk(Chunk chunk) {

		chunk.SetDirty();
		chunk.LevelNumber = this.levelNumber;
		chunk.ChunkNumber = this.currentLevel.ChunkCount - 1;

		chunk.transform.position = this.createPosition;
		chunk.transform.name = "Chunk " + chunk.LevelNumber + "-" + chunk.ChunkNumber;

		//移動速度の設定
		Chunk.ScrollSpeed = this.scrollSpeed;

		//Rigidbodyを追加
		Rigidbody rigidbody = chunk.transform.gameObject.AddComponent<Rigidbody>();
		rigidbody.constraints = RigidbodyConstraints.FreezeRotation | 
			RigidbodyConstraints.FreezePositionY | 
			RigidbodyConstraints.FreezePositionX;
		rigidbody.useGravity = false;	//重力無視
		//rigidbody.isKinematic = true;	//物理挙動無視
		rigidbody.mass = 1000;
		rigidbody.useConeFriction = false;

	}


	//[イベント]スクロール速度の変更があった場合
	private void scrollSpeedChanged(PlayerMove playerMove) {
		this.scrollSpeed = playerMove.ScrollSpeed;

		Chunk.ScrollSpeed = this.scrollSpeed;
	}


	//カレントレベルが終了
	private void levelFinished() {
		//レベルの更新
		this.toNextLevel();
	}

}	//end of class

	
