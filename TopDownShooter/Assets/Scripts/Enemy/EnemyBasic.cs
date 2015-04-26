//#define ASTARDEBUG
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using Pathfinding.RVO;

// visionCheck() returns the closest medpack or the player if closer if in field of view and not obstructed by a wall. else returns null.
//commCheck() should be called in all updates- passes information to those in radius
//move() moves the enemy to the curTarget- for example, curTarget = visionCheck(); move(); would move the enemy to the player if the player is in view.
//moveTo(Vector3) moves the enemy to a specific position- you can specify a point a certain magnitude away, for instance.
//lookAt(GameObject)
//StartFiring()
//StopFiring()

//This class defines the basic enemy operations including AI
//It is used as base class for specific enemy classes
[RequireComponent(typeof(Seeker))]
[AddComponentMenu("Pathfinding/AI/AIPath (generic)")]
public class EnemyBasic: MonoBehaviour {

	//define enemy types
	private const int ENEMY_HEALER = 0;
	private const int ENEMY_SOLDIER = 1;
	private const int ENEMY_DEFENDER = 2;
	private const int ENEMY_BOSS = 3;
	private bool reenable = true;                //check if need to reenable pathsearch, see onEnable(), onDisenable()
	private const float ARMOR_AMOUNT = 5;
	private const float MEDPACK_HEALTH = 5;

	//basic enemy parameters
	public int enemyType;
	public float speed = 3;
	public float wanderSpeed = 1;
	public float enemyHP = 5;
	public float maxHP = 5;	
	public int equipped = 0;	//weapon type equipped
	public float movementSpeed = 100;
	public float commScale = 4;
	public float visionScale = 10;
	public float alertScale = 15; 
	public float fov = 100;
	public bool isFiring = false;
	protected float difficulty;
	public float wanderTimeMax = 240;
	public float wanderTimeMin = 120;
	protected float timeLeft;
	public Transform bulletStartPosition;
	public Transform hands;
	//information that can be passed between enemies
	public PassedInfo passedInfo;
	//comm check true?


	//check if it is start of pathfinding
	private int EnemyState = 0;      //0 represent need to be initial, 1 represent keep pathfinding, 2 represent means halt

	//power up variables
	public float armorBonusHP;
	public int medPack = 0;
	public GameObject MedPack;

	//AI parameters
	protected float timeSinceStateChange = 0;
	protected int state = -1; //the states are defined in the specific enemy classes

	//Do we really need these 2 variables?
	//All enemies can search and move
	public bool canSearch = true;
	public bool canMove = true;

	//internal variables
	private Vector3 target;
	private Vector3 targetDir;
	private GameObject[] pUs;
	protected GameObject player;
	protected WeaponInfo equippedWeapon;
	public Rigidbody bullet;
	//private GameObject toReturn = null;
	private float angle;
	private int toUse = 0;
	private float lastBulletTime = 0;

	public float sleepVelocity = 0.4F;
	public bool isWander = false;
	protected GameObject curTarget;
	//The rest of the variables are related to Pathfinding

	/** Determines how often it will search for new paths. 
	 * If you have fast moving targets or AIs, you might want to set it to a lower value.
	 * The value is in seconds between path requests.
	 */
	public float repathRate = 0.5F;
	/** Rotation speed.
	 * Rotation is calculated using Quaternion.SLerp. This variable represents the damping, the higher, the faster it will be able to rotate.
	 */
	public float turningSpeed = 5;
	/** Distance from the target point where the AI will start to slow down.
	 * Note that this doesn't only affect the end point of the path
 	 * but also any intermediate points, so be sure to set #forwardLook and #pickNextWaypointDist to a higher value than this
 	 */
	public float slowdownDistance = 0.6F;
	/** Determines within what range it will switch to target the next waypoint in the path */
	public float pickNextWaypointDist = 2;
	/** Target point is Interpolated on the current segment in the path so that it has a distance of #forwardLook from the AI.
	  * See the detailed description of AIPath for an illustrative image */
	public float forwardLook = 1;
	/** Distance to the end point to consider the end of path to be reached.
	 * When this has been reached, the AI will not move anymore until the target changes and OnTargetReached will be called.
	 */
	public float endReachedDistance = 0.2F;
	public bool closestOnPathCheck = true;
	protected float minMoveScale = 0.05F;
	/** Cached Seeker component */
	protected Seeker seeker;
	/** Cached Transform component */
	protected Transform tr;
	/** Time when the last path request was sent */
	private float lastRepath = -9999;
	/** Current path which is followed */
	protected Path path;
	/** Cached CharacterController component */
	//protected CharacterController controller;
	/** Cached NavmeshController component */
	protected NavmeshController navController;
	protected RVOController rvoController;
	/** Cached Rigidbody component */
	protected Rigidbody rigid;
	/** Current index in the path which is current target */
	protected int currentWaypointIndex = 0;
	/** Holds if the end-of-path is reached
	 * \see TargetReached */
	protected bool targetReached = false;
	/** Only when the previous path has been returned should be search for a new path */
	protected bool canSearchAgain = true;
	protected Vector3 lastFoundWaypointPosition;
	protected float lastFoundWaypointTime = -9999;

	private float timer;
	private float timerStart;

	//The Start() and Update() functions will probably be overriden
	//However, base.Start() and base.Update() should be called
	//The contents must be relevant to all the enemy types

	protected virtual void Start () {
		difficulty = GameManager.getDifficulty ();

		movementSpeed = movementSpeed * Mathf.Pow(difficulty, 0.5f);
		visionScale = 5*difficulty + 10;

		//target = GameObject.FindGameObjectsWithTag ("Player") [0].transform;
		startHasRun = true;
		equippedWeapon = WeaponManager.getWeapon (equipped);
		bullet = WeaponManager.getEnemyBulletPrefab (equipped);

		if (enemyType != ENEMY_BOSS) {
			//instantiate the guns we need
			Transform gunprefab = (Transform)Instantiate (WeaponManager.getWeaponPrefab (equipped), hands.position, transform.rotation);
			gunprefab.GetChild (0).renderer.enabled = true;
			gunprefab.parent = this.transform;
		}

		passedInfo = new PassedInfo ();
		passedInfo.bossFound = false;
		passedInfo.bossPos = new Vector3 ();
		passedInfo.playerPos = new Vector3 ();
		passedInfo.lastTimeSeen = -999;

		timer = -1f;
		//OnEnable ();
	}

	protected virtual void Update () {
		if (Time.timeScale <= 0)
			return;

		//if armor broken, cancel effect.
		if (armorBonusHP <= 0) {
			armorBonusHP = 0;
		}
		if (timeLeft != null && timeLeft >0){
			timeLeft--;
		}
		if(timeLeft <= 0 && isWander == true){
			isWander = false;
			stopMove();
			timeLeft = 0;
		}

		GameObject target = null;

		commCheck();


		deathCheck ();
		timeSinceStateChange += Time.deltaTime;

		if (isWander) {
			RaycastHit hit;
			Collider other;
			Physics.Raycast (transform.position, transform.forward, out hit, 2f);
			Debug.DrawRay(transform.position, transform.forward, Color.blue, 5);
			other = hit.collider;
			if (other != null) {
				if (other.gameObject != null) {
					if (other.gameObject.tag == "Wall" || other.gameObject.tag == "Enemy" || other.gameObject.tag == "Boss") {
						float angle = Random.Range(-150f, 150f);
						transform.rotation = Quaternion.AngleAxis(angle,Vector3.up);
						rigidbody.velocity = transform.forward*wanderSpeed;
					}
				}
			}
		}
	}

	public GameObject visionCheck(){
		GameObject toReturn = null;
		GameObject closestPack = null;
		pUs = GameObject.FindGameObjectsWithTag ("MedPackPU");
		player = GameObject.FindGameObjectsWithTag ("Player") [0];
		if (pUs.Length != 0) {
			closestPack = pUs [0];
		} //TODO: make a proper function to find the closest pack
		Vector3 forward = transform.forward;
		
		int i;
		if (closestPack != null) {
			for (i = 0; i < pUs.Length; i++) {
				Transform powerUpPos = pUs [i].transform;
				GameObject powerUp = pUs [i];
				targetDir = powerUpPos.position - transform.position;
				angle = Vector3.Angle (targetDir, forward); 
			
				if ((powerUpPos.position - transform.position).magnitude < (visionScale) && angle <= fov) {
					if ((powerUpPos.position - transform.position).magnitude < (closestPack.transform.position - transform.position).magnitude) {
						closestPack = powerUp;
					}
				}
			}
			if ((closestPack.transform.position - transform.position).magnitude < (visionScale) && 
				Vector3.Angle (closestPack.transform.position - transform.position, forward) <= fov) {
				toReturn = closestPack;
				toUse = 1;
			}
		}
		Transform playerLocPos = player.transform;
		targetDir = playerLocPos.position - transform.position;
		angle = Vector3.Angle (targetDir, forward);
		
		if ((playerLocPos.position - transform.position).magnitude < (visionScale) && angle <= fov) {
			toReturn = player;
			this.passedInfo.playerPos = playerLocPos.position;
			this.passedInfo.lastTimeSeen = Time.timeSinceLevelLoad;
			toUse = 2;
		}
		
		if (toReturn != null) {
			RaycastHit hit;
			Collider other;
			//Debug.Log (transform.position.y.ToString());
			Physics.Raycast (transform.position, (toReturn.transform.position - transform.position), out hit, visionScale);
			other = hit.collider;
			if(other != null && other.gameObject != null){
				if (other.gameObject.tag == "Wall" || other.gameObject.tag == "Enemy") {
					Debug.DrawRay(transform.position, (toReturn.transform.position - transform.position), Color.red, 5);
					return null;
				}	
				else
					Debug.DrawRay(transform.position, (toReturn.transform.position - transform.position), Color.yellow, 5);
			}
		}
		
		return toReturn;
	}
	
	public bool commCheck(){
		GameObject[] eUs = GameObject.FindGameObjectsWithTag ("Enemy");
		
		int i;
		for(i = 0; i < eUs.Length; i++){
			Transform npcPos = eUs[i].transform;
			GameObject npc = eUs[i];
			EnemyBasic npcScript = npc.GetComponent<EnemyBasic>();
			if(npcScript == null)
			
			//TODO: remove npsScript!=null check. Make sure it is not null.
			if (npcScript != null && ((Mathf.Pow(difficulty, 0.5f)) * (npcPos.position - transform.position).magnitude) < (commScale)) {
				//pass information

				if(this.passedInfo.bossFound){
					npcScript.passedInfo.bossPos = this.passedInfo.bossPos;
				}
				if(npcScript.passedInfo.lastTimeSeen < this.passedInfo.lastTimeSeen){
					npcScript.passedInfo.lastTimeSeen = this.passedInfo.lastTimeSeen;
					npcScript.passedInfo.playerPos = this.passedInfo.playerPos;
					return true;
				}
			}
		}
		return false;
	}

	public void deathCheck(){
		player = GameObject.FindGameObjectsWithTag ("Player") [0];
		PlayerController playerScript = player.GetComponent<PlayerController>();
		float scale = .2f - (Mathf.Floor ((int)difficulty * 100) / 1000);

		if (enemyHP <= 0) {
			if(medPack > 0){
				medPack = 0;
				dropItem();
			}
			switch(enemyType){
				case ENEMY_HEALER:
					playerScript.maxHP += scale;
					playerScript.playerHP += scale;
					GameManager.displayHealthBonus(scale);
				break;
				case ENEMY_SOLDIER:
					playerScript.bonusDamage += scale;
					
					GameManager.displayDamageBonus(scale);
				break;
				case ENEMY_DEFENDER:
					playerScript.maxArmor += scale;
					playerScript.armorBonusHP += scale;
					playerScript.armorOn = true;
					GameManager.displayArmorBonus(scale);
				break;
			}

		Destroy(this.gameObject);}
	}

	public void increaseHP(float hp){
		enemyHP += hp;
		if (enemyHP > maxHP)
			enemyHP = maxHP;
		//Change the color depending on hp
		if (this.enemyType != 3){
			float hpRatio = enemyHP / maxHP;
			this.transform.GetChild (0).gameObject.renderer.material.SetColor ("_Color", Color.Lerp(Color.black, Color.white, hpRatio));
		}	
	}

	public void takeDamage(float damage) {
		float armorDamage = Mathf.Min (damage, armorBonusHP);
		float hpDamage = damage - armorDamage;
		armorBonusHP -= armorDamage;
		enemyHP -= hpDamage;

		//Change the color depending on hp
		if (this.enemyType != 3){
			float hpRatio = enemyHP / maxHP;
			this.transform.GetChild (0).gameObject.renderer.material.SetColor ("_Color", Color.Lerp(Color.black, Color.white, hpRatio));
		}	
	}
	
	public void activateArmor(){
		armorBonusHP = ARMOR_AMOUNT;
	}
	
	public void pickUp(){
		medPack++;
	}
	
	public void dropItem(){
		GameObject medPackClone = (GameObject) Instantiate(MedPack, transform.position, transform.rotation);
	}

	public void activateHealthPack(){
		enemyHP = Mathf.Min (maxHP, enemyHP + MEDPACK_HEALTH);
		//Change the color depending on hp
		if (this.enemyType != 3){
			float hpRatio = enemyHP / maxHP;
			this.transform.GetChild (0).gameObject.renderer.material.SetColor ("_Color", Color.Lerp(Color.black, Color.white, hpRatio));
		}	
	}
	
	public void useMedPack(GameObject enemy){
		EnemyBasic enemyScript = enemy.GetComponent<EnemyBasic>();
		enemyScript.activateHealthPack ();
		medPack--;
	}

	//Look at the object
	//TODO: add slight error
	protected void lookAt(GameObject target) {
		this.transform.LookAt (target.transform, Vector3.up);
	}

	protected void lookAt(Vector3 target){
		this.transform.LookAt (target, Vector3.up);
	}

	//Transfers bullet stats to bullets
	protected void FireBullet () {
		//var inFront = new Vector3 (0, 1, 0);
		if (bulletStartPosition == null) {
			Debug.LogError("Bullet start position is not specified.");
		}
		Rigidbody bulletClone = (Rigidbody) Instantiate(bullet, bulletStartPosition.position, transform.rotation);
		bulletClone.velocity = transform.forward * equippedWeapon.bulletSpeed * 100;
		
		EnemyBulletBehaviors bulletScript = bulletClone.GetComponent<EnemyBulletBehaviors> ();
		bulletScript.lifeSpan = equippedWeapon.bulletLength;
		bulletScript.damage = equippedWeapon.bulletDamage;
		lastBulletTime = Time.timeSinceLevelLoad;
	}
	
	protected void StartFiring () {
		if (isFiring)
		return;
		else {
			isFiring = true;
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

	protected string getTag(GameObject get){
		return get.gameObject.tag;
	}

	protected void changeState(int newState) {
		if (state != newState) {
			state = newState;
			timeSinceStateChange = 0;
		}
	}

	protected void move(){
		Vector3 dir = Vector3.zero;
		if (curTarget != null) {
						dir = curTarget.transform.position - transform.position;
				}
		if (dir != Vector3.zero) {
						rigidbody.velocity = dir * speed;
				}

		}

	protected void moveTo(Vector3 tar){
		Vector3 dir = Vector3.zero;
		if (tar != null) {
			dir = tar - transform.position;
		}
		if (dir != Vector3.zero) {
			rigidbody.velocity = dir * speed;
		}
		
	}

	protected void wander(){
		float rand = Random.Range(-180,180);
		RaycastHit hit;
		Collider other;

		var rotation = Quaternion.AngleAxis(rand,Vector3.up);
		timeLeft = Random.Range(wanderTimeMin, wanderTimeMax);
		isWander = true;
		transform.rotation = rotation;
		rigidbody.velocity = transform.forward * wanderSpeed;
	}

	protected void stopMove(){
		isWander = false;
		rigidbody.velocity = Vector3.zero;
	}

	//The rest of the functions are related to Pathfinding

	/** Returns if the end-of-path has been reached
	 * \see targetReached */
	public bool TargetReached {
		get {
			return targetReached;
		}
	}

	private bool startHasRun = false;
	
	/** Initializes reference variables.
	 * If you override this function you should in most cases call base.Awake () at the start of it.
	  * */
	protected virtual void Awake () {
		seeker = GetComponent<Seeker>();
		
		//This is a simple optimization, cache the transform component lookup
		tr = transform;
		
		//Cache some other components (not all are necessarily there)

		navController = GetComponent<NavmeshController>();
		rvoController = GetComponent<RVOController>();
		if ( rvoController != null ) rvoController.enableRotation = false;
		rigid = rigidbody;
	}
	
	/** Run at start and when reenabled.
	 * Starts RepeatTrySearchPath.
	 * 
	 * \see Start
	 */
	protected virtual void OnEnable () {
		
		lastRepath = -9999;
		canSearchAgain = true;

		lastFoundWaypointPosition = GetFeetPosition ();

		if (startHasRun) {
			//Make sure we receive callbacks when paths complete
			seeker.pathCallback += OnPathComplete;
			
			StartCoroutine (RepeatTrySearchPath ());
		}
	}
	
	public void OnDisable () {
		// Abort calculation of path
		if (seeker != null && !seeker.IsDone()) seeker.GetCurrentPath().Error();
		
		// Release current path
		if (path != null) path.Release (this);
		reenable = true;
		path = null;
		//Debug.Log ("disable pathfinding");
		
		//Make sure we receive callbacks when paths complete
		seeker.pathCallback -= OnPathComplete;
	}
	
	/** Tries to search for a path every #repathRate seconds.
	  * \see TrySearchPath
	  */
	protected IEnumerator RepeatTrySearchPath () {
		while (true) {
			float v = TrySearchPath ();
			yield return new WaitForSeconds (v);
		}
	}
	
	/** Tries to search for a path.
	 * Will search for a new path if there was a sufficient time since the last repath and both
	 * #canSearchAgain and #canSearch are true and there is a target.
	 * 
	 * \returns The time to wait until calling this function again (based on #repathRate) 
	 */
	public float TrySearchPath () {
		if (Time.time - lastRepath >= repathRate && canSearchAgain && canSearch && target != null) {
			SearchPath ();
			return repathRate;
		} else {
			//StartCoroutine (WaitForRepath ());
			float v = repathRate - (Time.time-lastRepath);
			return v < 0 ? 0 : v;
		}
	}
	
	/** Requests a path to the target */
	public virtual void SearchPath () {

		lastRepath = Time.time;
		//This is where we should search to
		Vector3 targetPosition = target;
		
		canSearchAgain = false;

		seeker.StartPath (GetFeetPosition(), targetPosition);
	}
	/**
	 * tries to find the path from the current position to target position
	 * If the enemy is too close to player, enemy will stop at his current position and shoot bullet
	 * @parm pos the target position
	 */
	public void chase(Vector3 pos){

		stopMove ();
		setTarget (pos);


		if (EnemyState == 0) {
			//OnEnable();
			EnemyState = 1;
			canMove = true;
		}

		if (reenable&&EnemyState == 1) {
			reenable = false;
			OnEnable();
			canMove = true;
		}

		//state == 2 combat
		if (state == 2) {
						if ((transform.position - target).magnitude < 4f) {
								if (EnemyState == 1) {//keep path finding state
										OnDisable ();
										stopMove ();
										canMove = false;
										EnemyState = 2;//halt state
										Debug.Log("should hault");
								}
						} else {
								if (EnemyState == 2) {
										canMove = true;
										EnemyState = 1;
										//OnEnable ();
										Debug.Log("re-chase player");
			
								}
						}
				}
		//Get velocity in world-space
		Vector3 velocity;

		if (canMove) {
			
			//Calculate desired velocity
			Vector3 dir = CalculateVelocity (GetFeetPosition());
			
			//Rotate towards targetDirection (filled in by CalculateVelocity)
			RotateTowards (targetDirection);

			dir.y = 0;
			if (dir.sqrMagnitude > sleepVelocity*sleepVelocity) {
				//If the velocity is large enough, move
			} else {
				//Otherwise, just stand still (this ensures gravity is applied)
				dir = Vector3.zero;
			}
			
			if ( this.rvoController != null ) {
				rvoController.Move (dir);
				velocity = rvoController.velocity;
			} 
			else 
				if (navController != null) {
					velocity = Vector3.zero;
				}
				else if (rigidbody != null) {
				//here is how to make enemy move, we can use force or velocity, I prefer use velocity
				rigidbody.velocity = dir; 
				velocity = rigidbody.velocity;
				} else {
				Debug.LogWarning ("No NavmeshController or CharacterController attached to GameObject");
				velocity = Vector3.zero;
				}

		}
		else {
			if ((transform.position - target).magnitude > 10f){
				//OnEnable();
				canMove = true;
				Debug.Log("this part is running?");
			}
			velocity = Vector3.zero;
		}
		
		
		//Animation
		
		//Calculate the velocity relative to this transform's orientation
		Vector3 relVelocity = tr.InverseTransformDirection (velocity);
		relVelocity.y = 0;
		
		float speed = relVelocity.z;

		if(targetReached&&(state != 2)){					
			state = 0; //STATE_IDLE
			OnDisable ();
			stopMove ();
			reenable = true;
		}

	}

	//go toward a specified object
	public virtual void goTo (Vector3 targetPos) {
		
		lastRepath = Time.time;
		//This is where we should search to
		
		canSearchAgain = false;

		//We should search from the current position
		seeker.StartPath (GetFeetPosition(), targetPos);

	}

	public void setTarget(Vector3 tar){
		this.target = tar;	
	}
	
	public virtual void OnTargetReached () {
	}
	
	/** Called when a requested path has finished calculation.
	  * A path is first requested by #SearchPath, it is then calculated, probably in the same or the next frame.
	  * Finally it is returned to the seeker which forwards it to this function.\n
	  */
	public virtual void OnPathComplete (Path _p) {
		ABPath p = _p as ABPath;
		if (p == null) throw new System.Exception ("This function only handles ABPaths, do not use special path types");
		
		canSearchAgain = true;
		
		//Claim the new path
		p.Claim (this);
		
		// Path coßuldn't be calculated of some reason.
		// More info in p.errorLog (debug string)
		if (p.error) {
			p.Release (this);
			return;
		}
		
		//Release the previous path
		if (path != null) path.Release (this);
		
		//Replace the old path
		path = p;
		
		//Reset some variables
		currentWaypointIndex = 0;
		targetReached = false;

		if (closestOnPathCheck) {
			Vector3 p1 = Time.time - lastFoundWaypointTime < 0.3f ? lastFoundWaypointPosition : p.originalStartPoint;
			Vector3 p2 = GetFeetPosition ();
			Vector3 dir = p2-p1;
			float magn = dir.magnitude;
			dir /= magn;
			int steps = (int)(magn/pickNextWaypointDist);

#if ASTARDEBUG
			Debug.DrawLine (p1,p2,Color.red,1);
#endif

			for (int i=0;i<=steps;i++) {
				CalculateVelocity (p1);
				p1 += dir;
			}

		}
	}
	
	public virtual Vector3 GetFeetPosition () {
		if (rvoController != null) {
			return tr.position - Vector3.up*rvoController.height*0.5f;
		}

		return tr.position;
	}
	
	/** Point to where the AI is heading.
	  * Filled in by #CalculateVelocity */
	protected Vector3 targetPoint;
	/** Relative direction to where the AI is heading.
	 * Filled in by #CalculateVelocity */
	protected Vector3 targetDirection;
	
	protected float XZSqrMagnitude (Vector3 a, Vector3 b) {
		float dx = b.x-a.x;
		float dz = b.z-a.z;
		return dx*dx + dz*dz;
	}
	
	/** Calculates desired velocity.
	 * Finds the target path segment and returns the forward direction, scaled with speed.
	 * A whole bunch of restrictions on the velocity is applied to make sure it doesn't overshoot, does not look too far ahead,
	 * and slows down when close to the target.
	 */
	protected Vector3 CalculateVelocity (Vector3 currentPosition) {
		if (path == null || path.vectorPath == null || path.vectorPath.Count == 0) return Vector3.zero; 
		
		List<Vector3> vPath = path.vectorPath;
		//Vector3 currentPosition = GetFeetPosition();
		
		if (vPath.Count == 1) {
			vPath.Insert (0,currentPosition);
		}
		
		if (currentWaypointIndex >= vPath.Count) { currentWaypointIndex = vPath.Count-1; }
		
		if (currentWaypointIndex <= 1) currentWaypointIndex = 1;
		
		while (true) {
			if (currentWaypointIndex < vPath.Count-1) {
				//There is a "next path segment"
				float dist = XZSqrMagnitude (vPath[currentWaypointIndex], currentPosition);
				if (dist < pickNextWaypointDist*pickNextWaypointDist) {
					lastFoundWaypointPosition = currentPosition;
					lastFoundWaypointTime = Time.time;
					currentWaypointIndex++;
				} else {
					break;
				}
			} else {
				break;
			}
		}
		
		Vector3 dir = vPath[currentWaypointIndex] - vPath[currentWaypointIndex-1];
		Vector3 targetPosition = CalculateTargetPoint (currentPosition,vPath[currentWaypointIndex-1] , vPath[currentWaypointIndex]);
		
		
		dir = targetPosition-currentPosition;
		dir.y = 0;
		float targetDist = dir.magnitude;
		
		float slowdown = Mathf.Clamp01 (targetDist / slowdownDistance);
		
		this.targetDirection = dir;
		this.targetPoint = targetPosition;
		
		if (currentWaypointIndex == vPath.Count-1 && targetDist <= endReachedDistance) {
			if (!targetReached) { targetReached = true; OnTargetReached (); }
			
			//Send a move request, this ensures gravity is applied
			return Vector3.zero;
		}
		
		Vector3 forward = tr.forward;
		float dot = Vector3.Dot (dir.normalized,forward);
		float sp = speed * Mathf.Max (dot,minMoveScale) * slowdown;
		
		if (Time.deltaTime	> 0) {
			sp = Mathf.Clamp (sp,0,targetDist/(Time.deltaTime*2));
		}
		return forward*sp;
	}
	
	/** Rotates in the specified direction.
	 * Rotates around the Y-axis.
	 * \see turningSpeed
	 */
	protected virtual void RotateTowards (Vector3 dir) {
		
		if (dir == Vector3.zero) return;
		
		Quaternion rot = tr.rotation;
		Quaternion toTarget = Quaternion.LookRotation (dir);
		
		rot = Quaternion.Slerp (rot,toTarget,turningSpeed*Time.deltaTime);
		Vector3 euler = rot.eulerAngles;
		euler.z = 0;
		euler.x = 0;
		rot = Quaternion.Euler (euler);
		
		tr.rotation = rot;
	}
	
	/** Calculates target point from the current line segment.
	 * \param p Current position
	 * \param a Line segment start
	 * \param b Line segment end
	 * The returned point will lie somewhere on the line segment.
	 * \see #forwardLook
	 * \todo This function uses .magnitude quite a lot, can it be optimized?
	 */
	protected Vector3 CalculateTargetPoint (Vector3 p, Vector3 a, Vector3 b) {
		a.y = p.y;
		b.y = p.y;
		
		float magn = (a-b).magnitude;
		if (magn == 0) return a;
		
		float closest = AstarMath.Clamp01 (AstarMath.NearestPointFactor (a, b, p));
		Vector3 point = (b-a)*closest + a;
		float distance = (point-p).magnitude;
		
		float lookAhead = Mathf.Clamp (forwardLook - distance, 0.0F, forwardLook);
		
		float offset = lookAhead / magn;
		offset = Mathf.Clamp (offset+closest,0.0F,1.0F);
		return (b-a)*offset + a;
	}
}