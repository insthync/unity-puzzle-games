using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoardControllerSwap : BoardControllerBase
{
    public const float MIN_DRAG_DISTANCE = 10f;
    public const float MIN_DRAG_DISTANCE_MOUSE = 0.3f;
    public float reverseDelay = 0.3f;
    protected Ray oldRay;
    protected Node swappingFrom;
    protected Node swappingTo;
    protected bool canSwap;
    protected bool isEndMovingNode;
    protected bool isCheckingSwapNodes;
    protected bool isClearingNodes;
    protected override void InitNode(bool isDropping)
    {
        base.InitNode(isDropping);
        SetHint();
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

    protected virtual void UpdateInput()
    {
        if (IsPause)
            return;

        FindInputRay();

        if (Input.GetMouseButtonDown(0) && selectedNode == null)
        {
            selectedNode = FindTouchedNode(mDefaultRay);
            if (selectedNode != null && selectedNode.isMoving)
                selectedNode = null;
            oldRay = mDefaultRay;
        }
        if (Input.GetMouseButtonUp(0) && selectedNode != null)
            selectedNode = null;
        if (selectedNode != null)
        {
            if (selectedNode != null && selectedNode.isMoving)
                selectedNode = null;

            float dist = MIN_DRAG_DISTANCE_MOUSE;
            if (Application.platform == RuntimePlatform.Android ||
                Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.WP8Player)
                dist = MIN_DRAG_DISTANCE;

            float minusX = oldRay.origin.x - mDefaultRay.origin.x;
            float minusY = oldRay.origin.y - mDefaultRay.origin.y;
            float diffX = Mathf.Abs(minusX);
            float diffY = Mathf.Abs(minusY);
            if (diffX >= dist || diffY >= dist)
            {
                if (diffX > diffY)
                {
                    if (minusX > 0)
                        SwapNodeLeft(selectedNode);
                    else
                        SwapNodeRight(selectedNode);
                }
                else
                {
                    if (minusY > 0)
                        SwapNodeDown(selectedNode);
                    else
                        SwapNodeUp(selectedNode);
                }
            }
            oldRay = mDefaultRay;
        }
    }

    public override void EndMovingNode()
    {
        base.EndMovingNode();
        isEndMovingNode = true;
        if (!isInitializing)
            CurrentState = State.CLEARING;
    }

    protected void SwapNodeUp(Node node)
    {
        if (node.y + 1 < gridHeight)
        {
            Node target = nodes[node.x, node.y + 1];
            if (!target.isMoving)
            {
                SwapNode(node, target, true);
                CheckSwapNode();
            }
            selectedNode = null;
        }
    }

    protected void SwapNodeDown(Node node)
    {
        if (node.y - 1 >= 0)
        {
            Node target = nodes[node.x, node.y - 1];
            if (!target.isMoving)
            {
                SwapNode(node, target, true);
                CheckSwapNode();
            }
            selectedNode = null;
        }
    }

    protected void SwapNodeLeft(Node node)
    {
        if (node.x - 1 >= 0)
        {
            Node target = nodes[node.x - 1, node.y];
            if (!target.isMoving)
            {
                SwapNode(node, target, true);
                CheckSwapNode();
            }
            selectedNode = null;
        }
    }

    protected void SwapNodeRight(Node node)
    {
        if (node.x + 1 < gridWidth)
        {
            Node target = nodes[node.x + 1, node.y];
            if (!target.isMoving)
            {
                SwapNode(node, target, true);
                CheckSwapNode();
            }
            selectedNode = null;
        }
    }

    protected virtual void SwapNode(Node from, Node to, bool byPlayer = false)
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
        to.StartMoveToTarget(new Vector2(fromX * cellWidth, fromY * cellHeight), swapSpeed);
        swappingFrom = from;
        swappingTo = to;
        if (byPlayer && !isStartMoveNodeOnce)
        {
            if (onStartMoveNode != null)
                onStartMoveNode();
            isStartMoveNodeOnce = true;
        }
    }

    protected virtual void CheckSwapNode()
    {
        isCheckingSwapNodes = true;
        StartCoroutine(CheckSwapNodeRoutine());
    }

    protected IEnumerator CheckSwapNodeRoutine()
    {
        yield return new WaitForSeconds(reverseDelay);
        List<Node[]> validList = new List<Node[]>();
        List<Node> listX = null;
        List<Node> listY = null;
        bool isValid = false;

        listX = CheckMatchHorizontal(swappingFrom);
        listY = CheckMatchVertical(swappingFrom);
        if (listX.Count >= matchCount || listY.Count >= matchCount)
        {
            if (listX.Count > listY.Count)
                validList.Add(listX.ToArray());
            else if (listY.Count > listX.Count)
                validList.Add(listY.ToArray());
            else
            {
                listX.AddRange(listY);
                validList.Add(listX.ToArray());
            }
            isValid = true;
        }
        listX.Clear();
        listY.Clear();
        listX = null;
        listY = null;

        listX = CheckMatchHorizontal(swappingTo);
        listY = CheckMatchVertical(swappingTo);
        if (listX.Count >= matchCount || listY.Count >= matchCount)
        {
            if (listX.Count > listY.Count)
                validList.Add(listX.ToArray());
            else if (listY.Count > listX.Count)
                validList.Add(listY.ToArray());
            else
            {
                listX.AddRange(listY);
                validList.Add(listX.ToArray());
            }
            isValid = true;
        }
        listX.Clear();
        listY.Clear();
        listX = null;
        listY = null;

        isCheckingSwapNodes = false;

        if (!isValid)
            CheckSwapInvalid();
        else
            CheckSwapValid(validList);

        swappingFrom = null;
        swappingTo = null;
        validList.Clear();
        validList = null;
    }

    protected virtual void CheckSwapValid(List<Node[]> validList)
    {
        CurrentState = State.CLEARING;
    }

    protected virtual void CheckSwapInvalid()
    {
        SwapNode(swappingFrom, swappingTo);
    }

    protected void MatchAndClear(List<Node[]> matchList)
    {
        List<Node> moveNodes = new List<Node>();    // Nodes that moving, maybe match 
        for (int i = 0; i < matchList.Count; ++i)
        {
            Node[] nodes = matchList[i];
            int element = nodes[0].element;
            mCurrentCombo++;
            if (onNodeMatch != null)
                onNodeMatch(nodes, element, nodes.Length, mCurrentCombo);
            for (int j = 0; j < nodes.Length; ++j)
                nodes[j].isMatched = true;
        }

        for (int i = 0; i < gridWidth; ++i)
        {
            int numInCol = 0;   // Number which used for increase distance to drop a node
            List<Node> newNodes = new List<Node>(); // New nodes in current column

            for (int j = 0; j < gridHeight; ++j)
            {
                Node node = nodes[i, j];
                if (node.isMatched)
                {
                    node.Kill();
                    numInCol++;
                    // Random a new node
                    int typeIdx = UnityEngine.Random.Range(0, nodePrototypes.Count);
                    Node newNode = Instantiate(nodePrototypes[typeIdx]) as Node;
                    newNode.typeIndex = typeIdx;
                    newNode.boardController = this;
                    newNode.transform.parent = cacheTransform;
                    newNode.gameObject.layer = gameObject.layer;
                    newNode.gameObject.SetActive(true);
                    // Set position of the new node
                    newNode.localPosition = new Vector3(i * cellWidth, (gridHeight + numInCol - 1) * cellHeight, 0);
                    newNodes.Add(newNode);
                    moveNodes.Add(newNode);
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
                        moveNodes.Add(node);
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

        if (moveNodes.Count > 0)
            StartCoroutine(StartDroppingNodesRoutine(moveNodes));
        else
        {
            if (isInitializing)
                isInitializing = false;
            if (isEndMovingNode)
            {
                if (onNoNodeMatches != null)
                    onNoNodeMatches();
                isStartMoveNodeOnce = false;
                isEndMovingNode = false;
            }
            SetHint();
            mCurrentCombo = 0;
            CurrentState = State.IDLE;
        }
    }

    protected IEnumerator StartClearingNodesRoutine()
    {
        yield return 0;
        if (!isClearingNodes)
        {
            isClearingNodes = true;
            List<Node[]> validList = new List<Node[]>();
            List<Node> listX = null;
            List<Node> listY = null;
            List<Node> checkList = new List<Node>();
            for (int i = 0; i < gridWidth; ++i)
            {
                for (int j = 0; j < gridHeight; ++j)
                {
                    Node theNode = nodes[i, j];
                    listX = CheckMatchHorizontal(theNode, checkList);
                    listY = CheckMatchVertical(theNode, checkList);
                    if (listX.Count >= matchCount || listY.Count >= matchCount)
                    {
                        if (listX.Count > listY.Count)
                        {
                            validList.Add(listX.ToArray());
                            checkList.AddRange(listX);
                        }
                        else if (listY.Count > listX.Count)
                        {
                            validList.Add(listY.ToArray());
                            checkList.AddRange(listY);
                        }
                        else
                        {
                            listX.AddRange(listY);
                            validList.Add(listX.ToArray());
                            checkList.AddRange(listX);
                        }
                    }
                    listX.Clear();
                    listY.Clear();
                    listX = null;
                    listY = null;
                }
            }

            if (validList.Count > 0)
                MatchAndClear(validList);
            else
            {
                if (isEndMovingNode)
                {
                    if (onNoNodeMatches != null)
                        onNoNodeMatches();
                    isStartMoveNodeOnce = false;
                    isEndMovingNode = false;
                }
                SetHint();
                mCurrentCombo = 0;
                CurrentState = State.IDLE;
            }

            validList.Clear();
            validList = null;
            isClearingNodes = false;
        }
    }

    protected IEnumerator StartDroppingNodesRoutine(List<Node> moveNodes = null)
    {
        yield return new WaitForSeconds(dropDelay);
        float maxDuration = 0;
        List<Node[]> validList = new List<Node[]>();
        List<Node> checkList = new List<Node>();
        for (int i = 0; i < gridWidth; ++i)
        {
            for (int j = 0; j < gridHeight; ++j)
            {
                Node node = nodes[i, j];
                node.StartMoveToTarget();
                node.isChecked = false;
                node.isMatched = false;

                if (moveNodes != null && moveNodes.Contains(node))
                {
                    if (node.moveDuration > maxDuration)
                        maxDuration = node.moveDuration;

                    List<Node> listX = null;
                    List<Node> listY = null;

                    listX = CheckMatchHorizontal(node, checkList);
                    listY = CheckMatchVertical(node, checkList);
                    if (listX.Count >= matchCount || listY.Count >= matchCount)
                    {
                        if (listX.Count > listY.Count)
                        {
                            validList.Add(listX.ToArray());
                            checkList.AddRange(listX);
                        }
                        else if (listY.Count > listX.Count)
                        {
                            validList.Add(listY.ToArray());
                            checkList.AddRange(listY);
                        }
                        else
                        {
                            listX.AddRange(listY);
                            validList.Add(listX.ToArray());
                            checkList.AddRange(listX);
                        }
                    }
                    listX.Clear();
                    listY.Clear();
                    listX = null;
                    listY = null;
                }
            }
        }

        yield return new WaitForSeconds(maxDuration);
        MatchAndClear(validList);
        validList.Clear();
        validList = null;
    }

    protected void SetHint()
    {
        canSwap = false;
        for (int i = 0; i < gridWidth; ++i)
        {
            for (int j = 0; j < gridHeight; ++j)
            {
                Node realNode = null;
                try
                {
                    realNode = nodes[i, j];
                    HintSwap hint = realNode.gameObject.GetComponent<HintSwap>();
                    if (hint == null)
                        hint = realNode.gameObject.AddComponent<HintSwap>();
                    hint.canMoveUp = false;
                    hint.canMoveDown = false;
                    hint.canMoveLeft = false;
                    hint.canMoveRight = false;
                }
                catch
                {
                    realNode = null;
                    continue;
                }

                if (realNode != null)
                {
                    GameObject temp = new GameObject("tempNode_" + i + "_" + j + "_1");
                    Node tempNode = (Node)temp.AddComponent<Node>();
                    tempNode.x = i;
                    tempNode.y = j;

                    GameObject temp2 = new GameObject("tempNode_" + i + "_" + j + "_2");
                    Node tempNode2 = (Node)temp2.AddComponent<Node>();
                    tempNode2.element = realNode.element;

                    if (i - 1 >= 0)
                    {
                        tempNode.element = nodes[i - 1, j].element;
                        tempNode2.x = i - 1;
                        tempNode2.y = j;
                        if (SetHintNode(tempNode, tempNode2))
                            canSwap = true;
                    }
                    if (i + 1 < gridWidth)
                    {
                        tempNode.element = nodes[i + 1, j].element;
                        tempNode2.x = i + 1;
                        tempNode2.y = j;
                        if (SetHintNode(tempNode, tempNode2))
                            canSwap = true;
                    }
                    if (j - 1 >= 0)
                    {
                        tempNode.element = nodes[i, j - 1].element;
                        tempNode2.x = i;
                        tempNode2.y = j - 1;
                        if (SetHintNode(tempNode, tempNode2))
                            canSwap = true;
                    }
                    if (j + 1 < gridHeight)
                    {
                        tempNode.element = nodes[i, j + 1].element;
                        tempNode2.x = i;
                        tempNode2.y = j + 1;
                        if (SetHintNode(tempNode, tempNode2))
                            canSwap = true;
                    }
                    DestroyObject(temp);
                    DestroyObject(temp2);
                }
            }
        }
        if (!canSwap)
        {
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
            }
        }
    }

    protected bool SetHintNode(Node node, Node node2)
    {
        Node realNode = nodes[node.x, node.y];
        HintSwap hint = (HintSwap)realNode.gameObject.GetComponent<HintSwap>();
        List<Node> listX = CheckMatchHorizontal(node);
        int diffX = node2.x - node.x;
        int diffY = node2.y - node.y;
        if (listX.Count >= matchCount)
        {
            if (diffX == -1)
                hint.canMoveLeft = true;
            else if (diffX == 1)
                hint.canMoveRight = true;
            else if (diffY == -1)
                hint.canMoveDown = true;
            else if (diffY == 1)
                hint.canMoveUp = true;
            listX.Clear();
            listX = null;
            return true;
        }
        List<Node> listY = CheckMatchVertical(node);
        if (listY.Count >= matchCount)
        {
            if (diffX == -1)
                hint.canMoveLeft = true;
            else if (diffX == 1)
                hint.canMoveRight = true;
            else if (diffY == -1)
                hint.canMoveDown = true;
            else if (diffY == 1)
                hint.canMoveUp = true;
            listY.Clear();
            listY = null;
            return true;
        }
        return false;
    }

    protected List<Node> CheckMatchHorizontal(Node node, List<Node> exceptList = null)
    {
        if (node == null)
            return new List<Node>();

        int element = node.element;
        int x = node.x;
        int y = node.y;
        int minX = x - matchCount >= 0 ? x - matchCount : 0;
        int maxX = x + matchCount < gridWidth ? x + matchCount : gridWidth - 1;
        List<Node> list = new List<Node>();
        for (int i = minX; i <= maxX; ++i)
        {
            Node theNode = nodes[i, y];
            if (i == x)
            {
                if (exceptList == null || !exceptList.Contains(node))
                    list.Add(node);
            }
            else
            {
                if (theNode.element == element)
                {
                    if (exceptList == null || !exceptList.Contains(node))
                        list.Add(theNode);
                }
                else
                {
                    if (list.Count >= matchCount)
                        return list;
                    else
                        list.Clear();
                }
            }
        }
        return list;
    }

    protected List<Node> CheckMatchVertical(Node node, List<Node> exceptList = null)
    {
        if (node == null)
            return new List<Node>();

        int element = node.element;
        int x = node.x;
        int y = node.y;
        int minY = y - matchCount >= 0 ? y - matchCount : 0;
        int maxY = y + matchCount < gridHeight ? y + matchCount : gridHeight - 1;
        List<Node> list = new List<Node>();
        for (int i = minY; i <= maxY; ++i)
        {
            Node theNode = nodes[x, i];
            if (i == y)
            {
                if (exceptList == null || !exceptList.Contains(node))
                    list.Add(node);
            }
            else
            {
                if (theNode.element == element)
                {
                    if (exceptList == null || !exceptList.Contains(node))
                        list.Add(theNode);
                }
                else
                {
                    if (list.Count >= matchCount)
                        return list;
                    else
                        list.Clear();
                }
            }
        }
        return list;
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
                    if (!isCheckingSwapNodes)
                    {
                        StartCoroutine(StartClearingNodesRoutine());
                        currentState = value;
                    }
                    else
                        currentState = State.IDLE;
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

    public override bool IsPause
    {
        get
        {
            return base.IsPause;
        }
        set
        {
            base.IsPause = value;
            if (CurrentState == State.CLEARING)
                CurrentState = State.IDLE;  // Re checking again
        }
    }
}
