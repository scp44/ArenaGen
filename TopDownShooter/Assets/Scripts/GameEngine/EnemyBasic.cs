using UnityEngine;
using System.Collections;

public class EnemyBasic : MonoBehaviour {
	public float enemyHP = 5;
	public float equipped = 0;
	public float speed = 100;

	public float commScale = 4;
	public float visionScale = 10;
	public float alertScale = 15; 
	public float fov = 100;

	//0 = HEALER, 1 = SOLDIER, 2 = DEFENDER, 3 = BOSS
	public float enemyType = 0;


	public Vector3 bossPos;
	public bool bossFound = false;
	public Transform playerPos;
	public double lastTimeSeen;




	//power up stuff
	public int armorCount;
	public bool armorOn = false;
	public int medPack = 0;
	public Rigidbody MedPack;

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

	public GameObject visionCheck(){
		GameObject[] pUs = GameObject.FindGameObjectsWithTag ("MedPackPU");
		GameObject player = GameObject.FindGameObjectsWithTag ("Player") [0];
		GameObject closestPack = pUs [0];
		GameObject toReturn = null;

		Vector3 forward = transform.forward;
		float angle;
		Vector3 targetDir;
		int toUse = 0;


		int i;
		for (i = 0; i < pUs.Length; i++) {
			Transform powerUpPos = pUs [i].transform;
			GameObject powerUp = pUs [i];
			targetDir = powerUpPos.position - transform.position;
			angle = Vector3.Angle (targetDir, forward); 

			if ((powerUpPos.position - transform.position).magnitude < (visionScale) && angle <= fov) {
				if ((powerUpPos.position - transform.position).magnitude < (closestPack.transform.position - transform.position).magnitude) {
					closestPack = powerUp;
				}
			}
		}
		if ((closestPack.transform.position - transform.position).magnitude < (visionScale) && 
		    Vector3.Angle (closestPack.transform.position - transform.position, forward) <= fov) {
				toReturn = closestPack;
				toUse = 1;
			}

	

		Transform playerLocPos = player.transform;
		targetDir = playerLocPos.position - transform.position;
		angle = Vector3.Angle (targetDir, forward);
		
		if ((playerLocPos.position - transform.position).magnitude < (visionScale) && angle <= fov) {
			this.playerPos = playerLocPos;
			//this.lastTimeSeen = this.LocationInfo.timestamp;
			toReturn = player;
			toUse = 2;
		}
		RaycastHit hit;
		Collider other;
		Physics.Raycast (transform.position, (toReturn.transform.position-transform.position),out hit, visionScale );
		other = hit.collider; 
		if (other.gameObject.tag == "Wall") {
			return null;
		}


		return toReturn;
	}

	public void commCheck(){
		GameObject[] eUs = GameObject.FindGameObjectsWithTag ("Enemy");

		int i;
		for(i = 0; i < eUs.Length; i++){
			Transform npcPos = eUs[i].transform;
			GameObject npc = eUs[i];
			EnemyBasic npcScript = npc.GetComponent<EnemyBasic>();
			if ((npcPos.position - transform.position).magnitude < (commScale)) {
				if(bossFound){
					//pass information
					npcScript.bossPos = this.bossPos;
				}
				if(npcScript.lastTimeSeen < this.lastTimeSeen){
					npcScript.lastTimeSeen = this.lastTimeSeen;
					npcScript.playerPos = this.playerPos;
				}
			}}
		
	}

	public void increaseHP(int HP){
		enemyHP += 5;
	}

	public void activateArmor(){
		armorOn = true;
		armorCount = 5;
	}

	public void pickUp(){
		medPack++;
	}

	public void dropItem(){
		Rigidbody medPackClone = (Rigidbody) Instantiate(MedPack, transform.position, transform.rotation);
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
		GameObject target = visionCheck ();
		//vision check + shoot at player
		if (target.tag == "Player") {
			FireBullet();
		}

		//if armor broken, cancel effect.
		if (armorOn && armorCount <= 0) {
			armorOn = false;
			armorCount = 0;
		}
		
		if(false/*armor in line of sight*/){
			//interrupt module
			//move to armor
		}
		
		if(false/*medpack in line of sight*/){
			//interrupt module
			//move to medpack
		}
		
		if(medPack > 0 /*&& (some enemy in site has < some HP || enemy is Boss)*/){
			//interrupt module
			//useMedPack(enemy);
		}

		if (enemyHP <= 0) {
			if(medPack > 0)
				dropItem();
			Destroy (this.gameObject);
		}


	}
}
