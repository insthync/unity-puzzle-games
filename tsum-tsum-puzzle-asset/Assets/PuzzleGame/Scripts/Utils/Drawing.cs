using System;
using UnityEngine;

public class Drawing
{
	//****************************************************************************************************
	//  static function DrawLine(rect : Rect) : void
	//  static function DrawLine(rect : Rect, color : Color) : void
	//  static function DrawLine(rect : Rect, width : float) : void
	//  static function DrawLine(rect : Rect, color : Color, width : float) : void
	//  static function DrawLine(Vector2 pointA, Vector2 pointB) : void
	//  static function DrawLine(Vector2 pointA, Vector2 pointB, color : Color) : void
	//  static function DrawLine(Vector2 pointA, Vector2 pointB, width : float) : void
	//  static function DrawLine(Vector2 pointA, Vector2 pointB, color : Color, width : float) : void
	//  
	//  Draws a GUI line on the screen.
	//  
	//  DrawLine makes up for the severe lack of 2D line rendering in the Unity runtime GUI system.
	//  This function works by drawing a 1x1 texture filled with a color, which is then scaled
	//   and rotated by altering the GUI matrix.  The matrix is restored afterwards.
	//****************************************************************************************************
	
	private static Texture2D _aaLineTex = null;
	private static Texture2D _lineTex = null;

	public static void DrawLine(Rect rect) { DrawLine(rect, GUI.contentColor, 1.0f); }
	public static void DrawLine(Rect rect, Color color) { DrawLine(rect, color, 1.0f); }
	public static void DrawLine(Rect rect, float width) { DrawLine(rect, GUI.contentColor, width); }
	public static void DrawLine(Rect rect, Color color, float width) { DrawLine(new Vector2(rect.x, rect.y), new Vector2(rect.x + rect.width, rect.y + rect.height), color, width); }
	public static void DrawLine(Vector2 pointA, Vector2 pointB) { DrawLine(pointA, pointB, GUI.contentColor, 1.0f); }
	public static void DrawLine(Vector2 pointA, Vector2 pointB, Color color) { DrawLine(pointA, pointB, color, 1.0f); }
	public static void DrawLine(Vector2 pointA, Vector2 pointB, float width) { DrawLine(pointA, pointB, GUI.contentColor, width, false); }
	public static void DrawLine(Vector2 pointA, Vector2 pointB, Color color, float width) { DrawLine(pointA, pointB, color, width, false); }
	public static void DrawLine(Vector2 pointA, Vector2 pointB, Color color, float width, bool antiAlias, Texture2D lineTex = null, Texture2D aaLineTex = null)
	{
		Color savedColor = GUI.color;
		Matrix4x4 savedMatrix = GUI.matrix;
		if (lineTex == null)
		{
			lineTex = _lineTex;
		}

		if (aaLineTex == null)
		{
			aaLineTex = _aaLineTex;
		}

		if (lineTex == null)
		{
			lineTex = new Texture2D(1, 1, TextureFormat.ARGB32, true);
			lineTex.SetPixel(0, 1, Color.white);
			lineTex.Apply();
			if (_lineTex == null)
			{
				_lineTex = lineTex;
			}
		}

		if (aaLineTex == null)
		{
			aaLineTex = new Texture2D(1, 3, TextureFormat.ARGB32, true);
			aaLineTex.SetPixel(0, 0, new Color(1, 1, 1, 0));
			aaLineTex.SetPixel(0, 1, Color.white);
			aaLineTex.SetPixel(0, 2, new Color(1, 1, 1, 0));
			aaLineTex.Apply();
			if (_aaLineTex == null)
			{
				_aaLineTex = aaLineTex;
			}
		}

		if (antiAlias) width *= 3;
		float angle = Vector3.Angle(pointB - pointA, Vector2.right) * (pointA.y <= pointB.y?1:-1);
		float m = (pointB - pointA).magnitude;
		if (m > 0.01f)
		{
			Vector3 dz = new Vector3(pointA.x, pointA.y, 0);
			
			GUI.color = color;
			GUI.matrix = translationMatrix(dz) * GUI.matrix;
			GUIUtility.ScaleAroundPivot(new Vector2(m, width), new Vector3(-0.5f, 0, 0));
			GUI.matrix = translationMatrix(-dz) * GUI.matrix;
			GUIUtility.RotateAroundPivot(angle, Vector2.zero);
			GUI.matrix = translationMatrix(dz + new Vector3(width / 2, -m / 2) * Mathf.Sin(angle * Mathf.Deg2Rad)) * GUI.matrix;
			
			if (!antiAlias)
				GUI.DrawTexture(new Rect(0, 0, 1, 1), lineTex);
			else
				GUI.DrawTexture(new Rect(0, 0, 1, 1), aaLineTex);
		}
		GUI.matrix = savedMatrix;
		GUI.color = savedColor;
	}
	
	public static void bezierLine(Vector2 start, Vector2 startTangent, Vector2 end, Vector2 endTangent, Color color, float width, bool antiAlias, int segments)
	{
		Vector2 lastV = cubeBezier(start, startTangent, end, endTangent, 0);
		for (int i = 1; i <= segments; ++i)
		{
			Vector2 v = cubeBezier(start, startTangent, end, endTangent, i/(float)segments);
			
			Drawing.DrawLine(
				lastV,
				v,
				color, width, antiAlias);
			lastV = v;
		}
	}
	
	private static Vector2 cubeBezier(Vector2 s, Vector2 st, Vector2 e, Vector2 et, float t){
		float rt = 1-t;
		float rtt = rt * t;
		return rt*rt*rt * s + 3 * rt * rtt * st + 3 * rtt * t * et + t*t*t* e;
	}
	
	private static Matrix4x4 translationMatrix(Vector3 v)
	{
		return Matrix4x4.TRS(v,Quaternion.identity,Vector3.one);
	}
}