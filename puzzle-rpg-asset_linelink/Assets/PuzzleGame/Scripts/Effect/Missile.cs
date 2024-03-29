using UnityEngine;
using System.Collections;

public class Missile : MonoBehaviour {
	public Vector3 targetOffset;
	public Transform target;
	public float time = 0.25f;
	public iTween.EaseType easeType = iTween.EaseType.easeInBack;
	public System.Action onShot = null;

	public void Shoot()
	{
		Vector3 targetPosition = target.position + targetOffset;
		iTween.MoveTo(gameObject, iTween.Hash("x", targetPosition.x, "y", targetPosition.y, "z", targetPosition.z, "looktarget", target, "islocal", false, "oncomplete", "Shot", "time", time, "easetype", easeType));
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
