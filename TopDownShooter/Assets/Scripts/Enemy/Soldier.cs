using UnityEngine;
using System.Collections;
using Pathfinding.RVO;


public class Soldier : EnemyBasic {
	private const int STATE_IDLE = 0;
	private const int STATE_ALERT = 1;
	private const int STATE_COMBAT = 2;
	private const int STATE_GUARD = 3;
	private const int STATE_FOLLOWING = 4;

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
		
				break;

		case STATE_FOLLOWING:
			break;
		case STATE_GUARD:
			break;
		default:
			break;
	}
}
}