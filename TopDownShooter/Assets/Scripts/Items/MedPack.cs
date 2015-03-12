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
			enemyScript.pickUp ();
		} else if (obj.gameObject.tag == "Player") {
			useMedPack(obj.gameObject);
		}
	}

	void useMedPack (GameObject obj){
		EnemyBasic objScript = obj.GetComponent<EnemyBasic> ();
		objScript.increaseHP(5);
	}

	void pickUp (/*object ID*/){
		//ID.medPack++;
	}
}
