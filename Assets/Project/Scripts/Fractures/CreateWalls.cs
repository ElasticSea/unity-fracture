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
        [SerializeField] private bool hasFameTop = false;
        [SerializeField] private bool hasFameBottom = true;
        [SerializeField] private bool hasFameRight = false;
        [SerializeField] private bool hasFameLeft = false;
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


                var frameWidth = .001f;
                var top = hasFameTop
                    ? Physics.OverlapBox(fractured.transform.position + Vector3.up * dimensions.y / 2, new Vector3(5, frameWidth, 5))
                    : new Collider[0];
                var right = hasFameRight
                    ? Physics.OverlapBox(fractured.transform.position + Vector3.right * dimensions.x / 2,
                        new Vector3(frameWidth, 5, 5))
                    : new Collider[0];
                var bottom = hasFameBottom
                    ? Physics.OverlapBox(fractured.transform.position + Vector3.down * dimensions.y / 2, new Vector3(5, frameWidth, 5))
                    : new Collider[0];
                var left = hasFameLeft
                    ? Physics.OverlapBox(fractured.transform.position + Vector3.left * dimensions.x / 2, new Vector3(frameWidth, 5, 5))
                    : new Collider[0];


                var overlaps = top
                    .Concat(right)
                    .Concat(bottom)
                    .Concat(left)
                    .Distinct();

                foreach (var overlap in overlaps)
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