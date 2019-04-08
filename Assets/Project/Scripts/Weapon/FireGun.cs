using UnityEngine;

namespace Project.Scripts.Weapon
{
    public class FireGun : MonoBehaviour
    {
        [SerializeField] private Transform barrelEnd;
        [SerializeField] private float radius = 0.1f;
        [SerializeField] private float velocity = 1000f;
        [SerializeField] private float mass = .5f;

        public float Radius
        {
            get => radius;
            set => radius = value;
        }

        public float Velocity
        {
            get => velocity;
            set => velocity = value;
        }

        public float Mass
        {
            get { return mass; }
            set { mass = value; }
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                FireBullet();
            }
        }

        private void FireBullet()
        {
            var bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bullet.transform.position = barrelEnd.position;
            bullet.transform.localScale = Vector3.one * Radius;

            var mat = bullet.GetComponent<Renderer>().material;
            mat.color = Color.red;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", Color.white);

            var rb = bullet.AddComponent<Rigidbody>();
            rb.velocity = transform.forward * Velocity;
            rb.mass = mass;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
    }
}