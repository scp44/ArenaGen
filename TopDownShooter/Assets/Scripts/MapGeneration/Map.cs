using UnityEngine;
using System.Collections.Generic;

public class Map: MonoBehaviour {

	//Testing variables
	public bool willGenerateIsland;

	//Player
	public Transform player;
	
	//Passable terrain
	public MapCell[] groundCellPrefabs;
	public MapCell roadPrefab;
	
	//Impassable terrain
	public MapCell waterPrefab;
	public Transform wallPrefab;
	public Transform wallNodePrefab;
	public MapCell[] miscObstacleCellPrefabs;

	//Powerups
	public Transform armorPrefab;
	public Transform medpackPrefab;
	public int numArmors;
	public int numMedpacks;
	public float powerupDistance;
	private List<IntVector2> armors;
	private List<IntVector2> medpacks;

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
	[Range(0f, 1f)]
	public float wallNodeProbability;

	//Enemy camps and start location
	public int numberOfEnemyCamps;
	public int enemyCampSize;
	public int startPositionSize;
	private IntVector2[] enemyCamps;
	private IntVector2 startLocation;

	//Walls
	public int shortestWallLength;
	public int minWallWaterDistance;
	public float maxWallLength;
	
	private MapCell [,] cells;
	private WallGraph walls;
	
	private const float tileLength = 0.5f;

	//Generate map
	public void generate() {
		cells = new MapCell[mapLength, mapWidth];
		walls = new WallGraph ();

		//Generate water around the map
		if (willGenerateIsland)
			generateIsland (20);
		else
			generateWaterBoundaries (20);

		generatePlayerStartLocation ();
		generateEnemyCamps ();
		generateGround ();
		generateWalls ();
		spawnPowerups ();
		spawnEnemies ();
		
		//Put the player at the starting position
		Vector3 startPosition3D = coordinatesFrom2D(startLocation, 0.6f);
		player.transform.position = startPosition3D;
		AstarPath.active.Scan();
	}

	//Pick a location for the player to start
	private void generatePlayerStartLocation() {
		do {
			startLocation = getRandomCoordinates (mapLength, mapWidth); 
		} while (!testCell(startLocation, startPositionSize));
		//print ("The start location is " + startLocation.toString());
	}

	//Pick enemy camps
	private void generateEnemyCamps() {
		enemyCamps = new IntVector2[numberOfEnemyCamps];
		for (int i=0; i<enemyCamps.Length; i++) {
			do {
				enemyCamps[i] = getRandomCoordinates (mapLength, mapWidth); 
			} while (!testCell(enemyCamps[i], enemyCampSize, checkCamps:true, checkStart:true));
			//print("An enemy camp is chosen to be at " + enemyCamps[i].toString());
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

	//Create ground cells
	private void generateGround() {
		//iterate over the map
		for (int i=0; i<mapLength; i++) {
			for (int j=0; j<mapWidth; j++) {
				if (cells[i,j]==null) 
					createCell(MapCellType.groundCell, new IntVector2(i,j));
			}
		}
	}

	//Create walls
	private void generateWalls() {
		//generate the graph nodes
		//iterate over the map
		//for (int i=0; i<mapLength; i++) {
		//	for (int j=0; j<mapWidth; j++) {
		IntVector2[] coordinatesArray = IntVector2.randomizeCoordinates (mapLength, mapWidth);
		for (int p=0; p<coordinatesArray.Length; p++) {
			IntVector2 coordinates = coordinatesArray[p];
			int i=coordinates.x;
			int j=coordinates.z;
			if (Random.value >= wallNodeProbability)
				continue;
			//Skip the cell if it is not passable
			if (cells[i, j] != null && !cells[i,j].isPassable)
				continue;
			if (!testCell (coordinates, minWallWaterDistance))
				continue;
			//Skip the cell if it is within an enemy camp
			float distanceToEnemyCamp = distanceToClosestEnemyCamp(coordinates);
			int usedShortestWallLength = shortestWallLength + (int)Random.value*(5);
			if (distanceToEnemyCamp < enemyCampSize)
				continue;
			else if (distanceToEnemyCamp < enemyCampSize + 3) {
				usedShortestWallLength = shortestWallLength/2;
			}
			//Skip the cell if it is within the player start position
			if (coordinates.distance(startLocation) < startPositionSize)
				continue;
			//Skip the cell if it is too close to another wall
			if (walls.distanceToClosestNode(coordinates) < usedShortestWallLength)
				continue;
			//Otherwise put it down
			if (cells[i,j] != null) {
				cells[i,j].isPassable = false;
				//placeWallNodeCell(coordinates);
				walls.addNode(coordinates);
			}
		}

		//connect graph nodes
		for (int i=0; i<walls.getSize(); i++) {
			List<int> neighbors = walls.getNeighbors(i, maxWallLength, notConnected:true);
			foreach (int j in neighbors) {
				if (wallAllowed(i,j)) {
					placeWall(i, j);
				}
			}
		}

		//place the node cells (only the connected ones)
		for (int i=0; i<walls.getSize(); i++) {
			if (walls.getNode(i).neighbors.Count > 0)
				placeWallNodeCell(walls.getNode(i).coordinates);
		}
	}

	//spawn powerups and medpacks
	private void spawnPowerups() {
		medpacks = new List<IntVector2> ();
		armors = new List<IntVector2> ();
		IntVector2[] coordinatesArray = IntVector2.randomizeCoordinates (mapLength, mapWidth);
		for (int p=0; p<coordinatesArray.Length; p++) {
			if (medpacks.Count == numMedpacks && armors.Count == numArmors)
				break;
			IntVector2 coordinates = coordinatesArray[p];

			if (distanceToClosestPowerup(coordinates)>powerupDistance && testCell (coordinates, 1) && !testCell(coordinates, 3)) {
				//spawn a medpack or powerup
				if (medpacks.Count < numMedpacks)
					placeMedpackAtCell(coordinates);
				else 
					placeArmorAtCell(coordinates);
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

	//Place a temp flag at the cell
	private void placeWallNodeCell(IntVector2 coordinates) {
		Vector3 nodeCoordinates = coordinatesFrom2D(coordinates, 0.5f);
		Transform node = Instantiate (wallNodePrefab) as Transform;
		node.position = nodeCoordinates;
	}

	private void placeMedpackAtCell(IntVector2 coordinates) {
		Vector3 nodeCoordinates = coordinatesFrom2D(coordinates, 0.5f);
		Transform medpack = Instantiate (medpackPrefab) as Transform;
		medpack.position = nodeCoordinates;
		medpacks.Add (coordinates);
	}

	private void placeArmorAtCell(IntVector2 coordinates) {
		Vector3 nodeCoordinates = coordinatesFrom2D(coordinates, 0.5f);
		Transform armor = Instantiate (armorPrefab) as Transform;
		armor.position = nodeCoordinates;
		armors.Add (coordinates);
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

	private bool withinRect(IntVector2 coordinates, IntVector2 pointA, IntVector2 pointB) {
		return (Mathf.Min (pointA.x, pointB.x) <= coordinates.x
						&& coordinates.x <= Mathf.Max (pointA.x, pointB.x)
						&& Mathf.Min (pointA.z, pointB.z) <= coordinates.z
						&& coordinates.z <= Mathf.Max (pointA.z, pointB.z));
	}
	
	//Convert coordinates in the map to the coordinates in the space
	private Vector3 coordinatesFrom2D (IntVector2 coordinates, float y) {
		return new Vector3(coordinates.x*tileLength, y, coordinates.z*tileLength);
	}

	//Convert coordinates in the map to the coordinates in the space
	private Vector3 coordinatesFrom2D (float x, float z, float y) {
		return new Vector3(x*tileLength, y, z*tileLength);
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

	private float distanceToClosestPowerup (IntVector2 coordinates) {
		float max = 99999f;
		float tempDistance;
		for (int i=0; i<armors.Count; i++) {
			if (withinMap(armors[i])) {
				tempDistance = armors[i].distance(coordinates);
				if (tempDistance != 0)
					max = max < tempDistance ? max : tempDistance;			
			}
		}
		for (int i=0; i<medpacks.Count; i++) {
			if (withinMap(medpacks[i])) {
				tempDistance = medpacks[i].distance(coordinates);
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

	//Check if we can place a wall
	private bool wallAllowed(int wallNode1, int wallNode2) {
		List<IntVector2> nodesInBetween = cellsCoveredByWall (walls.getNode (wallNode1).coordinates, walls.getNode (wallNode2).coordinates);
		foreach (IntVector2 cellCoordinates in nodesInBetween) {
			if (cells[cellCoordinates.x, cellCoordinates.z] != null && !cells[cellCoordinates.x, cellCoordinates.z].isPassable)
				return false;
			if (distanceToClosestEnemyCamp(cellCoordinates) < enemyCampSize) 
				return false;
			if (cellCoordinates.distance(startLocation) < startPositionSize)
				return false;
			if (walls.isReachable(wallNode1, wallNode2)) {
				return false;
			}
		}
		return true;
	}

	//Return a list of cells that will be covered by wall from pointA to pointB
	private List<IntVector2> cellsCoveredByWall (IntVector2 pointA, IntVector2 pointB, bool addWallNodeCells = false) {
		List<IntVector2> result = new List<IntVector2>();
		result.Add (pointA);
		result.Add (pointB);

		//Intersections with points on X axis
		for (float x=Mathf.Min (pointA.x, pointB.x)+0.5f; x <= Mathf.Max(pointA.x, pointB.x)-0.5f; x++) {
			//Figure out the line function value at each point
			float z = pointA.z + (x - pointA.x)*(pointB.z - pointA.z)/(pointB.x - pointA.x);
			//add left and right cells if they are not already in the list
			//0.5s are present because the coordinates are located in the middle of the cells rather than in the corners
			IntVector2 leftCell = new IntVector2((int)(x-0.5), Mathf.FloorToInt(z+0.5f));
			IntVector2 rightCell = new IntVector2((int)(x+0.5), Mathf.FloorToInt(z+0.5f));
			if (!result.Contains(leftCell) && withinRect(leftCell, pointA, pointB))
				result.Add (leftCell);
			if (!result.Contains(rightCell) && withinRect(rightCell, pointA, pointB))
				result.Add (rightCell);

			//If y is a corner value, then also add bottom two cells
			if (Mathf.FloorToInt(z+0.5f) == Mathf.CeilToInt(z+0.5f)) {
				leftCell = new IntVector2((int)(x-0.5), Mathf.FloorToInt(z));
				rightCell = new IntVector2((int)(x+0.5), Mathf.FloorToInt(z));
				if (!result.Contains(leftCell) && withinRect(leftCell, pointA, pointB))
					result.Add (leftCell);
				if (!result.Contains(rightCell) && withinRect(rightCell, pointA, pointB))
					result.Add (rightCell);
			}
		}

		//Intersections with points on Z axis
		for (float z=Mathf.Min (pointA.z, pointB.z)+0.5f; z <= Mathf.Max(pointA.z, pointB.z)-0.5f; z++) {
			//Figure out the x value at each point
			float x = pointA.x + (z - pointA.z)*(pointB.x - pointA.x)/(pointB.z - pointA.z);
			//add top and bottom cells if they are not already in the list
			//0.5s are present because the coordinates are located in the middle of the cells rather than in the corners
			IntVector2 topCell = new IntVector2(Mathf.FloorToInt(x+0.5f), (int)(z+0.5));
			IntVector2 bottomCell = new IntVector2(Mathf.FloorToInt(x+0.5f), (int)(z-0.5));
			if (!result.Contains(topCell) && withinRect(topCell, pointA, pointB))
				result.Add (topCell);
			if (!result.Contains(bottomCell) && withinRect(bottomCell, pointA, pointB))
				result.Add (bottomCell);
			
			//If y is a corner value, all 4 cells were already added in the previous loop
			//There is no need to repeat
		}

		//Remove wall node cells from the list if they are not needed
		if (!addWallNodeCells) {
			result.Remove(pointA);
			result.Remove(pointB);
		}

		return result;
	}

	private void placeWall(int indexA, int indexB) {
		IntVector2 coordinates1 = walls.getNode (indexA).coordinates;
		IntVector2 coordinates2 = walls.getNode (indexB).coordinates;
		//Determine the center
		float centerX, centerZ;
		centerX = (coordinates1.x + coordinates2.x) / 2.0f;
		centerZ = (coordinates1.z + coordinates2.z) / 2.0f;

		//Determine the length
		float length;
		length = coordinates1.distance (coordinates2);

		//Place the wall
		Transform wall = Instantiate (wallPrefab) as Transform;
		wall.position = coordinatesFrom2D (centerX, centerZ, 0.5f);
		wall.localScale = new Vector3(wall.localScale.x,wall.localScale.y*length,wall.localScale.z);
		wall.LookAt (coordinatesFrom2D(coordinates2, 0.5f));
		wall.Rotate (90, 0, 0);

		//Mark covered cells as impassable
		List<IntVector2> nodesInBetween = cellsCoveredByWall (coordinates1, coordinates2);
		foreach (IntVector2 cellCoordinates in nodesInBetween) {
			cells[cellCoordinates.x, cellCoordinates.z].isPassable = false;
		}

		//Add a new edge to the graph
		walls.addEdge(indexA,indexB);
	}
}
