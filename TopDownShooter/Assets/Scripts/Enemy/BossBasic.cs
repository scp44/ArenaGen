using UnityEngine;
using System.Collections;

public class BossBasic : EnemyBasic {
	//A boss has two weapons instead of one
	public int equippedLeft = 3;
	public int equippedRight = 3;
	public Transform bulletStartLeft;
	public Transform bulletStartRight;
	public Vector3 initialPosition;
	public float alertTime = 5;
	public Fire firePrefab;
	public float fireCooldown;
	public float firePlayerInitDistance;
	public float newFireStep;

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
	private bool canSeePlayer = false;
	private Vector3 playerPosition;

	//fire
	private float lastFireGenerated = 0f;
	private Vector3 lastFirePosition = new Vector3(-1,-1,-1);
	private bool isPlacingFire = false;

	protected enum BossWeapon {
		left,
		right
	};

	protected override void Start() {
		base.Start ();
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

		GameObject target = this.visionCheck ();
		if (target != null && target.gameObject.tag == "Player") {
			canSeePlayer = true;
			lookAt (target);
			playerPosition = target.transform.position;
			changeState(STATE_COMBAT);
		}
		else
			canSeePlayer = false;

		switch (state) {
		case STATE_IDLE:
			/*
			 * In the idle state the boss stands still and sometimes
			 * wanders around in a small area
			 */
			StopFiring (BossWeapon.left);
			StopFiring (BossWeapon.right);
			lastFirePosition = new Vector3(-1,-1,-1);
			break;
		case STATE_ALERT:
			/*
			 * In the alert state the boss fires a few more rounds at the
			 * last seen player position, then stays facing this position
			 */
			StopPlacingFire();
			if (timeSinceStateChange > alertTime)
				changeState(STATE_IDLE);
			break;
		case STATE_COMBAT:
			/*
			 * In the combat state the boss fires everything he has at the player
			 */
			if (!canSeePlayer)
				changeState(STATE_ALERT);
			StartFiring (BossWeapon.left);
			StartFiring (BossWeapon.right);
			StartPlacingFire();
			break;
		default:
			Debug.Log ("The boss has unknown state (" + state.ToString() + ")");
			break;
		}
	}

	protected void StartPlacingFire () {
		if (!isPlacingFire) {
			isPlacingFire = true;
			float delayTime;
			delayTime = Mathf.Max(0, fireCooldown - (Time.realtimeSinceStartup - lastFireGenerated));
			InvokeRepeating("PlaceFire", delayTime, fireCooldown);
	
		}
	}

	protected void StopPlacingFire () {
		if (isPlacingFire) {
			isPlacingFire = false;
			CancelInvoke("PlaceFire");
		}
	}

	private void PlaceFire() {
		//Determine the position of the new fire
		Vector3 firePosition;
		if (lastFirePosition.y == -1) {
			Vector2 newPosition = Random.insideUnitCircle * firePlayerInitDistance;
			firePosition = new Vector3(playerPosition.x + newPosition.x, 0.5f, playerPosition.z + newPosition.y);
		}
		else {
			Vector3 direction = (playerPosition - lastFirePosition).normalized;
			float step = newFireStep;
			firePosition = lastFirePosition + direction*step;
		}
		//Put the new fire on the map
		Fire newFire = (Fire)Instantiate (firePrefab, firePosition, transform.rotation);
		lastFirePosition = newFire.transform.position;
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
		EnemyBulletBehaviors bulletScript;
		if (weapon == BossWeapon.left) {
			if (equippedWeaponLeft.weaponType != 3) {
				bulletStartPosition = bulletStartLeft.position;
				lastBulletTimeLeft = Time.realtimeSinceStartup;
				Rigidbody bulletClone = (Rigidbody) Instantiate(bulletLeft, bulletStartPosition, transform.rotation);
				bulletClone.velocity = transform.forward * 100* equippedWeaponLeft.bulletSpeed;
				bulletScript = bulletClone.GetComponent<EnemyBulletBehaviors> ();
				bulletScript.lifeSpan = equippedWeaponLeft.bulletLength;
				bulletScript.damage = equippedWeaponLeft.bulletDamage;
			}
			else{
				int numBullets = WeaponManager.getnumBullets();
				Vector3 startAngle = Quaternion.AngleAxis (-(WeaponManager.getAngle()/2), Vector3.up) * transform.forward;

				bulletStartPosition = bulletStartLeft.position;
				lastBulletTimeLeft = Time.realtimeSinceStartup;

				for(int i = 0; i < numBullets; i++){
					Rigidbody bulletClone = (Rigidbody) Instantiate(bulletLeft, bulletStartPosition, transform.rotation);
					bulletClone.velocity = startAngle * 100* equippedWeaponLeft.bulletSpeed;
					bulletScript = bulletClone.GetComponent<EnemyBulletBehaviors> ();
					bulletScript.lifeSpan = equippedWeaponLeft.bulletLength;
					bulletScript.damage = equippedWeaponLeft.bulletDamage;
					
					startAngle = Quaternion.AngleAxis (WeaponManager.getAngleBetween(), Vector3.up) * startAngle;
				}

			}
		}
		else {
			if (equippedWeaponRight.weaponType != 3) {
				bulletStartPosition = bulletStartRight.position;
				lastBulletTimeRight = Time.realtimeSinceStartup;
				Rigidbody bulletClone = (Rigidbody) Instantiate(bulletRight, bulletStartPosition, transform.rotation);
				bulletClone.velocity = transform.forward * 100* equippedWeaponRight.bulletSpeed;
				bulletScript = bulletClone.GetComponent<EnemyBulletBehaviors> ();
				bulletScript.lifeSpan = equippedWeaponRight.bulletLength;
				bulletScript.damage = equippedWeaponRight.bulletDamage;
			}
			else{
				int numBullets = WeaponManager.getnumBullets();
				Vector3 startAngle = Quaternion.AngleAxis (-(WeaponManager.getAngle()/2), Vector3.up) * transform.forward;
				
				bulletStartPosition = bulletStartRight.position;
				lastBulletTimeRight = Time.realtimeSinceStartup;
				
				for(int i = 0; i < numBullets; i++){
					Rigidbody bulletClone = (Rigidbody) Instantiate(bulletRight, bulletStartPosition, transform.rotation);
					bulletClone.velocity = startAngle * 100* equippedWeaponRight.bulletSpeed;
					bulletScript = bulletClone.GetComponent<EnemyBulletBehaviors> ();
					bulletScript.lifeSpan = equippedWeaponRight.bulletLength;
					bulletScript.damage = equippedWeaponRight.bulletDamage;
					
					startAngle = Quaternion.AngleAxis (WeaponManager.getAngleBetween(), Vector3.up) * startAngle;
				}
				
			}
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
