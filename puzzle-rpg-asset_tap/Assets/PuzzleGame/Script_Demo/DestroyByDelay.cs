using UnityEngine;
using System.Collections;

public class DestroyByDelay : MonoBehaviour {

	public float delay;
	void Start () {
		Destroy (gameObject, delay);
	}
}
