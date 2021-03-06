﻿using UnityEngine;
using System.Collections;



public class EnemyBulletBehaviors : MonoBehaviour {
	
	public float lifeSpan = 10;
	public float damage = 1;
	private float count = 0;
	
	void OnTriggerEnter(Collider other){
		if(other.gameObject.tag == "Wall"){
			Destroy(this.gameObject);
		}
		else if (other.gameObject.tag == "Player") {
			//run enemy health minus
			
			PlayerController enemyScript = other.GetComponent<PlayerController>();
			enemyScript.playerHP -= damage;

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
