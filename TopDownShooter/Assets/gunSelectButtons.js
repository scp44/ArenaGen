#pragma strict
var selectedGuns = [null, null];

function Start () {

}

function Update () {

}

function StartGame () {
	Application.LoadLevel("ArenaGen");
}

function MainMenuReturn () {
	Application.LoadLevel("StartMenu");
}

function GunSelect(gun) {
	if(gun == selectedGuns[0])
	{
		selectedGuns[0] = null;
		//unhighlight button?
	}
	
	else if(gun == selectedGuns[1])
	{
		selectedGuns[1] = null;
		//unhighlight button?
	}
	
	else
	{
		if(selectedGuns[0] == null)
		{
			selectedGuns[0] = gun;
		}
		
		else if(selectedGuns[1] == null)
		{
			selectedGuns[1] = gun;
		}
	}
}

function StandardDisplay () {
	
}

function SpeedDisplay () {

}

function AoEDisplay () {

}

function SprayerDisplay () {

}

function LongDisplay () {

}