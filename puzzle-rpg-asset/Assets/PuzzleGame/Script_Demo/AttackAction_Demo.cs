using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class AttackAction_Demo : CharacterAction {
	public Missile missilePrefab;
	public override void DoAction(Character owner, List<Character> targets = null)
	{
		if (owner.controllerType == Character.ControllerType.NORMAL)
		{
			if (targets != null && targets.Count > 0)
			{
				for (int i = 0; i < targets.Count; ++i)
				{
					Character target = targets[i];
					if (target != null)
					{
						Missile missile = (Missile)Instantiate(missilePrefab);
						missile.transform.position = owner.transform.position;
						missile.target = target.transform;
						missile.gameObject.SetActive(true);
						missile.Shoot();
					}
				}
			}
		} else {
			transform.DOPunchScale(owner.transform.localScale * 1.05f, 1f);
		}
		base.DoAction(owner, targets);
	}
}
