using UnityEngine;
using System.Collections;

public class PlayerWeapon : Weapon {
	
	protected override IEnumerator  Start () {

		//発弾サイクルを実行する
		while(true){
			//有効性判定
			Debug.Log ("enabled " + this.ShotEnabled);
			while(!this.ShotEnabled){
				yield return new WaitForEndOfFrame();
			}

			this.Shoot();
			yield return new WaitForSeconds(this.ShotSycle);
		}
	
	}
	
	protected override void Update () {
	
	}

	protected override void Shoot(){
		Object.Instantiate(this.shotPrefab,this.transform.position,this.transform.rotation);
		//Debug.Log ("Shoot: " + this.ShotName);
	}

}
