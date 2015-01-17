using UnityEngine;
using System.Collections;


//生成すべきブロックの高さを管理するクラス
//またChunkGeneratorにサイクルの終了を伝えるクラス
public class Level {

	public Level(uint levelNumber,Map map,string blockName) {
		this.levelNumber = levelNumber;
		this.chunkCount = 0;
		this.map = map;
		this.blockName = blockName;
	}

	#region "定数"

	//レベル当たりの距離
	public const int FullDistance = 960;	//単位メートル

	//レベル当たりのチャンク数
	public const int ChunkCountByCycle = Level.FullDistance / Chunk.SIZE_Z;	//小数点以下は無視でよい

	#endregion


	//マップの定義情報
	private Map map = null;

	//現レベルのメインブロック
	private string blockName;
	public string BlockName {
		get {
			return this.blockName;
		}
	}


	//現レベルのレベル番号
	private uint levelNumber;
	public uint LevelNumber {
		get {
			return this.levelNumber;
		}
	}

	
	//現レベルでの移動距離
	//サイクル距離や全長とは、最後に生成したチャンクの最大Z座標(一番奥)を示している
	public uint CurrentDistance {
		get {
			return (uint)(Chunk.SIZE_Z * this.chunkCount);
		}
	}


	//生成したチャンクの数
	private int chunkCount = 0;
	public int ChunkCount {
		get {
			return this.chunkCount;
		}
		set {
			this.chunkCount = value;
		}
	}


	//ブロックを生成できる高さの範囲を規定する構造体
	private struct HeightRange {
		int min;
		public int Min {
			get {
				return this.min;
			}
		}
		int max;
		public int Max {
			get {
				return this.max;
			}
		}
		public HeightRange(int min, int max) {
			this.min = min;
			this.max = max;
		}
	}

	//チャンク番号(サイクル当たりの最大チャンク番号 から見た 現在のチャンク番号 の割合）と高さの関係	//ブロックの高さの分布(それぞれのHeightRangeは等間隔で出現)
	//minは必ず1以上でなければならない
	//maxは必ず31以下でなければならない
	private static HeightRange[] heightMap = new HeightRange[]{
		new HeightRange(1,2),
 		new HeightRange(1,4),
		new HeightRange(4,10),
		new HeightRange(9,14),
		new HeightRange(12,21),
		new HeightRange(18,28),
		new HeightRange(25,31),
		new HeightRange(31,31)
	};


	//チャンク番号からチャンクの全チャンクに対する割合に変換する
	//刻み数(stride)を指定可能にする
	//刻み数とは
	//たとえば刻み数を10とした場合
	//0-9 0
	//10-19 1
	//...
	//90-99 9 の10種に刻まれる
	//各幅の密度はいずれも等しい
	//どの刻み位置に入るかを通知する
	private int getChunkRange(int chunkNumber, int stride) {
		return stride * chunkNumber / Level.ChunkCountByCycle;
	}


	//段差
	private int planeCounter = 0;	//段差無しが連続した回数
	private const int limitPlane = 20;	//段差間の最低平面数


	//ブロック作成Y座標
	private int generateY = 1;	//ブロックを作成するY位置
	private const int MAX_Y = Chunk.SIZE_Y - 1;
	private const int MIN_Y = 1;


	//段の上がり下がり数を返す
	//0 平面 1:1段上がる -n:n段下がる
	//注意 min と maxで考え方に違いがある
	//現在の高さがmaxより高ければ必ずYがmax以下になるように補正される
	//現在の高さがminより低い場合は必ず +1 を返す
	//一度段差が発生すれば、必ずlimitPlane回以上平面が続く
	private int getStep(int minY, int maxY) {

		//平面が規定回数以上続いていない場合
		if (planeCounter < limitPlane) {
			planeCounter++;
			return 0;	//かならず平面を返す
		}

		//前回の作成高さがminよりも小さい場合
		//必ず1段だけ上がる
		if (this.generateY < minY) {
			planeCounter = 0;
			return 1;
		}

		//前回の高さがmaxより高い場合
		//max-min内に補正される
		if (this.generateY > maxY) {
			planeCounter = 0;
			return -Random.Range(this.generateY - maxY, this.generateY - minY);
		}


		//重み付きランダム(1段上がる率 対 n段下がる率 対 平面 = 5:1:4)
		float rate = Random.Range(0.0f, 1.0f);

		if (rate > 0.5 && this.generateY < maxY) {	//1段上がる
			planeCounter = 0;
			return 1;	//1段上がる
		}

		if (rate < 0.1 && this.generateY > minY) { //下がる
			planeCounter = 0;

			//何段下がるかを決定する
			//Random前に負符号があることに注意
			return -Random.Range(1, this.generateY - minY);	//randomでmax値は含まれない
		}

		//その他
		planeCounter++;
		return 0;
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
	//※レベル終了時はnullを通知する
	public Chunk NextChunk() {

		//満了ならばnullを通知
		if (this.chunkCount >= Level.ChunkCountByCycle) {
			return null;
		}



		BlockSet blockSet = map.GetBlockSet();
		Block b = blockSet.GetBlock(this.blockName);
		BlockData bd = new BlockData(b);
		ChunkData cd = new ChunkData(map, new Vector3i(0, 0, 0));	//0,0,0固定



		//チャンクデータにブロックを並べる(yは必ず0より大)
		for (int z = 0; z < Chunk.SIZE_Z; z++) {


			//有効な段の幅はgetStep内で把握するべきかもしれないが
			//getStepにはmin,maxを外部から指定可能にしたかったのでこのようにした。
			HeightRange heightRange = heightMap[this.getChunkRange(this.chunkCount, Level.heightMap.Length)];

			//段差の有無を判定
			int step = getStep(heightRange.Min, heightRange.Max);	//段差を取得			

			int changeX = -1;	//横位置列の中で高さが変化する位置(-1は無効値)
			//段差がある場合X位置列で高さが変化する位置を決める
			if (step != 0) {
				changeX = Random.Range(0, Chunk.SIZE_X - 1);
			}
			
			//ブロックを配置
			for (int x = 0; x < Chunk.SIZE_X; x++) {
				if (x == changeX) {	//段差が変化する位置になったら
					this.generateY += step;	//段差を変化させる
				}
				cd.SetBlock(bd, x, this.generateY, z);
			}
		}


		this.chunkCount++;	//カウントアップ

		//チャンクインスタンスとして返却
		return cd.GetChunkInstance(); 
	}



}	// end of class
