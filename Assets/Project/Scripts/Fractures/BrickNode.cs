using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BrickNode : MonoBehaviour {

	private Rigidbody rb;
	private bool frozen;
	private Vector3 frozenPos;
	private Quaternion frozenRot;
	public bool IsStatic => rb.isKinematic;

	//no idea
	public HashSet<BrickNode> Neighbours = new HashSet<BrickNode>();
	public BrickNode[] NeighboursArray = new BrickNode[0];

	//loopup tables for bricks to joints
	private Dictionary<Joint, BrickNode> JointToChunk = new Dictionary<Joint, BrickNode>();
	private Dictionary<BrickNode, Joint> BrickToJoint = new Dictionary<BrickNode, Joint>();

	public bool HasBrokenLinks { get; private set; }

	//shortcut to search neighbors
	private bool Contains(BrickNode brickNode)	{
		return Neighbours.Contains(brickNode);
	}


	//called by BrickGraphManager on init
	public void Setup()	{
		rb = GetComponent<Rigidbody>();
		Freeze();

		JointToChunk.Clear();
		BrickToJoint.Clear();
		foreach (var joint in GetComponents<Joint>())	{
			var brick = joint.connectedBody.GetComponent<BrickNode>();
			JointToChunk[joint] = brick;
			BrickToJoint[brick] = joint;
		}

		foreach (var brickNode in BrickToJoint.Keys) {
			Neighbours.Add(brickNode);

			if (brickNode.Contains(this) == false)	{
				brickNode.Neighbours.Add(this);
			}
		}

		NeighboursArray = Neighbours.ToArray();
	}


	private void OnJointBreak(float breakForce)	{
		Debug.Log("brokenjoint");
		HasBrokenLinks = true;

		//debug to show the brick has had some joints broken
		gameObject.GetComponent<Renderer>().material.color = Color.red;
	}

	public void CleanBrokenLinks()	{
		var brokenLinks = JointToChunk.Keys.Where(j => j == false).ToList();
		foreach (var link in brokenLinks)		{
			var body = JointToChunk[link];

			JointToChunk.Remove(link);
			BrickToJoint.Remove(body);

			body.Remove(this);
			Neighbours.Remove(body);
		}

		NeighboursArray = Neighbours.ToArray();
		HasBrokenLinks = false;
	}

	private void Remove(BrickNode brickNode)	{
		BrickToJoint.Remove(brickNode);
		Neighbours.Remove(brickNode);
		NeighboursArray = Neighbours.ToArray();
	}


	private void FixedUpdate()	{
		// Kinda hacky, but otherwise the chunks slowly drift apart.
		if (frozen)	{
			transform.position = frozenPos;
			transform.rotation = frozenRot;
		}
	}

	public void Unfreeze()	{
		//Debug.Log("unfroze brick");
		frozen = false;
		rb.constraints = RigidbodyConstraints.None;
		rb.useGravity = true;
		rb.gameObject.layer = LayerMask.NameToLayer("Default");

		//debug to show the brick has been frozen
		gameObject.GetComponent<Renderer>().material.color = Color.yellow;
	}

	private void Freeze()	{
		//Debug.Log("froze brick");
		frozen = true;
		rb.constraints = RigidbodyConstraints.FreezeAll;
		rb.useGravity = false;
		rb.gameObject.layer = LayerMask.NameToLayer("FrozenChunks");
		frozenPos = rb.transform.position;
		frozenRot = rb.transform.rotation;
	}
}
