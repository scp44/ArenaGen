using UnityEngine;
using System.Collections;

public class BossBasic : EnemyBasic {
	//A boss has two weapons instead of one
	public int equippedLeft;
	public int equippedRight;
	public Transform bulletStartLeft;
	public Transform bulletStartRight;
	public Vector3 initialPosition;
	[Range(0f, 1f)]
	public float idleAmount;

	//States of the boss AI
	private const int STATE_IDLE = 0;
	private const int STATE_ALERT = 1;
	private const int STATE_COMBAT = 2;

	//other private variables
	private Rigidbody bulletLeft;
	private Rigidbody bulletRight;
	private float lastBulletTimeLeft = 0;
	private float lastBulletTimeRight = 0;
	protected WeaponInfo equippedWeaponLeft;
	protected WeaponInfo equippedWeaponRight;
	private bool isFiringLeft = false;
	private bool isFiringRight = false;

	public Transform rightHand;
	public Transform leftHand;
	Transform gunRprefab;
	Transform gunLprefab;

	protected enum BossWeapon {
		left,
		right
	};

	protected override void Start() {
		base.Start ();
		equippedLeft = Random.Range (0, WeaponManager.numWeapons);
		equippedRight = (equippedLeft + Random.Range (0, WeaponManager.numWeapons))%WeaponManager.numWeapons;
		if (equippedRight == equippedLeft)
			equippedRight = (equippedRight+1)%3;
		equippedWeaponLeft = WeaponManager.getWeapon (equippedLeft);
		equippedWeaponRight = WeaponManager.getWeapon (equippedRight);
		bulletLeft = WeaponManager.getEnemyBulletPrefab (equippedLeft);
		bulletRight = WeaponManager.getEnemyBulletPrefab (equippedRight);

		gunRprefab = (Transform)Instantiate (WeaponManager.getWeaponPrefab(equippedRight), rightHand.position, transform.rotation);
		gunLprefab = (Transform)Instantiate (WeaponManager.getWeaponPrefab(equippedLeft), leftHand.position, transform.rotation);
		gunRprefab.GetChild(0).renderer.enabled = true;
		gunRprefab.parent = this.transform;
		gunLprefab.GetChild(0).renderer.enabled = true;
		gunLprefab.parent = this.transform;

		changeState (STATE_IDLE);
	}

	protected override void Update() {
		base.Update ();
		if (this.enemyHP <= 0) {
			Application.LoadLevel ("WinScreen");
		}

		GameManager.updateBossHealthBar (enemyHP, maxHP);

		GameObject target = this.visionCheck ();//wonder if it should return a boolean 
		if (target != null) {
			lookAt (target);
			float x = target.transform.position.x; //use to make enemy silly
			float z = target.transform.position.z;
			
			Vector3 newTarget = new Vector3(x,0f,z);
			
			chase (newTarget);
			if (target.gameObject.tag == "Player") {
				StartFiring (BossWeapon.left);
				StartFiring (BossWeapon.right);
			}
			else {
				StopFiring (BossWeapon.left);
				StopFiring (BossWeapon.right);
			}
		}
		else {
			StopFiring (BossWeapon.left);
			StopFiring (BossWeapon.right);
		}

		switch (state) {
		case STATE_IDLE:
			/*
			 * In the idle state the boss stands still and sometimes
			 * wanders around in a small area
			 */
			break;
		case STATE_ALERT:
			/*
			 * In the alert state the boss fires a few more rounds at the
			 * last seen player position, then stays facing this position
			 */
			break;
		case STATE_COMBAT:
			/*
			 * In the combat state the boss fires everything he has at the player
			 */
			break;
		default:
			Debug.Log ("The boss has unknown state (" + state.ToString() + ")");
			break;
		}
	}

	//Transfers bullet stats to bullets
	protected void FireBullet (BossWeapon weapon) {
		if (bulletStartLeft == null) {
			Debug.LogError("Left bullet start position is not specified.");
		}
		if (bulletStartRight == null) {
			Debug.LogError("Right bullet start position is not specified.");
		}
		Vector3 bulletStartPosition;
		Rigidbody bulletClone;
		EnemyBulletBehaviors bulletScript;
		if (weapon == BossWeapon.left) {
			bulletStartPosition = bulletStartLeft.position;
			lastBulletTimeLeft = Time.realtimeSinceStartup;
			bulletClone = (Rigidbody) Instantiate(bulletLeft, bulletStartPosition, transform.rotation);
			bulletClone.velocity = transform.forward * 100* equippedWeaponLeft.bulletSpeed;
			bulletScript = bulletClone.GetComponent<EnemyBulletBehaviors> ();
			bulletScript.lifeSpan = equippedWeaponLeft.bulletLength;
			bulletScript.damage = equippedWeaponLeft.bulletDamage;
		}
		else {
			bulletStartPosition = bulletStartRight.position;
			lastBulletTimeRight = Time.realtimeSinceStartup;
			bulletClone = (Rigidbody) Instantiate(bulletRight, bulletStartPosition, transform.rotation);
			bulletClone.velocity = transform.forward * 100 * equippedWeaponRight.bulletSpeed;
			bulletScript = bulletClone.GetComponent<EnemyBulletBehaviors> ();
			bulletScript.lifeSpan = equippedWeaponRight.bulletLength;
			bulletScript.damage = equippedWeaponRight.bulletDamage;
		}
	}

	private void FireBulletLeft() {
		FireBullet (BossWeapon.left);
	}

	private void FireBulletRight() {
		FireBullet (BossWeapon.right);
	}

	protected void StartFiring (BossWeapon weapon) {
		if (weapon == BossWeapon.left && !isFiringLeft) {
			isFiringLeft = true;
			float delayTime;
			delayTime = Mathf.Max(0, equippedWeaponLeft.bulletCooldown - (Time.realtimeSinceStartup - lastBulletTimeLeft));
			InvokeRepeating("FireBulletLeft", delayTime, equippedWeaponLeft.bulletCooldown);
		}
		else if (weapon == BossWeapon.right && !isFiringRight) {
			isFiringRight = true;
			float delayTime;
			delayTime = Mathf.Max(0, equippedWeaponRight.bulletCooldown - (Time.realtimeSinceStartup - lastBulletTimeRight));
			InvokeRepeating("FireBulletRight", delayTime, equippedWeaponRight.bulletCooldown);
		}
	}
	
	protected void StopFiring (BossWeapon weapon) {
		if (weapon == BossWeapon.left && isFiringLeft) {
			isFiringLeft = false;
			CancelInvoke("FireBulletLeft");
		}
		else if (weapon == BossWeapon.right && isFiringRight) {
			isFiringRight = false;
			CancelInvoke("FireBulletRight");
		}
	}
}
