using UnityEngine;
using System.Collections;

public abstract class Weapon : MonoBehaviour {
	
	//** 弾の種類 **
	//shotのプレハブ
	[SerializeField]
	protected GameObject shotPrefab = null;

	//** 発弾の有効性 **
	//弾を撃つことが可能か否か
	[SerializeField]
	private bool shotEnabled = false;
	public bool ShotEnabled{
		set{
			this.shotEnabled = value;
		}
		get{
			if(this.shotPrefab == null){
				return false;
			}

			if(this.shotSycle <= 0.0f){
				return false;
			}

			return this.shotEnabled;
		}
	}

	//** 発弾サイクル(秒) **
	//実際の発弾サイクルはプレイヤーの能力で補正される
	[SerializeField]
	private float shotSycle = 0.0f;
	public float ShotSycle{
		set{
			this.shotSycle = value;
		}
		get{
			return this.shotSycle;
		}
	}

	//弾の強さ(攻撃力)
	//実際に発弾する時はプレイヤーの能力で補正される
	[SerializeField]
	private float attack = 0.0f;
	public float Attack{
		set{
			this.attack = value;
		}
		get{
			return this.attack;
		}
	}

	//弾の進行速度
	//実際に発弾する時はプレイヤーの能力で補正される
	[SerializeField]
	private float speed = 0.0f;
	public float Speed{
		set{
			this.speed = value;
		}
		get{
			return this.speed;
		}
	}

	//** 抽象メソッド **
	//弾を撃つ
	protected abstract void Shoot();

	//初期化
	protected abstract IEnumerator Start ();

	//フレーム更新
	protected abstract void Update ();
}
