using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class gunSelectCSharp : MonoBehaviour {
	public int [] selected;
	public Button StartButton;
	public Button Standard;
	public Button Speed;
	public Button AoE;
	public Button Sprayer;
	public Button Long;
	public Text GunDisplay1;
	public Text GunDisplay2;
	// Use this for initialization
	void Start () {
		selected = new int[3];
		selected [0] = -1;
		selected [1] = -1;
		selected [2] = 1;

		StartButton.interactable = false;
	}
	
	// Update is called once per frame
	void Update () {

	}

	public void StartGame () {
		DontDestroyOnLoad (this);
		Application.LoadLevel("ArenaGen");
	}
	
	public void MainMenuReturn () {
		Application.LoadLevel("StartMenu");
	}
	
	public void GunSelect(int gun) {
		if(gun == selected[0])
		{
			selected[0] = -1;
			GunDisplay1.text = changeDisplay(-1);
		}
		
		else if(gun == selected[1])
		{
			selected[1] = -1;
			GunDisplay2.text = changeDisplay(-1);
		}
		
		else
		{
			if(selected[0] == -1)
			{
				selected[0] = gun;
				GunDisplay1.text = changeDisplay(gun);
			}
			
			else if(selected[1] == -1)
			{
				selected[1] = gun;
				GunDisplay2.text = changeDisplay(gun);
			}
		}

		//check if we can now play game
		if (selected [0] > -1 && selected [1] > -1) {
			StartButton.interactable = true;
			if (selected [0] != 0 && selected [1] != 0)
				Standard.interactable = false;
			if (selected [0] != 1 && selected [1] != 1)
				Speed.interactable = false;
			if (selected [0] != 2 && selected [1] != 2)
				AoE.interactable = false;
			if (selected [0] != 3 && selected [1] != 3)
				Sprayer.interactable = false;
			if (selected [0] != 4 && selected [1] != 4)
				Long.interactable = false;
		} else {
			StartButton.interactable = false;
			Standard.interactable = true;
			Speed.interactable = true;
			AoE.interactable = true;
			Sprayer.interactable = true;
			Long.interactable = true;
		}
	}

	//gun's number if PointerEnter, 0 if PointerExit
	public void DisplayGunInfo (int gun){
		if(selected[0] == -1)
		{
			GunDisplay1.text = changeDisplay(gun);
		}
		
		else if(selected[1] == -1 && gun != selected[0])
		{
			GunDisplay2.text = changeDisplay(gun);
		}
	}

	public void updateDifficulty (float difficulty) {
		AudioListener.volume = difficulty;
		selected [2] = (int)AudioListener.volume;
	}

	string changeDisplay (int gun) {
		if (gun == 0) {
			return "Damage: Fair\nSpeed: Fair\nRange: Medium";
		} else if (gun == 1) {
			return "Damage: Low\nSpeed: Fast\nRange: Short";
		} else if (gun == 2) {
			return "Damage: Fair\nSpeed: Slow\nnRange: Medium\n"
				+ "Effect: Affects an area of the map";
		} else if (gun == 3) {
			return "Damage: Low\nSpeed: Fair\nRange: Short\n"
				+ "Effect: Fires in a spray pattern, affecting a wedge of the map in front of you";
		} else if (gun == 4) {
			return "Damage: High\nSpeed: Slow\nRange: Long";
		} else
			return "";
	}
}
