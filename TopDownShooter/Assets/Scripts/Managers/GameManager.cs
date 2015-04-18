﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
	[Range(0f, 1f)]
	public float difficulty = 0f;
	public float minPlayerBossDistance = 30;
	public Map map;
	public Transform player;
	public Transform boss;
	public PauseMenu pauseScript;
	public Slider healthBar;
	public Slider armorBar;
	public Text healthBarText;
	public Text armorBarText;
	public Text fullHealthText;
	public Text fullArmorText;
	public Text BossText;
	public Slider bossHealthBar;
	public Text bossHealthText;
	public Canvas InstructionScreen;
	public Button StartButton;

	private static GameManager instance;

	// Use this for initialization
	void Awake() {
		instance = this;
		GameObject gunSelectInfo = GameObject.Find ("_Main");
		if (gunSelectInfo != null) {
			gunSelectCSharp gunSelectScript = gunSelectInfo.GetComponent<gunSelectCSharp>();
			difficulty = gunSelectScript.difficulty;
		}
		print ("difficulty is " + difficulty.ToString());
	}

	void Start () {
		InstructionScreen.enabled = true;
		Time.timeScale = 0;
		StartButton.interactable = false;
		// Generate the map
		map.generate ();
		fullHealthText.enabled = false;
		fullArmorText.enabled = false;
		armorBar.gameObject.SetActive (false);
		hideBossUI ();
	}

	void Update () {
		if (Application.GetStreamProgressForLevel("ArenaGen") == 1)
			StartButton.interactable = true;

		//pause menu
		if(Input.GetKey("escape")){
			//actually pause the game
			Time.timeScale = 0;
			//load pause menu
			pauseScript.activatePauseMenu();
		}

		//enable boss UI if player and boss are close enough
		if (player != null && boss != null && Vector3.Distance (player.position, boss.position) <= minPlayerBossDistance) {
			showBossUI();
		}
		else {
			hideBossUI();
		}
	}

	public void StartGame () {
		InstructionScreen.enabled = false;
		Time.timeScale = 1;
	}

	public static void updateHealthBar(float currentHealth, float maxHealth){
		instance.healthBar.value = currentHealth/maxHealth;
		instance.healthBarText.text = (currentHealth.ToString("F1") + "/" + maxHealth.ToString("F1")).PadLeft(9);
	}

	public static void updateBossHealthBar(float currentHealth, float maxHealth){
		instance.bossHealthBar.value = currentHealth/maxHealth;
		instance.bossHealthText.text = (currentHealth.ToString("F1") + "/" + maxHealth.ToString("F1")).PadLeft(9);
	}

	public static void updateArmorBar(float currentArmor, float maxArmor){
		if (currentArmor > 0) {
			instance.armorBar.gameObject.SetActive (true);
			instance.armorBarText.enabled = true;
			instance.armorBar.value = currentArmor/maxArmor;
			instance.armorBarText.text = (currentArmor.ToString("F1") + "/" + maxArmor.ToString("F1")).PadLeft(9);
		}
		else {
			instance.armorBar.gameObject.SetActive (false);
			instance.armorBarText.enabled = false;
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

	public static void showBossUI() {
		instance.bossHealthBar.gameObject.SetActive (true);
		instance.bossHealthText.enabled = true;
		instance.BossText.enabled = true;
	}

	public static void hideBossUI() {
		instance.bossHealthBar.gameObject.SetActive (false);
		instance.bossHealthText.enabled = false;
		instance.BossText.enabled = false;
	}

	public static float getDifficulty() {
		return instance.difficulty;
	}
}
