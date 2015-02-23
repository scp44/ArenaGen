using UnityEngine;
using System.Collections;

public class EnemyBasic : MonoBehaviour {
	public float enemyHP = 5;
	public float equipped = 0;
	public float speed = 100;

	public Rigidbody Bullet;
	// Use this for initialization

	float BulletLength(){
		if (equipped == 0) {
			return 50f;
		} else if (equipped == 1) {
			return 15f;
		}else if (equipped == 2) {
			return 100f;
		}else if (equipped == 3) {
			return 30f;
		}else if (equipped == 4) {
			return 200f;
		}
		else{
			return 10f;
		}
	}
	//number of frames between shots
	float BulletCooldown(){
		if (equipped == 0) {
			return 20f;
		} else if (equipped == 1) {
			return 1f;
		}else if (equipped == 2) {
			return 50f;
		}else if (equipped == 3) {
			return 5f;
		}else if (equipped == 4) {
			return 1f;
		}
		else{
			return 10f;
		}
	}
	//amount of damage per bullet
	float BulletDamage(){
		if (equipped == 0) {
			return 3f;
		} else if (equipped == 1) {
			return 1f;
		} else if (equipped == 2) {
			return 10f;
		} else if (equipped == 3) {
			return 2f;
		} else if (equipped == 4) {
			return 1f;
		} else {
			return 10f;
		}
	}
	//individual bullet velocity scale
	float BulletSpeed(){
		if (equipped == 0) {
			return 1f;
		} else if (equipped == 1) {
			return 3f;
		} else if (equipped == 2) {
			return 10f;
		} else if (equipped == 3) {
			return 1f;
		} else if (equipped == 4) {
			return 2f;
		} else {
			return 10f;
		}
	}
	
	//Transfers bullet stats to bullets
	void FireBullet () {
		var inFront = new Vector3 (0, 1, 0);
		
		Rigidbody bulletClone = (Rigidbody) Instantiate(Bullet, transform.position, transform.rotation);
		bulletClone.velocity = transform.forward * speed * BulletSpeed();
		EnemyBulletBehaviors bulletScript = bulletClone.GetComponent<EnemyBulletBehaviors>();
		bulletScript.lifeSpan = BulletLength ();
		bulletScript.damage = BulletDamage ();
		
		//bulletClone.GetComponent<MyRocketScript>().DoSomething();
	}

	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (enemyHP <= 0) {
			Destroy (this.gameObject);
		}
	}
}
