using System;
using System.Collections.Generic;
using System.Linq;
using Project.Scripts.Utils;
using UnityEngine;
using Random = System.Random;

namespace Project.Scripts.Fractures
{
    public class CreateWalls : MonoBehaviour
    {
        [SerializeField] private MeshFilter wall;
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
            if (ValidateMesh(wall.mesh) == false)
            {
                return;
            }
            
            var cubeMesh = BakeMesh(wall);

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
            fractured.name = "Fractured " + count;
            try
            {
                fracture.Bake(fractured);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            Destroy(wall.gameObject);

            // Mark anchored blocks
            var anchoredBlocks = AnchoredBlocks(wall.transform, wall.mesh.bounds);
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

        private bool ValidateMesh(Mesh mesh)
        {
            if (mesh.isReadable == false)
            {
                Debug.LogError($"Mesh [{mesh}] has to be readable.");
                return false;
            }
            
            if (mesh.vertices == null || mesh.vertices.Length == 0)
            {
                Debug.LogError($"Mesh [{mesh}] does not have any vertices.");
                return false;
            }
            
            if (mesh.uv == null || mesh.uv.Length == 0)
            {
                Debug.LogError($"Mesh [{mesh}] does not have any uvs.");
                return false;
            }

            return true;
        }

        private HashSet<Collider> AnchoredBlocks(Transform meshTransform, Bounds bounds)
        {
            var anchoredBlocks = new HashSet<Collider>();
            var frameWidth = .01f;
            var meshWorldCenter = meshTransform.TransformPoint(bounds.center);
            var meshWorldExtents = bounds.extents.Multiply(meshTransform.lossyScale);
            if (anchor.HasFlag(Anchor.Left))
            {
                OverlapBox(anchoredBlocks, meshWorldCenter - meshTransform.right * meshWorldExtents.x,
                    meshWorldExtents.Abs().SetX(frameWidth), meshTransform.rotation);
            }
            if (anchor.HasFlag(Anchor.Right))
            {
                OverlapBox(anchoredBlocks, meshWorldCenter + meshTransform.right * meshWorldExtents.x,
                    meshWorldExtents.Abs().SetX(frameWidth), meshTransform.rotation);
            }
            if (anchor.HasFlag(Anchor.Bottom))
            {
                OverlapBox(anchoredBlocks, meshWorldCenter - meshTransform.up * meshWorldExtents.y,
                    meshWorldExtents.Abs().SetY(frameWidth), meshTransform.rotation);
            }
            if (anchor.HasFlag(Anchor.Top))
            {
                OverlapBox(anchoredBlocks, meshWorldCenter + meshTransform.up * meshWorldExtents.y,
                    meshWorldExtents.Abs().SetY(frameWidth), meshTransform.rotation);
            }
            if (anchor.HasFlag(Anchor.Front))
            {
                OverlapBox(anchoredBlocks, meshWorldCenter - meshTransform.forward * meshWorldExtents.z,
                    meshWorldExtents.Abs().SetZ(frameWidth), meshTransform.rotation);
            }
            if (anchor.HasFlag(Anchor.Back))
            {
                OverlapBox(anchoredBlocks, meshWorldCenter + meshTransform.forward * meshWorldExtents.z,
                    meshWorldExtents.Abs().SetZ(frameWidth), meshTransform.rotation);
            }
            return anchoredBlocks;
        }

        private void OverlapBox(HashSet<Collider> anchoredBlocks, Vector3 center, Vector3 halfExntents, Quaternion rotation)
        {
            anchoredBlocks.UnionWith(Physics.OverlapBox(center, halfExntents, rotation));

            var testBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
            testBox.transform.position = center;
            testBox.transform.localScale = halfExntents * 2;
            testBox.transform.rotation = wall.transform.rotation;
        }

        private Mesh BakeMesh(MeshFilter meshFilter)
        {
            var oldMesh = meshFilter.mesh;

            var mesh = new Mesh
            {
                vertices = oldMesh.vertices,
                triangles = oldMesh.triangles,
                normals = oldMesh.normals,
                uv = oldMesh.uv
            };

            // Bake vertices
            var vertices = mesh.vertices;
            for (var i = 0; i < vertices.Length; i++)
            {
                vertices[i] = meshFilter.transform.TransformPoint(vertices[i]);
            }
            mesh.vertices = vertices;
            
            // Bake normals
            var normals = mesh.normals;
            for (var i = 0; i < normals.Length; i++)
            {
                normals[i] = meshFilter.transform.TransformDirection(normals[i]);
            }
            mesh.normals = normals;

            return mesh;
        }
    }
}