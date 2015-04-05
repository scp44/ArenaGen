using UnityEngine;
using System.Collections;



public class EnemyBullet2Behaviors : MonoBehaviour {
	
	public float lifeSpan = 10;
	public float damage = 1;
	private float count = 0;
	
	void OnTriggerEnter(Collider other){
		if(other.gameObject.tag == "Wall"){
			Destroy(this.gameObject);
		}
		else if (other.gameObject.tag == "Player") {
			//run enemy health minus
			
			AI enemyScript = other.GetComponent<AI>();
			enemyScript.enemyHP -= damage;
			
			
			Destroy(this.gameObject);
		}
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
