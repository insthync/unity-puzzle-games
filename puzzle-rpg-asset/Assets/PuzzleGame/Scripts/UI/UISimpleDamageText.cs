using UnityEngine;
using System.Collections;
using DG.Tweening;

public class UISimpleDamageText : UISimpleLabel
{
	public float time = 1f;
	public float fadeOutTime = 1;
	public float delay;
	public Vector3 moveUpOffset;
	public float scaleUp;
	public int damage;
	private bool isSetTween = false;
	private float _currentFontSize;
	private float _currentDamage;
	private float _currentFade;

	protected override void Update()
	{
		base.Update();
		if (!isSetTween)
		{
			transform.DOLocalMove(transform.localPosition + moveUpOffset, time).SetDelay(delay);
			_currentFontSize = fontSize;
			DOTween.To(GetSizeValue, OnUpdateSizeValue, fontSize * scaleUp, time).SetDelay(delay);
			_currentDamage = 0;
			DOTween.To(GetDamageValue, OnUpdateDamageValue, damage, time).SetDelay(delay);
			_currentFade = 1f;
			DOTween.To(GetFadeValue, OnUpdateFadeValue, 0f, fadeOutTime).SetDelay(delay + time);
			StartCoroutine(DestoryOnEnd());
			isSetTween = true;
		}
	}

	private float GetSizeValue()
	{
		return _currentFontSize;
	}

	private void OnUpdateSizeValue(float value)
	{
		_currentFontSize = value;
		fontSize = Mathf.CeilToInt(_currentFontSize);
	}

	private float GetDamageValue()
	{
		return _currentDamage;
	}

	private void OnUpdateDamageValue(float value)
	{
		_currentDamage = value;
		SetText(_currentDamage.ToString("N0"));
	}

	private float GetFadeValue()
	{
		return _currentFade;
	}

	private void OnUpdateFadeValue(float value)
	{
		_currentFade = value;
		textColor = new Color(textColor.r, textColor.g, textColor.b, _currentFade);
	}

	private IEnumerator DestoryOnEnd()
	{
		yield return new WaitForSeconds(delay + time + fadeOutTime);
		Destroy(gameObject);
	}
}
