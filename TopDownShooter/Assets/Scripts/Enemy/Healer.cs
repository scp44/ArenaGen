using UnityEngine;
using System.Collections;
using Pathfinding.RVO;

namespace Pathfinding {
	[RequireComponent(typeof(Seeker))]
	public class Healer : EnemyBasic {
		
		public Animation anim;

		/** Minimum velocity for moving */
		//public float sleepVelocity = 0.4F;
		
		/** Speed relative to velocity with which to play animations */
		public float animationSpeed = 0.2F;
		
		/** Effect which will be instantiated when end of path is reached.
		 * \see OnTargetReached */

		public GameObject endOfPathEffect;
		//private GameObject tar = new GameObject(); //save the last position of player
		private Vector3 newTarget;


		//state stuff
		private const int STATE_IDLE = 0;
		private const int STATE_ALERT = 1;
		private const int STATE_COMBAT = 2;
		private const int STATE_FLEE = 3;

		//use to make enemy can follow
		private bool awake = false;
		
		public new void Start () {
//			target = GameObject.FindGameObjectsWithTag ("Player") [0].transform;
			//Prioritize the walking animation
			anim["forward"].layer = 10;
			
			//Play all animations
			anim.Play ("awake");
			anim.Play ("forward");
			
			//Setup awake animations properties
			anim["awake"].wrapMode = WrapMode.Clamp;
			anim["awake"].speed = 0;
			anim["awake"].normalizedTime = 1F;

			state = STATE_ALERT;
			
			//Call Start in base script (AIPath)
			base.Start ();
		}

		protected virtual void Update () {
			if (Time.timeScale <= 0)
				return;

			GameObject weakEnemy = this.commCheck ();
			EnemyBasic weakEnemyScript = weakEnemy.GetComponent <EnemyBasic>();
			GameObject target = null;

			target = this.visionCheck ();

			if(armorBonusHP == 0 /*&& armor in line of sight*/){
				//interrupt module
				//target = armor;
			}
			target = this.visionCheck ();//wonder if it should return a boolean 
			//Test:just pick medpack
			/*
			if (target.tag.Equals ("MedPackPU")) {//get error: null referenceException
				lookAt(target);
				chase(target);
			}
			*/
	
		if (target != null) {
			lookAt (target);
			float x = target.transform.position.x; //use to make enemy silly
			float z = target.transform.position.z;

			newTarget = new Vector3(x,0f,z);

			awake = true; //this awake is set the enemy in "searching player" model, when it faile, it will turn of to idle (not implemented yet)
			chase (newTarget);
			if (target.gameObject.tag == "Player")
				StartFiring ();
			else
				StopFiring ();
		} else {
			if (awake == true) { //if target is null, but enemy is awake, it will go to the last position player at
					//Debug.Log(tar.ToString());
				StopFiring ();
					Debug.Log("targeet is null");
					Debug.Log(newTarget.ToString());
				chase (newTarget);
			}
		}

			if(enemyHP < 5 && target != null && target.tag.Equals ("MedPackPU")){
				Debug.Log("why not move");
				lookAt(target);
				//interrupt module
				chase (target.transform.position);
			}
			

			/*
			if (target != null && target.gameObject.tag == "Player") {
				if (this.passedInfo.bossFound)
					state = STATE_FLEE;
				else
					state = STATE_COMBAT;
			/*
				StartFiring();
				lookAt(target);
				//setTarget(target.transform);
				//SearchPath();
				chase(target);

				if (player != null)
					this.passedInfo.playerPos = player.transform.position;
				this.passedInfo.lastTimeSeen = Time.timeSinceLevelLoad;
				//target = null;*/
			//}

			switch (state) {
			case STATE_IDLE:
				if(enemyHP < maxHP && target != null && target.tag.Equals ("MedPackPU")){
					moveTo (target.transform.position);
				}
				if(medPack > 0 && weakEnemy != null &&
				   (weakEnemyScript.enemyHP < weakEnemyScript.maxHP || weakEnemy.tag == "BossEnemy")){
					useMedPack(weakEnemy);
				}
			     break;
			case STATE_ALERT:
				break;
			case STATE_COMBAT:
				if (target != null && target.gameObject.tag == "Player") {
					StartFiring();
					lookAt(target);
					if (player != null)
						this.passedInfo.playerPos = player.transform.position;
					this.passedInfo.lastTimeSeen = Time.timeSinceLevelLoad;
					//target = null;
				}
				else {
					StopFiring();
				}
				break;
			case STATE_FLEE:
				int num = Random.Range(1, difficulty+1);
				//less and less likely to go to the boss on higher difficulties
				if(num == difficulty){
					moveTo(this.passedInfo.bossPos);
				}
				else{
					//go to random point somewhere
				}
				break;
			default:
				break;

			}

			//if armor broken, cancel effect.
			/*if (armorBonusHP <= 0) {
				armorBonusHP = 0;
			}*/

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