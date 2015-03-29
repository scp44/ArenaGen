using UnityEngine;
using System.Collections;

public class MedPack : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter(Collider obj){
		if (obj.gameObject.tag == "Enemy") {
			EnemyBasic enemyScript = obj.GetComponent<EnemyBasic> ();
			if(enemyScript.medPack < 1 /*&& enemyScript.isMedic*/)
				enemyScript.pickUp ();
			else
				enemyScript.increaseHP(5);
			Destroy (this.gameObject);
		} else if (obj.gameObject.tag == "Player") {
			PlayerController objScript = obj.GetComponent<PlayerController> ();
			objScript.increaseHP(5);
			Destroy (this.gameObject);
		}
	}

	void useMedPack (GameObject obj){
		EnemyBasic objScript = obj.GetComponent<EnemyBasic> ();
		objScript.increaseHP(5);
	}
}
