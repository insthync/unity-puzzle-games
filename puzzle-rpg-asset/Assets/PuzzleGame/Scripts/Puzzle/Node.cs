using UnityEngine;
using System.Collections;
using DG.Tweening;

public class Node : MonoBehaviour
{
    [HideInInspector]
    public BoardControllerBase boardController;
    public int element;
    public GameObject killedEffect;
    public Vector3 effectOffset;
    [HideInInspector]
    public int typeIndex;
    [HideInInspector]
    public int x;
    [HideInInspector]
    public int y;
    [HideInInspector]
    public int level = 1;
    [HideInInspector]
    public bool isChecked = false;  // using by BoardController as triplet checked state
    [HideInInspector]
    public bool isMatched = false;  // using by BoardController as triplet matched state
    public bool isMoving { get; private set; }
    public bool moveTargetPrepared { get; private set; }
    public bool died { get; private set; }
    public float moveDuration { get; private set; }
    private Vector2 moveTarget = Vector2.zero;
    private float moveSpeed = 0;
    private Tweener _tweener;

    protected virtual void Awake()
    {
        isChecked = false;
        isMatched = false;
        isMoving = false;
        moveTargetPrepared = false;
        died = false;
    }

    protected virtual void Update()
    {
        ValidatePosition();
    }

    protected virtual void ValidatePosition()
    {
        if (boardController == null || !boardController.IsReadyToValidateNodePosition())
        {
            return;
        }

        Vector2 validatePosition = CalculatePositionInBoard();
        Vector2 currentPosition = transform.localPosition;
        if (_tweener == null &&
            !validatePosition.Equals(currentPosition) &&
            (boardController.selectedNode == null || boardController.selectedNode != this))
        {
            StartMoveToTarget(validatePosition, boardController.swapSpeed);
        }
    }

    public virtual Vector2 CalculatePositionInBoard()
    {
        return new Vector2(x * boardController.cellWidth, y * boardController.cellHeight);
    }

    public virtual Vector2 CalculateXYByPosition()
    {
        Vector2 currentPosition = transform.localPosition;
        return new Vector2(currentPosition.x / boardController.cellWidth, currentPosition.y / boardController.cellHeight);
    }

    public void PrepareMoveTarget(Vector2 moveTarget, float moveSpeed)
    {
        if (died)
            return;
        this.moveTarget = moveTarget;
        this.moveSpeed = moveSpeed;
        this.moveTargetPrepared = true;
        CalculateMoveDuration();
    }

    public void StartMoveToTarget()
    {
        if (died)
            return;

        if (moveTargetPrepared)
        {
            _tweener = transform.DOLocalMove(moveTarget, moveDuration);
            _tweener.onComplete = OnMovedToTarget;
            isMoving = true;
            moveTargetPrepared = false;
        }
    }

    public void StartMoveToTarget(Vector2 moveTarget, float moveSpeed)
    {
        PrepareMoveTarget(moveTarget, moveSpeed);
        StartMoveToTarget();
    }

    protected void OnMovedToTarget()
    {
        isMoving = false;
    }

    private void CalculateMoveDuration()
    {
        float distance = Vector2.Distance(new Vector2(transform.localPosition.x, transform.localPosition.y), moveTarget);
        moveDuration = distance / moveSpeed;
    }

    public Vector3 localPosition
    {
        get
        {
            return transform.localPosition;
        }
        set
        {
            transform.localPosition = value;
        }
    }

    public void Kill()
    {
        died = true;
        try
        {
            _tweener?.Kill();
        }
        catch
        {
        }
        if (killedEffect != null)
        {
            GameObject effect = Instantiate(killedEffect) as GameObject;
            effect.SetActive(true);
            effect.transform.position = transform.position + effectOffset;
        }
        Destroy(gameObject);
    }
}