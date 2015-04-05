using UnityEngine;
using System.Collections;

public class MenuButtonCSharp : MonoBehaviour {

	public void StartGame () {
		Application.LoadLevel("GunSelect");
	}
	
	public void ExitGame () {
		Application.Quit();
	}
}
