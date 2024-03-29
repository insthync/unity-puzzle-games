using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterAction : MonoBehaviour {
	public float actionDuration;
	protected List<Character> doingActions;

	protected virtual void Awake()
	{
		doingActions = new List<Character>();
	}

	public virtual void DoAction(Character owner, List<Character> targets = null)
    {
		doingActions.Add(owner);
		StartCoroutine(_DoAction(owner, targets));
    }

	private IEnumerator _DoAction(Character owner, List<Character> targets)
	{
		yield return new WaitForSeconds(actionDuration);
		doingActions.Remove(owner);
	}

	public bool isActionEnd(Character owner)
	{
		return !doingActions.Contains(owner);
	}
}
