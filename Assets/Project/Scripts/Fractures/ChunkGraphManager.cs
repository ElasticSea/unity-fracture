using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Project.Scripts.Utils;
using UnityEngine;

namespace Project.Scripts.Fractures
{
    public class ChunkGraphManager : MonoBehaviour
    {
        private ChunkNode[] nodes;

        public void Setup(Rigidbody[] bodies)
        {
            nodes = new ChunkNode[bodies.Length];
            for (int i = 0; i < bodies.Length; i++)
            {
                var node = bodies[i].GetOrAddComponent<ChunkNode>();
                node.Setup();
                nodes[i] = node;
            }
        }
        
        private void FixedUpdate()
        {
            var runSearch = false;
            foreach (var brokenNodes in nodes.Where(n => n.HasBrokenLinks))
            {
                brokenNodes.CleanBrokenLinks();
                runSearch = true;
            }
            
            if(runSearch)
                SearchGraph(nodes);
        }

        private Color[] colors =
        {
            Color.blue, 
            Color.green, 
            Color.magenta, 
            Color.yellow
        };
        
        public void SearchGraph(ChunkNode[] objects)
        {
            var anchors = objects.Where(o => o.IsStatic).ToList();
                
            ISet<ChunkNode> search = new HashSet<ChunkNode>(objects);
            var index = 0;
            foreach (var o in anchors)
            {
                if (search.Contains(o))
                {
                    var subVisited = new HashSet<ChunkNode>();
                    Traverse(o, search, subVisited);
                    var color = colors[index++ % colors.Length];
                    foreach (var sub in subVisited)
                    {
                        sub.Color = color;
                    }
                    search = search.Where(s => subVisited.Contains(s) == false).ToSet();
                }
            }
            foreach (var sub in search)
            {
                sub.Unfreeze();
                sub.Color = Color.black;
            }
        }

        private void Traverse(ChunkNode o, ISet<ChunkNode> search, ISet<ChunkNode> visited)
        {
            if (search.Contains(o) && visited.Contains(o) == false)
            {
                visited.Add(o);

                for (var i = 0; i < o.NeighboursArray.Length; i++)
                {
                    var neighbour = o.NeighboursArray[i];
                    Traverse(neighbour, search, visited);
                }
            }
        }
    }
}