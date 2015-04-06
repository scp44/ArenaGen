using UnityEngine;
using System.Collections.Generic;

public class WallGraphNode {
	public IntVector2 coordinates;
	public List<WallGraphNode> neighbors;
	public List<WallGraphNode> reachableNodes;
	public int numberInGraph;

	public WallGraphNode(IntVector2 coordinates, int number) {
		this.coordinates = coordinates;
		neighbors = new List<WallGraphNode>();
		reachableNodes = new List<WallGraphNode> ();
		reachableNodes.Add (this);
		numberInGraph = number;
	}
}	

public class WallGraph {
	private List<WallGraphNode> nodes;

	public WallGraph() {
		nodes = new List<WallGraphNode> ();
	}

	public void addNode(IntVector2 coordinates) {
		nodes.Add (new WallGraphNode (coordinates, nodes.Count));
	}

	public void addEdge(int i, int j) {
		//Debug.Log ("Adding a new edge " + i.ToString() + " " + j.ToString());
		//Debug.Log ("Before the new edge nodeA had " + nodes[i].reachableNodes.Count.ToString() + " reachable nodes");
		//Debug.Log ("Before the new edge nodeB had " + nodes[j].reachableNodes.Count.ToString() + " reachable nodes");
		if (!edgeExists(i,j)) {
			//Add the edge
			nodes[i].neighbors.Add (nodes[j]);
			nodes[j].neighbors.Add (nodes[i]);
			//Modify the reachable node list
			for(int k=0; k<nodes[i].reachableNodes.Count; k++) {
				for(int p=0; p<nodes[j].reachableNodes.Count; p++) {
					WallGraphNode nodeA = nodes[i].reachableNodes[k];
					WallGraphNode nodeB = nodes[j].reachableNodes[p];
					if (!nodeA.reachableNodes.Contains(nodeB))
						nodeA.reachableNodes.Add (nodeB);
					if (!nodeB.reachableNodes.Contains(nodeA))
						nodeB.reachableNodes.Add (nodeA);
				}
			}
			nodes[i].reachableNodes.Add (nodes[j]);
			nodes[j].reachableNodes.Add (nodes[i]);
		}
		//Debug.Log ("After the new edge nodeA had " + nodes[i].reachableNodes.Count.ToString() + " reachable nodes");
		//Debug.Log ("After the new edge nodeB had " + nodes[j].reachableNodes.Count.ToString() + " reachable nodes");
	}

	public bool isReachable(int i, int j) {
		return nodes [i].reachableNodes.Contains (nodes[j]);
	}

	public List<int> getNeighbors (int nodeNumber, float distance, bool notConnected=true) {
		List<int> result = new List<int> ();

		//iterate over the graph
		for (int i=0; i<nodes.Count; i++) {
			if (i!=nodeNumber && nodes[i].coordinates.distance(nodes[nodeNumber].coordinates)<=distance
			    && (!notConnected || !edgeExists(i, nodeNumber)))
				result.Add (i);
		}

		return result;
	}

	public bool edgeExists(int i, int j) {
		if (nodes[i].neighbors.Contains(nodes[j]))
			return true;
		return false;
	}

	public float distanceToClosestNode(IntVector2 coordinates) {
		float min = 9999f;
		foreach (WallGraphNode node in nodes) {
			float distance = coordinates.distance(node.coordinates);
			if (distance<min)
				min = distance;
		}
		return min;
	}

	public WallGraphNode getNode(int i) {
		return nodes [i];
	}

	public int getSize() {
		return nodes.Count;
	}
}
