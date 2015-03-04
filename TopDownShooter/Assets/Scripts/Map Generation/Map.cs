using UnityEngine;
using System.Collections.Generic;

public class Map: MonoBehaviour {

	//Testing variables
	public bool willGenerateIsland;
	public bool useAI;

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
	public float enemyProbInsideCamp;
	[Range(0f, 0.05f)]
	public float enemyProbOutsideCamp;

	//Enemy camps and start location
	public int numberOfEnemyCamps;
	public int enemyCampSize;
	public int startPositionSize;
	private IntVector2[] enemyCamps;
	private IntVector2 startLocation;
	
	private MapCell [,] cells;
	
	private const float tileLength = 0.5f;

	//Generate map
	public void generate() {
		cells = new MapCell[mapLength, mapWidth];

		//Generate water around the map
		if (willGenerateIsland)
			generateIsland (20);
		else
			generateWaterBoundaries (20);

		//Pick a location for the player to start
		do {
			startLocation = getRandomCoordinates (mapLength, mapWidth); 
		} while (!testCell(startLocation, startPositionSize));
		print ("The start location is " + startLocation.toString());
		//Pick enemy camps
		enemyCamps = new IntVector2[numberOfEnemyCamps];
		for (int i=0; i<enemyCamps.Length; i++) {
			do {
				enemyCamps[i] = getRandomCoordinates (mapLength, mapWidth); 
			} while (!testCell(enemyCamps[i], enemyCampSize, checkCamps:true, checkStart:true));
			print("An enemy camp is chosen to be at " + enemyCamps[i].toString());
		}

		//Generate Obstacles
		for (int i=0; i<mapLength; i++) {
			for (int j=0; j<mapWidth; j++) {
				//Skip the cell if it is already water
				if (cells[i, j] != null && cells[i,j].cellType == MapCellType.waterCell)
					continue;
				IntVector2 coordinates = new IntVector2(i,j);
				
				//Determine the cell type
				MapCellType cellType = MapCellType.groundCell;
				if (Random.value < obstacleProbability && coordinates.distance(startLocation) > 3f) {
					cellType = MapCellType.miscObstacleCell;
				}
				
				//Create the cell
				createCell(cellType, coordinates);
			}
		}

		spawnEnemies ();
		
		//Put the player at the starting position
		Vector3 startPosition3D = coordinatesFrom2D(startLocation, 0.6f);
		player.transform.position = startPosition3D;
		if (useAI) {
			AstarPath.active.Scan();
		}
	}

	//Spawn enemies
	private void spawnEnemies() {
		bool spawnEnemy = false;
		for (int i=0; i<mapLength; i++) {
			for (int j=0; j<mapWidth; j++) {
				IntVector2 coordinates = new IntVector2(i,j);
				//Check if we can place an enemy here
				if (testCell(coordinates, 2) && cells[i,j].isPassable && coordinates.distance(startLocation)>startPositionSize) {
					//Check if it is inside an enemy camp
					if (distanceToClosestEnemyCamp(coordinates)<=enemyCampSize) {
						spawnEnemy = Random.value < enemyProbInsideCamp;
					}
					else {
						spawnEnemy = Random.value < enemyProbOutsideCamp;
					}

					//Place an enemy
					if (spawnEnemy) {
						placeEnemyAtCell(coordinates);
						cells[i,j].isPassable = false;
					}
				}
			}
		}
	}

	//Generates water inside the map to make it look like an island
	private void generateIsland(int padding) {
		//Create water boundaries
		generateWaterBoundaries (padding);

		//Initialize the list of the water cells
		Queue<IntVector2> tempWaterCellQueue = new Queue<IntVector2> ();
		//Add the inner layer of coordinates to the queue
		for (int i=-1; i<mapLength+1; i++) {
			for (int j=-1; j<mapWidth+1; j++) {
				IntVector2 coordinates = new IntVector2(i,j);
				//Skip cells inside the map
				if (!withinMap(coordinates)) {
					tempWaterCellQueue.Enqueue(coordinates);
				}
			}
		}

		//List of water cells
		List<IntVector2> waterCellList = new List<IntVector2> ();
		//List of non-water cells that are neighbors of water cells
		List<IntVector2> notWaterCellList = new List<IntVector2> ();
		//length of the half diagonal of the map
		IntVector2 mapCenter = new IntVector2 ((int)(mapLength / 2), (int)(mapWidth / 2));
		float maxDistanceToCenter = mapCenter.distance (new IntVector2(mapLength, mapWidth));
		//Iterate over queue elements until it is not empty
		while (tempWaterCellQueue.Count>0) {
			//Remove a cell from the queue and add to the water cell list
			IntVector2 currentCell = tempWaterCellQueue.Dequeue();
			if (withinMap(currentCell))
				waterCellList.Add(currentCell);

			//Iterate over all neighbors that have not been visited
			foreach(IntVector2 currentNeighbor in getNeighbors(currentCell)) {
					//We only care about the cells that have not been visited yet
					if (!waterCellList.Contains(currentNeighbor) 
					    && !notWaterCellList.Contains(currentNeighbor)
					    && !tempWaterCellQueue.Contains(currentNeighbor)) {
						//Compute probability that it will be a water cell
						float distanceToCenter = currentNeighbor.distance(mapCenter);
						float relativeDistance = distanceToCenter/maxDistanceToCenter;
						//The probability function should map [0,1] (rel.distance) to [0,1] (probability)
						float waterCellProbability = Mathf.Pow((-1/(relativeDistance-2)), 2);

						//Add the cell to the appropriate list
						if (Random.value < waterCellProbability)
							tempWaterCellQueue.Enqueue(currentNeighbor);
						else
							notWaterCellList.Add (currentNeighbor);
					}
			}
		}

		//Put water instead of unreachable ground cells
		//Starting with the map center, explore the reachable ground
		List<IntVector2> exploredGround = new List<IntVector2>();
		Queue<IntVector2> unexploredGround = new Queue<IntVector2>();
		unexploredGround.Enqueue (mapCenter);
		while (unexploredGround.Count > 0) {
			IntVector2 currentCell = unexploredGround.Dequeue();
			//Look at all the neighbors
			foreach(IntVector2 neighbor in getNeighbors(currentCell)) {
				if (exploredGround.Contains(neighbor) || unexploredGround.Contains(neighbor))
					continue;
				if (waterCellList.Contains(neighbor)) {
					exploredGround.Add (neighbor);
				}
				else {
					unexploredGround.Enqueue(neighbor);
				}
			}
			exploredGround.Add (currentCell);
		}
		//Put water on the all other cells
		for (int i=0; i<mapLength; i++) {
			for (int j=0; j<mapWidth; j++) {
				IntVector2 coordinates = new IntVector2(i,j);
				if (!exploredGround.Contains(coordinates) 
				    && !waterCellList.Contains(coordinates)) {
					waterCellList.Add (coordinates);
				}
			}
		}

		//Put water cells in proper locations
		foreach (IntVector2 waterCell in waterCellList) {
			createCell(MapCellType.waterCell, waterCell);
		}
	}

	//Generates water around the map
	private void generateWaterBoundaries(int padding) {
		//Iterate over extended map
		for (int i=-padding; i<mapLength+padding; i++) {
			for (int j=-padding; j<mapWidth+padding; j++) {
				//print ("Looking at cell " + i.ToString() + ", " + j.ToString());
				//Skip map cells
				if (withinMap(new IntVector2(i,j))) {
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
			break;
		default:
			prefab = null;
			isPassable = false;
			break;
		}
		if (!isPassable) {
			//Control passability with elevation
			elevation = 0.5f;
		}
		MapCell cell = Instantiate (prefab) as MapCell;
		
		//Put the cell at the proper location
		cell.transform.position = coordinatesFrom2D (coordinates, elevation);
		//Assign the cell properties to the cell
		cell.coordinates = coordinates;
		cell.cellType = cellType;
		cell.isPassable = isPassable;

		//Put the cell into the array
		if (withinMap(coordinates))
			cells [coordinates.x, coordinates.z] = cell;
		
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

	private List<IntVector2> getNeighbors (IntVector2 currentCell) {
		List<IntVector2> result = new List<IntVector2>();
		for (int i=currentCell.x-1; i<=currentCell.x+1; i++) {
			for (int j=currentCell.z-1; j<=currentCell.z+1; j++) {
				IntVector2 cell = new IntVector2(i,j);
				if (withinMap(cell) && (currentCell.x == cell.x || currentCell.z == cell.z) && !currentCell.isEqual(cell))
				    result.Add (cell);
			}
		}
		return result;
	}

	private bool withinMap(IntVector2 coordinates) {
		return 0 <= coordinates.x && coordinates.x < mapLength && 0 <= coordinates.z && coordinates.z < mapWidth;
	}
	
	//Convert coordinates in the map to the coordinates in the space
	private Vector3 coordinatesFrom2D (IntVector2 coordinates, float y) {
		return new Vector3(coordinates.x*tileLength, y, coordinates.z*tileLength);
	}

	//Get the distance to the closest enemy camp
	private float distanceToClosestEnemyCamp (IntVector2 coordinates){
		float max = 99999f;
		float tempDistance;
		for (int i=0; i<enemyCamps.Length; i++) {
			if (withinMap(enemyCamps[i])) {
				tempDistance = enemyCamps[i].distance(coordinates);
				if (tempDistance != 0)
					max = max < tempDistance ? max : tempDistance;			
			}
		}
		return max;
	}

	//Make sure there is no object in the given radius
	private bool testCell(IntVector2 coordinates, int radius, bool checkCamps = false, bool checkStart = false) {
		if (checkCamps && distanceToClosestEnemyCamp(coordinates)<enemyCampSize) {
			return false;
		}
		if (checkStart && coordinates.distance(startLocation)<startPositionSize+enemyCampSize) {
			return false;
		}
		
		for (int i=coordinates.x-radius; i<coordinates.x+radius; i++) {
			for (int j=coordinates.z-radius; j<coordinates.z+radius; j++) {
				IntVector2 currentCell = new IntVector2(i,j);
				if (!currentCell.isEqual(coordinates) && (!withinMap(currentCell) || (cells[i,j]!=null && !cells[i,j].isPassable))) {
					return false;
				}
			}
		}
		return true;
	}
}
