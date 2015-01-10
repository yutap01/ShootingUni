using UnityEngine;
using System.Collections;


[AddComponentMenu("Map/MapGenerator")]
public class ChunkGenerator : MonoBehaviour {
	private Map map;	
	//private TerrainGenerator terrainGenerator;
	//private TreeGenerator treeGenerator;

	private float generateCycle = 0;
	private const int startChunks = 5;	//スタート時点で生成するチャンク数
	
	//ブロック作成Y座標
	private int GenerateY = 1;	//ブロックを作成するY位置
	private const int MAX_Y = Chunk.SIZE_Y - 1;
	private const int MIN_Y = 1;


	//段差
	private enum Step_enum {
		Step_down = -1,
		Step_plane = 0,
		Step_up = 1
	};

	void Awake() {
		map = GetComponent<Map>();

		if (this.generateCycle == 0) {
			GameObject objPlayer = GameObject.FindGameObjectWithTag(TagName.Player);
			float scrollSpeed = objPlayer.GetComponent<PlayerMove>().Speed;
			this.generateCycle = Chunk.SIZE_Z / scrollSpeed;
		}
		//terrainGenerator = new TerrainGenerator(map);
		//treeGenerator = new TreeGenerator(map);
	}

	void Start() {

		//スタート地点
		for (int i = 0; i < startChunks; i++) {
			this.createChunk(new Vector3(0, 0, i * Chunk.SIZE_Z));
		}
	}

	//生成したチャンクの数
	private static int chunkCount = 0;
	void Update() {
		Vector3 position = new Vector3(0, 0, (startChunks-1)*Chunk.SIZE_Z);
		if (Time.frameCount % this.generateCycle == 0) {
			this.createChunk(position);
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
			Step_enum step = getStep();	//段差を取得

			int changeX = -1;	//横位置列の中で高さが変化する位置(-1は無効値)
			if (step != Step_enum.Step_plane) {	//段差があるなら
				//横位置列で高さが変化する位置を決める
				changeX = Random.Range(0, Chunk.SIZE_X - 1);
			}

			for (int x = 0; x < Chunk.SIZE_X; x++) {
				if (x == changeX) {	//段差が変化する位置になったら
					GenerateY += (int)step;	//段差を変化させる
				}
				
				cd.SetBlock(bd, x, GenerateY, z);
			}
		}
		cd.SetBlock(bd, 8, 2, 8);


		Chunk c = cd.GetChunkInstance();
		c.SetDirty();
		c.transform.position = position;

		chunkCount++;
	}

	//段差の取得
	private int planeCounter = 0;	//段差無しが連続した回数
	private const int limitPlane = 16;	//段差間の最低平面数
private Step_enum getStep() {

		//前回段差であれば必ず非段差を返す
		if (planeCounter < limitPlane) {
			planeCounter++;
			return Step_enum.Step_plane;
		}

		float rate = Random.Range(0.0f, 1.0f);
		
		if (rate > 0.8 && GenerateY < MAX_Y) {	//1段上がる
			planeCounter = 0;
			return Step_enum.Step_up;
		}
		if (rate < 0.1 && GenerateY > MIN_Y) {	//1段下がる
			planeCounter = 0;
			return Step_enum.Step_down;
		}

		planeCounter++;
		return Step_enum.Step_plane;	 
	}

}	//end of class

	
