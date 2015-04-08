using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

	public Map map;
	public Slider healthBar;
	public PauseMenu pauseScript;
	public Slider armorBar;
	public Text fullHealthText;
	public Text fullArmorText;

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
		instance.fullHealthText.enabled = true;
	}

	public static void hideHealthMessage () {
		instance.fullHealthText.enabled = false;
	}

	public static void displayArmorMessage () {
		instance.fullArmorText.enabled = true;
	}
	
	public static void hideArmorMessage () {
		instance.fullArmorText.enabled = false;
	}
}
