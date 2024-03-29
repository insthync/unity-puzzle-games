using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoardControllerTap : BoardControllerBase
{
    protected bool isEndMovingNode;
    protected override void InitNode(bool isDropping)
    {
        // Assigning an nodes which used for playing as matching game
        for (int i = 0; i < gridWidth; ++i)
        {
            for (int j = 0; j < gridHeight; ++j)
            {
                int typeIdx = Random.Range(0, nodePrototypes.Count);
                // Assign the generated node into the boards
                // Update the position of the nodes through a board
                Node _node = Instantiate(nodePrototypes[typeIdx]) as Node;
                _node.x = i;
                _node.y = j;
                if (_node == null)
                {
                    nodePrototypes.RemoveAt(typeIdx);
                    j--;
                }
                else
                {
                    _node.boardController = this;
                    _node.transform.parent = cacheTransform;
                    _node.gameObject.layer = gameObject.layer;
                    _node.gameObject.SetActive(true);
                    if (isDropping)
                    {
                        _node.localPosition = new Vector3(i * cellWidth, (j + gridHeight) * cellHeight, 0);
                        _node.PrepareMoveTarget(new Vector2(i * cellWidth, j * cellHeight), dropSpeed);
                    }
                    else
                    {
                        _node.localPosition = new Vector3(i * cellWidth, j * cellHeight, 0);
                    }
                    _node.typeIndex = typeIdx;
                    nodes[i, j] = _node;
                }
            }
        }
        mBound = new Rect(0 + cacheTransform.position.x, 0 + cacheTransform.position.y, gridWidth * cellWidth * cacheTransform.localScale.x, gridHeight * cellHeight * cacheTransform.localScale.y);
        selectedNode = null;
    }

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

    protected void CheckAvailable()
    {
        for (int i = 0; i < gridWidth; ++i)
        {
            for (int j = 0; j < gridHeight; ++j)
            {
                Node node = nodes[i, j];
                node.isChecked = false;
            }
        }

        for (int i = 0; i < gridWidth; ++i)
        {
            for (int j = 0; j < gridHeight; ++j)
            {
                Node node = nodes[i, j];
                if (Checks(i, j, node.typeIndex).Count >= matchCount)
                    return;
            }
        }

        Debug.LogWarning("Can not find solution");
        if (reInitIfCantMatch)
        {
            isInitializing = true;
            ReInit();
        }
        else
        {
            if (onNoSolution != null)
                onNoSolution();
            else
                RandomMergeNodes();
        }
    }

    protected void RandomMergeNodes()
    {
        int rndX = Random.Range(0, gridWidth);
        int rndY = Random.Range(0, gridHeight);
        Node node = nodes[rndX, rndY];
        int type = node.typeIndex;
        int countMatch = 1;
        int countTraversal = 1;
        while (countTraversal < matchCount)
        {
            int randomSide = Random.Range(0, 1) == 1 ? 1 : -1;

            int newX = rndX - (countTraversal * randomSide);
            int newY = rndY - (countTraversal * randomSide);
            if (MergeNodeTo(newX, rndY, type))
            {
                ++countMatch;
                if (countMatch >= matchCount)
                    return;
            }
            if (MergeNodeTo(rndX, newY, type))
            {
                ++countMatch;
                if (countMatch >= matchCount)
                    return;
            }
            if (MergeNodeTo(newX, newY, type))
            {
                ++countMatch;
                if (countMatch >= matchCount)
                    return;
            }

            newX = rndX + (countTraversal * randomSide);
            newY = rndY + (countTraversal * randomSide);
            if (MergeNodeTo(newX, rndY, type))
            {
                ++countMatch;
                if (countMatch >= matchCount)
                    return;
            }
            if (MergeNodeTo(rndX, newY, type))
            {
                ++countMatch;
                if (countMatch >= matchCount)
                    return;
            }
            if (MergeNodeTo(newX, newY, type))
            {
                ++countMatch;
                if (countMatch >= matchCount)
                    return;
            }
            ++countTraversal;
        }
    }

    protected bool MergeNodeTo(int x, int y, int type)
    {
        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
        {
            Node node = Instantiate(nodePrototypes[type]) as Node;
            if (node != null)
            {
                if (nodes[x, y] != null && nodes[x, y].gameObject != null)
                    Destroy(nodes[x, y].gameObject);
                node.x = x;
                node.y = y;
                node.typeIndex = type;
                node.boardController = this;
                node.transform.parent = cacheTransform;
                node.gameObject.layer = gameObject.layer;
                node.gameObject.SetActive(true);
                node.localPosition = new Vector3(x * cellWidth, y * cellHeight, 0);
                nodes[x, y] = node;
                return true;
            }
        }
        return false;
    }

    public override void EndMovingNode()
    {
        base.EndMovingNode();
        mCurrentCombo = 0;
        isEndMovingNode = true;
        if (!isInitializing)
            CurrentState = State.CLEARING;
    }

    protected virtual void UpdateInput()
    {
        if (IsPause)
            return;

        FindInputRay();

        if (Input.GetMouseButtonDown(0) && selectedNode == null)
            TapNode(FindTouchedNode(mDefaultRay));
    }

    protected void TapNode(Node _node)
    {

        if (!isStartMoveNodeOnce)
        {
            if (onStartMoveNode != null)
                onStartMoveNode();
            isStartMoveNodeOnce = true;
        }

        if (_node != null)
        {
            int element = _node.element;

            List<Node> nodes = Matches(_node.x, _node.y, _node.typeIndex);
            if (nodes.Count >= matchCount)
            {
                mCurrentCombo++;
                if (onNodeMatch != null)
                    onNodeMatch(nodes.ToArray(), element, nodes.Count, mCurrentCombo);

                for (int i = 0; i < gridWidth; ++i)
                {
                    int numInCol = 0;   // Number which used for increase distance to drop a node
                    List<Node> newNodes = new List<Node>(); // New nodes in current column

                    for (int j = 0; j < gridHeight; ++j)
                    {
                        Node node = base.nodes[i, j];
                        if (node.isMatched)
                        {
                            node.Kill();
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
                                base.nodes[x, y] = node;
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
                        base.nodes[x, y] = node;
                    }
                }
                mCurrentCombo = 0;
                CurrentState = State.DROPPING;
            }
            else
            {
                for (int i = 0; i < nodes.Count; ++i)
                    nodes[i].isMatched = false;
            }
            nodes.Clear();
            nodes = null;
        }
    }

    protected List<Node> Matches(int posX, int posY, int type)
    {
        if (posX < 0 ||
            posY < 0 ||
            posX >= gridWidth ||
            posY >= gridHeight)
            return null;

        List<Node> currentMatches = new List<Node>();
        Node node = nodes[posX, posY];
        if (!node.isMatched && node.typeIndex == type)
        {
            node.isMatched = true;
            currentMatches.Add(node);
            List<Node> list = null;
            list = Matches(posX - 1, posY, type);
            if (list != null)
                currentMatches.AddRange(list);
            list = Matches(posX + 1, posY, type);
            if (list != null)
                currentMatches.AddRange(list);
            list = Matches(posX, posY - 1, type);
            if (list != null)
                currentMatches.AddRange(list);
            list = Matches(posX, posY + 1, type);
            if (list != null)
                currentMatches.AddRange(list);
        }
        return currentMatches;
    }

    protected List<Node> Checks(int posX, int posY, int type)
    {
        if (posX < 0 ||
            posY < 0 ||
            posX >= gridWidth ||
            posY >= gridHeight)
            return null;

        List<Node> currentMatches = new List<Node>();
        Node node = nodes[posX, posY];
        if (!node.isChecked && node.typeIndex == type)
        {
            node.isChecked = true;
            currentMatches.Add(node);
            List<Node> list = null;
            list = Checks(posX - 1, posY, type);
            if (list != null)
                currentMatches.AddRange(list);
            list = Checks(posX + 1, posY, type);
            if (list != null)
                currentMatches.AddRange(list);
            list = Checks(posX, posY - 1, type);
            if (list != null)
                currentMatches.AddRange(list);
            list = Checks(posX, posY + 1, type);
            if (list != null)
                currentMatches.AddRange(list);
        }
        return currentMatches;
    }

    protected IEnumerator StartDroppingNodesRoutine()
    {
        yield return new WaitForSeconds(dropDelay);
        float maxDuration = 0;
        for (int i = 0; i < gridWidth; ++i)
        {
            for (int j = 0; j < gridHeight; ++j)
            {
                Node node = nodes[i, j];
                node.StartMoveToTarget();
                node.isChecked = false;
                node.isMatched = false;

                if (node.moveDuration > maxDuration)
                    maxDuration = node.moveDuration;
            }
        }

        if (isEndMovingNode)
        {
            if (onNoNodeMatches != null)
                onNoNodeMatches();
            isStartMoveNodeOnce = false;
            isEndMovingNode = false;
        }

        yield return new WaitForSeconds(maxDuration);
        CheckAvailable();
        isInitializing = false;
        CurrentState = State.IDLE;
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
                    StartCoroutine(StartDroppingNodesRoutine());
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
}
