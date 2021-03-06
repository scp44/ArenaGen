//#define ASTARDEBUG
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using Pathfinding.RVO;


[RequireComponent(typeof(Seeker))]
[AddComponentMenu("Pathfinding/AI/AIPath (generic)")]
public class AI: MonoBehaviour {
	
	/** Determines how often it will search for new paths. 
	 * If you have fast moving targets or AIs, you might want to set it to a lower value.
	 * The value is in seconds between path requests.
	 */
	public float repathRate = 0.5F;

	private Transform target;

	private int enemyID; // 0 healer, 1 solder, 2 Defender, 3 boss

	public bool canSearch = true;

	public bool canMove = true;



	public float speed = 3;
	
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
	
	/** Do a closest point on path check when receiving path callback.
	 * Usually the AI has moved a bit between requesting the path, and getting it back, and there is usually a small gap between the AI
	 * and the closest node.
	 * If this option is enabled, it will simulate, when the path callback is received, movement between the closest node and the current
	 * AI position. This helps to reduce the moments when the AI just get a new path back, and thinks it ought to move backwards to the start of the new path
	 * even though it really should just proceed forward.
	 */
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
	protected CharacterController controller;
	
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

	public float enemyHP = 5;
	public float maxHP = 5;
	public float equipped = 0;
	public float movementSpeed = 100;
	
	public float commScale = 4;
	public float visionScale = 10;
	public float alertScale = 15; 
	public float fov = 100;
	
	//0 = HEALER, 1 = SOLDIER, 2 = DEFENDER, 3 = BOSS
	public float enemyType = 0;
	
	
	public Vector3 bossPos;
	public bool bossFound = false;
	public Transform playerPos;
	public double lastTimeSeen;
	
	//power up stuff
	public int armorCount;
	public bool armorOn = false;
	public int medPack = 0;
	public Rigidbody MedPack;
	
	public Rigidbody Bullet;
	// Use this for initialization

	/** Starts searching for paths.
	 * If you override this function you should in most cases call base.Start () at the start of it.
	 * \see OnEnable
	 * \see RepeatTrySearchPath
	 */
	protected virtual void Start () {
		target = GameObject.FindGameObjectsWithTag ("Player") [0].transform;
		startHasRun = true;
		OnEnable ();
	}
	
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
		//var inFront = new Vector3 (0, 1, 0);
		
		Rigidbody bulletClone = (Rigidbody) Instantiate(Bullet, transform.position, transform.rotation);
		bulletClone.velocity = transform.forward * movementSpeed * BulletSpeed();
		
		
		if (equipped == 0) {
			EnemyBulletBehaviors bulletScript = bulletClone.GetComponent<EnemyBulletBehaviors> ();
			bulletScript.lifeSpan = BulletLength ();
			bulletScript.damage = BulletDamage ();
		} else if (equipped == 1) {
			EnemyBullet1Behaviors bulletScript = bulletClone.GetComponent<EnemyBullet1Behaviors> ();
			bulletScript.lifeSpan = BulletLength ();
			bulletScript.damage = BulletDamage ();
		} else if (equipped == 2) {
			EnemyBullet2Behaviors bulletScript = bulletClone.GetComponent<EnemyBullet2Behaviors> ();
			bulletScript.lifeSpan = BulletLength ();
			bulletScript.damage = BulletDamage ();
		}else if (equipped == 3) {
			EnemyBullet3Behaviors bulletScript = bulletClone.GetComponent<EnemyBullet3Behaviors> ();
			bulletScript.lifeSpan = BulletLength ();
			bulletScript.damage = BulletDamage ();
		}else if (equipped == 4) {
			EnemyBullet4Behaviors bulletScript = bulletClone.GetComponent<EnemyBullet4Behaviors> ();
			bulletScript.lifeSpan = BulletLength ();
			bulletScript.damage = BulletDamage ();
		} else {
			EnemyBulletBehaviors bulletScript = bulletClone.GetComponent<EnemyBulletBehaviors> ();
			bulletScript.lifeSpan = BulletLength ();
			bulletScript.damage = BulletDamage ();
		}
		
		
		
		//bulletClone.GetComponent<MyRocketScript>().DoSomething();
	}
	
	public GameObject visionCheck(){
		GameObject[] pUs = GameObject.FindGameObjectsWithTag ("MedPackPU");
		GameObject player = GameObject.FindGameObjectsWithTag ("Player") [0];
		GameObject closestPack = pUs [0];
		GameObject toReturn = null;
		
		Vector3 forward = transform.forward;
		float angle;
		Vector3 targetDir;
		int toUse = 0;
		
		
		int i;
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
		
		
		
		Transform playerLocPos = player.transform;
		targetDir = playerLocPos.position - transform.position;
		angle = Vector3.Angle (targetDir, forward);
		
		if ((playerLocPos.position - transform.position).magnitude < (visionScale) && angle <= fov) {
			this.playerPos = playerLocPos;
			//this.lastTimeSeen = this.LocationInfo.timestamp;
			toReturn = player;
			toUse = 2;
		}
		RaycastHit hit;
		Collider other;
		Physics.Raycast (transform.position, (toReturn.transform.position-transform.position),out hit, visionScale );
		other = hit.collider; 
		if (other.gameObject.tag == "Wall") {
			return null;
		}
		
		
		return toReturn;
	}
	
	public void commCheck(){
		GameObject[] eUs = GameObject.FindGameObjectsWithTag ("Enemy");
		
		int i;
		for(i = 0; i < eUs.Length; i++){
			Transform npcPos = eUs[i].transform;
			GameObject npc = eUs[i];
			AI npcScript = npc.GetComponent<AI>();
			if ((npcPos.position - transform.position).magnitude < (commScale)) {
				if(bossFound){
					//pass information
					npcScript.bossPos = this.bossPos;
				}
				if(npcScript.lastTimeSeen < this.lastTimeSeen){
					npcScript.lastTimeSeen = this.lastTimeSeen;
					npcScript.playerPos = this.playerPos;
				}
			}}
		
	}
	
	public void increaseHP(int HP){
		enemyHP += 5;
	}
	
	public void activateArmor(){
		armorOn = true;
		armorCount = 5;
	}
	
	public void pickUp(){
		medPack++;
	}
	
	public void dropItem(){
		Rigidbody medPackClone = (Rigidbody) Instantiate(MedPack, transform.position, transform.rotation);
	}
	
	public void useMedPack(GameObject enemy){
		AI enemyScript = enemy.GetComponent<AI>();
		enemyScript.increaseHP (5);
		medPack--;
	}

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
		controller = GetComponent<CharacterController>();
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
		path = null;
		
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
		target = GameObject.FindGameObjectsWithTag ("Player") [0].transform;

		if (target == null) throw new System.InvalidOperationException ("Target is null");

		lastRepath = Time.time;
		//This is where we should search to
		Vector3 targetPosition = target.position;
		
		canSearchAgain = false;
		
		//Alternative way of requesting the path
		//ABPath p = ABPath.Construct (GetFeetPosition(),targetPoint,null);
		//seeker.StartPath (p);
		
		//We should search from the current position
		seeker.StartPath (GetFeetPosition(), targetPosition);
	}

	//go toward a specified object
	public virtual void goTo (GameObject obj) {
		target = obj.transform;

		if (target == null) throw new System.InvalidOperationException ("Target is null");
		
		lastRepath = Time.time;
		//This is where we should search to
		Vector3 targetPosition = target.position;
		
		canSearchAgain = false;
		
		//Alternative way of requesting the path
		//ABPath p = ABPath.Construct (GetFeetPosition(),targetPoint,null);
		//seeker.StartPath (p);
		
		//We should search from the current position
		seeker.StartPath (GetFeetPosition(), targetPosition);
	}

	public void setTarget(Transform tar){
		target = tar;	
	}
	
	public virtual void OnTargetReached () {
		//End of path has been reached
		//If you want custom logic for when the AI has reached it's destination
		//add it here
		//You can also create a new script which inherits from this one
		//and override the function in that script
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
		
		// Path couldn't be calculated of some reason.
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
		
		//The next row can be used to find out if the path could be found or not
		//If it couldn't (error == true), then a message has probably been logged to the console
		//however it can also be got using p.errorLog
		//if (p.error)
		
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
		} else
		if (controller != null) {
			return tr.position - Vector3.up*controller.height*0.5F;
		}

		return tr.position;
	}
	
	public virtual void Update () {
		//if armor broken, cancel effect.
		if (armorOn && armorCount <= 0) {
			armorOn = false;
			armorCount = 0;
		}
		
		GameObject target = null;
		if(!armorOn /*armor in line of sight*/){
			//interrupt module
			//target = armor;
		}
		
		if(enemyHP < 5 /*&& medpack in line of sight*/){
			//interrupt module
			//if(medPack is closer than current target)
			//target = medPack;
		}
		
		if (target != null)
			goTo (target);
		
		if(medPack > 0 /*&& ((enemy in com-check circle && has < some HP) || enemy is Boss)*/){
			//interrupt module
			//useMedPack(enemy);
		}
		
		if (enemyHP <= 0) {
			if(medPack > 0)
				dropItem();
			Destroy (this.gameObject);
		}

		if (!canMove) { return; }
		
		Vector3 dir = CalculateVelocity (GetFeetPosition());
		
		//Rotate towards targetDirection (filled in by CalculateVelocity)
		RotateTowards (targetDirection);
	
		if (rvoController != null) {
			rvoController.Move (dir);
		} else
		if (navController != null) {
#if FALSE
			navController.SimpleMove (GetFeetPosition(),dir);
#endif
		} else if (controller != null) {
			controller.SimpleMove (dir);
		} else if (rigid != null) {
			rigid.AddForce (dir);
		} else {
			transform.Translate (dir*Time.deltaTime, Space.World);
		}
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
	 * /see speed
	 * /see endReachedDistance
	 * /see slowdownDistance
	 * /see CalculateTargetPoint
	 * /see targetPoint
	 * /see targetDirection
	 * /see currentWaypointIndex
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
					//Mathfx.DistancePointSegmentStrict (vPath[currentWaypointIndex+1],vPath[currentWaypointIndex+2],currentPosition);
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
			//vPath[currentWaypointIndex] + Vector3.ClampMagnitude (dir,forwardLook);
		
		
		
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
		
#if ASTARDEBUG
		Debug.DrawLine (vPath[currentWaypointIndex-1] , vPath[currentWaypointIndex],Color.black);
		Debug.DrawLine (GetFeetPosition(),targetPosition,Color.red);
		Debug.DrawRay (targetPosition,Vector3.up, Color.red);
		Debug.DrawRay (GetFeetPosition(),dir,Color.yellow);
		Debug.DrawRay (GetFeetPosition(),forward*sp,Color.cyan);
#endif
		
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