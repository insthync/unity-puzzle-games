using UnityEngine;
using System.Collections;
using DG.Tweening;

public class Missile : MonoBehaviour {
	public Vector3 targetOffset;
	public Transform target;
	public float time = 0.25f;
	public Ease easeType = Ease.InBack;
	public System.Action onShot = null;

	public void Shoot()
	{
		Vector3 targetPosition = target.position + targetOffset;
		transform.DOMove(targetPosition, time).SetEase(easeType).onComplete = Shot;
	}

	public void Shot()
	{
		if (onShot != null)
		{
			onShot();
		}

		Destroy(gameObject);
	}
}
