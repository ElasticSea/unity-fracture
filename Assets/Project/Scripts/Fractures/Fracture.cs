using System.Linq;
using Project.Scripts.Utils;
using UnityEngine;

namespace Project.Scripts.Fractures
{
    public class Fracture
    {
        private int totalChunks;
        private int seed;
        private Mesh mesh;
        private Material insideMaterial;
        private Material outsideMaterial;
        private float jointBreakForce;
        private float totalMass;

        public Fracture(int totalChunks, int seed, Mesh mesh, Material insideMaterial, Material outsideMaterial, float jointBreakForce, float totalMass)
        {
            this.totalChunks = totalChunks;
            this.seed = seed;
            this.mesh = mesh;
            this.insideMaterial = insideMaterial;
            this.outsideMaterial = outsideMaterial;
            this.jointBreakForce = jointBreakForce;
            this.totalMass = totalMass;
        }

        public void Bake(GameObject go)
        {
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

            Voronoi(fractureTool, nvMesh);

            fractureTool.finalizeFracturing();

            for (var i = 1; i < fractureTool.getChunkCount(); i++)
            {
                var chunk = new GameObject("Chunk" + i);
                chunk.transform.SetParent(go.transform, false);

                Setup(i, chunk, fractureTool);
                joints(chunk, jointBreakForce);
            }
        }

        private void joints(GameObject child, float breakForce)
        {
            var rb = child.GetComponent<Rigidbody>();
            var mesh = child.GetComponent<MeshFilter>().mesh;
        
            var overlaps = mesh.vertices
                .Select(v => child.transform.TransformPoint(v))
                .SelectMany(v => Physics.OverlapSphere(v, .01f))
                .Where(o => o.GetComponent<Rigidbody>())
                .ToSet();

            foreach (var overlap in overlaps)
            { 
                if (overlap.gameObject != child.gameObject)
                {
                    var joint = overlap.gameObject.AddComponent<FixedJoint>();
                    joint.connectedBody = rb;
                    joint.breakForce = breakForce;
                }
            }
        }

        private void Setup(int i, GameObject chunk, NvFractureTool fractureTool)
        {
            var renderer = chunk.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = new[]
            {
                outsideMaterial,
                insideMaterial
            };

            var outside = fractureTool.getChunkMesh(i, false);
            var inside = fractureTool.getChunkMesh(i, true);

            var mesh = outside.toUnityMesh();
            mesh.subMeshCount = 2;
            mesh.SetIndices(inside.getIndexes(), MeshTopology.Triangles, 1);

            var meshFilter = chunk.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;

            var rigibody = chunk.AddComponent<Rigidbody>();
            rigibody.mass = totalMass / totalChunks;

            var mc = chunk.AddComponent<MeshCollider>();
            mc.inflateMesh = true;
            mc.convex = true;
        }

        private void Voronoi(NvFractureTool fractureTool, NvMesh mesh)
        {
            var sites = new NvVoronoiSitesGenerator(mesh);
            sites.uniformlyGenerateSitesInMesh(totalChunks);
            fractureTool.voronoiFracturing(0, sites);
        }
    }
}