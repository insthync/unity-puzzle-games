using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoardControllerBase : MonoBehaviour {
	
	public enum State {
		IDLE,
		CLEARING,
		DROPPING,
		DEAD
	}
	public const int MIN_MATCH_COUNT = 2;
	public State _currentState { get; protected set; }
	public bool _isPause { get; protected set; }
	public int _reInitCount { get; protected set; }
	public Transform _transform { get; protected set; }
	public Node _selectedNode { get; protected set; }
	public Node[,] _nodes { get; protected set; }
	public bool isInitializing { get; protected set; }
	public bool isStartMoveNodeOnce { get; protected set; }
	public Vector3 movingNodeOffset;
	public int gridWidth = 6;
	public int gridHeight = 4;
	public float cellWidth = 50;
	public float cellHeight = 50;
	public int matchCount = 3;
	public int levellingCount = 4;
	public float swapSpeed = 0.5f;
	public float dropSpeed = 0.5f;
	public float clearDelay = 0.5f;
	public float dropDelay = 0.5f;
	public float clearBoardDelay = 1;
	public List<Node> nodePrototypes;
	public System.Action onStartMoveNode = null;
	public System.Action<Node[], int, int, int> onNodeMatch = null;
	public System.Action onNoNodeMatches = null;
	public System.Action onEndMovingNode = null;
	public System.Action<bool> onPause = null;
	public Camera inputCamera;
    public bool isDropInitNodes;
    public bool reInitIfCantMatch;
    public Vector3 nodeCenterOffset;
	protected int currentCombo;
	protected Queue<MatchNode> _matches;
	protected Node _oldNode;
	protected Rect _bound;
	protected Ray ray;
	protected Ray defaultRay;

	protected virtual void Awake() {
		_currentState = State.IDLE;
		_isPause = false;
		_reInitCount = 0;
		_transform = transform;
		isInitializing = false;
		isStartMoveNodeOnce = false;
		if (inputCamera == null)
		{
			inputCamera = Camera.main;
		}
		Init();
	}

	protected virtual void Init ()
	{
		isInitializing = true;

		if (_matches == null) 
		{
			_matches = new Queue<MatchNode>();
		}

		StartCoroutine(_Init());
	}
	
	protected virtual IEnumerator _Init() {
		InitGrid();
        InitNode(isDropInitNodes);

		yield return 0;
		currentState = State.DROPPING;
	}

	protected virtual void InitGrid()
	{
		matchCount = matchCount < MIN_MATCH_COUNT ? MIN_MATCH_COUNT : matchCount;
		gridWidth = gridWidth < matchCount ? matchCount : gridWidth;
		gridHeight = gridHeight < matchCount ? matchCount : gridHeight;
		// Setup size of board
		_nodes = new Node[gridWidth, gridHeight];
	}

	protected virtual void InitNode(bool isDropping)
	{
		// Assigning an nodes which used for playing as matching game
		for (int i = 0; i < gridWidth; ++i) {
			for (int j = 0; j < gridHeight; ++j) {
				int typeIdx = 0;
				while (true)
				{
					int horizontalSameType = 0;
					int verticalSameType = 0;
					// Randoming new node til not matches
					typeIdx = UnityEngine.Random.Range(0, nodePrototypes.Count);
					for (int k = j - 1; k >= 0; --k)
					{
						Node horizontalPreviousNode = _nodes[i, k];
						if (horizontalPreviousNode.typeIndex == typeIdx)
						{
							horizontalSameType++;
						}
						else
						{
							break;
						}
					}
					for (int l = i - 1; l >= 0; --l)
					{
						Node verticalPreviousNode = _nodes[l, j];
						if (verticalPreviousNode.typeIndex == typeIdx)
						{
							verticalSameType++;
						}
						else
						{
							break;
						}
					}
					if (verticalSameType < matchCount - 1 && horizontalSameType < matchCount - 1)
					{
						break;
					}
				}
				GameObject nodeObj = Instantiate(nodePrototypes[typeIdx].gameObject) as GameObject;
				// Assign the generated node into the boards
				// Update the position of the nodes through a board
                Node _node = nodeObj.GetComponent<Node>();
				_node.x = i;
				_node.y = j;
				if (_node == null) {
					nodePrototypes.RemoveAt(typeIdx);
					j--;
				} else {
                    _node._transform.parent = _transform;
					_node.gameObject.layer = gameObject.layer;
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
					_nodes[i, j] = _node;
				}
			}
		}
		_bound = new Rect(0 + _transform.position.x, 0 + _transform.position.y, gridWidth * cellWidth * _transform.localScale.x, gridHeight * cellHeight * _transform.localScale.y);
		_selectedNode = null;

	}
	
	public virtual void ReInit()
	{
		Init();
		_reInitCount++;
	}

	protected void findInputRay()
	{
		ray = defaultRay = inputCamera.ScreenPointToRay(Input.mousePosition);
		if (!_bound.Contains(ray.origin))
		{
			float rayOriginX = ray.origin.x;
			float rayOriginY = ray.origin.y;
			float rayOriginZ = ray.origin.z;
			if (rayOriginX < _bound.xMin)
			{
				rayOriginX = _bound.xMin + cellWidth / 2;
			}
			if (rayOriginX > _bound.xMax)
			{
				rayOriginX = _bound.xMax - cellWidth / 2;
			}
			if (rayOriginY < _bound.yMin)
			{
				rayOriginY = _bound.yMin + cellHeight / 2;
			}
			if (rayOriginY > _bound.yMax)
			{
				rayOriginY = _bound.yMax - cellHeight / 2;
			}
			ray.origin = new Vector3(rayOriginX, rayOriginY, rayOriginZ);
		}
	}
	
	protected Node findTouchedNode(Ray ray, Node exceptNode = null) {
		RaycastHit2D[] hits2D = Physics2D.RaycastAll(ray.origin, ray.direction);
		foreach (RaycastHit2D hit in hits2D)
		{
			Transform hitTransform = hit.transform;
			Node node = hitTransform.gameObject.GetComponent<Node>();
			if (node != null && node != exceptNode)
			{
				return node;
			}
		}
		
		RaycastHit[] hits3D = Physics.RaycastAll(ray.origin, ray.direction);
		foreach (RaycastHit hit in hits3D)
		{
			Transform hitTransform = hit.transform;
			Node node = hitTransform.gameObject.GetComponent<Node>();
			if (node != null && node != exceptNode)
			{
				return node;
			}
		}
		
		return null;
	}

	public virtual void endMovingNode()
	{
		if (isStartMoveNodeOnce) {
			if (onEndMovingNode != null)
			{
				onEndMovingNode();
			}
		}
	}

	public virtual void resetCombo()
	{
		currentCombo = 0;
	}
	
	public virtual bool isPause {
		get {
			return _isPause;
		}
		set {
			_isPause = value;
			if (onPause != null)
				onPause(_isPause);
		}
	}

	public virtual State currentState {
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
