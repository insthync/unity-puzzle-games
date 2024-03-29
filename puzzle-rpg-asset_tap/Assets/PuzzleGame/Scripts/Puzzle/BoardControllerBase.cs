using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoardControllerBase : MonoBehaviour
{

    public enum State
    {
        IDLE,
        CLEARING,
        DROPPING,
        DEAD
    }
    public const int MIN_MATCH_COUNT = 2;
    public State currentState { get; protected set; }
    public bool isPause { get; protected set; }
    public int reInitCount { get; protected set; }
    public Transform cacheTransform { get; protected set; }
    public Node selectedNode { get; protected set; }
    public Node[,] nodes { get; protected set; }
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
    public System.Action onNoSolution = null;
    public Camera inputCamera;
    public bool isInitOnAwake = true;
    public bool isDropInitNodes;
    public bool reInitIfCantMatch;
    public Vector3 nodeCenterOffset;
    protected int mCurrentCombo;
    protected Queue<MatchNode> mMatches;
    protected Node mOldNode;
    protected Rect mBound;
    protected Ray mRay;
    protected Ray mDefaultRay;

    protected virtual void Awake()
    {
        currentState = State.IDLE;
        isPause = false;
        reInitCount = 0;
        cacheTransform = transform;
        isInitializing = false;
        isStartMoveNodeOnce = false;
        if (inputCamera == null)
            inputCamera = Camera.main;
        if (isInitOnAwake)
            Init();
    }

    public virtual void Init()
    {
        isInitializing = true;

        if (mMatches == null)
            mMatches = new Queue<MatchNode>();

        StartCoroutine(InitRoutine());
    }

    protected virtual IEnumerator InitRoutine()
    {
        InitGrid();
        InitNode(isDropInitNodes);

        yield return 0;
        CurrentState = State.DROPPING;
    }

    protected virtual void InitGrid()
    {
        matchCount = matchCount < MIN_MATCH_COUNT ? MIN_MATCH_COUNT : matchCount;
        gridWidth = gridWidth < matchCount ? matchCount : gridWidth;
        gridHeight = gridHeight < matchCount ? matchCount : gridHeight;
        // Setup size of board
        nodes = new Node[gridWidth, gridHeight];
    }

    protected virtual void InitNode(bool isDropping)
    {
        // Assigning an nodes which used for playing as matching game
        for (int i = 0; i < gridWidth; ++i)
        {
            for (int j = 0; j < gridHeight; ++j)
            {
                int typeIdx = 0;
                while (true)
                {
                    int horizontalSameType = 0;
                    int verticalSameType = 0;
                    // Randoming new node til not matches
                    typeIdx = Random.Range(0, nodePrototypes.Count);
                    for (int k = j - 1; k >= 0; --k)
                    {
                        Node horizontalPreviousNode = nodes[i, k];
                        if (horizontalPreviousNode.typeIndex == typeIdx)
                            horizontalSameType++;
                        else
                            break;
                    }
                    for (int l = i - 1; l >= 0; --l)
                    {
                        Node verticalPreviousNode = nodes[l, j];
                        if (verticalPreviousNode.typeIndex == typeIdx)
                            verticalSameType++;
                        else
                            break;
                    }
                    if (verticalSameType < matchCount - 1 && horizontalSameType < matchCount - 1)
                        break;
                }
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
                    _node.transform.parent = cacheTransform;
                    _node.gameObject.layer = gameObject.layer;
                    _node.gameObject.SetActive(true);
                    if (isDropping)
                    {
                        _node.localPosition = new Vector3(i * cellWidth, (j + gridHeight) * cellHeight, 0);
                        _node.PrepareMoveTarget(new Vector2(i * cellWidth, j * cellHeight), dropSpeed);
                    }
                    else
                        _node.localPosition = new Vector3(i * cellWidth, j * cellHeight, 0);
                    _node.typeIndex = typeIdx;
                    _node.boardController = this;
                    nodes[i, j] = _node;
                }
            }
        }
        mBound = new Rect(0 + cacheTransform.position.x, 0 + cacheTransform.position.y, gridWidth * cellWidth * cacheTransform.localScale.x, gridHeight * cellHeight * cacheTransform.localScale.y);
        selectedNode = null;

    }

    public virtual void ReInit()
    {
        reInitCount++;
        StartCoroutine(ReInitRoutine());
    }

    public void DestroyNodes()
    {
        for (int i = 0; i < gridWidth; ++i)
        {
            for (int j = 0; j < gridHeight; ++j)
            {
                Node node = nodes[i, j];
                if (node != null)
                    node.Kill();
            }
        }
    }

    protected virtual IEnumerator ReInitRoutine()
    {
        yield return 0;

        bool readyToKills = false;
        while (!readyToKills)
        {
            readyToKills = true;
            for (int i = 0; i < gridWidth; ++i)
            {
                for (int j = 0; j < gridHeight; ++j)
                {
                    Node node = nodes[i, j];
                    if (node != null && node.isMoving)
                    {
                        readyToKills = false;
                        break;
                    }
                }
                if (!readyToKills)
                    break;
            }
        }
        DestroyNodes();
        yield return new WaitForSeconds(clearBoardDelay);

        InitNode(true);
        CurrentState = State.DROPPING;
    }

    protected void FindInputRay()
    {
        mRay = mDefaultRay = inputCamera.ScreenPointToRay(Input.mousePosition);
        if (!mBound.Contains(mRay.origin))
        {
            float rayOriginX = mRay.origin.x;
            float rayOriginY = mRay.origin.y;
            float rayOriginZ = mRay.origin.z;
            if (rayOriginX < mBound.xMin)
                rayOriginX = mBound.xMin + cellWidth / 2;
            if (rayOriginX > mBound.xMax)
                rayOriginX = mBound.xMax - cellWidth / 2;
            if (rayOriginY < mBound.yMin)
                rayOriginY = mBound.yMin + cellHeight / 2;
            if (rayOriginY > mBound.yMax)
                rayOriginY = mBound.yMax - cellHeight / 2;
            mRay.origin = new Vector3(rayOriginX, rayOriginY, rayOriginZ);
        }
    }

    protected Node FindTouchedNode(Ray ray, Node exceptNode = null)
    {
        RaycastHit2D[] hits2D = Physics2D.RaycastAll(ray.origin, ray.direction);
        foreach (RaycastHit2D hit in hits2D)
        {
            Transform hitTransform = hit.transform;
            Node node = hitTransform.gameObject.GetComponent<Node>();
            if (node != null && node != exceptNode)
                return node;
        }

        RaycastHit[] hits3D = Physics.RaycastAll(ray.origin, ray.direction);
        foreach (RaycastHit hit in hits3D)
        {
            Transform hitTransform = hit.transform;
            Node node = hitTransform.gameObject.GetComponent<Node>();
            if (node != null && node != exceptNode)
                return node;
        }

        return null;
    }

    public virtual void EndMovingNode()
    {
        if (isStartMoveNodeOnce)
        {
            if (onEndMovingNode != null)
                onEndMovingNode();
        }
    }

    public virtual void ResetCombo()
    {
        mCurrentCombo = 0;
    }

    public virtual bool IsPause
    {
        get
        {
            return isPause;
        }
        set
        {
            isPause = value;
            if (onPause != null)
                onPause(isPause);
        }
    }

    public virtual State CurrentState
    {
        get
        {
            return currentState;
        }
        set
        {
            if (currentState != value)
                currentState = value;
        }
    }

    public virtual bool IsReadyToValidateNodePosition()
    {
        return currentState == State.IDLE;
    }
}
