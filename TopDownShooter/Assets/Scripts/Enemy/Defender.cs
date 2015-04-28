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
	private const int STATE_PROTECT_BOSS = 5;

	private bool ccd = false;
	private Vector3 commTarget;

	private Vector3 newTarget;
		
	protected override void Start () {
		state = STATE_IDLE;
		base.Start ();
	}
	
	protected override void Update() {



		base.Update ();	
		GameObject target = this.visionCheck ();//wonder if it should return a boolean 

		if (target != null){

			if (target.gameObject.tag=="Player"){
				if(state!=STATE_COMBAT){
					stopMove();
					state = STATE_COMBAT;
					if(state == STATE_PROTECT_BOSS)
						OnDisable();
				}

				
				lookAt (target);
				StartFiring ();

				newTarget = new Vector3(target.transform.position.x, 0.5f, target.transform.position.z);
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
		
			if (target.gameObject.tag == "MedPackPU"){
				StopFiring();
				if (state == STATE_IDLE){

					if(enemyHP!=maxHP){
						lookAt(target);
						newTarget = new Vector3(target.transform.position.x, 0.5f, target.transform.position.z);

						chase(newTarget);
					}
				}else{
					chase(newTarget);
				}
			}

		} else {
			StopFiring();
			if (state == 5&&passedInfo.bossFound)
				chase(passedInfo.bossPos);

			if (state == STATE_COMBAT){
				state = STATE_ALERT;
			}

			if (state == STATE_ALERT){
				stopMove();
				chase(newTarget);
			}

		}

		if (passedInfo.bossFound&&passedInfo.playerFound&&state==STATE_IDLE&&(protect == false)) {
			state = STATE_PROTECT_BOSS;
			OnEnable();
			reenable = false;
			newTarget = passedInfo.bossPos;
			chase (passedInfo.bossPos);
		}

		if (state == STATE_IDLE && timeLeft<= 0&&(enemyHP == maxHP||(enemyHP!=maxHP &&target == null))){
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
