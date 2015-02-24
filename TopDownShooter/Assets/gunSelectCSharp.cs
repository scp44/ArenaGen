using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class gunSelectCSharp : MonoBehaviour {
	int [] selected;
	Button StartButton;
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
		if (selected [0] > 0 && selected [1] > 0)
			StartButton.interactable = true;
		else
			StartButton.interactable = false;
	}

	public void StartGame () {
		Application.LoadLevel("ArenaGen");
	}
	
	public void MainMenuReturn () {
		Application.LoadLevel("StartMenu");
	}
	
	void GunSelect(int gun) {
		if(gun == selected[0])
		{
			selected[0] = 0;
			//unhighlight button?
		}
		
		else if(gun == selected[1])
		{
			selected[1] = 0;
			//unhighlight button?
		}
		
		else
		{
			if(selected[0] == 0)
			{
				selected[0] = gun;
			}
			
			else if(selected[1] == 0)
			{
				selected[1] = gun;
			}
		}
	}

	public void updateDifficulty (float difficulty) {
		AudioListener.volume = difficulty;
		selected [2] = (int)AudioListener.volume;
	}
	
	void StandardDisplay () {
		
	}
	
	void SpeedDisplay () {
		
	}
	
	void AoEDisplay () {
		
	}
	
	void SprayerDisplay () {
		
	}
	
	void LongDisplay () {
		
	}
}
