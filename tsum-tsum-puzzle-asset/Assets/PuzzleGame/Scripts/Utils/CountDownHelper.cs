using UnityEngine;
using System.Collections;

public class CountDownHelper : MonoBehaviour {
	public float timeLength;
	public System.Action<CountDownHelper> onCounting;
	public System.Action<CountDownHelper> onFinish;
	public float timeCounter { get; private set; }
	public float timeRate { get; private set; }
	public int seconds { get; private set; }
	private bool destroying;
	void Start()
	{
		timeCounter = 0;
		destroying = false;
	}

	void Update()
	{
		timeCounter += Time.deltaTime;
		if (timeCounter >= timeLength) {
			timeRate = 1;
			seconds = 0;
			Destroy();
		} else {
			timeRate = timeCounter / timeLength;
			seconds = Mathf.CeilToInt(timeLength - timeCounter);
			if (onCounting != null)
			{
				onCounting(this);
			}
		}
	}

	public void Destroy()
	{
		if (destroying)
			return;
		
		destroying = true;

		if (onFinish != null)
		{
			onFinish(this);
		}
		GameObject.DestroyObject(gameObject);
	}

	public static CountDownHelper StartCountDown(float timeLength, System.Action<CountDownHelper> onFinish = null, System.Action<CountDownHelper> onCounting = null)
	{
		GameObject g = new GameObject ();
		g.name = "countDownHelperObject";
		CountDownHelper component = g.AddComponent<CountDownHelper> ();
		component.timeLength = timeLength;
		component.onFinish = onFinish;
		component.onCounting = onCounting;
		return component;
	}
}
