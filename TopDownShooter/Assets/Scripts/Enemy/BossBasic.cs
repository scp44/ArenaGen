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

	//Rotation AI
	public float minIdleTime;
	public float maxIdleTime;
	private float currIdleTime=0;
	private float lastRotationTime=0;

	//Arena for phase 3
	public Transform bossArena;
	public float arenaBossDistance;
	public Fire rotatingFire;

	//Phases of the boss encounter
	[Range(0f, 1f)]
	public float phase2start;
	[Range(0f, 1f)]
	public float phase3start;
	public int phase=0;

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

	//rotating fire
	public float rotFireSpeed = 10f;
	public float minFireRotTime = 5;
	public float maxFireRotTime = 10;
	private float rotatingDirectionChangeTime = 0;
	private float lastRotatingDirectionChangeTime = 0;
	private float direction = 1;
	

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

		//use difficulty to determine some parameters
		fireCooldown = -2*difficulty+3; //3s to 1s
		firePlayerInitDistance = -4 * difficulty + 8; //8 to 4

		//boss has different sprayer gun stats
		equippedWeaponLeft.bulletLength = 8f;
		equippedWeaponLeft.bulletDamage = 1f;
		equippedWeaponLeft.bulletSpeed = 0.03f;
		equippedWeaponLeft.bulletCooldown = 2;

		changeState (STATE_IDLE);
	}

	protected override void Update() {
		base.Update ();
		if (this.enemyHP <= 0) {
			GameObject gunSelectInfo = GameObject.Find ("_Main");
			Destroy(gunSelectInfo);
			Application.LoadLevel ("WinScreen");
		}

		//update phase depending on health
		float healthRatio = enemyHP / maxHP;
		if (phase == 0)
			phase = 1;
		else if (phase == 1 && healthRatio < phase2start && healthRatio > phase3start)
			phase = 2;
		else if (phase == 2 && healthRatio < phase3start) {
			//Phase 3 transition code
			phase = 3;
			transitionIntoPhase3();
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
			if (phase == 2)
				StopPlacingFire();
			StopFiring (BossWeapon.right);
			StopFiring (BossWeapon.left);
			lastFirePosition = new Vector3(-1,-1,-1);
			//rotate periodically
			if (Time.timeSinceLevelLoad - lastRotationTime > currIdleTime) {
				currIdleTime = Random.Range (minIdleTime, maxIdleTime);
				this.transform.LookAt(transform.position + new Vector3(Random.Range (-1f, 1f), 0f, Random.Range (-1f,1f)));
				lastRotationTime = Time.timeSinceLevelLoad;
			}
			break;
		case STATE_ALERT:
			/*
			 * In the alert state the boss fires a few more rounds at the
			 * last seen player position, then stays facing this position
			 */
			if (phase == 1)
				StopPlacingFire();
			else if (phase == 2)
				StopFiring (BossWeapon.right);
			if (timeSinceStateChange > alertTime)
				changeState(STATE_IDLE);
			break;
		case STATE_COMBAT:
			/*
			 * In the combat state the boss fires everything he has at the player
			 */
			if (!canSeePlayer && phase != 3)
				changeState(STATE_ALERT);
			if (phase == 1) {
				StartFiring (BossWeapon.right);
				StartPlacingFire();
			}
			else if (phase == 2) {
				StopFiring (BossWeapon.right);
				StartFiring (BossWeapon.left);
				StartPlacingFire();
			}
			else if (phase == 3) {
				rotatingFire.transform.RotateAround(rotatingFire.transform.position, rotatingFire.transform.forward, direction*rotFireSpeed*Time.deltaTime);
				if (Time.timeSinceLevelLoad - lastRotatingDirectionChangeTime > rotatingDirectionChangeTime) {
					rotatingDirectionChangeTime = Random.Range (minFireRotTime, maxFireRotTime);
					lastRotatingDirectionChangeTime = Time.timeSinceLevelLoad;
					direction = -direction;
				}
			}
			break;
		default:
			Debug.Log ("The boss has unknown state (" + state.ToString() + ")");
			break;
		}
	}

	private void transitionIntoPhase3() {
		this.rigidbody.isKinematic = true; // no more physics
		StopPlacingFire();
		//Teleport the boss and the player to the final arena
		Vector3 newPosition = bossArena.transform.position;
		newPosition.y = 0.5f;
		transform.position = newPosition;
		Vector3 playerOffset = arenaBossDistance*(new Vector3(Random.Range(-1f,1f), 0f, Random.Range(-1f,1f))).normalized;
		player.transform.position = newPosition + playerOffset;
		transform.LookAt(player.transform.position);
		player.transform.LookAt(transform.position);
		//Generate fire outside of the arena
		int numFires = 90;
		float fireDistance = 0.95f*bossArena.transform.localScale.y/2f;
		for (int i=0; i<numFires; i++) {
			float angle = Mathf.Deg2Rad*i*(360f/numFires);
			float fire_x = Mathf.Sin(angle)*fireDistance;
			float fire_z = Mathf.Cos(angle)*fireDistance;
			Vector3 fireOffset = new Vector3(fire_x, 0f, fire_z);
			Vector3 firePosition = bossArena.transform.position + fireOffset;
			Fire newFire = (Fire)Instantiate (firePrefab, firePosition, transform.rotation);
			newFire.damage = 100;
			newFire.life = 10000;
		}
		//Place the rotating fire properly
		rotatingFire.transform.position = bossArena.transform.position;
		//rotatingFire.transform.LookAt (player.transform.position);
		//The fireball gun is more powerful
		equippedWeaponRight.bulletSpeed = 0.014f;
		equippedWeaponRight.bulletCooldown = -0.7f * difficulty + 1.2f;
		equippedWeaponRight.bulletLength = 300f;
		StopFiring (BossWeapon.right);
		StartFiring(BossWeapon.left);
		StartFiring(BossWeapon.right);
		changeState (STATE_COMBAT);
	}

	protected void StartPlacingFire () {
		if (!isPlacingFire) {
			isPlacingFire = true;
			float delayTime;
			delayTime = Mathf.Max(0.001f, fireCooldown - (Time.realtimeSinceStartup - lastFireGenerated));
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
		lastFireGenerated = Time.realtimeSinceStartup;
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
		//Depending on left/right weapon and gun (sprayer vs. not), perform appropriate action
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
				Vector3 direction = transform.forward;
				if (phase == 3)
					direction = Quaternion.AngleAxis(Random.Range(-80f, 80f), Vector3.up) * transform.forward;
				Rigidbody bulletClone = (Rigidbody) Instantiate(bulletRight, bulletStartPosition, Quaternion.LookRotation(direction));
				bulletClone.velocity = direction * 100* equippedWeaponRight.bulletSpeed;
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
