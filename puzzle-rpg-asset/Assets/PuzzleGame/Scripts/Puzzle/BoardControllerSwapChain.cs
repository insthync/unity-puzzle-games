using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoardControllerSwapChain : BoardControllerSwap {

	protected override void SwapNode(Node from, Node to, bool byPlayer = false)
	{
		if (byPlayer && (from.isMatched || to.isMatched))
			return;
		base.SwapNode(from, to, byPlayer);
	}

	protected override void CheckSwapValid (List<Node[]> validList)
	{
		mCurrentCombo++;
		for (int i = 0; i < validList.Count; ++i)
		{
			Node[] nodes = validList[i];
			for (int j = 0; j < nodes.Length; ++j)
				nodes[j].isMatched = true;
		}
	}
}
