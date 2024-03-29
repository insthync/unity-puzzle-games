using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoardControllerSlide : BoardControllerBase
{
    public const float MIN_DRAG_DISTANCE = 10f;
    public const float MIN_DRAG_DISTANCE_MOUSE = 0.3f;
    public bool isEndOnTouchEnd;
    public bool isSnapBackOnNoMatches;
    protected Ray oldRay;
    protected bool isSlideHorizontal;
    protected bool isSlideStart;
    protected bool isClearingNodes;
    protected Vector3 dragOffset;
    protected Vector3 startSlidePosition;
    protected int matchingCount;
    protected Node tempSelectedNode;

    // Update input only when game state is Idle
    protected virtual void Update()
    {
        if (IsPause || isInitializing)
            return;

        switch (currentState)
        {
            case State.IDLE:
                UpdateInput();
                break;
        }
    }

    // Update input from mouse and touch screen
    protected virtual void UpdateInput()
    {
        if (IsPause)
            return;

        // Find touched ray on where cursor position are in the game scene
        FindInputRay();

        // If pressed on mouse button and didn't defined any nodes
        // Find out which node that ray point to and define it as selected node
        if (Input.GetMouseButtonDown(0) && selectedNode == null)
        {
            selectedNode = FindTouchedNode(mDefaultRay);
            if (selectedNode != null)
            {
                // Set startSlidePosition as reference for snap it back when not match (Matching function will be called later)
                startSlidePosition = new Vector2(selectedNode.x, selectedNode.y);
            }
            // Set oldRay as reference to detect distance of movement later
            oldRay = mRay;
            // Set isSlideStart to false to detecting movement later
            isSlideStart = false;
        }

        // If leaving pressed mouse button and selected node have been defined
        // Call function to matching nodes and clear selected node
        if (Input.GetMouseButtonUp(0) && selectedNode != null)
        {
            // isEndOnTouchEnd mean player can't continue playing mathing game
            // If they leave pressed mouse button on their current turn
            // So player can't continue mathing node although time isn't up
            if (isEndOnTouchEnd)
                EndMovingNode();
            else
            {
                ReturnNodePosition(selectedNode);
                selectedNode = null;
            }
            isSlideStart = false;
        }
        // If selected node isn't empty moving selected node 
        // Set oldRay to current frame's ray to detect distance of movement later
        if (selectedNode != null)
        {
            // Movement distance for mouse and touch screen is difference so set it differently
            float dist = MIN_DRAG_DISTANCE_MOUSE;
            if (Application.platform == RuntimePlatform.Android ||
                Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.WP8Player)
                dist = MIN_DRAG_DISTANCE;

            // Moving nodes horizontal or vertical
            if (!isSlideStart)
            {
                // If touched on node but never moves find out it's moving horizontal or vertical
                float minusX = oldRay.origin.x - mRay.origin.x;
                float minusY = oldRay.origin.y - mRay.origin.y;
                float diffX = Mathf.Abs(minusX);
                float diffY = Mathf.Abs(minusY);
                if (diffX >= dist || diffY >= dist)
                {
                    if (diffX > diffY)
                        isSlideHorizontal = true;
                    else
                        isSlideHorizontal = false;
                    isSlideStart = true;
                }
            }
            else
            {
                // After found how nodes moving (Horizontal or Vertical)
                // Moving them following selected node
                float distX = mRay.origin.x - oldRay.origin.x;
                float distY = mRay.origin.y - oldRay.origin.y;
                if (isSlideHorizontal)
                    MoveNodeHorizontal(selectedNode.y, distX);
                else
                    MoveNodeVertical(selectedNode.x, distY);
            }
            // Set oldRay as reference to detect distance of movement later
            oldRay = mRay;
        }
    }

    // EndMovingNode the function will be called when player leave selected node or timeup
    public override void EndMovingNode()
    {
        if (selectedNode != null)
        {
            // Keep selected node as reference to set nodes position later
            tempSelectedNode = selectedNode;
            // Move nodes to position where them should be
            ReturnNodePosition(selectedNode);
            selectedNode = null;
        }
        // isStartMoveNodeOnce will be set to true when moving nodes horizontal or vertical first time
        // It will be set in functions MoveNodeHorizontal() and MoveNodeVertical()
        if (isStartMoveNodeOnce)
        {
            // Call base function for events
            base.EndMovingNode();
            // Set state to clearing to matching nodes for score and spawn new nodes to fulfill the board
            if (!isInitializing)
                CurrentState = State.CLEARING;
        }
    }

    // ReturnNodePosition() function to move nodes to position where them should be in the board
    // By tweening them to position that have been set
    protected void ReturnNodePosition(Node selectedNode)
    {
        if (isSlideHorizontal)
        {
            for (int i = 0; i < gridWidth; ++i)
            {
                Node node = nodes[i, selectedNode.y];
                node.StartMoveToTarget(new Vector2(i * cellWidth, selectedNode.y * cellHeight), swapSpeed);
            }
        }
        else
        {
            for (int i = 0; i < gridHeight; ++i)
            {
                Node node = nodes[selectedNode.x, i];
                node.StartMoveToTarget(new Vector2(selectedNode.x * cellWidth, i * cellHeight), swapSpeed);
            }
        }
    }

    // ReturnStartSlidePosition() function to move nodes back to position before movement
    protected void ReturnStartSlidePosition(Node selectedNode)
    {
        Node[,] applyNodes = new Node[gridWidth, gridHeight];
        for (int i = 0; i < gridWidth; ++i)
        {
            for (int j = 0; j < gridHeight; ++j)
                applyNodes[i, j] = nodes[i, j];
        }
        int moveCount = 0;
        if (isSlideHorizontal)
        {
            moveCount = selectedNode.x - (int)startSlidePosition.x;
            bool isMoveLeft = moveCount > 0;
            for (int i = 0; i < gridWidth; ++i)
            {
                Node node = nodes[i, selectedNode.y];
                int targetX = i - moveCount;
                if (targetX < 0)
                    targetX = gridWidth + targetX;
                if (targetX >= gridWidth)
                    targetX = targetX - gridWidth;
                node.x = targetX;
                node.transform.localPosition = new Vector3(targetX * cellWidth, selectedNode.y * cellHeight, node.transform.localPosition.z);
                applyNodes[targetX, selectedNode.y] = node;
            }
        }
        else
        {
            moveCount = selectedNode.y - (int)startSlidePosition.y;
            bool isMoveDown = moveCount > 0;
            for (int i = 0; i < gridHeight; ++i)
            {
                Node node = nodes[selectedNode.x, i];
                int targetY = i - moveCount;
                if (targetY < 0)
                    targetY = gridHeight + targetY;
                if (targetY >= gridHeight)
                    targetY = targetY - gridHeight;
                node.y = targetY;
                node.transform.localPosition = new Vector3(selectedNode.x * cellWidth, targetY * cellHeight, node.transform.localPosition.z);
                applyNodes[selectedNode.x, targetY] = node;
            }
        }
        nodes = applyNodes;
    }

    // MoveNodeHorizontal() function to move nodes following selected node movement horizontally
    protected void MoveNodeHorizontal(int y, float offsetX)
    {
        float minPos = float.MaxValue;
        float maxPos = float.MinValue;
        Node[] movedNodes = new Node[gridWidth];
        for (int i = 0; i < gridWidth; ++i)
        {
            Node node = nodes[i, y];
            if (node != null)
            {
                float newPos = node.transform.localPosition.x + offsetX;
                node.transform.localPosition = new Vector3(newPos,
                                                            node.transform.localPosition.y,
                                                            node.transform.localPosition.z);
                int oldX = node.x;
                int newX = oldX;
                if (offsetX > 0)
                {
                    if (Mathf.Abs(newPos - (i * cellWidth)) > cellWidth / 2)
                        newX++;
                }
                if (offsetX < 0)
                {
                    if (Mathf.Abs(newPos - (i * cellWidth)) > cellWidth / 2)
                        newX--;
                }
                if (newX < 0)
                    newX = gridWidth - 1;
                else if (newX >= gridWidth)
                    newX = 0;
                if (newX != oldX)
                {
                    node.x = newX;
                    movedNodes[newX] = node;
                    if (!isStartMoveNodeOnce)
                    {
                        if (onStartMoveNode != null)
                            onStartMoveNode();
                        isStartMoveNodeOnce = true;
                    }
                }
                if (minPos > newPos)
                    minPos = newPos;
                if (maxPos < newPos)
                    maxPos = newPos;
            }
        }
        for (int i = 0; i < gridWidth; ++i)
        {
            Node node = movedNodes[i];
            if (node == null)
                return;

            if (offsetX > 0 && i == 0)
            {
                // Left
                node.transform.localPosition = new Vector3(minPos - cellWidth,
                                                            node.transform.localPosition.y,
                                                            node.transform.localPosition.z);
            }

            if (offsetX < 0 && i == gridWidth - 1)
            {
                // Right
                node.transform.localPosition = new Vector3(maxPos + cellWidth,
                                                            node.transform.localPosition.y,
                                                            node.transform.localPosition.z);
            }
            nodes[i, y] = node;
        }
    }

    // MoveNodeVertical() function to move nodes following selected node movement vertically
    protected void MoveNodeVertical(int x, float offsetY)
    {
        float minPos = float.MaxValue;
        float maxPos = float.MinValue;
        Node[] movedNodes = new Node[gridHeight];
        for (int i = 0; i < gridHeight; ++i)
        {
            Node node = nodes[x, i];
            if (node != null)
            {
                float newPos = node.transform.localPosition.y + offsetY;
                node.transform.localPosition = new Vector3(node.transform.localPosition.x,
                                                            newPos,
                                                            node.transform.localPosition.z);
                int oldY = node.y;
                int newY = oldY;
                if (offsetY > 0)
                {
                    if (Mathf.Abs(newPos - (i * cellHeight)) > cellHeight / 2)
                        newY++;
                }
                if (offsetY < 0)
                {
                    if (Mathf.Abs(newPos - (i * cellHeight)) > cellHeight / 2)
                        newY--;
                }
                if (newY < 0)
                    newY = gridHeight - 1;
                else if (newY >= gridHeight)
                    newY = 0;
                if (newY != oldY)
                {
                    node.y = newY;
                    movedNodes[newY] = node;
                    if (!isStartMoveNodeOnce)
                    {
                        if (onStartMoveNode != null)
                            onStartMoveNode();
                        isStartMoveNodeOnce = true;
                    }
                }
                if (minPos > newPos)
                    minPos = newPos;
                if (maxPos < newPos)
                    maxPos = newPos;
            }
        }
        for (int i = 0; i < gridHeight; ++i)
        {
            Node node = movedNodes[i];
            if (node == null)
                return;

            if (offsetY > 0 && i == 0)
            {
                // Up
                node.transform.localPosition = new Vector3(node.transform.localPosition.x,
                                                            minPos - cellHeight,
                                                            node.transform.localPosition.z);
            }

            if (offsetY < 0 && i == gridHeight - 1)
            {
                // Down
                node.transform.localPosition = new Vector3(node.transform.localPosition.x,
                                                            maxPos + cellHeight,
                                                            node.transform.localPosition.z);
            }
            nodes[x, i] = node;
        }
    }

    // This function will be call after state set to Clearing
    // To mathing nodes for score and clear them
    protected IEnumerator StartClearingNodesRoutine()
    {
        yield return 0;
        // Check if this function never be called before to avoid bugs
        if (!isClearingNodes)
        {
            isClearingNodes = true;
            // Clear matching nodes array to fill with mathing nodes for current turn later
            mMatches.Clear();
            mMatches.TrimExcess();

            // Find out matching node by every nodes
            for (int x = 0; x < gridWidth; ++x)
            {
                for (int y = 0; y < gridHeight; ++y)
                {
                    Node node = nodes[x, y];
                    if (node.isChecked) // Skip when node is already checked
                        continue;
                    // Find mathing nodes by function Matches()
                    List<Node> currentMatches = Matches(x, y, node.typeIndex, true, true);
                    // Create MatchNode arrays and Add to mMatches to destroying and calling an events later
                    if (currentMatches.Count > 0)
                    {
                        MatchNode match = new MatchNode(node.element, currentMatches);
                        foreach (Node matchNode in currentMatches)
                            matchNode.isMatched = true;
                        mMatches.Enqueue(match);
                    }
                }
            }

            // Collecing amount of matching node for combo events
            matchingCount += mMatches.Count;

            if (mMatches.Count == 0)
            {
                // If no any matches nodes end this turn and set state to Idle

                // Reset nodes values
                for (int x = 0; x < gridWidth; ++x)
                {
                    for (int y = 0; y < gridHeight; ++y)
                    {
                        Node node = nodes[x, y];
                        node.isChecked = false;
                        node.isMatched = false;
                    }
                }

                // Reset board values
                isStartMoveNodeOnce = false;

                if (isInitializing)
                    isInitializing = false;
                else
                {
                    if (matchingCount <= 0 && isSnapBackOnNoMatches)
                    {
                        ReturnStartSlidePosition(tempSelectedNode);
                        ReturnNodePosition(tempSelectedNode);
                    }

                    if (onNoNodeMatches != null)
                        onNoNodeMatches();
                }
                mCurrentCombo = 0;
                matchingCount = 0;
                CurrentState = State.IDLE;
            }
            else
            {
                // If there are mathing nodes destroy them and spawn new nodes

                // Loop for spawn new nodes but not be dropped yet
                // They will be dropped when state chaged to Dropping
                for (int i = 0; i < gridWidth; ++i)
                {
                    int numInCol = 0;   // Number which used for increase distance to drop a node
                    List<Node> newNodes = new List<Node>(); // New nodes in current column

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

                // Destroying mathing nodes and call for events (onNodeMatch())
                while (mMatches.Count > 0)
                {
                    yield return new WaitForSeconds(clearDelay);
                    mCurrentCombo++;

                    MatchNode match = mMatches.Dequeue();

                    if (onNodeMatch != null)
                        onNodeMatch(match.matches.ToArray(), match.element, match.quantity, mCurrentCombo);

                    match.Kill();
                    yield return 0;
                }

                // Mathing all nodes then call for dropping new nodes
                CurrentState = State.DROPPING;
            }
            isClearingNodes = false;
        }
    }

    // Matches() the function to find matching nodes
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
                    break;
            }
            for (int x = posX - 1; x >= 0; --x)
            {
                Node node = nodes[x, posY];
                if (!node.isChecked && node.typeIndex == type)
                {
                    node.isChecked = true;
                    horizontalMatches.Add(node);
                    currentMatches.AddRange(Matches(x, posY, type, false, true));
                }
                else
                    break;
            }
        }
        if (checkVertical)
        {
            verticalMatches.Add(nodes[posX, posY]);
            for (int y = posY + 1; y < gridHeight; ++y)
            {
                Node node = nodes[posX, y];
                if (node.typeIndex == type)
                {
                    node.isChecked = true;
                    verticalMatches.Add(node);
                    currentMatches.AddRange(Matches(posX, y, type, true, false));
                }
                else
                    break;
            }
            for (int y = posY - 1; y >= 0; --y)
            {
                Node node = nodes[posX, y];
                if (!node.isChecked && node.typeIndex == type)
                {
                    node.isChecked = true;
                    verticalMatches.Add(node);
                    currentMatches.AddRange(Matches(posX, y, type, true, false));
                }
                else
                    break;
            }
        }
        if (horizontalMatches.Count >= matchCount)
            currentMatches.AddRange(horizontalMatches);
        if (verticalMatches.Count >= matchCount)
            currentMatches.AddRange(verticalMatches);
        return currentMatches;
    }

    // This function will be call after state set to Dropping
    // To fulfilling nodes in the board
    // Make fulfilling nodes moving to position it should be by tweener
    protected IEnumerator StartDroppingNodesRoutine()
    {
        yield return new WaitForSeconds(dropDelay);

        for (int i = 0; i < gridWidth; ++i)
        {
            for (int j = 0; j < gridHeight; ++j)
            {
                Node node = nodes[i, j];
                node.StartMoveToTarget();
                node.isChecked = false;
                node.isMatched = false;
            }
        }

        CurrentState = State.CLEARING;
    }

    public override State CurrentState
    {
        get
        {
            return currentState;
        }
        set
        {
            switch (value)
            {
                case State.CLEARING:
                    StartCoroutine(StartClearingNodesRoutine());
                    currentState = value;
                    break;
                case State.DROPPING:
                    if (currentState != value)
                    {
                        StartCoroutine(StartDroppingNodesRoutine());
                        currentState = value;
                    }
                    break;
                default:
                    currentState = value;
                    break;
            }
        }
    }

    public override bool IsReadyToValidateNodePosition()
    {
        return false;
    }
}
