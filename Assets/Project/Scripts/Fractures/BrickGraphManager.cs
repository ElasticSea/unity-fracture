using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Project.Scripts.Utils;
using UnityEngine;


public class BrickGraphManager : MonoBehaviour{
	private BrickNode[] nodes;

	public void Setup(Rigidbody[] bodies)	{
		nodes = new BrickNode[bodies.Length];
		for (int i = 0; i < bodies.Length; i++)		{
			var node = bodies[i].GetComponent<BrickNode>();
			node.Setup();
			nodes[i] = node;
		}
	}

	private void FixedUpdate()	{
		var runSearch = false;
		foreach (var brokenNodes in nodes.Where(n => n.HasBrokenLinks))		{
			brokenNodes.CleanBrokenLinks();
			runSearch = true;
		}

		if(runSearch) 
			SearchGraph(nodes);
	}


	public void SearchGraph(BrickNode[] objects)	{
		UnityEngine.Debug.Log("search");

		//list of objects where IsStatic == true
		var anchors = objects.Where(o => o.IsStatic).ToList();

		//search the set of bricks
		ISet<BrickNode> search = new HashSet<BrickNode>(objects);
		var index = 0;
		foreach (var o in anchors) {
			if (search.Contains(o))			{
				var subVisited = new HashSet<BrickNode>();
				Traverse(o, search, subVisited);

				search = search.Where(s => subVisited.Contains(s) == false).ToSet();
			}
		}

		//unfreeze all 
		foreach (var sub in search)		{
			sub.Unfreeze();
		}
	}

	private void Traverse(BrickNode o, ISet<BrickNode> search, ISet<BrickNode> visited)	{
		if (search.Contains(o) && visited.Contains(o) == false)		{
			visited.Add(o);

			for (var i = 0; i < o.NeighboursArray.Length; i++)			{
				var neighbour = o.NeighboursArray[i];
				Traverse(neighbour, search, visited);
			}
		}
	}
}
