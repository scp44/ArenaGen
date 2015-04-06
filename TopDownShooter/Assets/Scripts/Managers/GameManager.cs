using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

	public Map map;
	public Slider healthBar;
	public PauseMenu pauseScript;
	public Slider armorBar;
	public Text fullHealthText;

	private static GameManager instance;

	// Use this for initialization
	void Start () {
		instance = this;
		// Generate the map
		map.generate ();
		fullHealthText.enabled = false;
		armorBar.gameObject.SetActive (false);
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

	public static void updateHealthBar(float value){
		instance.healthBar.value = value;
	}

	public static void updateArmorBar(float value){
		if (value > 0) {
			instance.armorBar.gameObject.SetActive (true);
			instance.armorBar.value = value;
		}
		else {
			instance.armorBar.gameObject.SetActive (false);
		}
	}

	public static void displayHealthMessage () {
		Debug.Log ("Text should be enabled.");
		instance.fullHealthText.enabled = true;
	}

	public static void hideHealthMessage () {
		Debug.Log ("Text should be disabled.");
		instance.fullHealthText.enabled = false;
	}
}
