using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoardControllerLineLinkPhysic : BoardControllerBase {
	public bool canLinkDifferenceNode;
    public int nodeQuantity = 20;
    public Transform[] nodeDropTransforms;
    protected List<Node> dropNodes;
    public GameObject highlightPrototype;
    public LineRenderer linePrototype;
    public Vector3 lineOffset;
    public Vector3 highlightOffset;
    public int canCircuitLinkOn = 0;
	public bool showLinkingLine;
    protected List<GameObject> highlights = new List<GameObject>();
    protected List<LineRenderer> lines = new List<LineRenderer>();
	protected LinkedList<Node> linkedList;
	protected override void Init ()
	{
        if (dropNodes == null)
            dropNodes = new List<Node>();
        dropNodes.Clear();

        lines = new List<LineRenderer>();
        linkedList = new LinkedList<Node>();

        StartCoroutine(DropNodes(nodeQuantity));
	}

    protected IEnumerator DropNodes(int nodeQuantity)
    {

        for (int i = 0; i < nodeQuantity; ++i)
        {
            int typeIdx = Random.Range(0, nodePrototypes.Count);
            Node node = Instantiate(nodePrototypes[typeIdx]) as Node;
            node.typeIndex = typeIdx;
            node.transform.parent = _transform;
            node.transform.position = nodeDropTransforms[Random.Range(0, nodeDropTransforms.Length)].position;
            node.gameObject.layer = gameObject.layer;
            node.gameObject.SetActive(true);
            dropNodes.Add(node);
            yield return new WaitForSeconds(dropDelay);
        }
    }

	protected virtual void Update() {
		if (isPause || isInitializing)
			return;
		
		switch (_currentState)
		{
		case State.IDLE:
			UpdateInput();
			break;
		}
	}
	
	protected virtual void UpdateInput() {
		if (isPause)
			return;
		
		findInputRay();

		if (Input.GetMouseButtonDown(0) && _selectedNode == null)
		{
			_selectedNode = findTouchedNode(defaultRay);
			if (_selectedNode != null && _selectedNode.isMoving)
			{
				_selectedNode = null;
			} else {
				linkedList.AddLast(_selectedNode);
			}
		}
		if (Input.GetMouseButtonUp(0) && _selectedNode != null)
		{
			_selectedNode = null;
			endMovingNode();
		}
		if (_selectedNode != null)
		{
			if (linkedList == null || linkedList.Last == null)
			{
				return;
			}
			Node _node = findTouchedNode(defaultRay);
			Node _last = linkedList.Last.Value;

			if (linkedList.Count > 1 && _selectedNode.Equals(_last))
			{
				
				if (_node != null && !_node.Equals(_last) && 
				    Mathf.Abs(_node.x - _last.x) <= 1 &&
				    Mathf.Abs(_node.y - _last.y) <= 1)
				{
					if (linkedList.Last.Previous != null && 
					    _node.Equals(linkedList.Last.Previous.Value))
					{
						linkedList.RemoveLast();
					}
				}
			} else {
				if (_node != null && !_node.Equals(_last) && 
				    Mathf.Abs(_node.x - _last.x) <= 1 &&
				    Mathf.Abs(_node.y - _last.y) <= 1)
				{
					if (linkedList.Last.Previous != null && 
					    _node.Equals(linkedList.Last.Previous.Value))
					{
						linkedList.RemoveLast();
					} else {
						if (linkedList.Contains(_node) && !_node.Equals(_selectedNode))
							return;

                        if (_node.Equals(_selectedNode) && (linkedList.Count < canCircuitLinkOn || canCircuitLinkOn < 3))
                            return;

						if (canLinkDifferenceNode || _node.typeIndex == _last.typeIndex)
						{
							linkedList.AddLast(_node);
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
        if (_selectedNode != null)
        {
            LinkedListNode<Node> linkedListNode = linkedList.First;
            while (linkedListNode != null)
            {
            	bool isBreak = false;
                Node node1 = linkedListNode.Value;
                linkedListNode = linkedListNode.Next;
                if (linkedListNode != null)
                {
                    Node node2 = linkedListNode.Value;

					try {
						RaycastHit2D[] hits = Physics2D.LinecastAll(node1._transform.position, node2._transform.position);
						var hitEnumerator = hits.GetEnumerator();
						while (hitEnumerator.MoveNext())
						{
							RaycastHit2D hit = (RaycastHit2D)hitEnumerator.Current;
							if (hit.collider.gameObject != node1.gameObject && hit.collider.gameObject != node2.gameObject)
							{
								linkedListNode.List.Remove(node2);
								isBreak = true;
								continue;
							}
						}
					} catch {
						continue;
					}

					if (isBreak)
					{
						continue;
					}
					
                    pointA = new Vector3(node1._transform.position.x + nodeCenterOffset.x + lineOffset.x,
                                                 node1._transform.position.y + nodeCenterOffset.y + lineOffset.y,
                                                 node1._transform.position.z + nodeCenterOffset.z + lineOffset.z);
                    pointB = new Vector3(node2._transform.position.x + nodeCenterOffset.x + lineOffset.x,
                                                 node2._transform.position.y + nodeCenterOffset.y + lineOffset.y,
                                                 node2._transform.position.z + nodeCenterOffset.z + lineOffset.z);

                    points.Add(new Vector3[] { pointA, pointB });
                    hightlights.Add(new Vector3(pointA.x - lineOffset.x + highlightOffset.x,
                        pointA.y - lineOffset.y + highlightOffset.y, 
                        pointA.z - lineOffset.z + highlightOffset.z));
                }
            }

			if (linkedList.Last != null)
			{
	            Node lastNode = linkedList.Last.Value;
	            if (linkedList.Count > 1 && lastNode.Equals(_selectedNode))
	            {
	                DrawLines(points);
	                DrawHighlights(hightlights);
	            }
	            else
	            {
	                if (showLinkingLine)
	                {
	                    pointA = new Vector3(lastNode._transform.position.x + nodeCenterOffset.x + lineOffset.x,
	                                                 lastNode._transform.position.y + nodeCenterOffset.y + lineOffset.y,
	                                                 lastNode._transform.position.z + nodeCenterOffset.z + lineOffset.z);
	                    pointB = new Vector3(defaultRay.origin.x,
	                                                 defaultRay.origin.y,
	                                                 defaultRay.origin.z);

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

    protected void HideAllHighlights()
    {
        for (int i = 0; i < highlights.Count; ++i)
        {
            GameObject highlight = highlights[i];
            if (highlight != null)
            {
                highlight.SetActive(false);
            }
        }
    }

    protected void HideAllLines()
    {
        for (int i = 0; i < lines.Count; ++i)
        {
            LineRenderer line = lines[i];
            if (line != null)
            {
                line.gameObject.SetActive(false);
            }
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
                highlight.transform.parent = _transform;
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
                {
                    highlight.SetActive(false);
                }
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
                line.transform.parent = _transform;
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
                {
                    line.gameObject.SetActive(false);
                }
                else
                {
                    line.gameObject.SetActive(true);
                    line.SetPosition(0, points[i][0]);
                    line.SetPosition(1, points[i][1]);
                }
            }
        }
    }

	public override void endMovingNode()
	{
		base.endMovingNode();
		
		if (linkedList.Count >= matchCount)
		{
			matchByLinkedList();
		} else {
			isStartMoveNodeOnce = false;
		}
		linkedList.Clear();
        UpdateLines();
		currentCombo = 0;
	}

	protected void matchByLinkedList()
	{
		List<Node> nodes = new List<Node>();
		LinkedListNode<Node> linkedListNode = linkedList.First;
		if (linkedListNode == null || linkedListNode.Value == null)
		{
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
        StartCoroutine(ClearNodes(nodes.ToArray()));
	}

	protected void OnNodeMatch(int element, Node[] nodes)
	{
		if (nodes.Length == 0)
			return;

		if (onNodeMatch != null)
		{
			onNodeMatch(nodes, element, nodes.Length, currentCombo);
		}
	}

    protected IEnumerator ClearNodes(Node[] nodes)
    {
        for (int i = 0; i < nodes.Length; ++i)
        {
            Node node = nodes[i];
            if (node != null)
            {
                SpriteRenderer sprite = node.GetComponent<SpriteRenderer>();
                if (sprite != null)
                {
                    sprite.enabled = false;
                }
                MeshRenderer mesh = node.GetComponent<MeshRenderer>();
                if (mesh != null)
                {
                    mesh.enabled = false;
                }
                yield return new WaitForSeconds(clearDelay);
            }
        }
        yield return 0;
        int destroyingCount = 0;
        for (int i = 0; i < nodes.Length; ++i)
        {
            Node node = nodes[i];
            if (node != null)
            {
                GameObject.DestroyObject(node.gameObject);
                ++destroyingCount;
            }
        }
        yield return 0;
        StartCoroutine(DropNodes(destroyingCount));
    }
	
	public override State currentState {
		get {
			return _currentState;
		}
		set {
			if (_currentState != value) {
				_currentState = value;
			}
		}
	}
}
