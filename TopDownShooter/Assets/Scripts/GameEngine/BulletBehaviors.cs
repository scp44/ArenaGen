using UnityEngine;
using System.Collections;



public class BulletBehaviors : MonoBehaviour {
	public Vector3 startPosition;
	public float lifeSpan = 10;
	public float damage = 1;
	private float count = 0;

	void Update(){
		count++;
		if (count >= lifeSpan) {
			Destroy (this.gameObject);
		}
	}

	void OnTriggerEnter(Collider other){
		if(other.gameObject.tag == "Wall"){
			Destroy(this.gameObject);
		}
		else if (other.gameObject.tag == "Enemy") {
			//run enemy health minus
			EnemyBasic enemyScript = other.GetComponent<EnemyBasic>();
			enemyScript.takeDamage (damage);
			Destroy(this.gameObject);
			//if enemy is not firing, look at player
			if (!enemyScript.isFiring) {
				Vector3 bulletDirection = this.transform.position - startPosition;
				other.transform.rotation = Quaternion.LookRotation(-bulletDirection);
			}
		}
	}
}
