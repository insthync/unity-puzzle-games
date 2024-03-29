using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CeilAndFloorControls : MonoBehaviour {
	public GameObject ceilObject;
    public Transform blowPoint;
	public Vector2 blowForce;
	public static CeilAndFloorControls instance { get; protected set; }
	private List<PhysicNodeActivator> nodes;
	private bool isBlowing;
	void Awake () {
		instance = this;
		nodes = new List<PhysicNodeActivator> ();
		isBlowing = false;
	}

	void Update()
	{
		if (isBlowing)
		{
			Blow();
		}
	}

	public void AddActivatedNode(PhysicNodeActivator node)
	{
		if (node == null || nodes.Contains(node))
		{
			return;
		}

		nodes.Add (node);
	}

	public void StartBlow()
	{
		isBlowing = true;
	}

	public void StopBlow()
	{
		isBlowing = false;
	}

	public void Blow()
	{
		for (int i = 0; i < nodes.Count; ++i)
		{
			if (nodes[i] != null)
			{
				nodes[i].Blow(blowPoint.position, Random.Range(blowForce.x, blowForce.y));
			}
		}
	}
}
