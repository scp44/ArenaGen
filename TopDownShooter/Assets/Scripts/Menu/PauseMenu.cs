﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour {
	public GameObject PauseBackground;
	public GameObject MainMenuButton;
	public GameObject ReturnButton;


	// Use this for initialization
	void Start () {
		PauseBackground.SetActive(false);
		MainMenuButton.SetActive(false);
		ReturnButton.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void activatePauseMenu(){
		PauseBackground.SetActive(true);
		MainMenuButton.SetActive(true);
		ReturnButton.SetActive(true);
	}

	public void mainMenu(){
		Application.LoadLevel ("StartMenu");
	}

	public void unpause(){
		Time.timeScale = 1;
		PauseBackground.SetActive(false);
		MainMenuButton.SetActive(false);
		ReturnButton.SetActive(false);
	}
}