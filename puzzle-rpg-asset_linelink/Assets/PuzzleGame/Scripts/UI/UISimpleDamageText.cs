using UnityEngine;
using System.Collections;

public class UISimpleDamageText : UISimpleLabel {
	public float time = 1f;
	public float fadeOutTime = 1;
	public float delay;
	public Vector3 moveUpOffset;
	public float scaleUp;
	public int damage;
	private bool isSetTween = false;
	protected override void Update ()
	{
		base.Update ();
		if (!isSetTween)
		{
			iTween.MoveTo(gameObject, iTween.Hash("position", transform.localPosition + moveUpOffset, "islocal", true, "delay", delay, "time", time));
			iTween.ValueTo(gameObject, iTween.Hash("from", fontSize, "to", fontSize * scaleUp, "onupdate", "OnUpdateSizeValue", "delay", delay, "time", time));
			iTween.ValueTo(gameObject, iTween.Hash("from", 0, "to", damage, "onupdate", "OnUpdateDamageValue", "delay", delay, "time", time));
			iTween.ValueTo(gameObject, iTween.Hash("from", 1f, "to", 0f, "onupdate", "OnUpdateFadeValue", "delay", delay + time, "time", fadeOutTime));
			StartCoroutine(DestoryOnEnd());
			isSetTween = true;
		}
	}

	private void OnUpdateSizeValue(int value)
	{
		fontSize = value;
	}
	
	private void OnUpdateDamageValue(int value)
	{
		if (value > 0)
		{
			SetText("" + value);
		} else {
			SetText("");
		}
	}

	private void OnUpdateFadeValue(float value)
	{
		textColor = new Color(textColor.r, textColor.g, textColor.b, value);
	}

	private IEnumerator DestoryOnEnd()
	{
		yield return new WaitForSeconds(delay + time + fadeOutTime);
		Destroy(gameObject);
	}
}
