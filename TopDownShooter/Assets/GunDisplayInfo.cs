using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GunDisplayInfo : MonoBehaviour {

	public Text Display;

	// Use this for initialization
	void Start () {
		Display = gameObject.GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void StandardDisplay () {
		Display.text = "Damage: Fair\nSpeed: Fair\nCooldown: Fair\nRange: Medium";
	}
	
	public void SpeedDisplay () {
		Display.text = "Damage: Low\nSpeed: Fast\nCooldown: Fast\nRange: Short";
	}
	
	public void AoEDisplay () {
		Display.text = "Damage: Fair\nSpeed: Slow\nCooldown: Slow\nRange: Medium\n"
			+ "Effect: Affects an area of the map";
	}
	
	public void SprayerDisplay () {
		Display.text = "Damage: Low\nSpeed: Fair\nCooldown: Fast\nRange: Short"
			+ "Effect: Fires in a spray pattern, affecting a wedge of the map in front of you";
	}
	
	public void LongDisplay () {
		Display.text = "Damage: High\nSpeed: Slow\nCooldown: Slow\nRange: Long";
	}

	public void clearDisplay() {
		Display.text = "";
	}
}
