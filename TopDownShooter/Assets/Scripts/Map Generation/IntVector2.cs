using UnityEngine;
[System.Serializable]

// A struct used to represent two dimensional coordinates
public struct IntVector2 {
	
	public int x, z;
	
	public IntVector2 (int x, int z) {
		this.x = x;
		this.z = z;
	}
	
	public static IntVector2 operator + (IntVector2 a, IntVector2 b) {
		a.x += b.x;
		a.z += b.z;
		return a;
	}

	public bool isEqual(int x, int z) {
		return ((x == this.x) && (z== this.z));
	}

	public bool isEqual(IntVector2 v) {
		return ((v.x == this.x) && (v.z== this.z));
	}

	public float distance(IntVector2 v) {
		return Mathf.Sqrt(Mathf.Pow(x - v.x, 2) + Mathf.Pow(z - v.z, 2));
	}
}