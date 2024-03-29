using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoardControllerSwapFree : BoardControllerSwap {

	protected override void SwapNode(Node from, Node to, bool byPlayer = false)
	{
		int fromX = from.x;
		int fromY = from.y;
		int toX = to.x;
		int toY = to.y;
		to.x = fromX;
		to.y = fromY;
		from.x = toX;
		from.y = toY;
		nodes[toX, toY] = from;
		nodes[fromX, fromY] = to;
		// Moving "from" node to "to" node position by tween
		from.StartMoveToTarget (new Vector2 (toX * cellWidth, toY * cellHeight), swapSpeed);
		to.StartMoveToTarget (new Vector2 (fromX * cellWidth, fromY * cellHeight), swapSpeed);
		swappingFrom = from;
		swappingTo = to;
		if (byPlayer && !isStartMoveNodeOnce)
		{
			if (onStartMoveNode != null)
				onStartMoveNode();
			isStartMoveNodeOnce = true;
		}
	}

	protected override void CheckSwapNode()
	{
        // Do nothing
	}
}
