using UnityEngine;
using System.Collections;



public class BulletBehaviors : MonoBehaviour {
	public Vector3 startPosition;
	public float lifeSpan = 10;
	public float damage = 1;
	public WeaponInfo info;
	public bool isAoE = false;
	public GameObject explosion;

	private float timer;
	private float timeStart;

	void Start(){
		timeStart = Time.timeSinceLevelLoad;
	}

	void Update(){
		timer = Time.timeSinceLevelLoad - timeStart;
		if (timer >= lifeSpan) {
			if(isAoE){
				Explode ();
			}
			Destroy (this.gameObject);
		}
	}

	void OnTriggerEnter(Collider other){
		if(other.gameObject.tag == "Wall"){
			if(isAoE){
				Explode ();
			}
			Destroy(this.gameObject);
		}
		else if (other.gameObject.tag == "Enemy" || other.gameObject.tag == "Boss") {
			//run enemy health minus
			EnemyBasic enemyScript = other.GetComponent<EnemyBasic>();
			GameObject player = GameObject.FindGameObjectsWithTag ("Player") [0];
			PlayerController playerScript = player.GetComponent<PlayerController>();

			if(!isAoE){
				enemyScript.takeDamage (damage+playerScript.bonusDamage);
				Destroy(this.gameObject);
			}
			else{
				Explode();
				Destroy(this.gameObject);
			}
			//if enemy is not firing, look at player
			if (!enemyScript.isFiring) {
				Vector3 bulletDirection = this.transform.position - startPosition;
				other.transform.rotation = Quaternion.LookRotation(-bulletDirection);
				enemyScript.chase(player.transform.position);
			}
		}
	}

	void Explode(){
		GameObject explosionClone = (GameObject) Instantiate(explosion, transform.position, transform.rotation);
		ExplosionScript explode = explosionClone.GetComponent<ExplosionScript> ();
		explode.damage = damage;
		explode.startPosition = startPosition;
	}
}
