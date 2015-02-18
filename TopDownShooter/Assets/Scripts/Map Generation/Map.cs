using UnityEngine;

public class Map: MonoBehaviour {
	
	//Player
	public Transform player;
	
	//Passable terrain
	public MapCell[] groundCellPrefabs;
	public MapCell roadPrefab;
	
	//Impassable terrain
	public MapCell waterPrefab;
	public MapCell mountainPrefab;
	public MapCell wallPrefab;
	public MapCell[] miscObstacleCellPrefabs;
	
	//Enemy types
	public Transform[] enemyTypes;
	
	//Define map size
	public int mapLength;
	public int mapWidth;
	
	//Define amount of obstacles and enemies
	[Range(0f, 0.1f)]
	public float obstacleProbability;
	[Range(0f, 0.1f)]
	public float enemyProbability;
	
	private MapCell [,] cells;
	
	private const float tileLength = 0.5f;
	
	//Generate map
	public void generate() {
		//Pick a location for the player to start
		IntVector2 startPosition = getRandomCoordinates (mapLength, mapWidth);
		
		//Iterate over locations
		for (int i=0; i<mapLength; i++) {
			for (int j=0; j<mapWidth; j++) {
				IntVector2 coordinates = new IntVector2(i,j);
				
				//Determine the cell type
				MapCellType cellType = MapCellType.groundCell;
				if (Random.value < obstacleProbability && coordinates.distance(startPosition) > 3f) {
					cellType = MapCellType.miscObstacleCell;
				}
				
				//Create the cell
				MapCell cell = createCell(cellType, coordinates);
				
				//Possibly, spawn an enemy at the cell
				if (Random.value < enemyProbability && cell.isPassable && coordinates.distance(startPosition) > 3f) {
					placeEnemyAtCell(coordinates);
				}
			}
		}
		
		//Generate water around the map
		generateWaterBoundaries (20);
		
		//Put the player at the starting position
		Vector3 startPosition3D = coordinatesFrom2D(startPosition, 0.6f);
		player.transform.position = startPosition3D;
	}
	
	//Generates water around the map
	private void generateWaterBoundaries(int padding) {
		//Iterate over extended map
		for (int i=-padding; i<mapLength+padding; i++) {
			for (int j=-padding; j<mapWidth+padding; j++) {
				//print ("Looking at cell " + i.ToString() + ", " + j.ToString());
				//Skip map cells
				if (0<=i && i<mapLength && 0<=j && j<mapWidth) {
					//print ("Skipping this cell");
					j += mapWidth;
				}
				//print ("Putting water in this cell");
				//Otherwise create water cells
				IntVector2 coordinates = new IntVector2(i,j);
				createCell(MapCellType.waterCell, coordinates);
			}
		}
	}
	
	//Create a cell with given type and coordinates
	private MapCell createCell (MapCellType cellType, IntVector2 coordinates) {
		//Pick an appropriate prefab, for now only ground and obstacle cells are used
		MapCell prefab;
		bool isPassable;
		float elevation = 0f;;
		switch(cellType) {
		case MapCellType.groundCell:
			prefab = groundCellPrefabs [Random.Range (0, groundCellPrefabs.Length)];
			isPassable = true;
			break;
		case MapCellType.wallCell:
			prefab = wallPrefab;
			isPassable = false;
			break;
		case MapCellType.miscObstacleCell:
			prefab = miscObstacleCellPrefabs[Random.Range(0,miscObstacleCellPrefabs.Length)];
			isPassable = false;
			break;
		case MapCellType.waterCell:
			prefab = waterPrefab;
			isPassable = false;
			elevation = 0.5f;
			break;
		default:
			prefab = null;
			isPassable = false;
			break;
		}
		MapCell cell = Instantiate (prefab) as MapCell;
		
		//Put the cell at the proper location
		cell.transform.position = coordinatesFrom2D (coordinates, elevation);
		//Assign the cell properties to the cell
		cell.coordinates = coordinates;
		cell.cellType = cellType;
		cell.isPassable = isPassable;
		
		return cell;
	}
	
	//Place an enemy soldier at the cell with given coordinates
	private void placeEnemyAtCell(IntVector2 coordinates) {
		Vector3 enemyCoordinates = coordinatesFrom2D(coordinates, 0.5f);
		Transform prefab = enemyTypes [Random.Range (0, enemyTypes.Length)];
		Transform enemy = Instantiate (prefab) as Transform;
		enemy.position = enemyCoordinates;
	}
	
	//Get random coordinates within range
	private IntVector2 getRandomCoordinates(int maxX, int maxZ) {
		int x = Random.Range (0, maxX);
		int z = Random.Range (0, maxZ);
		return new IntVector2 (x,z);
	}
	
	//Convert coordinates in the map to the coordinates in the space
	private Vector3 coordinatesFrom2D (IntVector2 coordinates, float y) {
		return new Vector3(coordinates.x*tileLength, y, coordinates.z*tileLength);
	}
}
