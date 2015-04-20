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

	private Vector3 newTarget;


	
	protected override void Start () {
		state = STATE_IDLE;
		base.Start ();
	}
	
	protected override void Update() {
		base.Update ();	
		GameObject target = this.visionCheck ();//wonder if it should return a boolean 

		if (target != null) {
			Debug.Log(target.gameObject.tag.ToString());
			if (target.gameObject.tag=="Player"){
				lookAt (target);
				StartFiring ();

				newTarget = new Vector3(target.transform.position.x, 0f, target.transform.position.z);
		
				state = STATE_COMBAT;
					
				chase (newTarget);
			}

			if (target.gameObject.tag == "MedPackPU"){
				Debug.Log("hack");
			}

		} else {
			if (state == STATE_COMBAT)
				state = STATE_ALERT;
			//Debug.Log("called when target is null");
			//Debug.Log(target.gameObject.tag.ToString());
			if(state == STATE_ALERT){
				StopFiring();
				//if((newTarget - transform.position).magnitude < 1f){
				if(targetReached){
					Debug.Log("reached");
					state = STATE_IDLE;
					OnDisable ();
					stopMove ();
					wander();
				}else{
					Debug.Log("still chasing");

					chase(newTarget);
				}
			}

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
