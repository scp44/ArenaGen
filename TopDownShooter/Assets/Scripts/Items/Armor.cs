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
			if(enemyScript.armorBonusHP<=0){
				enemyScript.activateArmor ();
				Destroy (this.gameObject);
			}
		} else if (obj.gameObject.tag == "Player") {
			PlayerController playerScript = obj.GetComponent<PlayerController> ();
			if(playerScript.armorBonusHP<=0){
				playerScript.activateArmor ();
				Destroy (this.gameObject);
			}
		}
	}
}
