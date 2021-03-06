﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MedPack : MonoBehaviour {
	int timer;
	Text warning;
	// Use this for initialization
	void Start () {
		warning = GetComponent<Text>();
		int timer = 0;
	}
	
	// Update is called once per frame
	void Update () {
		if (timer > 0) {
			timer--;
			if(timer == 0)
				warning.text = "";
		}
	}

	void OnTriggerEnter(Collider obj){
		if (obj.gameObject.tag == "Enemy") {
			AI enemyScript = obj.GetComponent<AI> ();
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
				warning.text = "Your health is already full";
				timer = 10;
			}

		}
	}

	void useMedPack (GameObject obj){
		AI objScript = obj.GetComponent<AI> ();
		objScript.increaseHP(5);
	}
}
