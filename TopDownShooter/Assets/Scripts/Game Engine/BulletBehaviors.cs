using UnityEngine;
using System.Collections;



public class BulletBehaviors : MonoBehaviour {

	public float lifeSpan = 10;
	private float count = 0;

	void OnTriggerEnter(Collider other){
		if(other.gameObject.tag == "Wall"){
			Destroy(this.gameObject);
		}
		else if (other.gameObject.tag == "Enemy") {
			//run enemy health minus

			EnemyBasic enemyScript = other.GetComponent<EnemyBasic>();
			enemyScript.enemyHP -= 1;


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
