using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MedPack : MonoBehaviour {
	float timer;
	float timerStart;
	// Use this for initialization
	void Start () {
		timer = -1f;;
	}
	
	// Update is called once per frame
	void Update () {
		if (timer > 0 && Time.timeSinceLevelLoad - timerStart > timer) {
			GameManager.hideHealthMessage();
			timer = -1f;
		}
	}

	void OnTriggerEnter(Collider obj){
		if (obj.gameObject.tag == "Enemy") {
			EnemyBasic enemyScript = obj.GetComponent<EnemyBasic> ();
			if(enemyScript.medPack < 1 && enemyScript.enemyType == 0){
				enemyScript.pickUp ();
				Destroy (this.gameObject);
			}
			else if(enemyScript.enemyHP < enemyScript.maxHP){
				enemyScript.increaseHP(5);
				Destroy (this.gameObject);
			}
		} else if (obj.gameObject.tag == "Player") {
			PlayerController playerScript = obj.GetComponent<PlayerController> ();
			if(playerScript.playerHP < playerScript.maxHP){
				playerScript.increaseHP(5);
				Destroy (this.gameObject);
			}
			else{
				//warning.text = "Your health is already full";
				GameManager.displayHealthMessage();
				timerStart = Time.timeSinceLevelLoad;
				timer = 2;
			}

		}
	}

	void useMedPack (GameObject obj){
		EnemyBasic objScript = obj.GetComponent<EnemyBasic> ();
		objScript.increaseHP(5);
	}
}
