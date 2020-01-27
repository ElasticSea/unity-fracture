using System.Linq;
using Project.Scripts.Utils;
using UnityEngine;

namespace Project.Scripts.Fractures
{
    public static class FractureUtils
    {
        public static void ConnectTouchingChunks(GameObject chunk, float jointBreakForce, float touchRadius = .01f)
        {
            var rb = chunk.GetComponent<Rigidbody>();
            var mesh = chunk.GetComponent<MeshFilter>().mesh;
        
            var overlaps = mesh.vertices
                .Select(v => chunk.transform.TransformPoint(v))
                .SelectMany(v => Physics.OverlapSphere(v, touchRadius))
                .Where(o => o.GetComponent<Rigidbody>())
                .ToSet();

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