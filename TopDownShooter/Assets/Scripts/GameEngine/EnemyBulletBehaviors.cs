using UnityEngine;
using System.Collections;



public class EnemyBulletBehaviors : MonoBehaviour {
	
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
		else if (other.gameObject.tag == "Player") {
			//run enemy health minus
			PlayerController playerScript = other.GetComponent<PlayerController>();
			playerScript.takeDamage (damage);
			Destroy(this.gameObject);
		}
	}
}
