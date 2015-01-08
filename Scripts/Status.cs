using UnityEngine;
using System.Collections;

//ゲーム内にモノとして存在するオブジェクト全般に適用する生存情報
public class Status : MonoBehaviour {

	public enum LifeStatus{
		Alive,	//生存中
		Die,		//死んだばかり(時間を経てDeadへ移行する)
		Dead	//死亡(多くの場合GameObjectが消滅する)
	};


	// ** 生命力 **
	//0以下の場合、生命状態がDeathを経てDead状態になる
	[SerializeField]
	private float hp = 0.0f;
	public float Hp{
		get {
			return this.hp;
		}
		set {
			this.hp = value;
		}
	}

	// ** 攻撃力 **
	[SerializeField]
	private float attack = 0.0f;
	public float Attack{
		get{
			return this.attack;
		}
		set{
			this.attack = value;
		}
	}

	// ** 素早さ **
	[SerializeField]
	private float speed = 0.0f;
	public float Speed{
		get{
			return this.speed;
		}
		set{
			this.speed = value;
		}
	}

	// ** Die から Deadへ遷移する時間 **
	//[SerializeField]
	//private float deadSpan = 2.0f;

}
