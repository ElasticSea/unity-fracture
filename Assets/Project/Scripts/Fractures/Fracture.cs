using System.Collections.Generic;
using System.Linq;
using Project.Scripts.Utils;
using UnityEngine;

namespace Project.Scripts.Fractures
{
    public static class Fracture
    {
        public static ChunkGraphManager FractureGameObject(GameObject gameObject, Anchor anchor, int seed, int totalChunks,Material insideMaterial, Material outsideMaterial, float jointBreakForce, float density)
        {
            var mesh = GetWorldMesh(gameObject);
            
            NvBlastExtUnity.setSeed(seed);

            var nvMesh = new NvMesh(
                mesh.vertices,
                mesh.normals,
                mesh.uv,
                mesh.vertexCount,
                mesh.GetIndices(0),
                (int) mesh.GetIndexCount(0)
            );

            var fractureTool = new NvFractureTool();
            fractureTool.setRemoveIslands(false);
            fractureTool.setSourceMesh(nvMesh);

            var sites = new NvVoronoiSitesGenerator(nvMesh);
            sites.uniformlyGenerateSitesInMesh(totalChunks);
            fractureTool.voronoiFracturing(0, sites);

            fractureTool.finalizeFracturing();

            var fractureGameObject = new GameObject("Fracture");
            var chunkMass = (mesh.Volume() * density) / totalChunks;
            for (var i = 1; i < fractureTool.getChunkCount(); i++)
            {
                var chunk = new GameObject("Chunk " + i);
                chunk.transform.SetParent(fractureGameObject.transform, false);

                var chunkMesh = ExtractChunkMesh(fractureTool, i);
                Setup(chunk, insideMaterial, outsideMaterial, chunkMesh, chunkMass);
                ConnectTouchingChunks(chunk, jointBreakForce);
            }

            var bounds = gameObject.GetCompositeMeshBounds();
            var anchoredBlocks = AnchoredBlocks(anchor, gameObject.transform, bounds);
            foreach (var overlap in anchoredBlocks)
            {
                var rb = overlap.GetComponent<Rigidbody>();
                if (rb)
                {
                    rb.isKinematic = true;
                }
            }

            var graphManager = fractureGameObject.AddComponent<ChunkGraphManager>();
            graphManager.Setup(fractureGameObject.GetComponentsInChildren<Rigidbody>());
            
            return graphManager;
        }
        
        private static IEnumerable<Collider> AnchoredBlocks(Anchor anchor, Transform meshTransform, Bounds bounds)
        {
            var anchoredBlocks = new HashSet<Collider>();
            var frameWidth = .01f;
            var meshWorldCenter = meshTransform.TransformPoint(bounds.center);
            var meshWorldExtents = bounds.extents.Multiply(meshTransform.lossyScale);
            
            if (anchor.HasFlag(Anchor.Left))
            {
                var center = meshWorldCenter - meshTransform.right * meshWorldExtents.x;
                var halfExtents = meshWorldExtents.Abs().SetX(frameWidth);
                anchoredBlocks.UnionWith(Physics.OverlapBox(center, halfExtents, meshTransform.rotation));
            }
            
            if (anchor.HasFlag(Anchor.Right))
            {
                var center = meshWorldCenter + meshTransform.right * meshWorldExtents.x;
                var halfExtents = meshWorldExtents.Abs().SetX(frameWidth);
                anchoredBlocks.UnionWith(Physics.OverlapBox(center, halfExtents, meshTransform.rotation));
            }
            
            if (anchor.HasFlag(Anchor.Bottom))
            {
                var center = meshWorldCenter - meshTransform.up * meshWorldExtents.y;
                var halfExtents = meshWorldExtents.Abs().SetY(frameWidth);
                anchoredBlocks.UnionWith(Physics.OverlapBox(center, halfExtents, meshTransform.rotation));
            }
            
            if (anchor.HasFlag(Anchor.Top))
            {
                var center = meshWorldCenter + meshTransform.up * meshWorldExtents.y;
                var halfExtents = meshWorldExtents.Abs().SetY(frameWidth);
                anchoredBlocks.UnionWith(Physics.OverlapBox(center, halfExtents, meshTransform.rotation));
            }
            
            if (anchor.HasFlag(Anchor.Front))
            {
                var center = meshWorldCenter - meshTransform.forward * meshWorldExtents.z;
                var halfExtents = meshWorldExtents.Abs().SetZ(frameWidth);
                anchoredBlocks.UnionWith(Physics.OverlapBox(center,  halfExtents, meshTransform.rotation));
            }
            
            if (anchor.HasFlag(Anchor.Back))
            {
                var center = meshWorldCenter + meshTransform.forward * meshWorldExtents.z;
                var halfExtents = meshWorldExtents.Abs().SetZ(frameWidth);
                anchoredBlocks.UnionWith(Physics.OverlapBox(center,  halfExtents, meshTransform.rotation));
            }
            
            return anchoredBlocks;
        }

        private static Mesh ExtractChunkMesh(NvFractureTool fractureTool, int index)
        {
            var outside = fractureTool.getChunkMesh(index, false);
            var inside = fractureTool.getChunkMesh(index, true);
            var chunkMesh = outside.toUnityMesh();
            chunkMesh.subMeshCount = 2;
            chunkMesh.SetIndices(inside.getIndexes(), MeshTopology.Triangles, 1);
            return chunkMesh;
        }

        private static Mesh GetWorldMesh(GameObject gameObject)
        {
            var combineInstances = gameObject
                .GetComponentsInChildren<MeshFilter>()
                .Where(mf => ValidateMesh(mf.mesh))
                .Select(mf => new CombineInstance()
                {
                    mesh = mf.mesh,
                    transform = mf.transform.localToWorldMatrix
                }).ToArray();
            
            var totalMesh = new Mesh();
            totalMesh.CombineMeshes(combineInstances, true);
            return totalMesh;
        }
        
        private static bool ValidateMesh(Mesh mesh)
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

        private static void Setup(GameObject chunk, Material insideMaterial, Material outsideMaterial, Mesh mesh, float mass)
        {
            var renderer = chunk.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = new[]
            {
                outsideMaterial,
                insideMaterial
            };

            var meshFilter = chunk.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;

            var rigibody = chunk.AddComponent<Rigidbody>();
            rigibody.mass = mass;

            var mc = chunk.AddComponent<MeshCollider>();
            mc.inflateMesh = true;
            mc.convex = true;
        }
        
        private static void ConnectTouchingChunks(GameObject chunk, float jointBreakForce, float touchRadius = .01f)
        {
            var rb = chunk.GetComponent<Rigidbody>();
            var mesh = chunk.GetComponent<MeshFilter>().mesh;
        
            var overlaps = mesh.vertices
                .Select(v => chunk.transform.TransformPoint(v))
                .SelectMany(v => Physics.OverlapSphere(v, touchRadius))
                .Where(o => o.GetComponent<Rigidbody>())
                .Distinct();

            foreach (var overlap in overlaps)
            { 
                if (overlap.gameObject != chunk.gameObject)
                {
                    var joint = overlap.gameObject.AddComponent<FixedJoint>();
                    joint.connectedBody = rb;
                    joint.breakForce = jointBreakForce;
                }
            }
        }
    }
}