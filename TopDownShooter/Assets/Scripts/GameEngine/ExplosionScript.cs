using UnityEngine;
using System.Collections;

public class ExplosionScript : MonoBehaviour {
	public float damage = 1;
	public Vector3 startPosition;
	private int timer;
	private int lifespan = 3;
	// Use this for initialization
	void Start () {
		timer = 0;
	}
	
	// Update is called once per frame
	void Update () {
		timer++;
		if (timer >= lifespan)
			Destroy (this.gameObject);
	}

	void OnTriggerEnter(Collider other){
		//run enemy health minus
		if (other.tag == "Enemy") {
			EnemyBasic enemyScript = other.GetComponent<EnemyBasic> ();
			enemyScript.takeDamage (damage);
			if(timer >= lifespan)
				Destroy (this.gameObject);
			//if enemy is not firing, look at player
			if (!enemyScript.isFiring) {
				Vector3 bulletDirection = this.transform.position - startPosition;
				other.transform.rotation = Quaternion.LookRotation (-bulletDirection);
			}
		} else {
			Destroy (this.gameObject);
		}
	}
}
