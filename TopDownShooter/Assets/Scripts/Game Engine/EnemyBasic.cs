using UnityEngine;
using System.Collections;

public class EnemyBasic : MonoBehaviour {
	public float enemyHP = 5;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (enemyHP <= 0) {
			Destroy (this.gameObject);
		}
	}
}
