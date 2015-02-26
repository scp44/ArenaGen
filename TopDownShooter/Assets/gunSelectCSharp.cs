using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class gunSelectCSharp : MonoBehaviour {
	int [] selected;
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
		selected [0] = 0;
		selected [1] = 0;
		selected [2] = 1;

		StartButton.interactable = false;
	}
	
	// Update is called once per frame
	void Update () {

	}

	public void StartGame () {
		Application.LoadLevel("ArenaGen");
	}
	
	public void MainMenuReturn () {
		Application.LoadLevel("StartMenu");
	}
	
	public void GunSelect(int gun) {
		if(gun == selected[0])
		{
			selected[0] = 0;
			GunDisplay1.text = changeDisplay(0);
		}
		
		else if(gun == selected[1])
		{
			selected[1] = 0;
			//unhighlight button?
			GunDisplay2.text = changeDisplay(0);
		}
		
		else
		{
			if(selected[0] == 0)
			{
				selected[0] = gun;
				GunDisplay1.text = changeDisplay(gun);
			}
			
			else if(selected[1] == 0)
			{
				selected[1] = gun;
				GunDisplay2.text = changeDisplay(gun);
			}
		}

		//check if we can now play game
		if (selected [0] > 0 && selected [1] > 0) {
			StartButton.interactable = true;
			if (selected [0] != 1 && selected [1] != 1)
				Standard.interactable = false;
			if (selected [0] != 2 && selected [1] != 2)
				Speed.interactable = false;
			if (selected [0] != 3 && selected [1] != 3)
				AoE.interactable = false;
			if (selected [0] != 4 && selected [1] != 4)
				Sprayer.interactable = false;
			if (selected [0] != 5 && selected [1] != 5)
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
		if(selected[0] == 0)
		{
			GunDisplay1.text = changeDisplay(gun);
		}
		
		else if(selected[1] == 0 && gun != selected[0])
		{
			GunDisplay2.text = changeDisplay(gun);
		}
	}

	public void updateDifficulty (float difficulty) {
		AudioListener.volume = difficulty;
		selected [2] = (int)AudioListener.volume;
	}

	string changeDisplay (int gun) {
		if (gun == 1) {
			return "Damage: Fair\nSpeed: Fair\nCooldown: Fair\nRange: Medium";
		} else if (gun == 2) {
			return "Damage: Low\nSpeed: Fast\nCooldown: Fast\nRange: Short";
		} else if (gun == 3) {
			return "Damage: Fair\nSpeed: Slow\nCooldown: Slow\nRange: Medium\n"
				+ "Effect: Affects an area of the map";
		} else if (gun == 4) {
			return "Damage: Low\nSpeed: Fair\nCooldown: Fast\nRange: Short\n"
				+ "Effect: Fires in a spray pattern, affecting a wedge of the map in front of you";
		} else if (gun == 5) {
			return "Damage: High\nSpeed: Slow\nCooldown: Slow\nRange: Long";
		} else
			return "";
	}
}
