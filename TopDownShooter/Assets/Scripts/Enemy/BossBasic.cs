using UnityEngine;
using System.Collections;

public class BossBasic : MonoBehaviour {
	public float enemyHP = 5;
	public float maxHP = 5;
	public float equipped = 0;
	public int gun1 = 0;
	public int gun2 = 0;
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
	
	public Rigidbody Bullet;
	// Use this for initialization
	
	public AI searchScript;
	
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

	void SwitchGun(){
		if (equipped == gun1) {
			equipped = gun2;
		} else {
			equipped = gun1;
		}
	}
	
	public GameObject visionCheck(){
		GameObject[] pUs = GameObject.FindGameObjectsWithTag ("MedPackPU");
		GameObject player = GameObject.FindGameObjectsWithTag ("Player") [0];
		GameObject closestPack = pUs[0];
		
		Vector3 forward = transform.forward;
		float angle;
		Vector3 targetDir;
		
		
		Transform playerLocPos = player.transform;
		targetDir = playerLocPos.position - transform.position;
		angle = Vector3.Angle (targetDir, forward);
		
		if ((playerLocPos.position - transform.position).magnitude < (visionScale) && angle <= fov) {
			this.playerPos = playerLocPos;
			//this.lastTimeSeen = this.LocationInfo.timestamp;
			return player;
		}
		
		int i;
		for (i = 0; i < pUs.Length; i++) {
			Transform powerUpPos = pUs [i].transform;
			GameObject powerUp = pUs [i];
			targetDir = powerUpPos.position - transform.position;
			angle = Vector3.Angle (targetDir,forward); 
			
			if ((powerUpPos.position - transform.position).magnitude < (visionScale) && angle <= fov) {
				if((powerUpPos.position - transform.position).magnitude < (closestPack.transform.position - transform.position).magnitude){
					closestPack = powerUp;		}
			}
		}
		if ((closestPack.transform.position - transform.position).magnitude < (visionScale) && angle <= fov) {
			return closestPack;
		}
		return null;
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
	
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		//if armor broken, cancel effect.
		if (armorOn && armorCount <= 0) {
			armorOn = false;
			armorCount = 0;
		}
		
		GameObject target = null;
		if(!armorOn /*armor in line of sight*/){
			//interrupt module
			//target = armor;
		}
		
		if(enemyHP < 5 /*&& medpack in line of sight*/){
			//interrupt module
			//if(medPack is closer than current target)
			//target = medPack;
		}
		
		if (target != null)
			searchScript.goTo (target);
		
		if (enemyHP <= 0) {
			Destroy (this.gameObject);
		}
		
		
	}
}
