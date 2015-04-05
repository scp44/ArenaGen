using UnityEngine;
using System.Collections;
using Pathfinding.RVO;

namespace Pathfinding {
	[RequireComponent(typeof(Seeker))]
	public class Healer : AI {
		
		public Animation anim;
		
		/** Minimum velocity for moving */
		public float sleepVelocity = 0.4F;
		
		/** Speed relative to velocity with which to play animations */
		public float animationSpeed = 0.2F;
		
		/** Effect which will be instantiated when end of path is reached.
		 * \see OnTargetReached */
		public GameObject endOfPathEffect;
		
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
			
			//Call Start in base script (AIPath)
			base.Start ();
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
		

	}
}