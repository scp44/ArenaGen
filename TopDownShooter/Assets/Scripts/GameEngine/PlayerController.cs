using UnityEngine;
using System.Collections;
using UnityEngine.UI;

//[RequireComponent (typeof (CharacterController))]
public class PlayerController : MonoBehaviour {

	private const float ARMOR_AMOUNT = 5;
	private const float MEDPACK_HEALTH = 5;

	//player HP
	public float playerHP = 10;
	public float maxHP = 10;
	public float maxArmor = 5;
	public float bonusDamage = 0;
	public float armorBonusHP=0;
	public bool armorOn = false;

	private Quaternion targetRotation;

	//Player mechanics
	public float rotationSpeed = 450;
	public float walkSpeed = 5;
	public float runSpeed = 3;

	private Rigidbody bullet;
	//bullet speed scale
	public float speed = 10;

	public LineRenderer lineRenderer;

	//gun and cooldown tracker
	public int gun1;
	public int gun2;
	private int equipped;
	private WeaponInfo equippedWeapon;
	private float cdStartTime=0;
	public Text equippedtxt;

	public Transform gunPrefPos;
	public Transform bulletPos;
	Transform gun1prefab;
	Transform gun2prefab;

	// Use this for initialization
	void Start () {
		//line renderer stuff
		lineRenderer = GetComponent<LineRenderer> ();
		lineRenderer.SetVertexCount (2);
		lineRenderer.SetWidth (0.01f, 0.01f);
		lineRenderer.SetColors (Color.yellow, Color.yellow);

		GameObject gunSelectInfo = GameObject.Find ("_Main");
		if (gunSelectInfo == null) {
			gun1 = 4;
			gun2 = 3;
		}
		else {
			gunSelectCSharp gunSelectScript = gunSelectInfo.GetComponent <gunSelectCSharp> ();
			gun1 = gunSelectScript.selected[0];
			gun2 = gunSelectScript.selected[1];
		}

		//instantiate the guns we need
		gun1prefab = (Transform)Instantiate (WeaponManager.getWeaponPrefab(gun1), gunPrefPos.position, transform.rotation);
		gun2prefab = (Transform)Instantiate (WeaponManager.getWeaponPrefab(gun2), gunPrefPos.position, transform.rotation);
		gun1prefab.gameObject.SetActive (true);
		gun2prefab.gameObject.SetActive (false);
		gun1prefab.parent = this.transform;
		gun2prefab.parent = this.transform;

		equipped = gun1;
		equippedWeapon = WeaponManager.getWeapon(gun1);
		bullet = WeaponManager.getPlayerBulletPrefab(gun1);
	}
	
	// Update is called once per frame
	void Update () {
		equippedtxt.text = changeGunText (equipped);
		if (Time.timeScale <= 0)
			return;
		
		//if armor broken, cancel effect.
		if (armorOn && armorBonusHP <= 0) {
			armorOn = false;
			armorBonusHP = 0;
		}
		
		//update armor and health bars
		GameManager.updateArmorBar (armorBonusHP, maxArmor);
		GameManager.updateHealthBar (playerHP, maxHP);
		
		Vector3 movement = new Vector3 (0, 0, 0);

		var mouse = Input.mousePosition;
		var screenPoint = Camera.main.WorldToScreenPoint(transform.localPosition);
		var offset = new Vector2(mouse.x - screenPoint.x, mouse.y - screenPoint.y);
		var angle = Mathf.Atan2(offset.x, offset.y) * Mathf.Rad2Deg;
		transform.rotation = Quaternion.Euler (0, angle, 0);

		//Vector3 input = new Vector3 (Input.GetAxisRaw ("Horizontal"), 0, Input.GetAxisRaw ("Vertical"));
		float moveHorizontal = Input.GetAxis ("Horizontal");
		float moveVertical = Input.GetAxis ("Vertical");
		
		movement = new Vector3 (moveHorizontal, 0.5f, moveVertical);
		rigidbody.velocity = movement * walkSpeed;
		
		//fire on click. should handle special firing such as spreads.
//		if (Input.GetMouseButtonDown (0) && Time.timeSinceLevelLoad - cdStartTime > equippedWeapon.bulletCooldown) {
//
//			cdStartTime = Time.timeSinceLevelLoad;
//			FireBullet ();
//		}

		if(Input.GetMouseButton (0)&& Time.timeSinceLevelLoad - lastBulletTime > equippedWeapon.bulletCooldown){
			FireBullet();
			//StartFiring();
		}
		else {
			lineRenderer.SetPosition (0, transform.position);
			lineRenderer.SetPosition (1, transform.position + 4*transform.forward);
		}
		
		if(Input.GetMouseButtonUp(0)){
			StopFiring ();
		}

		//gun switcher
		if (Input.GetMouseButtonDown (1)) {
			StopFiring ();
			SwitchGun ();
		}

		if (playerHP <= 0) {
			GameObject gunSelectInfo = GameObject.Find ("_Main");
			Destroy(gunSelectInfo);
			Application.LoadLevel ("GameOver");
		}
	}

	private bool isFiring = false;
	private float lastBulletTime = -5;

	protected void StartFiring () {
		if (isFiring)
			return;
		else {
			isFiring = true;
			//InvokeRepeating("FireBullet", 0, BulletCooldown());
			float delayTime;
			delayTime = Mathf.Max(0.001f, equippedWeapon.bulletCooldown - (Time.timeSinceLevelLoad - lastBulletTime));
			InvokeRepeating("FireBullet", delayTime, equippedWeapon.bulletCooldown);
		}
	}
	
	protected void StopFiring () {
		if (isFiring) {
			isFiring = false;
			CancelInvoke("FireBullet");
		}
	}

	string changeGunText(int gun){
		if (gun == 0) {
			return "Normal";
		} else if (gun == 1) {
			return "Speed";
		} else if (gun == 2) {
			return "AoE";
		} else if (gun == 3) {
			return "Sprayer";
		} else if (gun == 4) {
			return "Long";
		} else
			return "error";
	}

	//Transfers bullet stats to bullets
	void FireBullet () {
		//var inFront = new Vector3 (0, 1, 0);
		lastBulletTime = Time.timeSinceLevelLoad;
		lineRenderer.SetPosition (0, transform.position);
		lineRenderer.SetPosition (1, transform.position + 4*transform.forward);
		//not sprayer, don't need to fire multiple bullets
		if (equippedWeapon.weaponType != 3) {
			Rigidbody bulletClone = (Rigidbody)Instantiate (bullet, bulletPos.position, transform.rotation);
			//TODO: figure out why player bullets are 10 times slower

			bulletClone.velocity = 10 * transform.forward * speed * equippedWeapon.bulletSpeed;
			BulletBehaviors bulletScript = bulletClone.GetComponent<BulletBehaviors> ();
			bulletScript.lifeSpan = equippedWeapon.bulletLength;
			bulletScript.damage = equippedWeapon.bulletDamage;
			bulletScript.startPosition = transform.position;

			//if weapon is AoE
			if (equippedWeapon.weaponType == 2)
				bulletScript.isAoE = true;
			else
				bulletScript.isAoE = false;
		}

		//weapon is sprayer, need to fire multiple bullets
		else {
			int numBullets = WeaponManager.getnumBullets();
			Vector3 startAngle = Quaternion.AngleAxis (-(WeaponManager.getAngle()/2), Vector3.up) * transform.forward;

			for(int i = 0; i < numBullets; i++){
				Rigidbody bulletClone = (Rigidbody) Instantiate(bullet, bulletPos.position, transform.rotation);
				//TODO: figure out why player bullets are 10 times slower
				bulletClone.velocity = 10*startAngle * speed * equippedWeapon.bulletSpeed;
				BulletBehaviors bulletScript = bulletClone.GetComponent<BulletBehaviors>();
				bulletScript.lifeSpan = equippedWeapon.bulletLength;
				bulletScript.damage = equippedWeapon.bulletDamage;
				bulletScript.startPosition = transform.position;
				
				bulletScript.isAoE = false;

				startAngle = Quaternion.AngleAxis (WeaponManager.getAngleBetween(), Vector3.up) * startAngle;
			}
		}



		//bulletClone.GetComponent<MyRocketScript>().DoSomething();
	}
	//Gun Swap
	void SwitchGun(){
		if (equipped == gun1) {
			equipped = gun2;
			equippedWeapon = WeaponManager.getWeapon(gun2);
			bullet = WeaponManager.getPlayerBulletPrefab(gun2);
			gun1prefab.gameObject.SetActive(false);
			gun2prefab.gameObject.SetActive(true);
		} else {
			equipped = gun1;
			equippedWeapon = WeaponManager.getWeapon(gun1);
			bullet = WeaponManager.getPlayerBulletPrefab(gun1);
			gun1prefab.gameObject.SetActive(true);
			gun2prefab.gameObject.SetActive(false);
		}
		equippedtxt.text = changeGunText (equipped);
	}

	public void increaseHP(int hp){
		playerHP += hp;
	}

	public void takeDamage(float damage) {
		float armorDamage = Mathf.Min (damage, armorBonusHP);
		float hpDamage = damage - armorDamage;
		if (!GameManager.godmode()) {
			armorBonusHP -= armorDamage;
			playerHP -= hpDamage;
		}
	}

	public void activateArmor(){
		armorOn = true;
		armorBonusHP = Mathf.Min (maxArmor, armorBonusHP + ARMOR_AMOUNT);
	}

	public void activateHealthPack(){
		playerHP = Mathf.Min (maxHP, playerHP + MEDPACK_HEALTH);
	}
}