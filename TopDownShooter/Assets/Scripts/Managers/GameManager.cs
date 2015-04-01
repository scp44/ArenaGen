using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

	public Map map;
	public Image healthBar;

	// Use this for initialization
	void Start () {
		// Generate the map
		map.generate ();
	}

	public void updateHealthBar(GameObject player){
		PlayerController playerScript = player.GetComponent<PlayerController>();
		healthBar.fillAmount = playerScript.playerHP / playerScript.maxHP;
	}
}
