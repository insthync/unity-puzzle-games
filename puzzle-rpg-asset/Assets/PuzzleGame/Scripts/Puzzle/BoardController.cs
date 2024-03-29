using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoardController : BoardControllerBase {

	protected virtual void Update() {
		if (IsPause || isInitializing)
			return;

		switch (currentState)
		{
		case State.IDLE:
			UpdateInput();
			break;
		}
	}

	protected virtual void UpdateInput() {
		if (IsPause)
			return;

		FindInputRay();

		if (Input.GetMouseButtonDown(0) && selectedNode == null)
		{
			selectedNode = FindTouchedNode(mDefaultRay);
		}

		if (Input.GetMouseButtonUp(0) && selectedNode != null)
		{
			EndMovingNode();
		}

		if (selectedNode != null)
		{
			// Moving, Swapping a selected node
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = -inputCamera.transform.position.z;
            Vector3 screenPos = inputCamera.ScreenToWorldPoint(mousePos);

            selectedNode.transform.position = new Vector3(screenPos.x, screenPos.y, 0) + movingNodeOffset;
            //Node swappingNode = findTouchedNode(ray, _selectedNode);
            // Find node by position for better result
            Vector2 nodeXY = selectedNode.CalculateXYByPosition();
            int x = Mathf.RoundToInt(nodeXY.x);
            int y = Mathf.RoundToInt(nodeXY.y);
            if (x < 0)
            {
                x = 0;
            }
            if (x >= nodes.GetLength(0))
            {
                x = nodes.GetLength(0) - 1;
            }
            if (y < 0)
            {
                y = 0;
            }
            if (y >= nodes.GetLength(1))
            {
                y = nodes.GetLength(1) - 1;
            }
            Node swappingNode = nodes[x, y];
			if (swappingNode != null)
			{
				if (mOldNode != swappingNode) {
					SwapMovingNodes(swappingNode, selectedNode);

					if (!isStartMoveNodeOnce)
					{
						if (onStartMoveNode != null)
						{
							onStartMoveNode();
						}
						isStartMoveNodeOnce = true;
					}
				}
			}
			mOldNode = swappingNode;
		}
	}

	/// <summary>
	/// Call when player leave a pressed node or timeup.
	/// </summary>
	public override void EndMovingNode()
	{
		if (selectedNode == null) {
			return;
		}
		selectedNode.localPosition = new Vector3(selectedNode.localPosition.x, selectedNode.localPosition.y, 0);
		selectedNode.StartMoveToTarget (new Vector2 (selectedNode.x * cellWidth, selectedNode.y * cellHeight), swapSpeed);
		selectedNode = null;
        if (isStartMoveNodeOnce)
        {
            base.EndMovingNode();
            if (!isInitializing)
                CurrentState = State.CLEARING;
		}
	}

    /// <summary>
    /// Call when player move a node to target
    /// node will move from its position to selected node.
    /// </summary>
    protected void SwapMovingNodes(Node from, Node to)
    {
        int fromX = from.x;
        int fromY = from.y;
        int toX = to.x;
        int toY = to.y;
        if (fromX == toX && fromY == toY)
        {
            return;
        }

        int currentX = toX;
        int currentY = toY;
        if (fromX != toX)
        {
            //  Traversal with X and Y axis
            while (currentX != fromX || currentY != fromY)
            {
                Node currentNode = null;
                Node swapNode = null;
                int swapX = currentX;
                int swapY = currentY;
                if (currentX != fromX)
                {
                    if (fromX > currentX)
                    {
                        ++swapX;
                    }
                    else
                    {
                        --swapX;
                    }
                }

                if (fromY != toY)
                {
                    while (currentY != fromY)
                    {
                        if (currentY != fromY)
                        {
                            if (fromY > currentY)
                            {
                                ++swapY;
                            }
                            else
                            {
                                --swapY;
                            }
                        }

                        currentNode = nodes[currentX, currentY];
                        swapNode = nodes[swapX, swapY];
                        SwapMovingNode(swapNode, currentNode);

                        if (currentY != fromY)
                        {
                            if (fromY > currentY)
                            {
                                ++currentY;
                            }
                            else
                            {
                                --currentY;
                            }
                        }
                    }
                }
                else
                {
                    currentNode = nodes[currentX, currentY];
                    swapNode = nodes[swapX, swapY];
                    SwapMovingNode(swapNode, currentNode);
                }

                if (currentX != fromX)
                {
                    if (fromX > currentX)
                    {
                        ++currentX;
                    }
                    else
                    {
                        --currentX;
                    }
                }
            }
        }
        else
        {
            //  Traversal with only Y axis
            while (currentY != fromY)
            {
                Node currentNode = null;
                Node swapNode = null;
                int swapX = currentX;
                int swapY = currentY;
                if (currentY != fromY)
                {
                    if (fromY > currentY)
                    {
                        ++swapY;
                    }
                    else
                    {
                        --swapY;
                    }
                }

                currentNode = nodes[currentX, currentY];
                swapNode = nodes[swapX, swapY];
                SwapMovingNode(swapNode, currentNode);

                if (currentY != fromY)
                {
                    if (fromY > currentY)
                    {
                        ++currentY;
                    }
                    else
                    {
                        --currentY;
                    }
                }
            }
        }
    }

    protected void SwapMovingNode(Node from, Node to)
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
        from.StartMoveToTarget(new Vector2(toX * cellWidth, toY * cellHeight), swapSpeed);
    }

    protected IEnumerator StartClearingNodesRoutine()
	{
		mMatches.Clear();
		mMatches.TrimExcess();
		
		for(int x = 0; x < gridWidth; ++x)
		{
			for(int y = 0; y < gridHeight; ++y)
			{
				Node node = nodes[x, y];
				if(node.isChecked)	// Skip when node is already checked
				{
					continue;
				}
				List<Node> currentMatches = Matches(x, y, node.typeIndex, true, true);
				if(currentMatches.Count > 0)
				{
					MatchNode match = new MatchNode(node.element, currentMatches);
					foreach (Node matchNode in currentMatches)
					{
						matchNode.isMatched = true;
					}
					mMatches.Enqueue(match);
				}
			}
		}

		if (mMatches.Count == 0)
		{
			// Reset values
			for (int x = 0; x < gridWidth; ++x)
			{
				for (int y = 0; y < gridHeight; ++y)
				{
					Node node = nodes[x, y];
					node.isChecked = false;
					node.isMatched = false;
				}
			}
			isStartMoveNodeOnce = false;

			if (isInitializing) {
				isInitializing = false;
			} else {
				if (onNoNodeMatches != null)
				{
					onNoNodeMatches();
				}
			}
			mCurrentCombo = 0;
			CurrentState = State.IDLE;
		}
		else
		{
			for (int i = 0; i < gridWidth; ++i)
			{
				int numInCol = 0;	// Number which used for increase distance to drop a node
				List<Node> newNodes = new List<Node>();	// New nodes in current column

				for (int j = 0; j < gridHeight; ++j)
				{
					Node node = nodes[i, j];
					if (node.isMatched)
					{
						numInCol++;
						// Random a new node
						int typeIdx = Random.Range(0, nodePrototypes.Count);
                        Node newNode = Instantiate(nodePrototypes[typeIdx]) as Node;
                        newNode.typeIndex = typeIdx;
                        newNode.boardController = this;
                        newNode.transform.parent = cacheTransform;
                        newNode.gameObject.layer = gameObject.layer;
                        newNode.gameObject.SetActive(true);
                        // Set position of the new node
                        newNode.localPosition = new Vector3(i * cellWidth, (gridHeight + numInCol - 1) * cellHeight, 0);
						newNodes.Add(newNode);
					}
					else
					{
						// Dropping it to lower space
						if (numInCol > 0)
						{
							int x = i;
							int y = j - numInCol;
							node.x = x;
							node.y = y;
							node.PrepareMoveTarget(new Vector2(x * cellWidth, y * cellHeight), dropSpeed);
							nodes[x, y] = node;
						}
					}
				}

				for (int j = 0; j < numInCol; ++j)
				{
					Node node = newNodes[j];
					int x = i;
					int y = (gridHeight - numInCol + j);
					node.x = x;
					node.y = y;
					node.PrepareMoveTarget(new Vector2(x * cellWidth, y * cellHeight), dropSpeed);
					nodes[x, y] = node;
				}
			}

			while (mMatches.Count > 0)
			{
				yield return new WaitForSeconds(clearDelay);
				mCurrentCombo++;

				MatchNode match = mMatches.Dequeue();

				if (onNodeMatch != null)
				{
					onNodeMatch(match.matches.ToArray(), match.element, match.quantity, mCurrentCombo);
				}

				match.Kill();
				yield return 0;
			}

			CurrentState = State.DROPPING;
		}
	}

	protected List<Node> Matches(int posX, int posY, int type, bool checkHorizontal, bool checkVertical)
	{
		List<Node> currentMatches = new List<Node>();
		List<Node> horizontalMatches = new List<Node>();
		List<Node> verticalMatches = new List<Node>();
		if (checkHorizontal)
		{
			horizontalMatches.Add(nodes[posX, posY]);
			for (int x = posX + 1; x < gridWidth; ++x)
			{
				Node node = nodes[x, posY];
				if (node.typeIndex == type)
				{
					node.isChecked = true;
					horizontalMatches.Add(node);
					currentMatches.AddRange(Matches(x, posY, type, false, true));
				}
				else
				{
					break;
				}
			}
			for (int x = posX - 1; x >= 0; --x)
			{
				Node node = nodes[x, posY];
				if(!node.isChecked && node.typeIndex == type)
				{
					node.isChecked = true;
					horizontalMatches.Add(node);
					currentMatches.AddRange(Matches(x, posY, type, false, true));
				}
				else
				{
					break;
				}
			}
		}
		if (checkVertical)
		{
			verticalMatches.Add(nodes[posX, posY]);
			for(int y = posY + 1; y < gridHeight; ++y)
			{
				Node node = nodes[posX, y];
				if (node.typeIndex == type)
				{
					node.isChecked = true;
					verticalMatches.Add(node);
					currentMatches.AddRange(Matches(posX, y, type, true, false));
				}
				else
				{
					break;
				}
			}
			for (int y = posY - 1; y >= 0; --y)
			{
				Node node = nodes[posX, y];
				if(!node.isChecked && node.typeIndex == type)
				{
					node.isChecked = true;
					verticalMatches.Add(node);
					currentMatches.AddRange(Matches(posX, y, type, true, false));
				}
				else
				{
					break;
				}
			}
		}
		if (horizontalMatches.Count >= matchCount)
		{
			currentMatches.AddRange(horizontalMatches);
		}
		if (verticalMatches.Count >= matchCount)
		{
			currentMatches.AddRange(verticalMatches);
		}
		return currentMatches;
	}

	protected IEnumerator StartDroppingNodesRoutine()
	{
		yield return new WaitForSeconds(dropDelay);
		
		for ( int i = 0; i < gridWidth; ++i)
		{
			for ( int j = 0; j < gridHeight; ++j)
			{
				Node node = nodes[i, j];
				node.StartMoveToTarget();
				node.isChecked = false;
				node.isMatched = false;
			}
		}

		CurrentState = State.CLEARING;
	}

	public override State CurrentState {
		get {
			return currentState;
		}
		set {
			if (currentState != value) {
				currentState = value;
				switch (currentState)
				{
				case State.CLEARING:
					StartCoroutine(StartClearingNodesRoutine());
					break;
				case State.DROPPING:
					StartCoroutine(StartDroppingNodesRoutine());
					break;
				case State.DEAD:
					break;
				}
			}
		}
	}
}