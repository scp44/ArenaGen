using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

	public Map map;
	public Slider healthBar;

	// Use this for initialization
	void Start () {
		// Generate the map
		map.generate ();
	}

	public void updateHealthBar(GameObject player){
		PlayerController playerScript = player.GetComponent<PlayerController>();
		healthBar.value = playerScript.playerHP / playerScript.maxHP;
	}
}
