using UnityEngine;

public class Map: MonoBehaviour {

	//Define possible tiles
	public Transform[] groundTypes;
	public Transform[] roadTypes;

	//Define possible obstacles
	public Transform[] obstacleTypes;

	//Define map size
	public int mapLength;
	public int mapWidth;

	//Define amount of obstacles generated
	[Range(0f, 1f)]
	public float obstacleProbability;

	private const int tileLength = 2;

	//Generate map
	public void generate() {
		// Iterate over locations
		for (int i=0; i<mapLength; i++) {
			for (int j=0; j<mapWidth; j++) {
				// Pick a random prefab
				//Transform cell = groundTypes[Random.Range (0,groundTypes.Length)];
				Transform prefab = Random.value < 0.5 ? groundTypes[0] : roadTypes[0];
				Transform cell = Instantiate(prefab) as Transform;

				// Put it in the proper location
				cell.position = new Vector3(i*(-1)*tileLength, 0, j*tileLength);

				// Place an obstacle on top with proper probability
				if (Random.value < obstacleProbability && !(i==0 && j==0)) {
					Transform obstaclePrefab = obstacleTypes[Random.Range (0,(obstacleTypes.Length-1))];
					Transform obstacle = Instantiate(obstaclePrefab) as Transform;
					obstacle.position = new Vector3(i*(-1)*tileLength, 0.7f, j*tileLength);
				}
			}
		}
	}
}
