using System.Collections.Generic;
using System.Linq;
using Project.Scripts.Utils;
using UnityEngine;
using Random = System.Random;

namespace Project.Scripts.Fractures
{
    public class CreateWalls : MonoBehaviour
    {
        [SerializeField] private Vector3 dimensions =  new Vector3(5,5,.5f);
        [SerializeField] private float distance =1 ;
        [SerializeField] private int count = 1;
        [SerializeField] private Anchor anchor = Anchor.Bottom;
        [SerializeField] private int chunks = 500;
        [SerializeField] private float density = 50;
        [SerializeField] private float internalStrength = 100;
            
        [SerializeField] private Material insideMaterial;
        [SerializeField] private Material outsideMaterial;

        private Random rng = new Random();

        private void Start()
        {
             for (int j = 0; j < count; j++)
            {
                var cubeMesh = MeshUtils.GetCubeMesh(dimensions);

                var fracture = new Fracture(
                    chunks,
                    rng.Next(),
                    cubeMesh,
                    insideMaterial,
                    outsideMaterial,
                    internalStrength,
                    density * (dimensions.x * dimensions.y * dimensions.z)
                );

                var fractured = new GameObject();
                fractured.transform.SetParent(transform);
                fractured.transform.position = Vector3.up * dimensions.y / 2 + Vector3.forward * distance * j;
                fractured.name = "Fractured " + count;
                fracture.Bake(fractured);

                // Mark anchored blocks
                var anchoredBlocks = new HashSet<Collider>();
                var frameWidth = .001f;
                var wallBounds = new Bounds(fractured.transform.position, dimensions);
                if (anchor.HasFlag(Anchor.Left))
                {
                    anchoredBlocks.UnionWith(Physics.OverlapBox(wallBounds.center.SetX(wallBounds.min.x), dimensions.SetX(frameWidth)));
                }
                if (anchor.HasFlag(Anchor.Right))
                {
                    anchoredBlocks.UnionWith(Physics.OverlapBox(wallBounds.center.SetX(wallBounds.max.x), dimensions.SetX(frameWidth)));
                }
                if (anchor.HasFlag(Anchor.Bottom))
                {
                    anchoredBlocks.UnionWith(Physics.OverlapBox(wallBounds.center.SetY(wallBounds.min.y), dimensions.SetY(frameWidth)));
                }
                if (anchor.HasFlag(Anchor.Top))
                {
                    anchoredBlocks.UnionWith(Physics.OverlapBox(wallBounds.center.SetY(wallBounds.max.y), dimensions.SetY(frameWidth)));
                }
                if (anchor.HasFlag(Anchor.Front))
                {
                    anchoredBlocks.UnionWith(Physics.OverlapBox(wallBounds.center.SetZ(wallBounds.min.z), dimensions.SetZ(frameWidth)));
                }
                if (anchor.HasFlag(Anchor.Back))
                {
                    anchoredBlocks.UnionWith(Physics.OverlapBox(wallBounds.center.SetZ(wallBounds.max.z), dimensions.SetZ(frameWidth)));
                }
                foreach (var overlap in anchoredBlocks)
                {
                    var rb = overlap.GetComponent<Rigidbody>();
                    if (rb)
                    {
                        rb.isKinematic = true;
                    }
                }

                var graphManager = fractured.AddComponent<ChunkGraphManager>();
                graphManager.Setup(fractured.gameObject.GetComponentsInChildren<Rigidbody>());
            }
        }
    }
}