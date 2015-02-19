﻿using UnityEngine;
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
		if (willGenerateIsland)
			generateIsland (5);
		else
			generateWaterBoundaries (20);
		
		//Put the player at the starting position
		Vector3 startPosition3D = coordinatesFrom2D(startPosition, 0.6f);
		player.transform.position = startPosition3D;
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
			for (int i=currentCell.x-1; i<=currentCell.x+1; i++) {
				for (int j=currentCell.z-1; j<=currentCell.z+1; j++) {
					IntVector2 currentNeighbor = new IntVector2(i,j);
					//We only care about the cells that have not been visited yet
					if (withinMap(currentNeighbor) 
					    && !waterCellList.Contains(currentNeighbor) 
					    && !notWaterCellList.Contains(currentNeighbor)
					    && (currentCell.x == currentNeighbor.x || currentCell.z == currentNeighbor.z)) {
						//Compute probability that it will be a water cell
						float distanceToCenter = currentNeighbor.distance(mapCenter);
						float relativeDistance = distanceToCenter/maxDistanceToCenter;
						//The probability function should map [0,1] (rel.distance) to [0,1] (probability)
						float waterCellProbability = Mathf.Pow((-1/(relativeDistance-2)), 2);

						//print("Looking at cell " + currentNeighbor.x.ToString() + ", " + currentNeighbor.z.ToString() + ".");
						//print("The distance to the center is " + distanceToCenter.ToString());
						//print("The max distance to the center is " + maxDistanceToCenter.ToString());
						//print("The water probability is " + waterCellProbability.ToString());

						//Add the cell to the appropriate list
						if (Random.value < waterCellProbability)
							tempWaterCellQueue.Enqueue(currentNeighbor);
						else
							notWaterCellList.Add (currentNeighbor);
					}
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

	private bool withinMap(IntVector2 coordinates) {
		return 0 <= coordinates.x && coordinates.x < mapLength && 0 <= coordinates.z && coordinates.z < mapWidth;
	}
	
	//Convert coordinates in the map to the coordinates in the space
	private Vector3 coordinatesFrom2D (IntVector2 coordinates, float y) {
		return new Vector3(coordinates.x*tileLength, y, coordinates.z*tileLength);
	}
}
