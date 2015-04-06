using UnityEngine;
using System.Collections;

//[RequireComponent (typeof (CharacterController))]
public class PlayerController : MonoBehaviour {

	//player HP
	public float playerHP = 10;
	public float maxHP = 10;
	public int armorCount;
	public bool armorOn = false;

	private Quaternion targetRotation;

	//Player mechanics
	public float rotationSpeed = 450;
	public float walkSpeed = 5;
	public float runSpeed = 3;

	public Rigidbody Bullet;
	//bullet speed scale
	public float speed = 10;

	//gun and cooldown tracker
	public float gun1;
	public float gun2;
	private float equipped;
	private float cd1;
	private float cd2;
	private float cdEq = 0;

	//Gun Types and bullet mechanics
	//#of updates til bullet selfdestructs
	float BulletLength(){
		if (equipped == 0) {
			return 50f;
		} else if (equipped == 1) {
			return 15f;
		}else if (equipped == 2) {
			return 100f;
		}else if (equipped == 3) {
			return 30f;
		}else if (equipped == 4) {
			return 200f;
		}
		else{
			return 10f;
		}
	}
	//number of frames between shots
	float BulletCooldown(){
		if (equipped == 0) {
			return 20f;
		} else if (equipped == 1) {
			return 1f;
		}else if (equipped == 2) {
			return 50f;
		}else if (equipped == 3) {
			return 5f;
		}else if (equipped == 4) {
			return 1f;
		}
		else{
			return 10f;
		}
	}
	//amount of damage per bullet
	float BulletDamage(){
		if (equipped == 0) {
			return 3f;
		} else if (equipped == 1) {
			return 1f;
		} else if (equipped == 2) {
			return 10f;
		} else if (equipped == 3) {
			return 2f;
		} else if (equipped == 4) {
			return 1f;
		} else {
			return 10f;
		}
	}
	//individual bullet velocity scale
	float BulletSpeed(){
		if (equipped == 0) {
			return 1f;
		} else if (equipped == 1) {
			return 3f;
		} else if (equipped == 2) {
			return 10f;
		} else if (equipped == 3) {
			return 1f;
		} else if (equipped == 4) {
			return 2f;
		} else {
			return 10f;
		}
	}

	//Transfers bullet stats to bullets
	void FireBullet () {
		var inFront = new Vector3 (0, 1, 0);

		Rigidbody bulletClone = (Rigidbody) Instantiate(Bullet, transform.position, transform.rotation);
		bulletClone.velocity = transform.forward * speed * BulletSpeed();
		BulletBehaviors bulletScript = bulletClone.GetComponent<BulletBehaviors>();
		bulletScript.lifeSpan = BulletLength ();
		bulletScript.damage = BulletDamage ();

		//bulletClone.GetComponent<MyRocketScript>().DoSomething();
	}
	//Gun Swap
	void SwitchGun(){
		if (equipped == gun1) {
			equipped = gun2;
		} else {
			equipped = gun1;
		}
	}

	public void increaseHP(int HP){
		playerHP += 5;
	}

	public void activateArmor(){
		armorOn = true;
		armorCount = 5;
	}


	// Use this for initialization
	void Start () {
		equipped = gun1;

	}

	// Update is called once per frame
	void Update () {
		if (Time.timeScale <= 0)
			return;
	
		//if armor broken, cancel effect.
		if (armorOn && armorCount <= 0) {
			armorOn = false;
			armorCount = 0;
		}

		//update armor and health bars
		GameManager.updateArmorBar (armorCount/maxHP);
		GameManager.updateHealthBar (playerHP/maxHP);

		Vector3 movement = new Vector3 (0, 0, 0);

		var mouse = Input.mousePosition;
		var screenPoint = Camera.main.WorldToScreenPoint(transform.localPosition);
		var offset = new Vector2(mouse.x - screenPoint.x, mouse.y - screenPoint.y);
		var angle = Mathf.Atan2(offset.x, offset.y) * Mathf.Rad2Deg;
		transform.rotation = Quaternion.Euler(0, angle, 0);

		//Vector3 input = new Vector3 (Input.GetAxisRaw ("Horizontal"), 0, Input.GetAxisRaw ("Vertical"));
		float moveHorizontal = Input.GetAxis ("Horizontal");
		float moveVertical = Input.GetAxis ("Vertical");
		
		movement = new Vector3 (moveHorizontal, 0.0f, moveVertical);
		rigidbody.velocity = movement * walkSpeed;

		//fire on click. should handle special firing such as spreads.
		if (Input.GetMouseButtonDown (0) && cdEq > BulletCooldown ()) {
			cdEq = 0;
			FireBullet ();
		}
		cdEq++;


		//gun switcher
		if (Input.GetMouseButtonDown (1)) {
			SwitchGun ();
		}

		if (playerHP <= 0)
			Application.LoadLevel ("GameOver");
	}

}