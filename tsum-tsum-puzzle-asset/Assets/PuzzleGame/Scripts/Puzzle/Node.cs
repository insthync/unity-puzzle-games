using UnityEngine;
using System.Collections;
using DG.Tweening;

public class Node : MonoBehaviour
{
    public Transform _transform { get; protected set; }
    public int element;
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
        _transform = transform;
        isChecked = false;
        isMatched = false;
        isMoving = false;
        moveTargetPrepared = false;
        died = false;
    }

    public void PrepareMoveTarget(Vector2 moveTarget, float moveSpeed)
    {
        if (died)
            return;
        this.moveTarget = moveTarget;
        this.moveSpeed = moveSpeed;
        this.moveTargetPrepared = true;
        calculateMoveDuration();
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

    private void calculateMoveDuration()
    {
        float distance = Vector2.Distance(new Vector2(_transform.localPosition.x, _transform.localPosition.y), moveTarget);
        moveDuration = distance / moveSpeed;
    }

    public Vector3 localPosition
    {
        get
        {
            return _transform.localPosition;
        }
        set
        {
            _transform.localPosition = value;
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
        Destroy(gameObject);
    }
}