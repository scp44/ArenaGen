using UnityEngine;
using System.Collections.Generic;

public class WeaponManager : MonoBehaviour {

	public List<WeaponInfo> weaponStats;
	public List<Transform> weaponPrefabs;
	public List<EnemyBulletBehaviors> enemyBulletPrefabs;
	public List<BulletBehaviors> playerBulletPrefabs;
	public static int numWeapons;
	public float angle = 90;
	public int numBullets = 5;
	public float angleBetween;
	private static WeaponManager instance;

	void Awake() {
		instance = this;
		//Check that the lists have the same length
		if (weaponStats.Count != weaponPrefabs.Count
		    || weaponPrefabs.Count != enemyBulletPrefabs.Count
		    || playerBulletPrefabs.Count != enemyBulletPrefabs.Count)
			Debug.LogError("Not all lists specified in WeaponManager have the same length.");
		numWeapons = weaponStats.Count;
		angleBetween = angle / numBullets;
	}

	public static WeaponInfo getWeapon(int weaponType) {
		foreach (WeaponInfo weapon in instance.weaponStats)
			if (weapon.weaponType == weaponType)
				return weapon;
		Debug.LogError ("Could not get weapon of type " + weaponType.ToString());
		return new WeaponInfo();
	}

	public static Transform getWeaponPrefab(int weaponType) {
		for (int i=0; i<numWeapons; i++)
			if (instance.weaponStats[i].weaponType == weaponType)
				return instance.weaponPrefabs[i];
		Debug.LogError ("Could not get weapon of type " + weaponType.ToString());
		return null;
	}

	public static Rigidbody getEnemyBulletPrefab(int weaponType) {
		for (int i=0; i<numWeapons; i++)
			if (instance.weaponStats[i].weaponType == weaponType)
				return instance.enemyBulletPrefabs[i].rigidbody;
		Debug.LogError ("Could not get enemy bullet of type " + weaponType.ToString());
		return new Rigidbody();
	}

	public static float getAngle (){
		return instance.angle;
	}

	public static float getAngleBetween (){
		return instance.angleBetween;
	}

	public static int getnumBullets (){
		return instance.numBullets;
	}

	public static Rigidbody getPlayerBulletPrefab(int weaponType) {
		for (int i=0; i<numWeapons; i++)
			if (instance.weaponStats[i].weaponType == weaponType)
				return instance.playerBulletPrefabs[i].rigidbody;
		Debug.LogError ("Could not get player bullet of type " + weaponType.ToString());
		return new Rigidbody();
	}
}

[System.Serializable]
public struct WeaponInfo {
	public int weaponType;
	public float bulletDamage;
	public float bulletSpeed;
	public float bulletCooldown;
	public float bulletLength;
}
