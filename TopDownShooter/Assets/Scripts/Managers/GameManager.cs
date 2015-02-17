using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	public Map map;

	// Use this for initialization
	void Start () {
		// Generate the map
		map.generate ();
	}

}
