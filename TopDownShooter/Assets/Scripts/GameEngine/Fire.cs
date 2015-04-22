using UnityEngine;
using System.Collections;



public class Fire: MonoBehaviour {
	
	public float life = 10;
	public float damage = 1;
	public float ticksPerMinute = 60;
	private float timeCreated;
	private bool isFiring = false;
	private float lastTimeFired=0;
	private PlayerController player;

	void Start() {
		timeCreated = Time.realtimeSinceStartup;
	}

	void Update(){
		if (Time.realtimeSinceStartup - timeCreated > life) {
			Destroy(this.gameObject);
		}
	}

	void OnTriggerEnter(Collider other){
		if (other.gameObject.tag == "Player") {
			PlayerController playerScript = other.GetComponent<PlayerController>();
			player = playerScript;
			startFiring();
		}
	}

	void OnTriggerExit(Collider other){
		if (other.gameObject.tag == "Player") {
			player = null;
			stopFiring();
		}
	}

	private void startFiring () {
		if (isFiring)
			return;
		else {
			isFiring = true;
			float delayTime;
			delayTime = Mathf.Max(0.001f, ticksPerMinute/60f - (Time.realtimeSinceStartup - lastTimeFired));
			InvokeRepeating("fire", delayTime, ticksPerMinute/60f);
		}
	}

	private void stopFiring () {
		if (isFiring) {
			isFiring = false;
			CancelInvoke("fire");
		}
	}

	private void fire() {
		if (player == null) {
			Debug.LogError("Fire is trying to damage an unexisting player.");
		}
		else {
			player.takeDamage(damage);
		}
	}
}
