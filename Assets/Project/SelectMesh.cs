using System;
using Project.Scripts.Fractures;
using UnityEngine;

namespace Project
{
    public class SelectMesh : MonoBehaviour
    {
        [SerializeField] private KeyCodeFractureThisPair[] keyCodeFractureThisPairs;

        [Serializable]
        private class KeyCodeFractureThisPair
        {
            public KeyCode KeyCode;
            public FractureThis FractureThis;

            public void Deconstruct(out KeyCode keyCode, out FractureThis fractureThis)
            {
                keyCode = KeyCode;
                fractureThis = FractureThis;
            }
        }

        private void Update()
        {
            foreach (var (keyCode, fractureThis) in keyCodeFractureThisPairs)
            {
                if (Input.GetKeyDown(keyCode))
                {
                    foreach (var chunkGraphManager in FindObjectsOfType<ChunkGraphManager>())
                    {
                        DestroyImmediate(chunkGraphManager.gameObject);
                    }

                    fractureThis.FractureGameobject();
                }
            }
        }
    }
}