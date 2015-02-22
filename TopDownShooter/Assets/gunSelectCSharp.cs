using UnityEngine;
using System.Collections;

public class gunSelectCSharp : MonoBehaviour {
	int [] selected;
	// Use this for initialization
	void Start () {
		selected = new int[3];
		selected [0] = 0;
		selected [1] = 0;
		selected [2] = 1;
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
