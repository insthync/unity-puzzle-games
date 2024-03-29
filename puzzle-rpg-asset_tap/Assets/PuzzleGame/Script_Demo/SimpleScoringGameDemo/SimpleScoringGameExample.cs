using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SimpleScoringGameExample : MonoBehaviour {
	public BoardControllerBase board;
	public int scorePerQuantity;
	public Text[] scoreTexts;
	public Text countDownText;
	public Slider countDownGage;
	public float countDownTimeLength;
	public GameObject startScene;
	public GameObject endScene;
	protected CountDownHelper countDownTimer;
	protected int _score;
	protected float _timeCounter;
	public int score {
		get {
			return _score;
		}
		set {
			_score = value;
			SetScoreTexts();
		}
	}
	public float timeCounter
	{
		get {
			return _timeCounter;
		}
		set {
			_timeCounter = value;
			SetCountDownTime();
		}
	}

	void Awake () {
		board.onNodeMatch = OnBoardNodeMatch;
		score = 0;
		timeCounter = 0;
	}

	void Start () {
		if (startScene != null) {
			startScene.SetActive(true);
		}
		if (endScene != null) {
			endScene.SetActive(false);
		}
		board.IsPause = true;
	}

	public void StartGame()
	{
		score = 0;
		timeCounter = 0;
		CountDownHelper.StartCountDown (countDownTimeLength, EndGame, OnCounting);
		if (startScene != null) {
			startScene.SetActive(false);
		}
		if (endScene != null) {
			endScene.SetActive(false);
		}
		board.IsPause = false;
	}

	public void EndGame(CountDownHelper owner)
	{
		board.EndMovingNode();
		board.IsPause = true;
		if (endScene != null) {
			endScene.SetActive(true);
		}
	}

	protected void OnCounting(CountDownHelper owner)
	{
		if (countDownGage != null) {
			countDownGage.value = 1 - owner.timeRate;
		}
		timeCounter = owner.timeCounter;
	}
	
	protected void OnBoardNodeMatch(Node[] nodes, int type, int quantity, int currentCombo)
	{
		int addingScore = quantity * scorePerQuantity * currentCombo;
		score += addingScore;
	}

	protected void SetScoreTexts()
	{
		for (int i = 0; i < scoreTexts.Length; ++i) {
			if (scoreTexts[i] != null)
			{
				scoreTexts[i].text = score.ToString("N0");
			}
		}
	}

	protected void SetCountDownTime()
	{
		if (countDownText != null) {
			int minutes = Mathf.FloorToInt((countDownTimeLength - timeCounter) / 60f);
			int seconds = Mathf.FloorToInt((countDownTimeLength - timeCounter) - minutes * 60);
			string niceTime = string.Format("{0:0}:{1:00}", minutes, seconds);
			countDownText.text = niceTime;
		}
	}
}
