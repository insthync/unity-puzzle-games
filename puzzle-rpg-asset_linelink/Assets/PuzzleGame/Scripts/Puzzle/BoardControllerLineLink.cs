using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoardControllerLineLink : BoardControllerBase
{
    public bool canLinkDifferenceNode;
    public GameObject highlightPrototype;
    public LineRenderer linePrototype;
    public Vector3 lineOffset;
    public Vector3 highlightOffset;
    public int canCircuitLinkOn = 0;
    public bool showLinkingLine;
    protected List<GameObject> highlights = new List<GameObject>();
    protected List<LineRenderer> lines = new List<LineRenderer>();
    protected LinkedList<Node> linkedList = new LinkedList<Node>();

    public override void Init()
    {
        base.Init();
        linkedList.Clear();
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
            else
                linkedList.AddLast(selectedNode);
        }
        if (Input.GetMouseButtonUp(0) && selectedNode != null)
        {
            EndMovingNode();
            selectedNode = null;
        }
        if (selectedNode != null)
        {
            if (linkedList == null || linkedList.Last == null)
                return;
            Node _node = FindTouchedNode(mDefaultRay);
            Node _last = linkedList.Last.Value;

            if (linkedList.Count > 1 && selectedNode.Equals(_last))
            {

                if (_node != null && !_node.Equals(_last) &&
                    Mathf.Abs(_node.x - _last.x) <= 1 &&
                    Mathf.Abs(_node.y - _last.y) <= 1)
                {
                    if (linkedList.Last.Previous != null &&
                        _node.Equals(linkedList.Last.Previous.Value))
                        linkedList.RemoveLast();
                }
            }
            else
            {
                if (_node != null && !_node.Equals(_last) &&
                    Mathf.Abs(_node.x - _last.x) <= 1 &&
                    Mathf.Abs(_node.y - _last.y) <= 1)
                {
                    if (linkedList.Last.Previous != null &&
                        _node.Equals(linkedList.Last.Previous.Value))
                        linkedList.RemoveLast();
                    else
                    {
                        if (linkedList.Contains(_node) && !_node.Equals(selectedNode))
                            return;

                        if (_node.Equals(selectedNode) && (linkedList.Count < canCircuitLinkOn || canCircuitLinkOn < 3))
                            return;

                        if (canLinkDifferenceNode || _node.typeIndex == _last.typeIndex)
                        {
                            linkedList.AddLast(_node);
                            if (!isStartMoveNodeOnce)
                            {
                                if (onStartMoveNode != null)
                                    onStartMoveNode();
                                isStartMoveNodeOnce = true;
                            }
                        }
                    }
                }
            }
            UpdateLines();
        }
    }

    protected void UpdateLines()
    {
        List<Vector3[]> points = new List<Vector3[]>();
        List<Vector3> hightlights = new List<Vector3>();
        Vector3 pointA = Vector3.zero;
        Vector3 pointB = Vector3.zero;
        if (selectedNode != null)
        {
            LinkedListNode<Node> linkedListNode = linkedList.First;
            while (linkedListNode != null)
            {
                Node node1 = linkedListNode.Value;
                if (node1 == null)
                {
                    linkedList.Clear();
                    return;
                }
                linkedListNode = linkedListNode.Next;
                if (linkedListNode != null)
                {
                    Node node2 = linkedListNode.Value;

                    pointA = new Vector3(node1.transform.position.x + nodeCenterOffset.x + lineOffset.x,
                                                 node1.transform.position.y + nodeCenterOffset.y + lineOffset.y,
                                                 node1.transform.position.z + nodeCenterOffset.z + lineOffset.z);
                    pointB = new Vector3(node2.transform.position.x + nodeCenterOffset.x + lineOffset.x,
                                                 node2.transform.position.y + nodeCenterOffset.y + lineOffset.y,
                                                 node2.transform.position.z + nodeCenterOffset.z + lineOffset.z);

                    points.Add(new Vector3[] { pointA, pointB });
                    hightlights.Add(new Vector3(pointA.x - lineOffset.x + highlightOffset.x,
                        pointA.y - lineOffset.y + highlightOffset.y,
                        pointA.z - lineOffset.z + highlightOffset.z));
                    if (linkedListNode.Next == null)
                    {
                        hightlights.Add(new Vector3(pointB.x - lineOffset.x + highlightOffset.x,
                            pointB.y - lineOffset.y + highlightOffset.y,
                            pointB.z - lineOffset.z + highlightOffset.z));
                    }
                }
            }

            if (linkedList.Last != null)
            {
                Node lastNode = linkedList.Last.Value;
                if (linkedList.Count > 1 && lastNode.Equals(selectedNode))
                {
                    DrawLines(points);
                    DrawHighlights(hightlights);
                }
                else
                {
                    if (showLinkingLine)
                    {
                        pointA = new Vector3(lastNode.transform.position.x + nodeCenterOffset.x + lineOffset.x,
                                                     lastNode.transform.position.y + nodeCenterOffset.y + lineOffset.y,
                                                     lastNode.transform.position.z + nodeCenterOffset.z + lineOffset.z);
                        pointB = new Vector3(mDefaultRay.origin.x,
                                                     mDefaultRay.origin.y,
                                                     mDefaultRay.origin.z);

                        points.Add(new Vector3[] { pointA, pointB });
                    }
                    DrawLines(points);
                    DrawHighlights(hightlights);
                }
            }
        }
        else
        {
            HideAllHighlights();
            HideAllLines();
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
        }
    }

    protected List<Node> Checks(int posX, int posY, int type)
    {
        if (posX < 0 ||
            posY < 0 ||
            posX >= gridWidth ||
            posY >= gridHeight)
        {
            return null;
        }

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
            list = Checks(posX - 1, posY - 1, type);
            if (list != null)
                currentMatches.AddRange(list);
            list = Checks(posX - 1, posY + 1, type);
            if (list != null)
                currentMatches.AddRange(list);
            list = Checks(posX + 1, posY - 1, type);
            if (list != null)
                currentMatches.AddRange(list);
            list = Checks(posX + 1, posY + 1, type);
            if (list != null)
                currentMatches.AddRange(list);
        }
        return currentMatches;
    }

    protected void HideAllHighlights()
    {
        for (int i = 0; i < highlights.Count; ++i)
        {
            GameObject highlight = highlights[i];
            if (highlight != null)
                highlight.SetActive(false);
        }
    }

    protected void HideAllLines()
    {
        for (int i = 0; i < lines.Count; ++i)
        {
            LineRenderer line = lines[i];
            if (line != null)
                line.gameObject.SetActive(false);
        }
    }

    protected void DrawHighlights(List<Vector3> points)
    {
        if (points == null || highlightPrototype == null)
            return;

        int count = points.Count;
        while (highlights.Count < count)
        {
            GameObject highlight = Instantiate(highlightPrototype) as GameObject;
            if (highlight != null)
            {
                highlight.transform.parent = cacheTransform;
                highlight.layer = gameObject.layer;
                highlights.Add(highlight);
            }
        }

        for (int i = 0; i < highlights.Count; ++i)
        {
            GameObject highlight = highlights[i];
            if (highlight != null)
            {
                if (i >= count)
                    highlight.SetActive(false);
                else
                {
                    highlight.SetActive(true);
                    highlight.transform.position = points[i];
                }
            }
        }
    }

    protected void DrawLines(List<Vector3[]> points)
    {
        if (points == null || linePrototype == null)
            return;

        int lineCount = points.Count;
        while (lines.Count < lineCount)
        {
            LineRenderer line = Instantiate(linePrototype) as LineRenderer;
            if (line != null)
            {
                line.transform.parent = cacheTransform;
                line.gameObject.layer = gameObject.layer;
                line.SetVertexCount(2);
                lines.Add(line);
            }
        }

        for (int i = 0; i < lines.Count; ++i)
        {
            LineRenderer line = lines[i];
            if (line != null)
            {
                if (i >= lineCount)
                    line.gameObject.SetActive(false);
                else
                {
                    line.gameObject.SetActive(true);
                    line.SetPosition(0, points[i][0]);
                    line.SetPosition(1, points[i][1]);
                }
            }
        }
    }

    public override void EndMovingNode()
    {
        base.EndMovingNode();

        if (linkedList.Count >= matchCount)
            MatchByLinkedList();
        else
            isStartMoveNodeOnce = false;
        linkedList.Clear();
        selectedNode = null;
        UpdateLines();
    }

    protected void MatchByLinkedList()
    {
        List<Node> nodes = new List<Node>();
        LinkedListNode<Node> linkedListNode = linkedList.First;
        if (linkedListNode == null)
        {
            linkedList.Clear();
            return;
        }
        int element = linkedListNode.Value.element;
        while (linkedListNode != null)
        {
            Node _node = linkedListNode.Value;
            _node.isMatched = true;
            if (_node.element != element)
            {
                OnNodeMatch(element, nodes.ToArray());
                element = _node.element;
                nodes.Clear();
            }
            nodes.Add(_node);
            linkedListNode = linkedListNode.Next;
        }
        ++mCurrentCombo;
        OnNodeMatch(element, nodes.ToArray());

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

        CurrentState = State.DROPPING;
    }

    protected void OnNodeMatch(int element, Node[] nodes)
    {
        if (nodes.Length == 0)
            return;

        if (onNodeMatch != null)
            onNodeMatch(nodes, element, nodes.Length, mCurrentCombo);
    }

    protected IEnumerator StartClearingNodesRoutine()
    {
        mMatches.Clear();
        mMatches.TrimExcess();

        for (int x = 0; x < gridWidth; ++x)
        {
            for (int y = 0; y < gridHeight; ++y)
            {
                Node node = nodes[x, y];
                if (node.isChecked) // Skip when node is already checked
                    continue;
                List<Node> currentMatches = Matches(x, y, node.typeIndex, true, true);
                if (currentMatches.Count > 0)
                {
                    MatchNode match = new MatchNode(node.element, currentMatches);
                    foreach (Node matchNode in currentMatches)
                        matchNode.isMatched = true;
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

            if (isInitializing)
                isInitializing = false;
            else
            {
                if (onNoNodeMatches != null)
                    onNoNodeMatches();
            }
            CheckAvailable();
            mCurrentCombo = 0;
            CurrentState = State.IDLE;
        }
        else
        {
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
            if (currentState != value)
            {
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
