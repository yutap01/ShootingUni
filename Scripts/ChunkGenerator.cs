using UnityEngine;
using System.Collections;


[AddComponentMenu("Map/MapGenerator")]
public class ChunkGenerator : MonoBehaviour {
	private Map map;	
	//private TerrainGenerator terrainGenerator;
	//private TreeGenerator treeGenerator;

	//チャンクの移動速度(m/flame)チャンク生成サイクルに影響
	private float scrollSpeed = 0;
	//チャンクの生成位置
	private Vector3 createPosition = new Vector3(0,0,0);

	[SerializeField]
	private int startChunks = 0;	//スタート時点で生成するチャンク数
	
	//ブロック作成Y座標
	private int GenerateY = 1;	//ブロックを作成するY位置
	private const int MAX_Y = Chunk.SIZE_Y - 1;
	private const int MIN_Y = 1;


	void Awake() {
		map = GetComponent<Map>();

		//if (this.generateCycle == 0) {
			GameObject objPlayer = GameObject.FindGameObjectWithTag(TagName.Player);
			PlayerMove playerMove = objPlayer.GetComponent<PlayerMove>();
			//チャンク生成サイクルの決定
			this.scrollSpeed = playerMove.ScrollSpeed;
			//スクロール速度の変更イベントへ登録
			playerMove.ScrollSpeedChanged += new PlayerMove.PlayerValueChangeHandler(this.scrollSpeedChanged);			

		//}
		//terrainGenerator = new TerrainGenerator(map);
		//treeGenerator = new TreeGenerator(map);
	}


	void Start() {

		//スタート地点
		Vector3 position = this.createPosition;
		for (int i = 0; i < startChunks; i++) {
			this.createChunk(position);
			position.z += Chunk.SIZE_Z;
		}
		this.createPosition = position;
		this.createPosition.z -= Chunk.SIZE_Z;	//ループ内で一回余計に足している分を補正
	}


	//生成したチャンクの数
	private static int chunkCount = 0;
	private float chunkOffset = 0;	//最後に生成したチャンクが生成箇所から移動した距離
	void Update() {
		Vector3 position = this.createPosition;
		chunkOffset += this.scrollSpeed;
		if (chunkOffset > Chunk.SIZE_Z) {
			//チャンク生成場所を補正する(移動しすぎた分、生成位置を補正する)
			//この方式だと速度を変える度に生成箇所が0に近づいていってしまう・・。
			//遠くなっていく方がマシなのだが・・・。
			//しかもこの方式は速くなっていく分には良いが遅くなっていく場合には不適合
			this.createPosition.z -= (this.chunkOffset - Chunk.SIZE_Z);
			this.createChunk(position);

			//一回余計に作って生成箇所を後ろ側にしていく？？
			//速度を変える度にチャンクの保有数が増えてしまう。
			//手前に戻り過ぎたら一回分余計に作ってプラスする処理を入れる必要がある
			//最初の生成ポイントからチャンク一つ分手前の生成ポイント
			float z = (this.startChunks - 2) * Chunk.SIZE_Z;
			if (this.createPosition.z < z) {	//元々の生成ポイントの一つ手前よりも現在の生成ポイントが近かったら
				//生成ポイントを1チャンク分後ろへ下げてすぐ生成する
				this.createPosition.z += Chunk.SIZE_Z;
				this.createChunk(this.createPosition);
			}

			this.chunkOffset = 0;
		}
	}


	//チャンクの作り方
	//1:マップからブロックセットを得る
	//2:ブロックセットから指定名のブロックを得る
	//3:ブロックデータの初期化(対となるブロックを渡す)
	//4:チャンクデータを作成(引数としてmap,map内のインデックスが必要だが、本プロジェクトでは無意味)
	//5:チャンクデータ(チャンクの生データ)にブロックデータ(ブロックの配置情報)をセットする
	//6:(ビルド済の)チャンクデータからチャンクを得る
	//7:チャンクは自身のupdate内で(dirtyであれば)自動的にビルドされる

	//チャンクの生成
	private void createChunk(Vector3 position) {
		BlockSet bs = map.GetBlockSet();
		Block b = bs.GetBlock("Grass");
		BlockData bd = new BlockData(b);
		ChunkData cd = new ChunkData(this.map,new Vector3i(0,0,0));	//0,0,0固定


		//チャンクデータにブロックを並べる(yは必ず0より大)
		for (int z = 0; z < Chunk.SIZE_Z; z++) {
			
			//横一列毎に段差の有無を判定する
			int step = getStep();	//段差を取得

			int changeX = -1;	//横位置列の中で高さが変化する位置(-1は無効値)
			if (step != 0) {	//段差があるなら
				//横位置列で高さが変化する位置を決める
				changeX = Random.Range(0, Chunk.SIZE_X - 1);
			}

			for (int x = 0; x < Chunk.SIZE_X; x++) {
				if (x == changeX) {	//段差が変化する位置になったら
					GenerateY += step;	//段を変化させる
				}
				
				cd.SetBlock(bd, x, GenerateY, z);
			}
		}


		Chunk c = cd.GetChunkInstance();
		c.SetDirty();
		c.transform.position = position;

		chunkCount++;
	}

	//段差の取得
	private int planeCounter = 0;	//段差無しが連続した回数
	private const int limitPlane = 16;	//段差間の最低平面数

	//段の上がり下がり数を返す
	//0 平面 1:1段上がる -n:n段下がる
	private int getStep() {

		//前回段差であれば必ず非段差を返す
		if (planeCounter < limitPlane) {
			planeCounter++;
			return 0;	//平面
		}

		float rate = Random.Range(0.0f, 1.0f);
		
		if (rate > 0.5 && GenerateY < MAX_Y) {	//1段上がる
			planeCounter = 0;
			return 1;	//1段上がる
		}
		if (rate < 0.1 && GenerateY > MIN_Y) {	//1段下がる
			planeCounter = 0;

			//Random前に負符号があることに注意
			return -Random.Range(1,GenerateY-MIN_Y+1);	//randomでmax値は含まれない
		}

		planeCounter++;
		return 0;	 
	}

	//[イベント]スクロール速度の変更があった場合
	private void scrollSpeedChanged(PlayerMove playerMove) {
		this.scrollSpeed = playerMove.ScrollSpeed;
	}

}	//end of class

	
