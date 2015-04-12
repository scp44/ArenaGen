using UnityEngine;
using System.Collections;

public class ExplosionScript : MonoBehaviour {
	public float damage = 1;
	public Vector3 startPosition;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

	}

	void OnTriggerEnter(Collider other){
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
