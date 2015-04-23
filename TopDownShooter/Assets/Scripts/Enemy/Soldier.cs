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
		//private const int STATE_FOLLOWING = 4;
		//private GameObject toGuard;
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
			/*Healer healerScript = null;
			if(toGuard != null)
				healerScript = toGuard.GetComponent <Healer>();
*/

			GameObject target = null;

			target = visionCheck ();
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
					//if(TargetReached){
					timeLeft = 0;
					changeState(STATE_IDLE);
					//}
				}
				break;
			case STATE_COMBAT:
				if (target != null && target.gameObject.tag == "Player") {
					lookAt (target);
					float x = target.transform.position.x; //use to make enemy silly
					float z = target.transform.position.z;
					
					newTarget = new Vector3(x,0.5f,z);
					chase (newTarget);
					StartFiring();
					
					if (player != null)
					this.passedInfo.playerPos = player.transform.position;
					this.passedInfo.lastTimeSeen = Time.timeSinceLevelLoad;
					//target = null;
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
	/*	public GameObject commCheck(){
			GameObject[] eUs = GameObject.FindGameObjectsWithTag ("Enemy");
			GameObject weakest = null;
			GameObject healer = null;
			float lowestHP = 9999;
			int i;
			for(i = 0; i < eUs.Length; i++){
				Transform npcPos = eUs[i].transform;
				GameObject npc = eUs[i];
				EnemyBasic npcScript = npc.GetComponent<EnemyBasic>();
				if ((npcPos.position - transform.position).magnitude < (commScale)) {
					//pass information
					if(this.passedInfo.bossFound){
						npcScript.passedInfo.bossPos = this.passedInfo.bossPos;
					}
					if(npcScript.passedInfo.lastTimeSeen < this.passedInfo.lastTimeSeen){
						npcScript.passedInfo.lastTimeSeen = this.passedInfo.lastTimeSeen;
						npcScript.passedInfo.playerPos = this.passedInfo.playerPos;
					}
					//check HP. If lowest percentage of HP, keep track for medic

					if(npc.tag == "Healer")
						healer = npc;
				}
			}

			return healer;
		}*/
	}
}