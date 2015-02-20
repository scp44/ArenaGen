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
		cells = new MapCell[mapLength, mapWidth];

		//Generate water around the map
		if (willGenerateIsland)
			generateIsland (20);
		else
			generateWaterBoundaries (20);

		//Pick a location for the player to start
		IntVector2 startPosition;
		do {
			startPosition = getRandomCoordinates (mapLength, mapWidth); 
		} while (cells[startPosition.x, startPosition.z] != null);
		
		//Iterate over locations
		for (int i=0; i<mapLength; i++) {
			for (int j=0; j<mapWidth; j++) {
				//Skip the cell if it is already water
				if (cells[i, j] != null)
					continue;
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
		
		//Put the player at the starting position
		Vector3 startPosition3D = coordinatesFrom2D(startPosition, 0.6f);
		player.transform.position = startPosition3D;
		AstarPath.active.Scan();
	}

	/*
	//Generates water inside the map to make it look like an island
	//Alternative method (starting from center)
	//Needs to be improved
	private void generateIsland_2(int padding) {
		//Create water boundaries
		generateWaterBoundaries (padding);
		
		//Initialize the list of the water cells with all the cells
		List<IntVector2> waterCellList = new List<IntVector2> ();
		//Add the inner layer of coordinates to the queue
		for (int i=0; i<mapLength; i++) {
			for (int j=0; j<mapWidth; j++) {
				IntVector2 coordinates = new IntVector2(i,j);
				waterCellList.Add(coordinates);
			}
		}

		//Cells that are definitely water cells
		List<IntVector2> notGroundCellList = new List<IntVector2> ();
		//length of the half diagonal of the map
		IntVector2 mapCenter = new IntVector2 ((int)(mapLength / 2), (int)(mapWidth / 2));
		float maxDistanceToCenter = mapCenter.distance (new IntVector2(mapLength, mapWidth));

		//Start with map center being the only ground cell
		Queue<IntVector2> groundCells = new Queue<IntVector2> ();
		groundCells.Enqueue (mapCenter);

		//Iterate over queue elements until it is not empty
		while (groundCells.Count>0) {
			//Remove a cell from the queue and remove it from the water cell list
			IntVector2 currentCell = groundCells.Dequeue();
			waterCellList.Remove(currentCell);
			
			//Iterate over all water neighbors
			foreach(IntVector2 currentNeighbor in getNeighbors(currentCell)) {
				//We only care about the cells that have not been visited yet
				if (waterCellList.Contains(currentNeighbor) 
				    && !notGroundCellList.Contains(currentNeighbor)
				    && !groundCells.Contains(currentNeighbor)) {
					//Compute probability that it will be a water cell
					float distanceToCenter = currentNeighbor.distance(mapCenter);
					float relativeDistance = distanceToCenter/maxDistanceToCenter;
					//The probability function should map [0,1] (rel.distance) to [0,1] (probability)
					float waterCellProbability = Mathf.Pow((-1/(relativeDistance-2)), 3);
					if (relativeDistance < 0.3)
						waterCellProbability = 0f;
					
					//Add the cell to the appropriate list
					if (Random.value < waterCellProbability)
						notGroundCellList.Add(currentNeighbor);
					else
						groundCells.Enqueue (currentNeighbor);
				}
			}
		}

		//Put water cells in proper locations
		foreach (IntVector2 waterCell in waterCellList) {
			createCell(MapCellType.waterCell, waterCell);
		}
	}
	*/

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
}
