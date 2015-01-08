using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;	//Jsonライブラリ

public class block64 : MonoBehaviour {

	//[SerializeField]
	public Texture baseTexture = null;

	//[SerializeField]
	public TextAsset jsonText = null;

	[SerializeField]
	//各種ブロックデータまでのパス(rootを含めて記載する 最後のスラッシュは不要)
	private string blockPath = null;

	//デコードデータ
	private JsonData jsonData = null;

	//テクスチャ全体のサイズ
	private Vector2 baseTextureSize = new Vector2(0,0);

	//箱に割り当てたいシェーダー
	private Shader shader = null;

	//uv列の頂点インデックス
	public enum uv_enum{
		uv_bottomLeft,
		uv_bottomRight,
		uv_topLeft,
		uv_topRight
	};

	//cubeの各面
	private enum plane_enum{
		plane_front,
		plane_back,
		plane_left,
		plane_right,
		plane_top,
		plane_bottom
	};

	//頂点座標対応
	private int[,]  cubeVertexes = new int[,]{
		{7,6,11,10},		//正面
		{0,1,2,3},			//背面
		{16,18,19,17},	//左
		{20,22,23,21},	//右
		{5,4,9,8},			//上
		{12,14,15,13}	//下
	};

	//各面が検索する画像ファイル名(優先度順)
	private string[][] serchFiles = new string[][]{
		new string[]{"front.png","side.png","other.png"},
		new string[]{"back.png","side.png","other.png"},
		new string[]{"left.png","side.png","other.png"},
		new string[]{"right.png","side.png","other.png"},
		new string[]{"top.png","up_and_low.png","other.png"},
		new string[]{"bottom.png","up_and_low.png","other.png"}
	};

	//各頂点のuvをキャッシュしておく辞書
	private Dictionary<string,Vector2[]> uvDictionary = null;
	
	//データファイルの読み込みと初期化
	void Awake () {
		if(jsonText == null){
			Debug.LogError(this.gameObject.name + ">block.cs jsonファイル(.txt)が指定されていない");
		}
		this.jsonData = LitJson.JsonMapper.ToObject(jsonText.text);
	
		//テクスチャ全体のサイズを取得
		this.baseTextureSize.x = (int)jsonData["Meta"]["width"];
		this.baseTextureSize.y = (int)jsonData["Meta"]["height"];
		//Debug.Log ("width:" +  baseTextureSize.x + " height:" + baseTextureSize.y);

		//シェーダーの初期化
		this.shader = Shader.Find ("Mobile/Unlit(Supports Lightmap)");

		//uv列の辞書の初期化
		this.uvDictionary = new Dictionary<string,Vector2[]>();

		//テスト
		/*
		string[] array =  {"bdc_coarse_dirt02.png","bdc_coarse_dirt_top2.png","bdc_cobblestone10.png"};
		foreach(string frameName in array){
			Debug.Log(frameName);
			Debug.Log (this.getRect(frameName));
		}
		*/
	}


	public void Start(){
		float start = Time.realtimeSinceStartup;
		float blockSize = 1.0f;

		//テスト用
		string[] blocks = new string[]{
			"Brick01","Brick02","Cactus",
			"EndStone","Farmland","Furnace","CobbleStone","CommandBlock","Diamond","EndStone","Glass"
		};

		//ブロックの生成
		for(int x = 0;x < 10;x++){
			for(int z = 0;z < 10;z++){
				int idx = Random.Range(0,blocks.Length);
				//Debug.Log ("blocks[" + idx + "] = " + blocks[idx]); 
				this.createBlock(new Vector3(x,2,z), new Vector3(1,1,1),
				                 blocks[idx]);
			}
		}
		float elapsed = Time.realtimeSinceStartup - start;
		Debug.Log("elapsed time : " + elapsed + "sec.");
	}

	//指定位置に、指定スケールで、指定名のブロックを生成する
	private GameObject createBlock(Vector3 position,Vector3 scale, string blockName){
		GameObject cube = this.createCube(position,scale);
		cube.renderer.material.mainTexture = this.baseTexture;
		//cube.renderer.material.mainTextureScale = new Vector2(100,100);

		//マテリアルにシェーダーを設定
		var renderers = this.gameObject.GetComponentsInChildren<SpriteRenderer>();
		foreach (var r in renderers) {
			r.material.shader = this.shader;
			//r.material.mainTextureScale = new Vector2(100,100);	//タイリングの制御
		}

		//マテリアルのタイリングを制御する
		//各々の場合
		//gameObject.renderer.material = sampleMaterial;
		//gameObject.renderer.material.mainTextureScale = new Vector2(2, 1);
		//sampleMaterialを設定したGameObject全ての場合
		//sampleMaterial.mainTextureScale = new Vector2(2, 1);
		
		//タグの設定(レイヤーにすべきか？)
		//cube.tag = "Ground";
		//cube.layer = 11;	//TODO 名前で指定
		//cube.transform.parent = this.transform;


		MeshFilter mf = cube.GetComponent<MeshFilter>();

		//uv列の取得に失敗した場合はnullが設定される
		mf.mesh.uv = this.getUvList(blockName);
		/*
		Vector2[] uvList = this.getUvList(blockName);
		for (int i = 0 ; i < uvList.Length;i++){
			uvList[i] = uvList[i]/100;
		}
		mf.mesh.uv = uvList;	//現在はスプライトフレーム名
		*/
		//mf.renderer.material.mainTextureScale = new Vector2(100,100);

		return cube;
	}


	private float lastCreatedTime = 0.0f;

	//サイクル処理
	public void Update(){


		//ブロック生成テスト
		/*
		if(Time.time - this.lastCreatedTime > 2.0f){
			this.lastCreatedTime = Time.time;
		}
		*/
	
	}



	//指定パスが１枚のスプライトフレームであるとみなし、uv座標(4頂点分)を生成
	private Vector2[] getUvPlane(string blockName,string[] serchFiles,string blockPath){

		//ブロックまでのパスに対応するJsonDataを取得する
		JsonData blockJson = this.getJsonDataFromPath(blockPath + "/" + blockName);
		if(blockJson == null){
			Debug.Log ("Block " + blockPath + "/" + blockName + "is nothing");
			return null;
		}

		//優先度の高い順に各ファイル名のキーを検索
		JsonData fileJson = null;
		foreach(string filename in serchFiles){
			//Debug.Log ("serching");
			if(JsonDataContainsKey(blockJson,filename)){
				fileJson = blockJson[filename];
				break;
			}
		}

		//検索失敗
		if(fileJson == null){
			Debug.Log ("Block " + blockPath + "/" + blockName);
			foreach(string filename in serchFiles){
				Debug.Log (filename + " ");
			}
			Debug.Log ("is nothing");
			return null;
		}
	
		//Lit Jsonはfloatが扱えない
		float left = (float)(double)fileJson["x1"];
		float bottom = (float)(double)fileJson["y1"];
		float right = (float)(double)fileJson["x2"];
		float top = (float)(double)fileJson["y2"];

		return new Vector2[]{
			new Vector2(left,bottom),
			new Vector2(right,bottom),
			new Vector2(left,top),
			new Vector2(right,top)
		};
	}

	//指定パスのjsonObjectを取得する
	private JsonData getJsonDataFromPath(string spriteFramePath){

		//Debug.Log ("path:" + spriteFramePath);

		//パスをデリミタで分割
		string[] delimiter = {"/" };	//要素数1の配列
		string[] layers = spriteFramePath.Split (delimiter,System.StringSplitOptions.None);

		JsonData ret = this.jsonData;
		//パスの最下層のJsonDataを取得	
		foreach(string layer in layers){
			Debug.Log (layer);
			ret = ret[layer];
		}

		return (ret == this.jsonData)? null : ret;
	}


	//指定位置に指定スケールでprimitiveを生成する
	//position はworld尺
	private GameObject createCube(Vector3 position,Vector3 scale){
		GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
		cube.transform.position = position;
		cube.transform.localScale = scale;

		return cube;
	}

	//指定ブロック名のuv列を返す
	private Vector2[] getUvList(string blockName){

		//各種ブロックまでのパスは root/ブロック名 で表される
		//各面に割り当てられるテクスチャの名前は、
		//top,bottom,left,right,front,back (.png)である
		//top,bottomがぞれぞれ存在しない場合はup_and_rowを検索する
		//up_and_rowが存在しない場合は、otherを検索する（なければエラー)
		//left,right,front,backのそれぞれが存在しない場合は、sideを検索する
		//sideが存在しない場合は、otherを検索する

		//キャッシュされていればそれを使用する
		if(this.uvDictionary.ContainsKey(blockName)){
			return this.uvDictionary[blockName];
		}

		//各平面の拡張点
		plane_enum[] planeEnumValues = (plane_enum[])System.Enum.GetValues(typeof(plane_enum));
		uv_enum[] uvEnumValues = (uv_enum[])System.Enum.GetValues(typeof(uv_enum));
		Vector2[] uv = new Vector2[planeEnumValues.Length * uvEnumValues.Length];	//6面×4点=24頂点
		//各頂点をuv列へ変換
		foreach(plane_enum plane in planeEnumValues){
			foreach(uv_enum vertex_pos in uvEnumValues){
				int planeIndex = (int)plane;
				int vertexIndex = (int)vertex_pos;

				//各面の4頂点のuvを取得
				Vector2[] planeUv =  this.getUvPlane(blockName,this.serchFiles[planeIndex],this.blockPath);
				if(planeUv == null){	//正しくuvを取得できなかった
					return null;
				}

				uv[cubeVertexes[planeIndex,vertexIndex]] = new Vector2(
					planeUv[vertexIndex].x,
					planeUv[vertexIndex].y
					);
			}
		}
		
		//作成したuv列をキャッシュする
		this.uvDictionary[blockName] = uv;

		return uv;
	}

	//指定JsonData直下に指定のキーがあるか否かを判定
	static public  bool JsonDataContainsKey(JsonData data,string key){
		try{
			IDictionary dic = data as IDictionary;
			return(dic.Contains(key));
		} catch{
			return false;
		}
	}

}	//end of class
	/*
		Vector2[] vectors = new Vector2[System.Enum.GetNames(typeof()).Length]

		//主要キー名
		const string f1 = "frames";
		//f2はファイル名
		const string f3 = "frame";

		ret.x = (int)this.jsonData[f1][frameName][f3]["x"];
		ret.y = (int)this.jsonData[f1][frameName][f3]["y"];


		ret.width = (int)this.jsonData[f1][frameName][f3]["w"];
		ret.height = (int)this.jsonData[f1][frameName][f3]["h"];

		//unityの仕様なのか・・テクスチャの座標が上下逆
		ret.y = this.baseTextureSize.y - ret.y - ret.height;

		return ret;
	}
*/	

	/*
	//テクスチャの指定Rectをuvの列(左上,右上,左下,右下)に変換する(4点 正方形一つ分)
	private Vector2[] getUvPlane(Rect rect){
		//テクスチャのサイズは2のべき乗ベースであることを前提としている
		//一般的なテクスチャとy軸が逆であることに注意(下が0,正が上方向)

		Vector2[] uvList = new Vector2[4];

		//左下
		uvList[(int)uv_enum.uv_bottomLeft] = new Vector2();
		uvList[(int)uv_enum.uv_bottomLeft].x = (float)rect.x / this.baseTextureSize.x;
		uvList[(int)uv_enum.uv_bottomLeft].y = (float)rect.y / this.baseTextureSize.y;

		//右下
		uvList[(int)uv_enum.uv_bottomRight] = new Vector2();
		uvList[(int)uv_enum.uv_bottomRight].x = (float)(rect.x + rect.width) / this.baseTextureSize.x;
		uvList[(int)uv_enum.uv_bottomRight].y = (float)rect.y / this.baseTextureSize.y;

		//左上
		uvList[(int)uv_enum.uv_topLeft] = new Vector2();
		uvList[(int)uv_enum.uv_topLeft].x = (float)rect.x / this.baseTextureSize.x;
		uvList[(int)uv_enum.uv_topLeft].y = (float)(rect.y + rect.height) / this.baseTextureSize.y;

		//右上
		uvList[(int)uv_enum.uv_topRight] = new Vector2();
		uvList[(int)uv_enum.uv_topRight].x = (float)(rect.x + rect.width) / this.baseTextureSize.x;
		uvList[(int)uv_enum.uv_topRight].y = (float)(rect.y + rect.height)/ this.baseTextureSize.y;

		return uvList;
	}
	*/



/*
		Vector2[] planeUv = this.getUvPlane(spriteFramePath);

		//テスト

		foreach(Vector2 vertex in planeUv){
			Debug.Log("x: " + vertex.x + " y:" + vertex.y);
		}


		//各平面の拡張点
		plane_enum[] planeEnumValues = System.Enum.GetValues(typeof(plane_enum));
		uv_enum[] uvEnumValues = System.Enum.GetValues(typeof(uv_enum));
		Vector2[] uv = new Vector2[planeEnumValues.Length * uvEnumValues.Length];	//6面×4点=24頂点
		//各頂点をuv列へ変換
		foreach(plane_enum plane in planeEnumValues){
			foreach(uv_enum vertex_pos in uvEnumValues){
				int planeIndex = (int)plane;
				int vertexIndex = (int)vertex_pos;
				uv[cubeVertexes[planeIndex,vertexIndex]] = new Vector2(
						planeUv[vertexIndex].x,
						planeUv[vertexIndex].y
					);
			}
		}

		//作成したuv列をキャッシュする
		this.uvDictionary[spriteFramePath] = uv;

		return uv;
	}
}
*/