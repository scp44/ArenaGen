using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

	public Map map;
	public Slider healthBar;
	public PauseMenu pauseScript;

	// Use this for initialization
	void Start () {
		// Generate the map
		map.generate ();
		//pauseScript = GetComponent <PauseMenu> ();
	}

	void Update () {
		if(Input.GetKey("escape")){
			Debug.Log ("Pause here");
			//actually pause the game
			Time.timeScale = 0;
			//load pause menu
			pauseScript.activatePauseMenu();
		}
	}

	public void updateHealthBar(GameObject player){
		PlayerController playerScript = player.GetComponent<PlayerController>();
		healthBar.value = playerScript.playerHP / playerScript.maxHP;
	}
}
