﻿using UnityEngine;
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
			AI enemyScript = obj.GetComponent<AI> ();
			if(!enemyScript.armorOn){
				enemyScript.activateArmor ();
				Destroy (this.gameObject);
			}
		} else if (obj.gameObject.tag == "Player") {
			PlayerController playerScript = obj.GetComponent<PlayerController> ();
			if(!playerScript.armorOn){
				playerScript.activateArmor ();
				Destroy (this.gameObject);
			}
		}
	}
}
