using UnityEngine;
using System.Collections;

//[RequireComponent (typeof (CharacterController))]
public class PlayerController : MonoBehaviour {

	private const float ARMOR_AMOUNT = 5;
	private const float MEDPACK_HEALTH = 5;

	//player HP
	public float playerHP = 10;
	public float maxHP = 10;
	public float armorBonusHP;
	public bool armorOn = false;

	private Quaternion targetRotation;

	//Player mechanics
	public float rotationSpeed = 450;
	public float walkSpeed = 5;
	public float runSpeed = 3;

	private Rigidbody bullet;
	//bullet speed scale
	public float speed = 10;

	//gun and cooldown tracker
	public int gun1;
	public int gun2;
	private int equipped;
	private WeaponInfo equippedWeapon;
	private float cdStartTime=0;

	// Use this for initialization
	void Start () {
		equipped = gun1;
		equippedWeapon = WeaponManager.getWeapon(gun1);
		bullet = WeaponManager.getPlayerBulletPrefab(gun1);
	}
	
	// Update is called once per frame
	void Update () {
		if (Time.timeScale <= 0)
			return;
		
		//if armor broken, cancel effect.
		if (armorOn && armorBonusHP <= 0) {
			armorOn = false;
			armorBonusHP = 0;
		}
		
		//update armor and health bars
		GameManager.updateArmorBar (armorBonusHP/maxHP);
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
		if (Input.GetMouseButtonDown (0) && Time.timeSinceLevelLoad - cdStartTime > equippedWeapon.bulletCooldown) {
			cdStartTime = Time.timeSinceLevelLoad;
			FireBullet ();
		}
		
		//gun switcher
		if (Input.GetMouseButtonDown (1)) {
			SwitchGun ();
		}
		
		if (playerHP <= 0)
			Application.LoadLevel ("GameOver");
	}

	//Transfers bullet stats to bullets
	void FireBullet () {
		var inFront = new Vector3 (0, 1, 0);

		Rigidbody bulletClone = (Rigidbody) Instantiate(bullet, transform.position, transform.rotation);
		//TODO: figure out why player bullets are 10 times slower
		bulletClone.velocity = 10*transform.forward * speed * equippedWeapon.bulletSpeed;
		BulletBehaviors bulletScript = bulletClone.GetComponent<BulletBehaviors>();
		bulletScript.lifeSpan = equippedWeapon.bulletLength;
		bulletScript.damage = equippedWeapon.bulletDamage;
		bulletScript.startPosition = transform.position;

		//bulletClone.GetComponent<MyRocketScript>().DoSomething();
	}
	//Gun Swap
	void SwitchGun(){
		if (equipped == gun1) {
			equipped = gun2;
			equippedWeapon = WeaponManager.getWeapon(gun2);
			bullet = WeaponManager.getPlayerBulletPrefab(gun2);
		} else {
			equipped = gun1;
			equippedWeapon = WeaponManager.getWeapon(gun1);
			bullet = WeaponManager.getPlayerBulletPrefab(gun1);
		}
	}

	public void increaseHP(int hp){
		playerHP += hp;
	}

	public void takeDamage(float damage) {
		float armorDamage = Mathf.Min (damage, armorBonusHP);
		float hpDamage = damage - armorDamage;
		armorBonusHP -= armorDamage;
		playerHP -= hpDamage;
	}

	public void activateArmor(){
		armorOn = true;
		armorBonusHP = ARMOR_AMOUNT;
	}
}