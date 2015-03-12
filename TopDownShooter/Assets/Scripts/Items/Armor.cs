using UnityEngine;
using System.Collections;

public class Armor : MonoBehaviour {
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

	}

	void OnTriggerEnter (Collider obj){
		if (obj.gameObject.tag == "Enemy") {
			EnemyBasic enemyScript = obj.GetComponent<EnemyBasic> ();
			enemyScript.activateArmor ();
		} else if (obj.gameObject.tag == "Player") {
			EnemyBasic playerScript = obj.GetComponent<EnemyBasic> ();
			playerScript.activateArmor ();
		}
	}
}
