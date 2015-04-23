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
			stopMove();
			//Debug.Log(target.gameObject.tag.ToString());
			if (target.gameObject.tag != null && target.gameObject.tag=="Player"){
				lookAt (target);
				StartFiring ();

				newTarget = new Vector3(target.transform.position.x, 0.5f, target.transform.position.z);
				p1 = target.gameObject;
		
				state = STATE_COMBAT;
					
				chase (newTarget);
			}else if(ccd){
				float x = commTarget.x; //use to make enemy silly
				float z = commTarget.z;
				
				newTarget = new Vector3(x,0.5f,z);
				chase (newTarget);
				StartFiring();
				
				if (player != null)
					this.passedInfo.playerPos = player.transform.position;
				this.passedInfo.lastTimeSeen = Time.timeSinceLevelLoad;
			}
<<<<<<< HEAD
			else if (target.gameObject.tag == "MedPackPU"){
=======

			if (target.gameObject.tag == "MedPackPU"){
				if (state == STATE_IDLE&&enemyHP==5){
					Debug.Log("wander");
					wander();
					return;
				}

>>>>>>> origin/AIv2
				if (state == STATE_COMBAT)
					state = STATE_ALERT;

				if (state != STATE_ALERT){

					if(enemyHP!=5){
						//Debug.Log("should go to medpack");
						lookAt(target);
<<<<<<< HEAD
						newTarget = new Vector3(target.transform.position.x, 0.5f, target.transform.position.z);
						chase(newTarget);
=======
						newTarget = new Vector3(target.transform.position.x, 0f, target.transform.position.z);
						goTo(newTarget);
>>>>>>> origin/AIv2
					}
					else{
						//Debug.Log("find medpack, but hp is full,wander");
						wander();
						/*
						if(targetReached){					
							state = STATE_IDLE;
							OnDisable ();
							stopMove ();
							wander();
							Debug.Log("wander(medpack)-");
						}else{
							Debug.Log("still chasing(Medpack");
							
							chase(newTarget);
						}
						*/
					}
				}else{


					if(targetReached){					
						state = STATE_IDLE;
						OnDisable ();
						stopMove ();
						wander();
						//Debug.Log("wander(medpack)");
					}else{
						//Debug.Log("still chasing(Medpack");
					
						chase(newTarget);
					}
				}


				//chase (newTarget);

			}

		} else {



			if (state == STATE_COMBAT){
				state = STATE_ALERT;
				counter = 0;
				
			}
			//Debug.Log("called when target is null");
			//Debug.Log(target.gameObject.tag.ToString());
			if(state == STATE_ALERT){
				//counter = counter + 1;
				StopFiring();
				//if((newTarget - transform.position).magnitude < 1f){
				//if (counter <= 180){
			//chase (p1.transform.position);
			//
			//else{
			//counter = 0;
				//	chase(newTarget);
					if(targetReached){

						state = STATE_IDLE;
						OnDisable ();
						stopMove ();
						wander();
						//Debug.Log("wander");
					}else{
						//Debug.Log("still chasing");

						chase(newTarget);
					}
				//}
			}

			if (state == STATE_IDLE)
				wander();
			//if(state == STATE_IDLE){
			//	wander();
			//}
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
