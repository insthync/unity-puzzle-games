using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof (SpriteRenderer))]
public class SpriteHelper : MonoBehaviour
{
	public Sprite defaultSprite;
	public Transform myTransform { get; protected set; }
	public SpriteRenderer mySpriteRenderer { get; protected set; }
	protected bool _flipHorizontal;
	protected bool _flipVertical;
	protected Vector2 _position = Vector2.zero;
	protected float _scale = 1.0f;
	
	public int depth;
	public bool spriteSideIsLeftToRight = false;	// user can set default sprite side

	private static int spriteHelperTweenId = 0;
	protected string iTweenMoveName;

	public bool isMoving { get; protected set; }

	// Animation
	public Dictionary<string, Sprite[]> animationSprites;
	public Sprite[] currentAnimationSprites;
	public string currentAnimationName { get; protected set; }
	public int currentAnimationFrame { get; protected set; }
	public bool isPlayingAnimation { get; protected set; }
	public int animationSpeed;
	public int framesPerSecond;
	private bool _animationStopped;
	private float _loopCount;
	private int _playedCount;
	private float _elapsedTime;
	private float _totalElapsedTime;
	private float _secondsPerFrame;
	private float _duration;
	private float _totalDuration;

	// Events
	public System.Action<SpriteHelper> onMovedToTarget;
	public System.Action<SpriteHelper, int> onAnimatingFrame;
	public System.Action<SpriteHelper> onAnimationFinish;

	void Awake()
	{
		++spriteHelperTweenId;
		iTweenMoveName = "_itween_" + spriteHelperTweenId;
		myTransform = transform;
		mySpriteRenderer = GetComponent<SpriteRenderer>();
		if (defaultSprite != null)
		{
			mySpriteRenderer.sprite = defaultSprite;
		}
		isMoving = false;
		isPlayingAnimation = false;
		_animationStopped = false;
	}

	void Update()
	{
		if (_animationStopped)
			return;

		if (isPlayingAnimation && currentAnimationSprites != null)
		{
			bool isComplete = false;
			float deltaTime = Time.deltaTime;
			if (animationSpeed <= 0)
			{
				animationSpeed = 1;
			}
			deltaTime *= animationSpeed;

			_totalElapsedTime += deltaTime;
			_totalElapsedTime = Mathf.Clamp( _totalElapsedTime, 0, _totalDuration );

			// using our fresh totalElapsedTime, figure out what iteration we are on
			if (_loopCount >= 0) {
				_playedCount = Mathf.FloorToInt( _totalElapsedTime / _duration );
			}

			// figure out the current elapsedTime
			if (_loopCount > 0 && _playedCount >= _loopCount)
			{
				// we finished all iterations so clamp to the end of this tick
				_elapsedTime = _duration;
				isComplete = true;
			}
			else if( _totalElapsedTime < _duration )
			{
				_elapsedTime = _totalElapsedTime; // havent finished a single iteration yet
			}
			else
			{
				// TODO: when we increment a completed iteration (go from 0 to 1 for example) we should probably run through once setting
				// _elapsedTime = duration so that complete handlers in a chain or flow fire when expected
				_elapsedTime = _totalElapsedTime % _duration; // have finished at least one iteration
			}

			if (isComplete)
			{
				if (onAnimationFinish != null)
				{
					onAnimationFinish(this);
				}
				isPlayingAnimation = false;
				_animationStopped = true;
				currentAnimationSprites = null;
				return;
			}

			// Change current sprite frame
			int desiredFrame = Mathf.FloorToInt( _elapsedTime / _secondsPerFrame );
			if (desiredFrame != currentAnimationFrame) {
				currentAnimationFrame = desiredFrame;
				mySpriteRenderer.sprite = currentAnimationSprites[currentAnimationFrame];
				if (onAnimatingFrame != null)
				{
					onAnimatingFrame(this, currentAnimationFrame);
				}
			}
		} else {
			if (currentAnimationSprites != null && _animationStopped)
			{
				currentAnimationSprites = null;
			}
		}
	}

	public void AddAnimationSprite(string name, Sprite[] sprites)
	{
		if (animationSprites == null)
		{
			animationSprites = new Dictionary<string, Sprite[]>();
		}
		if (animationSprites.ContainsKey(name))
		{
			animationSprites.Remove(name);
		}
		animationSprites.Add(name, sprites);
	}

	public void PlayAnimation(string name, int loopCount = 0)
	{
		if (animationSprites == null || !animationSprites.ContainsKey(name))
		{
			return;
		}

		currentAnimationName = name;
		currentAnimationSprites = animationSprites[name];
		_loopCount = loopCount;
		_secondsPerFrame = 1f / framesPerSecond;
		_duration = _secondsPerFrame * currentAnimationSprites.Length;
		_totalElapsedTime = 0;
		
		_playedCount = 0;
		if (_loopCount <= 0)
			_totalDuration = float.PositiveInfinity;
		else
			_totalDuration = _duration * _loopCount;
		
		isPlayingAnimation = true;
		_animationStopped = false;
		currentAnimationFrame = -1;
	}

	public void PauseAnimation()
	{
		isPlayingAnimation = false;
	}

	public void ResumeAnimation()
	{
		isPlayingAnimation = true;
	}

	public void StopAnimation()
	{
		isPlayingAnimation = false;
		_animationStopped = true;
	}

	public void PointTo(Vector2 pos) {
		
		if (pos.x > position.x) {
			if (!spriteSideIsLeftToRight)
				flipHorizontal = true;
			else 
				flipHorizontal = false;
		} else if (pos.x < position.x) {
			if (!spriteSideIsLeftToRight)
				flipHorizontal = false;
			else
				flipHorizontal = true;
		}
	}

	public void MoveTo(Vector2 pos, float moveSpeed, System.Action<SpriteHelper> onMovedToTarget = null, iTween.EaseType easeType = iTween.EaseType.linear) {
		if (isMoving)
		{
			return;
		}
		this.onMovedToTarget = onMovedToTarget;
		iTween.StopByName (iTweenMoveName);
		float diff_x = pos.x - position.x;
		float diff_y = pos.y - position.y;
		float distance = Mathf.Sqrt((diff_x * diff_x) + (diff_y * diff_y));
		float duration = distance / (moveSpeed * scale);
		
		Hashtable tweenAttr = new Hashtable();
		tweenAttr.Add("name", iTweenMoveName);
		tweenAttr.Add("easetype", easeType);
		tweenAttr.Add("x", pos.x);
		tweenAttr.Add("y", pos.y);
		tweenAttr.Add("z", depth + pd(pos.x, pos.y));
		tweenAttr.Add("time", duration);
		tweenAttr.Add("oncomplete", "OnMovedToTarget");
		tweenAttr.Add("islocal", true);
		iTween.MoveTo(gameObject, tweenAttr);
		isMoving = true;
	}
	
	public void StopMove()
	{
		isMoving = false;
		iTween.StopByName(gameObject, iTweenMoveName);
	}
	
	protected void OnMovedToTarget()
	{
		isMoving = false;
		if (onMovedToTarget != null)
		{
			onMovedToTarget(this);
		}
	}

	private float pd(float px, float py) {
		float abs_px = Mathf.Abs(px);
		float abs_py = Mathf.Abs(py);
		
		if (abs_py<0.01f && abs_px<0.01f)
			return  py - (px / 10);
		else
			if (abs_py<0.1f && abs_px<0.1f)
				return  (py / 10) - (px / 100);
		else
			if (abs_py<1 && abs_px<1)
				return  (py / 100) - (px / 1000);
		else
			return  (py / 1000) - (px / 10000);
	}
	
	public float alpha {
		get {
			return tintColor.a;
		}
		set {
			Color oldColor = tintColor;
			tintColor = new Color(oldColor.r, oldColor.g, oldColor.b, value);
		}
	}
	
	public Color tintColor {
		get {
			return mySpriteRenderer.color;
		}
		set {
			mySpriteRenderer.color = value;
		}
	}
	
	public Vector2 position {
		get {
			return myTransform.localPosition;
		}
		set {
			_position = value;
			myTransform.localPosition = new Vector3(value.x, value.y, depth + pd(value.x, value.y));
		}
	}
	
	public float scale {
		get {
			return _scale;	
		}
		set {
			_scale = value;
			myTransform.localScale = new Vector3(_scale, _scale, 1);
			flipHorizontal = _flipHorizontal;
			flipVertical = _flipVertical;
		}
	}
	
	public bool flipHorizontal {
		get {
			return _flipHorizontal;
		}
		set {
			_flipHorizontal = value;
			Vector3 oldScale = myTransform.localScale;
			float xScale = oldScale.x;
			if (_flipHorizontal) {
				if (xScale > 0) {
					xScale *= -1;
				}
			} else {
				if (xScale < 0) {
					xScale *= -1;
				}
			}
			myTransform.localScale = new Vector3(xScale, oldScale.y, oldScale.z);
		}
	}
	
	public bool flipVertical {
		get {
			return _flipVertical;
		}
		set {
			_flipVertical = value;
			Vector3 oldScale = myTransform.localScale;
			float yScale = oldScale.y;
			if (_flipVertical) {
				if (yScale > 0) {
					yScale *= -1;
				}
			} else {
				if (yScale < 0) {
					yScale *= -1;
				}
			}
			myTransform.localScale = new Vector3(oldScale.x, yScale, oldScale.z);
		}
	}
	
	public int layer {
		get {
			return gameObject.layer;
		}
		set {
			gameObject.layer = value;
		}
	}
}
