using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class gunSelectCSharp : MonoBehaviour {
	public int [] selected;
	public float difficulty;
	public Button StartButton;
	public Button Standard;
	public Button Speed;
	public Button AoE;
	public Button Sprayer;
	public Button Long;
	public Text GunDisplay1;
	public Text GunDisplay2;
	public Slider DifficultySlider;
	public Texture [] GunImage;
	public RawImage GunDisplayImage1;
	public RawImage GunDisplayImage2;

	// Use this for initialization
	void Start () {
		selected = new int[2];
		selected [0] = -1;
		selected [1] = -1;

		StartButton.interactable = false;
		GunDisplayImage1.enabled = false;
		GunDisplayImage2.enabled = false;
	}

	public void StartGame () {
		DontDestroyOnLoad (this);
		difficulty = DifficultySlider.value;
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
			GunDisplayImage1.enabled = false;
		}
		
		else if(gun == selected[1])
		{
			selected[1] = -1;
			GunDisplay2.text = changeDisplay(-1);
			GunDisplayImage2.enabled = false;
		}
		
		else
		{
			if(selected[0] == -1)
			{
				selected[0] = gun;
				GunDisplay1.text = changeDisplay(gun);
				GunDisplayImage1.enabled = true;
				GunDisplayImage1.texture = changeImage (gun);
			}
			
			else if(selected[1] == -1)
			{
				selected[1] = gun;
				GunDisplay2.text = changeDisplay(gun);
				GunDisplayImage2.enabled = true;
				GunDisplayImage2.texture = changeImage (gun);
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
			GunDisplayImage1.enabled = true;
			if(gun >=0)
				GunDisplayImage1.texture = changeImage (gun);
			else
				GunDisplayImage1.enabled = false;
		}
		
		else if(selected[1] == -1 && gun != selected[0])
		{
			GunDisplay2.text = changeDisplay(gun);
			GunDisplayImage2.enabled = true;
			if(gun >= 0)
				GunDisplayImage2.texture = changeImage (gun);
			else
				GunDisplayImage2.enabled = false;
		}
	}

	Texture changeImage (int gun){
		if (gun >= 0)
			return GunImage [gun];
		else
			return null;
	}

	/*
	public void updateDifficulty (float difficulty) {
		AudioListener.volume = difficulty;
		//selected [2] = (int)AudioListener.volume;
		selected [2] = difficulty;
	}
	*/

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
