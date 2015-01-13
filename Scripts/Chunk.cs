using UnityEngine;
using System.Collections;

public class Chunk : MonoBehaviour {
	
	public const int SIZE_X_BITS = 4;
	public const int SIZE_Y_BITS = 5;
	public const int SIZE_Z_BITS = 4;
	
	public const int SIZE_X = 1 << SIZE_X_BITS;
	public const int SIZE_Y = 1 << SIZE_Y_BITS;
	public const int SIZE_Z = 1 << SIZE_Z_BITS;

	//追加
	private const float limitDistance = Chunk.SIZE_Z*2;	//プレイヤーよりlimitDistaceメートル後方(-z)へ移動した時点で消去される
	private static float scrollSpeed = 0;	//スクロール量

	//ChunkGeneratorから付与される値
	private uint cycleNumber = 0;	//ChunkGeneratorで作成された時のサイクル番号
	public uint CycleNumber {
		get {
			return this.cycleNumber;
		}
		set {
			this.cycleNumber = value;
		}
	}
	private int chunkNumber = 0;	//あるサイクルで、何番目のチャンクであるか。
	public int ChunkNumber {
		get {
			return this.chunkNumber;
		}
		set {
			this.chunkNumber = value;
		}
	}

	private BlockSet blockSet;
	private ChunkData chunkData;	//ブロックを管理
	
	private MeshFilter filter;		//テクスチャを管理
	private MeshCollider meshCollider;	//当たり判定を管理


	//MapのBuildの中で trueになる処理が呼ばれる
	//trueになると、自身のupdateの中で、自身のBuildを呼ぶ
	private bool dirty = false;

	//CreateChunkとBuildが別れている理由がわからない(ブロックの追加等に伴うメッシュデータの再構築)
	//CreateChunkはゲームオブジェクト自身を作成している
	//Buildはメッシュのデータを再構築する
	//MapのSetBlockAndRecomputeでMapのBuildが呼ばれる
	//MapのBuild内でChunkのBuildが呼ばれる

	private bool lightDirty = false;

	
	//Chunkの基本形を生成し、BlockSetとChunkDataを設定する
	public static Chunk CreateChunk(Vector3i pos, Map map, ChunkData chunkData) {
		GameObject go = new GameObject("("+pos.x+" "+pos.y+" "+pos.z+")  "+map.transform.childCount);
		go.transform.parent = map.transform;
		go.transform.localPosition = new Vector3(pos.x*Chunk.SIZE_X, pos.y*Chunk.SIZE_Y, pos.z*Chunk.SIZE_Z);
		go.transform.localRotation = Quaternion.identity;
		go.transform.localScale = Vector3.one;
		
    //追加
    go.layer = LayerName.Ground;

		Chunk chunk = go.AddComponent<Chunk>();
		chunk.blockSet = map.GetBlockSet();
		chunk.chunkData = chunkData;

		//スクロール速度について
		if (Chunk.scrollSpeed == 0) {
			GameObject objPlayer = GameObject.FindGameObjectWithTag(TagName.Player);
			PlayerMove playerMove = objPlayer.GetComponent<PlayerMove>();
			Chunk.scrollSpeed = playerMove.ScrollSpeed;	//初期化
			//イベントへ登録
			playerMove.ScrollSpeedChanged += new PlayerMove.PlayerValueChangeHandler(Chunk.scrollSpeedChanged);
		}

		return chunk;
	}
	
	//追加
	public static Chunk CreateChunk(float z, Map map, ChunkData chunkData) {
		GameObject go = new GameObject("(" + z + ")  " + map.transform.childCount);
		
		go.transform.parent = map.transform;
		go.transform.localPosition = new Vector3(0,0,z);
		go.transform.localRotation = Quaternion.identity;
		go.transform.localScale = Vector3.one;

		//追加
		go.layer = LayerName.Ground;

		Chunk chunk = go.AddComponent<Chunk>();
		chunk.blockSet = map.GetBlockSet();
		chunk.chunkData = chunkData;

		return chunk;
	}

	
	void Update() {


		//ビルドされていなければ、ビルドする
		if(dirty) {
			Build();
			dirty = lightDirty = false;
		}

		//スクロール移動
		//Apply Root Motionにチェックがあると進まなくなる
		//ｙを０にしたらいかん
		//update毎に位置を決めるためdeltatimeは使用しない
		Vector3 position = this.transform.position;
		position.z -= Chunk.scrollSpeed;
		this.transform.position = position;

		/*
		if(lightDirty) {
			BuildLighting();
			lightDirty = false;
		}
		 */ 

		//z座標が-limitDistance未満になったら削除
		if (this.transform.position.z < -Chunk.limitDistance) {
			GameObject.Destroy(this.gameObject);
		}
	}
	

	private void Build() {

		//Debug.Log("Chunk is built");

		//必要なコンポーネントを追加
		if(filter == null) {
			gameObject.AddComponent<MeshRenderer>().sharedMaterials = blockSet.GetMaterials();
			
			gameObject.renderer.castShadows = true;
			gameObject.renderer.receiveShadows = true;
			filter = gameObject.AddComponent<MeshFilter>();
			meshCollider = gameObject.AddComponent<MeshCollider>();
		}
		

		//謎 ChunkBuilderは何をしているのか？？
		//ChunkとChunkBuilderの関係は？
		//ChunkBuilderはStaticなクラス Meshに関する設定全てを管理している様子
		
		//ChunkBuilderのMeshDataを再構築し、フィルタとコライダに設定

		//Debug.Log("filter " + filter.sharedMesh.vertexCount);
		
		filter.sharedMesh = ChunkBuilder.BuildChunk(filter.sharedMesh, chunkData);
		if (filter.sharedMesh == null) {
			Debug.Log("null filter");
		}
		meshCollider.sharedMesh = null;
		meshCollider.sharedMesh = filter.sharedMesh;



		//システムに再利用を促しているのかな？？
		//再利用するために残るのか？？
		//そうではない。ゲームオブジェクト中の全コンポーネントのアクティベートを制御するためのもののようだ
		/* 全てのチャンクを削除の対象とするためにコメントアウトした
		if(gameObject.active && filter.sharedMesh == null) {
			gameObject.SetActiveRecursively(false);
		}
		 */
 		/*
		if(!gameObject.active && filter.sharedMesh != null) {
			gameObject.SetActiveRecursively(true);
		}
		 */ 
	}
	
	/*
	private void BuildLighting() {
		if(filter != null && filter.sharedMesh != null) {
			ChunkBuilder.BuildChunkLighting(filter.sharedMesh, chunkData);
		}
	}
	 */ 
	
	public void SetDirty() {
		dirty = true;
	}

	public void SetLightDirty() {
		//lightDirty = true;
	}


	//[イベント]スクロール速度の変更があった場合
	private static void scrollSpeedChanged(PlayerMove playerMove) {
		Chunk.scrollSpeed = playerMove.ScrollSpeed;
	}



	//ブロック座標として、正しい範囲各要素が(0-15)を指定されているか否か
	public static bool IsCorrectLocalPosition(Vector3i local) {
		return IsCorrectLocalPosition(local.x, local.y, local.z);
	}
	public static bool IsCorrectLocalPosition(int x, int y, int z) {
		return (x & SIZE_X - 1) == x &&
				 (y & SIZE_Y - 1) == y &&
				 (z & SIZE_Z - 1) == z;
	}

	
	//Coordは座標
	//ブロック座標がチャンクの範囲を超えた時に、隣接するチャンク座標とそのチャンクのブロック座標へ変換する
	public static bool FixCoords(ref Vector3i chunk, ref Vector3i local) {
		bool changed = false;
		if(local.x < 0) {
			chunk.x--;
			local.x += Chunk.SIZE_X;
			changed = true;
		}
		if(local.y < 0) {
			chunk.y--;
			local.y += Chunk.SIZE_Y;
			changed = true;
		}
		if(local.z < 0) {
			chunk.z--;
			local.z += Chunk.SIZE_Z;
			changed = true;
		}
		
		if(local.x >= Chunk.SIZE_X) {
			chunk.x++;
			local.x -= Chunk.SIZE_X;
			changed = true;
		}
		if(local.y >= Chunk.SIZE_Y) {
			chunk.y++;
			local.y -= Chunk.SIZE_Y;
			changed = true;
		}
		if(local.z >= Chunk.SIZE_Z) {
			chunk.z++;
			local.z -= Chunk.SIZE_Z;
			changed = true;
		}
		return changed;
	}


	//ブロック座標から、チャンク座標に変換
	public static Vector3i ToChunkPosition(Vector3i point) {
		return ToChunkPosition( point.x, point.y, point.z );
	}
	public static Vector3i ToChunkPosition(int pointX, int pointY, int pointZ) {
		int chunkX = pointX >> SIZE_X_BITS;
		int chunkY = pointY >> SIZE_Y_BITS;
		int chunkZ = pointZ >> SIZE_Z_BITS;
		return new Vector3i(chunkX, chunkY, chunkZ);
	}
	

	//グローバル座標からブロック座標を求める
	public static Vector3i ToLocalPosition(Vector3i point) {
		return ToLocalPosition(point.x, point.y, point.z);
	}
	public static Vector3i ToLocalPosition(int pointX, int pointY, int pointZ) {
		int localX = pointX & (SIZE_X-1);
		int localY = pointY & (SIZE_Y-1);
		int localZ = pointZ & (SIZE_Z-1);
		return new Vector3i(localX, localY, localZ);
	}
	

	//チャンク座標ので指定したチャンクの、ブロック座標で指定したブロックの座標をグローバル座標へ変換
	public static Vector3i ToWorldPosition(Vector3i chunkPosition, Vector3i localPosition) {
		int worldX = (chunkPosition.x << SIZE_X_BITS) + localPosition.x;
		int worldY = (chunkPosition.y << SIZE_Y_BITS) + localPosition.y;
		int worldZ = (chunkPosition.z << SIZE_Z_BITS) + localPosition.z;
		return new Vector3i(worldX, worldY, worldZ);
	}
	
}

