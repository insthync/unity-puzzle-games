using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MatchNode {
	public int element { get; protected set; }
	public int quantity { get; protected set; }
	public List<Node> matches { get; protected set; }
	public MatchNode(int element, List<Node> matches)
	{
		this.element = element;
		this.matches = matches;
		this.quantity = matches.Count;
	}

	public void Kill()
	{
		foreach (Node node in matches)
		{
			node.Kill();
		}
		matches.Clear();
	}

	public float GetMaxMoveDuration()
	{
		float maxDuration = 0;
		for (int i = 0; i < matches.Count; ++i)
		{
			Node node = matches[i];
			if (node.moveDuration > maxDuration)
				maxDuration = node.moveDuration;
		}
		return maxDuration;
	}
}