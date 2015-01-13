﻿using UnityEngine;
using System.Collections;


//生成すべきブロックの高さを管理するクラス
//またChunkGeneratorにサイクルの終了を伝えるクラス
public class Cycle {
	//サイクル終了イベントを通知するデリゲート
	public delegate void CycleFinishDelegate();
	
	//サイクル終了イベント
	public event CycleFinishDelegate Finished;

	public Cycle(uint cycleNumber,CycleFinishDelegate finishDelegate) {
		this.cycleNumber = cycleNumber;
		this.chunkCount = 0;
		this.Finished += finishDelegate;
	}

	#region "定数"

	//サイクル当たりの距離
	public const int FullDistance = 3200;	//単位メートル

	//サイクル当たりのチャンク数
	public const int ChunkCountByCycle = Cycle.FullDistance / Chunk.SIZE_Z;	//小数点以下は無視でよい

	#endregion


	//現サイクルのサイクル番号
	private uint cycleNumber;
	public uint CycleNumber {
		get {
			return this.cycleNumber;
		}
	}

	//現サイクルでの移動距離
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

			//チャンクを規定回数生成した
			if (this.chunkCount >= Cycle.ChunkCountByCycle) {
				if (this.Finished != null) {
					Finished();
				}
			}
		}
	}

	//チャンクの移動速度(m/flame)チャンク生成サイクルに影響
	//private float scrollSpeed = 0;
	/*public float ScrollSpeed {
		get {
			return this.scrollSpeed;
		}
		set {
			this.scrollSpeed = value;
		}
	}*/




	//サイクルが満了したか否かを返す
	//サイクルが満了したか否か
	public bool IsGoneAround() {
		return false;//dummy
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
		return stride * chunkNumber / Cycle.ChunkCountByCycle;
	}


	//段差の取得
	private int planeCounter = 0;	//段差無しが連続した回数
	private const int limitPlane = 20;	//段差間の最低平面数


	//ブロック作成Y座標
	private int GenerateY = 1;	//ブロックを作成するY位置
	private const int MAX_Y = Chunk.SIZE_Y - 1;
	private const int MIN_Y = 1;


	//次にブロックを生成すべきY座標を通知する
	public int GetGenerateY() {
		HeightRange heightRange = heightMap[this.getChunkRange(this.chunkCount, Cycle.heightMap.Length)];

		//横一列毎に段差の有無を判定する
		int step = getStep(heightRange.Min, heightRange.Max);	//段差を取得
		if (step != 0) {	//段差があるなら
			this.GenerateY += step;	//段を変化させる
		}

		return this.GenerateY;
	}

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
		if (GenerateY < minY) {
			planeCounter = 0;
			return 1;
		}

		//前回の高さがmaxより高い場合
		//max-min内に補正される
		if (GenerateY > maxY) {
			planeCounter = 0;
			return -Random.Range(GenerateY - maxY, GenerateY - minY);
		}


		//重み付きランダム(1段上がる率 対 n段下がる率 対 平面 = 5:1:4)
		float rate = Random.Range(0.0f, 1.0f);

		if (rate > 0.5 && GenerateY < maxY) {	//1段上がる
			planeCounter = 0;
			return 1;	//1段上がる
		}

		if (rate < 0.1 && GenerateY > minY) { //下がる
			planeCounter = 0;

			//何段下がるかを決定する
			//Random前に負符号があることに注意
			return -Random.Range(1, GenerateY - minY);	//randomでmax値は含まれない
		}

		//その他
		planeCounter++;
		return 0;
	}

}	// end of class
