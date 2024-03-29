using UnityEngine;
using System.Collections;

public class Menu_Demo : MonoBehaviour {

	void OnGUI()
	{
		if (GUI.Button (new Rect (10,10,250,25), "Drag and Drop")) {
			Application.LoadLevel ("_demoPuzzle");
		}
		if (GUI.Button (new Rect (10,40,250,25), "Line Link")) {
			Application.LoadLevel ("_demoPuzzle_linelink");
		}
		if (GUI.Button (new Rect (10,70,250,25), "Slide")) {
			Application.LoadLevel ("_demoPuzzle_slide");
		}
		if (GUI.Button (new Rect (10,100,250,25), "Swap")) {
			Application.LoadLevel ("_demoPuzzle_swap");
		}
		if (GUI.Button (new Rect (10,130,250,25), "Tap")) {
			Application.LoadLevel ("_demoPuzzle_tap");
		}

		if (GUI.Button (new Rect (10,160,250,25), "Drag and Drop + Score")) {
			Application.LoadLevel ("_demoScoringGame");
		}
		if (GUI.Button (new Rect (10,190,250,25), "Line Link + Score")) {
			Application.LoadLevel ("_demoScoringGame_linelink");
		}
		if (GUI.Button (new Rect (10,220,250,25), "Slide + Score")) {
			Application.LoadLevel ("_demoScoringGame_slide");
		}
		if (GUI.Button (new Rect (10,250,250,25), "Swap + Score")) {
			Application.LoadLevel ("_demoScoringGame_swap");
		}
		if (GUI.Button (new Rect (10,280,250,25), "Tap + Score")) {
			Application.LoadLevel ("_demoScoringGame_tap");
		}
	}

	public void PlayDNDRPG()
	{
		Application.LoadLevel ("_demoPuzzle");
	}
	public void PlayLLKRPG()
	{
		Application.LoadLevel ("_demoPuzzle_linelink");
	}
	public void PlaySLIRPG()
	{
		Application.LoadLevel ("_demoPuzzle_slide");
	}
	public void PlaySWARPG()
	{
		Application.LoadLevel ("_demoPuzzle_swap");
	}
	public void PlayTAPRPG()
	{
		Application.LoadLevel ("_demoPuzzle_tap");
	}

	public void PlayDNDScore()
	{
		Application.LoadLevel ("_demoScoringGame");
	}
	public void PlayLLKScore()
	{
		Application.LoadLevel ("_demoScoringGame_linelink");
	}
	public void PlaySLIScore()
	{
		Application.LoadLevel ("_demoScoringGame_slide");
	}
	public void PlaySWAScore()
	{
		Application.LoadLevel ("_demoScoringGame_swap");
	}
	public void PlayTAPScore()
	{
		Application.LoadLevel ("_demoScoringGame_tap");
	}
}
