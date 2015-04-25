using UnityEngine;
using System.Collections;
using Pathfinding.RVO;

namespace Pathfinding {
	[RequireComponent(typeof(Seeker))]
	public class Soldier : EnemyBasic {
		private const int STATE_IDLE = 0;
		private const int STATE_ALERT = 1;
		private const int STATE_COMBAT = 2;
		private const int STATE_GUARD = 3;
		private bool ccd = false;

		private Vector3 newTarget;
		private Vector3 commTarget;
		// Use this for initialization
		void Start () {
			state = STATE_IDLE;
			base.Start();
		}
		
		// Update is called once per frame
		void Update () {
			base.Update();
		

			GameObject target = null;

			target = visionCheck ();
			//check if the player is seen by allies
			if (commCheck() == true){
				ccd = commCheck();
				commTarget = this.passedInfo.playerPos;
			}
			Vector3 targetPos;

			switch (state) {
			case STATE_IDLE:


				if ((target != null && target.gameObject.tag == "Player")||ccd) {
						state = STATE_COMBAT;
						break;
				}
				else{
					if(timeLeft <= 0){
						wander();
					}
				}
				
				break;
			case STATE_ALERT:
				if(target == null){
					chase(passedInfo.playerPos);

					timeLeft = 0;
					changeState(STATE_IDLE);

				}
				break;
			case STATE_COMBAT:
				if (target != null && target.gameObject.tag == "Player") {
					lookAt (target);
					float x = target.transform.position.x; 
					float z = target.transform.position.z;
					
					newTarget = new Vector3(x,0.5f,z);
					chase (newTarget);
					StartFiring();
					
					if (player != null)
					this.passedInfo.playerPos = player.transform.position;
					this.passedInfo.lastTimeSeen = Time.timeSinceLevelLoad;

				}else if(ccd){
					float x = commTarget.x; 
					float z = commTarget.z;

					newTarget = new Vector3(x,0.5f,z);
					chase (newTarget);
					StartFiring();
					
					if (player != null)
						this.passedInfo.playerPos = player.transform.position;
					this.passedInfo.lastTimeSeen = Time.timeSinceLevelLoad;
				}
				else {
					StopFiring();
					changeState(STATE_ALERT);
				}
				break;
			default:
				changeState(STATE_IDLE);
				break;
				
			}
			deathCheck ();
	}
	
	}
}