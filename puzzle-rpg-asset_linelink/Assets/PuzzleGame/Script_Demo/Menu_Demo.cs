using UnityEngine;
using System.Collections;

public class Menu_Demo : MonoBehaviour {

	void OnGUI()
	{
		if (GUI.Button (new Rect (10,10,300,25), "Free move swap match 3 puzzle game")) {
			Application.LoadLevel ("_demoPuzzle");
		}
		if (GUI.Button (new Rect (10,40,300,25), "Line link match 3 puzzle game")) {
			Application.LoadLevel ("_demoPuzzle_linelink");
		}
		if (GUI.Button (new Rect (10,70,300,25), "Slide match 3 puzzle game")) {
			Application.LoadLevel ("_demoPuzzle_slide");
		}
		if (GUI.Button (new Rect (10,100,300,25), "Classic swap match 3 puzzle game")) {
			Application.LoadLevel ("_demoPuzzle_swap");
		}
		if (GUI.Button (new Rect (10,130,300,25), "Tap match 3 puzzle game")) {
			Application.LoadLevel ("_demoPuzzle_tap");
		}

		if (GUI.Button (new Rect (320,10,360,25), "Free move swap match 3 puzzle simple scoring game")) {
			Application.LoadLevel ("_demoScoringGame");
		}
		if (GUI.Button (new Rect (320,40,360,25), "Line link match 3 puzzle simple scoring game")) {
			Application.LoadLevel ("_demoScoringGame_linelink");
		}
		if (GUI.Button (new Rect (320,70,360,25), "Slide match 3 puzzle simple scoring game")) {
			Application.LoadLevel ("_demoScoringGame_slide");
		}
		if (GUI.Button (new Rect (320,100,360,25), "Classic swap match 3 puzzle simple scoring game")) {
			Application.LoadLevel ("_demoScoringGame_swap");
		}
		if (GUI.Button (new Rect (320,130,360,25), "Tap match 3 puzzle simple scoring game")) {
			Application.LoadLevel ("_demoScoringGame_tap");
		}
	}
}
