using UnityEngine;
using System.Collections;

public class Armor : MonoBehaviour {
	float timer;
	float timerStart;
	// Use this for initialization
	void Start () {
		timer = -1f;;
	}
	
	// Update is called once per frame
	void Update () {
		if (timer > 0 && Time.timeSinceLevelLoad - timerStart > timer) {
			GameManager.hideArmorMessage();
			timer = -1f;
		}
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
			if(playerScript.armorBonusHP < playerScript.maxArmor){
				playerScript.activateArmor ();
				Destroy (this.gameObject);
			}
			else{
				//warning.text = "Your health is already full";
				GameManager.displayArmorMessage();
				timerStart = Time.timeSinceLevelLoad;
				timer = 2;
			}
		}
	}
}
