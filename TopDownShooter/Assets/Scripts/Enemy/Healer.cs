using UnityEngine;
using System.Collections;
using Pathfinding.RVO;

namespace Pathfinding {
	[RequireComponent(typeof(Seeker))]
	public class Healer : EnemyBasic {
		
		//public Animation anim;

		/** Minimum velocity for moving */
		//public float sleepVelocity = 0.4F;
		
		/** Speed relative to velocity with which to play animations */
		//public float animationSpeed = 0.2F;
		
		/** Effect which will be instantiated when end of path is reached.
		 * \see OnTargetReached */
		public int maxFollow;
		public int followers;
		public GameObject endOfPathEffect;
		private Vector3 newTarget;

		public Map mapScript;

		//state stuff
		private const int STATE_IDLE = 0;
		private const int STATE_ALERT = 1;
		private const int STATE_COMBAT = 2;
		private const int STATE_FLEE = 3;
		private const int STATE_IS_FLEEING = 4;
		private bool ccd = false;
		private Vector3 commTarget;

		//use to make enemy can follow
		private bool awake = false;
		
		public new void Start () {

			state = STATE_IDLE;
			maxFollow = Mathf.FloorToInt(difficulty / 30);
			mapScript = GameManager.getMap ();
			//Call Start in base script (AIPath)
			base.Start ();
		}

		protected virtual void Update () {
			base.Update();

			GameObject weakEnemy = this.commCheck ();
			EnemyBasic weakEnemyScript = null;
			if (weakEnemy != null)
				weakEnemyScript = weakEnemy.GetComponent <EnemyBasic>();

			GameObject target = null;

			target = this.visionCheck ();

			if (commCheck() != null){
				ccd = commCheck();
				commTarget = this.passedInfo.playerPos;
			}
			switch (state) {
			case STATE_IDLE:
				if ((target != null && target.gameObject.tag == "Player")||ccd ) {
					if (this.passedInfo.bossFound){
						state = STATE_FLEE;
						break;
					}
					else{
						state = STATE_COMBAT;
						break;
					}
				}
				else if(medPack < 1 && target != null && target.tag.Equals ("MedPackPU")){
					moveTo(target.transform.position);
				}
				else if(medPack > 0 && weakEnemy != null &&
				   (weakEnemyScript.enemyHP < weakEnemyScript.maxHP || weakEnemy.tag == "Boss")){
					useMedPack(weakEnemy);
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
					float x = target.transform.position.x; //use to make enemy silly
					float z = target.transform.position.z;
						
					newTarget = new Vector3(x,0.5f,z);
					chase (newTarget);
					StartFiring();

					if (player != null)
						this.passedInfo.playerPos = player.transform.position;
					this.passedInfo.lastTimeSeen = Time.timeSinceLevelLoad;
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
			case STATE_FLEE:
				int num = Random.Range(1, (int)(100*difficulty)+1);
				//less and less likely to go to the boss on higher difficulties
				if(num == difficulty){
					newTarget = this.passedInfo.bossPos;
					chase(newTarget);
				}
				else{
					newTarget = mapScript.getRandomPassableDestination();
					chase(newTarget);
				}
				changeState(STATE_IS_FLEEING);
				break;
			case STATE_IS_FLEEING:
				if(newTarget.Equals(this.transform.position))
				   changeState(STATE_IDLE);
				break;
			default:
				changeState(STATE_IDLE);
				break;

			}

			deathCheck ();
		}
		
		/** Point for the last spawn of #endOfPathEffect */
		protected Vector3 lastTarget;
		
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

		public GameObject commCheck(){
			GameObject[] eUs = GameObject.FindGameObjectsWithTag ("Enemy");
			GameObject weakest = null;
			GameObject boss = null;
			float lowestHP = 9999;
			int i;
			for(i = 0; i < eUs.Length; i++){
				Transform npcPos = eUs[i].transform;
				GameObject npc = eUs[i];
				EnemyBasic npcScript = npc.GetComponent<EnemyBasic>();
				//TODO: remove npsScript!=null check. Make sure it is not null.
				if (npcScript != null && ((Mathf.Pow (difficulty, 0.5f)) * (npcPos.position - transform.position).magnitude) < (commScale)) {
					//pass information
					if(this.passedInfo.bossFound){
						npcScript.passedInfo.bossPos = this.passedInfo.bossPos;
					}
					if(npcScript.passedInfo.lastTimeSeen < this.passedInfo.lastTimeSeen){
						npcScript.passedInfo.lastTimeSeen = this.passedInfo.lastTimeSeen;
						npcScript.passedInfo.playerPos = this.passedInfo.playerPos;
					}
					//check HP. If lowest percentage of HP, keep track for medic
					if((npcScript.enemyHP/npcScript.maxHP) < lowestHP)
						weakest = npc;
					if(npc.tag == "BossEnemy")
						boss = npc;
				}
			}

			if (weakest != null) {
				EnemyBasic weakestScript = weakest.GetComponent<EnemyBasic> ();
				if (weakestScript.enemyHP < weakestScript.maxHP)
					return weakest;
			}
			return boss;
		}

	}
}