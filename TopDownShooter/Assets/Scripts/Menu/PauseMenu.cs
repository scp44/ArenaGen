using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour {
	public GameObject PauseBackground;
	public GameObject MainMenuButton;
	public GameObject ReturnButton;
	public Text difficultyTxt;
	public Text statTxt;


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
		difficultyTxt.text = "Difficulty: " + (Mathf.Floor((GameManager.getDifficulty () * 100))).ToString ();

		Transform player = GameManager.getPlayer ();
		PlayerController playerScript = player.GetComponent<PlayerController> ();
		statTxt.text = 	"Player Stats\n" +
						"HP Upgrade: " + (playerScript.maxHP - 10).ToString("N1") +"\n" +
						"Damage Upgrade: " + (playerScript.bonusDamage).ToString("N1") + "\n" +
						"Armor Upgrade: " + (playerScript.maxArmor - 5).ToString("N1");
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
