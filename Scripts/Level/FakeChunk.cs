using UnityEngine;
using System.Collections;

public class FakeChunk : MonoBehaviour {
	public const int SIZE_X = 18;
	public const int SIZE_Y = 18;
	public const int SIZE_Z = 18;


	private BlockSet blockSet;	//何のために必要か不明
	private ChunkData chunkData;	//何のために必要か不明

	private MeshFilter filter;		//テクスチャを管理
	private MeshCollider meshCollider;	//当たり判定を管理

	public static FakeChunk CreateFakeChunk(Vector3 pos, Map map, ChunkData chunkData) {
		
		//ゲームオブジェクト
		GameObject go = new GameObject("(" + pos.x + " " + pos.y + " " + pos.z + ")  " + map.transform.childCount);
		
		//親
		go.transform.parent = map.transform;
		
		//位置大きさ角度
		go.transform.localPosition = new Vector3(pos.x * FakeChunk.SIZE_X, pos.y * FakeChunk.SIZE_Y, pos.z * FakeChunk.SIZE_Z);
		go.transform.localRotation = Quaternion.identity;	//回転していないクオータニオン
		go.transform.localScale = Vector3.one;

		//レイヤー
		go.layer = LayerName.Ground;

		//FakeChunkを追加
		FakeChunk chunk = go.AddComponent<FakeChunk>();
		
		//BlockSet
		chunk.blockSet = map.GetBlockSet();

		//ChunkData
		chunk.chunkData = chunkData;


		return chunk;
	}



	private bool dirty = false;
	private bool lightDirty = false;
	
	void update() {
		if (dirty) {
			Build();
			dirty = lightDirty = false;
		}
	}


	private void Build() {

		//メッシュのレンダラとフィルタとコライダをアロケート
		if (filter == null) {
			gameObject.AddComponent<MeshRenderer>().sharedMaterials = blockSet.GetMaterials();
			gameObject.renderer.castShadows = false;
			gameObject.renderer.receiveShadows = false;	//影が表示されない理由はこれか？？
			filter = gameObject.AddComponent<MeshFilter>();
			meshCollider = gameObject.AddComponent<MeshCollider>();
		}


		//謎 ChunkBuilderは何をしているのか？？
		//ChunkとChunkBuilderの関係は？
		//ChunkBuilderはStaticなクラス Meshに関する設定全てを管理している様子

		//ChunkBuilderのMeshDataを再構築し、フィルタとコライダに設定
		filter.sharedMesh = ChunkBuilder.BuildChunk(filter.sharedMesh, chunkData);
		
		
		
		
		
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
		if (!gameObject.active && filter.sharedMesh != null) {
			gameObject.SetActiveRecursively(true);
		}
	}

}
