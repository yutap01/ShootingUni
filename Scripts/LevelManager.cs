using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[AddComponentMenu("Map/MapGenerator")]
public class LevelManager : MonoBehaviour {

	private Map map;	
	//private TerrainGenerator terrainGenerator;
	//private TreeGenerator treeGenerator;

	//チャンクの生成位置
	private Vector3 createPosition = new Vector3(0,0,0);
	
	//スクロール速度(PlayerのScrollSpeedに依存)
	private float scrollSpeed = 0;

	[SerializeField]
	private int startChunks = 0;	//スタート時点で生成するチャンク数
		


	//現在のサイクル番号(最小は0)
	//サイクル番号を設定するとサイクル距離が自動的にゼロにリセットされる
	private uint levelNumber = 0;
	public uint LevelNumber {
		get {
			return this.levelNumber;
		}
		set {
			this.levelNumber = value;
			this.currentLevel = new Level(this.levelNumber,this.map,this.blockName(levelNumber));
		}
	}

	//レベル番号に応じた地面ブロック名を取得
	private string blockName(uint levelNumer) {
		BlockSet blockSet = this.map.GetBlockSet();
		int idx = (int)levelNumber % blockSet.GetCount();
		return blockSet.GetBlock(idx).GetName();
	}

	//現在のレベル
	private Level currentLevel = null;

	//全長
	public ulong FullDistance {
		get {
			return Level.FullDistance * this.LevelNumber + this.currentLevel.CurrentDistance;
		}
	}

	void Awake() {
		map = GetComponent<Map>();

		GameObject objPlayer = GameObject.FindGameObjectWithTag(TagName.Player);
		PlayerMove playerMove = objPlayer.GetComponent<PlayerMove>();
			
		//チャンク生成サイクルの決定
		this.LevelNumber = 0;
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
	}


	private float chunkOffset = 0;	//最後に生成したチャンクが生成箇所から移動した距離
	void Update() {
		Vector3 position = this.createPosition;
		chunkOffset += this.scrollSpeed;

		//チャンク生成タイミング
		if (chunkOffset > Chunk.SIZE_Z) {

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
		chunk.LevelNumber = this.LevelNumber;
		chunk.ChunkNumber = this.currentLevel.ChunkCount - 1;

		chunk.transform.position = this.createPosition;
		chunk.transform.name = "Chunk " + chunk.LevelNumber + "-" + chunk.ChunkNumber;

		//移動速度の設定
		Chunk.ScrollSpeed = this.scrollSpeed;
	}


	//[イベント]スクロール速度の変更があった場合
	private void scrollSpeedChanged(PlayerMove playerMove) {
		this.scrollSpeed = playerMove.ScrollSpeed;

		Chunk.ScrollSpeed = this.scrollSpeed;
	}


	//カレントサイクルが終了
	private void levelFinished() {
		//レベルの更新
		this.LevelNumber++;	//プロパティが新たなサイクルを自動生成する
	}

}	//end of class

	
