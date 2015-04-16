using UnityEngine;
using System.Collections;

public class EndScreen : MonoBehaviour {

	public void mainMenuButton(){
		Application.LoadLevel ("StartMenu");
	}

	public void creditsButton(){
		Application.LoadLevel ("Credits");
	}
}
