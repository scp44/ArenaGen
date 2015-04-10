using UnityEngine;
using System.Collections;

public class BossBasic : EnemyBasic {
	//A boss has two weapons instead of one
	public int equipped1;
	public int equipped2;
	public Vector3 initialPosition;
	[Range(0f, 1f)]
	public float idleAmount;

	//States of the boss AI
	private const int STATE_IDLE = 0;
	private const int STATE_ALERT = 1;
	private const int STATE_COMBAT = 2;

	protected override void Start() {
		base.Start ();
		changeState (STATE_IDLE);
	}

	protected override void Update() {
		base.Update ();

		switch (state) {
		case STATE_IDLE:
			/*
			 * In the idle state the boss stands still and sometimes
			 * wanders around in a small area
			 */
			break;
		case STATE_ALERT:
			/*
			 * In the alert state the boss fires a few more rounds at the
			 * last seen player position, then stays facing this position
			 */
			break;
		case STATE_COMBAT:
			/*
			 * In the combat state the boss fires everything he has at the player
			 */
			break;
		default:
			Debug.Log ("The boss has unknown state (" + state.ToString() + ")");
			break;
		}
	}
}
