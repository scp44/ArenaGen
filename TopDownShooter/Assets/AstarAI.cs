using UnityEngine;
using System.Collections;
//Note this line, if it is left out, the script won't know that the class 'Path' exists and it will throw compiler errors
//This line should always be present at the top of scripts which use pathfinding
using Pathfinding;
public class AstarAI : MonoBehaviour {
	private Transform target;

	//private Seeker seeker;
	private CharacterController controller;

	//path
	public Path path;

	//AI speed
	public float speed = 40;

	//not quite sure about this
	public float nextWaypointDistance = 3;

	//the waypoint we are currently moving towards
	private int currentWaypoint = 0;
	private Transform player;
	private bool findNew = true;
	private int count = 0;

	public void Start () {
		player = GameObject.FindGameObjectsWithTag ("Player") [0].transform;

	}

	public void Update() {
		//player = GameObject.FindGameObjectsWithTag ("Player") [0].transform;
		  

			if (((player.position - transform.position).magnitude < (0.5 * speed))&&findNew) {
				findNew = false;
				target = player;

		
				//Get a reference to the Seeker component we added earlier
				Seeker seeker = GetComponent<Seeker> ();
				controller = GetComponent<CharacterController> ();
		
				//Start a new path to the targetPosition, return the result to the OnPathComplete function
				//if (seeker.IsDone ())
					seeker.StartPath (transform.position, target.position, OnPathComplete);
			currentWaypoint = 0;
			} else {
			}

		
	}

	
	public void OnPathComplete (Path p) {
		Debug.Log ("Yay, we got a path back. Did it have an error? "+p.error);
		if (!p.error) {
			path = p;
			//Reset the waypoint counter
			currentWaypoint = 0;
		}
	}

	public void FixedUpdate(){

		if (path == null) {
			//We have no path to move after yet
			Debug.Log ("no path to move after yet");
			return;
		}
		
		if (currentWaypoint >= path.vectorPath.Count) {
			Debug.Log ("End Of Path Reached");
			currentWaypoint = 0;
			return;
		}

		if ((player.position - transform.position).magnitude < (0.5 * speed)) {

			Vector3 direction = (path.vectorPath [currentWaypoint] - transform.position).normalized * speed;
			controller.SimpleMove (direction);
			Vector3 lookDirection = direction;
			lookDirection.Set (lookDirection.x, 0f, lookDirection.z);
			transform.rotation = Quaternion.LookRotation (lookDirection);

			if (Vector3.Distance (transform.position, path.vectorPath [currentWaypoint]) < nextWaypointDistance)
				currentWaypoint++;
			count ++;
			if (count > 100) {
				count = 0;
				findNew = true;
			}
		} else {
			findNew = true;
		}
	}

} 