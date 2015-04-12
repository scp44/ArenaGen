﻿using UnityEngine;
using System.Collections;



public class BulletBehaviors : MonoBehaviour {
	public Vector3 startPosition;
	public float lifeSpan = 10;
	public float damage = 1;
	public WeaponInfo info;
	public bool isAoE = false;
	private float count = 0;
	public Rigidbody explosion;

	void Update(){
		count++;
		if (count >= lifeSpan) {
			if(isAoE){
				Explode ();
			}
			Destroy (this.gameObject);
		}
	}

	void OnTriggerEnter(Collider other){
		if(other.gameObject.tag == "Wall"){
			if(isAoE){
				Explode ();
			}
			Destroy(this.gameObject);
		}
		else if (other.gameObject.tag == "Enemy" && !isAoE) {
			//run enemy health minus
			EnemyBasic enemyScript = other.GetComponent<EnemyBasic>();
			enemyScript.takeDamage (damage);
			Destroy(this.gameObject);
			//if enemy is not firing, look at player
			if (!enemyScript.isFiring) {
				Vector3 bulletDirection = this.transform.position - startPosition;
				other.transform.rotation = Quaternion.LookRotation(-bulletDirection);
			}
		}
	}

	void Explode(){
		Rigidbody explosionClone = (Rigidbody) Instantiate(explosion, transform.position, transform.rotation);
		ExplosionScript explode = explosionClone.GetComponent<ExplosionScript> ();
		explode.damage = damage;
		explode.startPosition = startPosition;
	}
}
