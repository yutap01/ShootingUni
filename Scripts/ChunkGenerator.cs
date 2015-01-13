using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[AddComponentMenu("Map/MapGenerator")]
public class ChunkGenerator : MonoBehaviour {

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
	private uint cycleNumber = 0;
	public uint CycleNumber {
		get {
			return this.cycleNumber;
		}
		set {
			this.cycleNumber = value;
			this.currentCycle = new Cycle(this.cycleNumber,new Cycle.CycleFinishDelegate(this.cycleFinished));
		}
	}

	//現在のサイクル
	private Cycle currentCycle = null;

	//全長
	public ulong FullDistance {
		get {
			return Cycle.FullDistance * this.CycleNumber + this.currentCycle.CurrentDistance;
		}
	}

	void Awake() {
		map = GetComponent<Map>();

		GameObject objPlayer = GameObject.FindGameObjectWithTag(TagName.Player);
		PlayerMove playerMove = objPlayer.GetComponent<PlayerMove>();
			
		//チャンク生成サイクルの決定
		this.CycleNumber = 0;
		this.scrollSpeed = playerMove.ScrollSpeed;
		
		//スクロール速度の変更イベントへ登録
		playerMove.ScrollSpeedChanged += new PlayerMove.PlayerValueChangeHandler(this.scrollSpeedChanged);			

		//terrainGenerator = new TerrainGenerator(map);
		//treeGenerator = new TreeGenerator(map);
	}


	void Start() {
		//スタート地点
		Vector3 position = this.createPosition;
		for (int i = 0; i < startChunks; i++) {
			this.createChunk(position,this.map,this.getBlockName());
			position.z += Chunk.SIZE_Z;
		}
		this.createPosition = position;
		this.createPosition.z -= Chunk.SIZE_Z;	//ループ内で一回余計に足している分を補正
	}


	private float chunkOffset = 0;	//最後に生成したチャンクが生成箇所から移動した距離
	void Update() {
		Vector3 position = this.createPosition;
		chunkOffset += this.scrollSpeed;

		//チャンクの生成
		if (chunkOffset > Chunk.SIZE_Z) {
			
			//生成するブロックの名前を取得する
			//TODOサイクル毎にどのブロックを使用するかを決定する
			//サイクル更新時にcurrentCycleに伝える
			//CreateChunkの名前が不要になる
			string blockName = getBlockName();

			//チャンク生成場所を補正する(移動しすぎた分、生成位置を補正する)
			//この方式だと速度を変える度に生成箇所が0に近づいていってしまう・・。
			//遠くなっていく方がマシなのだが・・・。
			//しかもこの方式は速くなっていく分には良いが遅くなっていく場合には不適合
			this.createPosition.z -= (this.chunkOffset - Chunk.SIZE_Z);
			this.createChunk(position,this.map,blockName);

			//一回余計に作って生成箇所を後ろ側にしていく？？
			//速度を変える度にチャンクの保有数が増えてしまう。
			//手前に戻り過ぎたら一回分余計に作ってプラスする処理を入れる必要がある
			//最初の生成ポイントからチャンク一つ分手前の生成ポイント
			float z = (this.startChunks - 2) * Chunk.SIZE_Z;
			if (this.createPosition.z < z) {	//元々の生成ポイントの一つ手前よりも現在の生成ポイントが近かったら
				//生成ポイントを1チャンク分後ろへ下げてすぐ生成する
				this.createPosition.z += Chunk.SIZE_Z;
				this.createChunk(this.createPosition,this.map,blockName);
			}

			this.chunkOffset = 0;
		}
	}


	//チャンクの生成
	//チャンクの作り方
	//1:マップからブロックセットを得る
	//2:ブロックセットから指定名のブロックを得る
	//3:ブロックデータの初期化(対となるブロックを渡す)
	//4:チャンクデータを作成(引数としてmap,map内のインデックスが必要だが、本プロジェクトでは無意味)
	//5:チャンクデータ(チャンクの生データ)にブロックデータ(ブロックの配置情報)をセットする
	//6:(ビルド済の)チャンクデータからチャンクを得る
	//7:チャンクは自身のupdate内で(dirtyであれば)自動的にビルドされる
	private void createChunk(Vector3 position, Map map, string blockName) {
		BlockSet blockSet = map.GetBlockSet();
		Block b = blockSet.GetBlock(blockName);
		BlockData bd = new BlockData(b);
		ChunkData cd = new ChunkData(map, new Vector3i(0, 0, 0));	//0,0,0固定


		/* 保留
				int changeX = -1;	//横位置列の中で高さが変化する位置(-1は無効値)
				//横位置列で高さが変化する位置を決める
			changeX = Random.Range(0, Chunk.SIZE_X - 1);
		}
		if (x == changeX) {	//段差が変化する位置になったら
		*/


		//チャンクデータにブロックを並べる(yは必ず0より大)
		for (int z = 0; z < Chunk.SIZE_Z; z++) {
			//サイクルに高さを決定してもらう
			int y = currentCycle.GetGenerateY();
			for (int x = 0; x < Chunk.SIZE_X; x++) {
				cd.SetBlock(bd, x, y, z);
			}
		}

		//チャンクインスタンスを取得し、値を設定
		Chunk c = cd.GetChunkInstance();
		c.SetDirty();
		c.transform.position = position;
		c.CycleNumber = this.CycleNumber;
		c.ChunkNumber = this.currentCycle.ChunkCount;	//インクリメント前であることに注意(n個目のNumberはn-1となる)
		c.transform.name = "Chunk " + c.CycleNumber + "-" + c.ChunkNumber;

		this.currentCycle.ChunkCount++;	//サイクルが終了したかどうかはCycleが判定し、通知する
	}


	//チャンク全体に適用するブロック名を通知する
	private string getBlockName() {
		//テスト用
		return "Grass";

		//実際は距離との関係で求める
		BlockSet blockSet = this.map.GetBlockSet();
		int rand = Random.Range(0, blockSet.GetCount());
		return blockSet.GetBlock(rand).GetName();
	}


	//[イベント]スクロール速度の変更があった場合
	private void scrollSpeedChanged(PlayerMove playerMove) {
		this.scrollSpeed = playerMove.ScrollSpeed;
	}

	//[イベント]カレントサイクルが終了
	private void cycleFinished() {
		//サイクルの更新
		this.CycleNumber++;	//プロパティが新たなサイクルを自動生成する
	}

}	//end of class

	
