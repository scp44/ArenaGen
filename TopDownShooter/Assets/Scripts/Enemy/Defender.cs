﻿using UnityEngine;
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
	//private bool wad = false;
	private Vector3 commTarget;

	private Vector3 newTarget;

	//private bool getpos = false;

	//private GameObject p1;


	
	protected override void Start () {
		state = STATE_IDLE;
		base.Start ();
	}
	
	protected override void Update() {



		base.Update ();	
		GameObject target = this.visionCheck ();//wonder if it should return a boolean 
		//int counter = 0;
	 
		/*
		if (commCheck() ==true){
			ccd = commCheck();
			commTarget = this.passedInfo.playerPos;
			Debug.Log("Meow");
		}
		*/


		if (target != null){//||ccd) {

			//Debug.Log(target.gameObject.tag.ToString());
			if (target.gameObject.tag=="Player"){
				//stopMove();
				if(state!=STATE_COMBAT){
					//Debug.Log("Change state");
					stopMove();
					state = STATE_COMBAT;
					if(state == STATE_PROTECT_BOSS)
						OnDisable();
					//OnEnable();
				}

				
				lookAt (target);
				StartFiring ();

				newTarget = new Vector3(target.transform.position.x, 0.5f, target.transform.position.z);
				//p1 = target.gameObject;

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
				StopFiring();
				if (state == STATE_IDLE){

					if(enemyHP!=maxHP){
						//Debug.Log("should go to medpack, hp is low");
						//Debug.Log(enemyHP.ToString());
						lookAt(target);
						newTarget = new Vector3(target.transform.position.x, 0.5f, target.transform.position.z);

						chase(newTarget);
					}
					else{
						//Debug.Log("find medpack, but hp is full,keep wander");
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
				//Debug.Log("no target, should keep alert until reach target");
				state = STATE_ALERT;
			}

			if (state == STATE_ALERT){
				stopMove();
				//StopFiring();
				//Debug.Log("no target, alert");
				chase(newTarget);
			}

		}

		if (passedInfo.bossFound&&passedInfo.playerFound&&state==STATE_IDLE&&(protect == false)) {
			//Debug.Log("boss position:");
			//Debug.Log (passedInfo.bossPos.ToString ());
			state = STATE_PROTECT_BOSS;
			//Debug.Log("protect boss");
			OnEnable();
			reenable = false;
			newTarget = passedInfo.bossPos;
			chase (passedInfo.bossPos);
			//return;
		}
		
		/*if (passedInfo.playerPos != opoint && (getpos == false)) {
			getpos = true;
			state = STATE_COMBAT;
			chase (passedInfo.playerPos);
			Debug.Log (passedInfo.playerPos.ToString ());
			return;
		}*/

		if (state == STATE_IDLE && timeLeft<= 0&&(enemyHP == maxHP||(enemyHP!=maxHP &&target == null))){
			//Debug.Log(state.ToString());
			//wad = true;
			//Debug.Log("wander (no target)");
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
