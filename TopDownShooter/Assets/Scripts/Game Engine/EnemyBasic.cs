using UnityEngine;
using System.Collections;

public class EnemyBasic : MonoBehaviour {
	public float enemyHP = 5;
	public float equipped = 0;
	public float speed = 100;

	//power up stuff
	int armorTimer;
	bool armorOn = false;
	public int medPack = 0;

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
		

		if (equipped == 0) {
			EnemyBulletBehaviors bulletScript = bulletClone.GetComponent<EnemyBulletBehaviors> ();
			bulletScript.lifeSpan = BulletLength ();
			bulletScript.damage = BulletDamage ();
		} else if (equipped == 1) {
			EnemyBullet1Behaviors bulletScript = bulletClone.GetComponent<EnemyBullet1Behaviors> ();
			bulletScript.lifeSpan = BulletLength ();
			bulletScript.damage = BulletDamage ();
		} else if (equipped == 2) {
			EnemyBullet2Behaviors bulletScript = bulletClone.GetComponent<EnemyBullet2Behaviors> ();
			bulletScript.lifeSpan = BulletLength ();
			bulletScript.damage = BulletDamage ();
		}else if (equipped == 3) {
			EnemyBullet3Behaviors bulletScript = bulletClone.GetComponent<EnemyBullet3Behaviors> ();
			bulletScript.lifeSpan = BulletLength ();
			bulletScript.damage = BulletDamage ();
		}else if (equipped == 4) {
			EnemyBullet4Behaviors bulletScript = bulletClone.GetComponent<EnemyBullet4Behaviors> ();
			bulletScript.lifeSpan = BulletLength ();
			bulletScript.damage = BulletDamage ();
		} else {
			EnemyBulletBehaviors bulletScript = bulletClone.GetComponent<EnemyBulletBehaviors> ();
			bulletScript.lifeSpan = BulletLength ();
			bulletScript.damage = BulletDamage ();
		}

	
		
		//bulletClone.GetComponent<MyRocketScript>().DoSomething();
	}

	public void increaseHP(int HP){
		enemyHP += 5;
	}

	public void activateArmor(){
		armorOn = true;
		armorTimer = 50;
	}

	public void pickUp(){
		medPack++;
	}

	public void dropItem(){
		//drop item here somehow...?
	}

	public void useMedPack(GameObject enemy){
		EnemyBasic enemyScript = enemy.GetComponent<EnemyBasic>();
		enemyScript.increaseHP (5);
		medPack--;
	}

	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (armorOn) {
			armorTimer--;
		}

		//if(armor in line of sight){
			//interrupt module
			//move to armor
		//}

		//if(medPack in line of sight){
			//interrupt module
			//move to medPack
		//}

		if(medPack > 0 /*&& (some enemy has < some HP || enemy is Boss)*/){
			//interrupt module
			//useMedPack(enemy);
		}

		if (enemyHP <= 0) {
			if(medPack > 0)
				dropItem();
			Destroy (this.gameObject);
		}

		Transform player = GameObject.FindGameObjectsWithTag ("Player") [0].transform;
		if ((player.position - transform.position).magnitude < (0.3 * speed)) {
			//FireBullet();
		}
	}
}
