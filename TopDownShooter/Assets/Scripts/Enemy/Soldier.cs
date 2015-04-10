using UnityEngine;
using System.Collections;
using Pathfinding.RVO;


public class Soldier : EnemyBasic {
	private const int STATE_IDLE = 0;
	private const int STATE_ALERT = 1;
	private const int STATE_COMBAT = 2;
	private const int STATE_GUARD = 3;
	private const int STATE_FOLLOWING = 4;
	private GameObject toGuard;
	// Use this for initialization
	void Start () {
		state = STATE_IDLE;
	}
	
	// Update is called once per frame
	void Update () {
		base.Update();
		GameObject target = null;

		target = visionCheck ();
		Vector3 targetPos;

		switch(state){
		case STATE_IDLE:
			if(target != null){
				if(getTag(target) == "Player"){
					changeState(STATE_COMBAT);
				}
			}
			toGuard = commCheck();
			if(toGuard != null){
				changeState(STATE_FOLLOWING);
			}
			else{
				if(timeLeft <= 0){
				wander();
				}

			}
			break;
		case STATE_ALERT:
			break;
		case STATE_COMBAT:
			if (target != null) {
				lookAt (target);
				float x = target.transform.position.x; //use to make enemy silly
				float z = target.transform.position.z;
				
				 targetPos = new Vector3(x,0.5f,z);
				chase (targetPos);
				if (getTag(target) == "Player")
					StartFiring ();
					if (player != null){
					this.passedInfo.playerPos = player.transform.position;
					this.passedInfo.lastTimeSeen = Time.timeSinceLevelLoad;}
				else{
					StopFiring ();
					changeState(STATE_IDLE);
				}
			}
			else{changeState(STATE_IDLE);}
				break;

		case STATE_FOLLOWING:
			if(toGuard != null){
				lookAt(toGuard);
				float x = target.transform.position.x; //use to make enemy silly
				float z = target.transform.position.z;
				targetPos = new Vector3(x,0.5f,z);

				chase (targetPos);
				if(target != null){
					if(getTag(target) == "Player"){
						changeState(STATE_COMBAT);
					}
				}
			}
			else{changeState(STATE_IDLE);}
			break;
		case STATE_GUARD:
			break;
		default:
			break;
	}
}
	public GameObject commCheck(){
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
	}
}