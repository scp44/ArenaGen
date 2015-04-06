using UnityEngine;
using System.Collections;



public class BulletBehaviors : MonoBehaviour {

	public float lifeSpan = 10;
	public float damage = 1;
	private float count = 0;

	void OnTriggerEnter(Collider other){
		if(other.gameObject.tag == "Wall"){
			Destroy(this.gameObject);
		}
		else if (other.gameObject.tag == "Enemy") {
			//run enemy health minus

			takeDamage (damage, other.gameObject);

			Destroy(this.gameObject);
		}
	}

	void takeDamage(float damage, GameObject other) {
		EnemyBasic enemyScript = other.GetComponent<EnemyBasic>();
		
		if (enemyScript.armorOn) {
			damage--;
			enemyScript.armorBonusHP--;
		}
		
		enemyScript.enemyHP -= damage;
	}

	void Update(){
		count++;
		if (count >= lifeSpan) {
			Destroy (this.gameObject);
		}
		if (Input.GetMouseButtonDown(1)) {
			
			Destroy(this.gameObject);
		}
	}


}
