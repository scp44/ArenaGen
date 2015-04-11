using UnityEngine;
using System.Collections;
using Pathfinding.RVO;

public class Defender : EnemyBasic {
		
	/** Effect which will be instantiated when end of path is reached.
	 * \see OnTargetReached */
	public GameObject endOfPathEffect;
	/** Point for the last spawn of #endOfPathEffect */
	protected Vector3 lastTarget;
	
	protected override void Start () {
		base.Start ();
	}
	
	protected override void Update() {
		base.Update ();	
		GameObject target = this.visionCheck ();//wonder if it should return a boolean 
		if (target != null) {
			lookAt (target);
			float x = target.transform.position.x; //use to make enemy silly
			float z = target.transform.position.z;
			
			Vector3 newTarget = new Vector3(x,0f,z);
			
			chase (newTarget);
			if (target.gameObject.tag == "Player")
				StartFiring ();
			else
				StopFiring ();
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
