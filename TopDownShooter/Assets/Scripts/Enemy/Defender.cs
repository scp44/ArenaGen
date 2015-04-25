using UnityEngine;
using System.Collections;
using Pathfinding.RVO;

public class Defender : EnemyBasic {
		
	/** Effect which will be instantiated when end of path is reached.
	 * \see OnTargetReached */
	public GameObject endOfPathEffect;
	/** Point for the last spawn of #endOfPathEffect */
	protected Vector3 lastTarget;

	private const int STATE_IDLE = 0;
	private const int STATE_ALERT = 1;
	private const int STATE_COMBAT = 2;
	private const int STATE_FLEE = 3;
	private const int STATE_IS_FLEEING = 4;
	private bool ccd = false;
	private bool wad = false;
	private Vector3 commTarget;

	private Vector3 newTarget;

	private GameObject p1;


	
	protected override void Start () {
		state = STATE_IDLE;
		base.Start ();
	}
	
	protected override void Update() {
		base.Update ();	
		GameObject target = this.visionCheck ();//wonder if it should return a boolean 
		int counter = 0;
		if (commCheck() ==true){
			ccd = commCheck();
			commTarget = this.passedInfo.playerPos;
			Debug.Log("Meow");
		}



		if (target != null||ccd) {

			//Debug.Log(target.gameObject.tag.ToString());
			if (target.gameObject.tag != null && target.gameObject.tag=="Player"){
				//stopMove();
				lookAt (target);
				StartFiring ();

				newTarget = new Vector3(target.transform.position.x, 0.5f, target.transform.position.z);
				p1 = target.gameObject;
		
				state = STATE_COMBAT;
					
				chase (newTarget);
				//break;
			}else if(ccd){
			
				float x = commTarget.x; //use to make enemy silly
				float z = commTarget.z;
				
				newTarget = new Vector3(x,0.5f,z);
				chase (newTarget);
				StartFiring();
				
				if (player != null)
					this.passedInfo.playerPos = player.transform.position;
				this.passedInfo.lastTimeSeen = Time.timeSinceLevelLoad;

				//break;
			}
	
			if (target.gameObject.tag == "MedPackPU"){

				if (state != STATE_ALERT){

					if(enemyHP!=5){
						//Debug.Log("should go to medpack");
						lookAt(target);
						newTarget = new Vector3(target.transform.position.x, 0.5f, target.transform.position.z);
						chase(newTarget);
					}
					else{
						Debug.Log("find medpack, but hp is full,wander");
					}
				}else{


					if(targetReached){					
						state = STATE_IDLE;
						OnDisable ();
						stopMove ();
					}else{
	
						chase(newTarget);
					}
				}
			}

		} else {



			if (state == STATE_COMBAT){
				state = STATE_ALERT;
				//counter = 0;
				
			}
			if(state == STATE_ALERT){
				//counter = counter + 1;
				StopFiring();
				//	chase(newTarget);
					if(targetReached){

						state = STATE_IDLE;
						OnDisable ();
					}else{
						//Debug.Log("still chasing");

						chase(newTarget);
					}
			}

		}

		if (state == STATE_IDLE && timeLeft<= 0){
			wad = true;
			Debug.Log("wander (no target asd;fl)");
			//stopMove();
			wander();
			
		}
		
	}
	
	/**
	 * Called when the end of path has been reached.
	 * An effect (#endOfPathEffect) is spawned when this function is called
	 * However, since paths are recalculated quite often, we only spawn the effect
	 * when the current position is some distance away from the previous spawn-point
	*/
	public override void OnTargetReached () {
		
		if (endOfPathEffect != null && Vector3.Distance (tr.position, lastTarget) > 1) {
			GameObject.Instantiate (endOfPathEffect,tr.position,tr.rotation);
			lastTarget = tr.position;
		}
	}
	
	public override Vector3 GetFeetPosition ()
	{
		return tr.position;
	}
}
