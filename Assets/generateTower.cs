using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class generateTower : MonoBehaviour {
	[SerializeField] private Transform Sphere;
	[SerializeField] private float brickMass = 5f;
	[SerializeField] private int breakForce = 250;

	[SerializeField] private int bricksWide = 5;
	[SerializeField] private int layers = 5;

	GameObject[,] brickArray;

    // Start is called before the first frame update
    void Start() {

		//holds an array of all bricks
		brickArray = new GameObject[layers,bricksWide];


		//loop through each layer
		for (int l = 0; l < layers; l++) {

			float layerOffset = 0f;
			if (l%2==0) layerOffset = 0.5f;

			for (int b = 0; b < bricksWide; b++) {

			// INSTANTIATE BRICK
			
				var brick = GameObject.CreatePrimitive(PrimitiveType.Cube);
				brick.transform.SetParent(transform);
				brick.transform.position = this.transform.position + new Vector3(0f+layerOffset+b*1f,-0.25f+l*0.5f,-1f);
				brick.transform.localScale = new Vector3(1f,0.5f,0.5f);
				brick.name = "Brick "+l+"-"+b;

				Debug.Log("created "+brick.name);

				var mat = brick.GetComponent<Renderer>().material;
				mat.color = Color.white;

				var rb = brick.AddComponent<Rigidbody>();
				rb.mass = brickMass;
				//rb.isKinematic = true;
				//rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

				//lock the bottom ones
				if (l == 0) {
					rb.isKinematic = true;
					mat.color = Color.green;
				}

				//add brick script
				brick.AddComponent<BrickNode>();

			// ADD FIXED JOINTS

				//as long as its not the first brick in the row, connect it to the last brick placed
				if (b > 0) {
					var joint = brick.gameObject.AddComponent<FixedJoint>();
					joint.connectedBody = brickArray[l,b-1].GetComponent<Rigidbody>();
					joint.breakForce = breakForce;
				}
				//if it's not the bottom row, connect it to the brick below it
				if (l > 0) {
					var joint2 = brick.gameObject.AddComponent<FixedJoint>();
					joint2.connectedBody = brickArray[l-1,b].GetComponent<Rigidbody>();
					joint2.breakForce = breakForce;

					if (b+1<bricksWide) {
						var joint3 = brick.gameObject.AddComponent<FixedJoint>();
						joint3.connectedBody = brickArray[l-1,b+1].GetComponent<Rigidbody>();
						joint3.breakForce = breakForce;
					}
				}


				//add brick to array of bricks
				brickArray[l,b] = brick;
			}
		}


		//holds graph of interconnected pieces
		var graphManager = gameObject.AddComponent<BrickGraphManager>();
		graphManager.Setup(gameObject.GetComponentsInChildren<Rigidbody>());
    }

}
