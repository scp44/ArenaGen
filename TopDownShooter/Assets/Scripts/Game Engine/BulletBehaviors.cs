using UnityEngine;
using System.Collections;



public class BulletBehaviors : MonoBehaviour {


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

}
