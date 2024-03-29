using UnityEngine;
using System.Collections;

public class CameraHelper : MonoBehaviour {
	public Camera myCamera = null;
	public float targetWidth;
	public float targetHeight;
	public bool isOrthoPixelPerfect;
	public bool isInitialized { get; protected set; }
	// Use this for initialization
	void Awake () 
	{
		isInitialized = false;
		// obtain camera component so we can modify its viewport
		if (myCamera == null) {
			myCamera = GetComponent<Camera>();
		}
	}

	void Start ()
	{
		StartCoroutine(InitCamera());
	}

	private IEnumerator InitCamera()
	{
		yield return 0;
		if (myCamera != null) {
			if (isOrthoPixelPerfect)
			{
				myCamera.orthographicSize = targetHeight / 2f;
			}
			// set the desired aspect ratio (the values in this example are
			// hard-coded for 16:9, but you could make them into public
			// variables instead so you can set them at design time)
			float targetaspect = targetWidth / targetHeight;
			
			// determine the game window's current aspect ratio
			float windowaspect = (float)Screen.width / (float)Screen.height;
			
			// current viewport height should be scaled by this amount
			float scaleheight = windowaspect / targetaspect;

			// if scaled height is less than current height, add letterbox
			if (scaleheight < 1.0f)
			{  
				Rect rect = myCamera.rect;
				
				rect.width = 1.0f;
				rect.height = scaleheight;

				rect.x = 0;
				rect.y = (1.0f - scaleheight) / 2.0f;
				
				myCamera.rect = rect;
			}
			else // add pillarbox
			{
				float scalewidth = 1.0f / scaleheight;
				
				Rect rect = myCamera.rect;
				
				rect.width = scalewidth;
				rect.height = 1.0f;

				rect.x = (1.0f - scalewidth) / 2.0f;
				rect.y = 0;
				
				myCamera.rect = rect;
			}
		}
		isInitialized = true;
	}

	public Rect rect {
		get {
			return myCamera.rect;
		}
		set {
			myCamera.rect = value;
		}
	}
}
