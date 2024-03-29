using UnityEngine;
using System.Collections;

public class UISimpleLabel : UIAbstractLabel {
	public Font font;
	public int fontSize = 10;
	public Color textColor = Color.white;
	public string text { get; protected set; }
	protected Vector2 position;
	protected virtual void Update ()
	{
		Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
		position = new Vector2(screenPos.x, Screen.height - screenPos.y);
	}

	protected virtual void OnGUI()
	{
		if (font != null)
		{
			GUI.skin.label.font = font;
		}
		GUI.skin.label.fontSize = fontSize;
		Vector2 textSize = GUI.skin.label.CalcSize(new GUIContent(text));
		Rect rect = new Rect(position.x - (textSize.x / 2), position.y, textSize.x, textSize.y);
		
		GUI.skin.label.normal.textColor = textColor;
		GUI.Label(rect, text);
	}
	
	public override void SetText(string text)
	{
		this.text = text;
	}
}
