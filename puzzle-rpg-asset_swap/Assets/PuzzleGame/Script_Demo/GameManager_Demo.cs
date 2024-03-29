using UnityEngine;
using System.Collections;

public class GameManager_Demo : GameManager {
	public SpriteHelper blackFadeSprite;
	public SpriteHelper messageWinSprite;
	public SpriteHelper messageLoseSprite;
	public CharacterAction defaultAttackAction;
	public UISimpleDamageText damageText;
	protected override void Awake ()
	{
		base.Awake ();
		blackFadeSprite.gameObject.SetActive (false);
		messageWinSprite.gameObject.SetActive (false);
		messageLoseSprite.gameObject.SetActive (false);
	}

	protected override void OnAreaChange (int area)
	{
		base.OnAreaChange (area);
	}

	protected override void OnWin ()
	{
		base.OnWin ();
		StartCoroutine (fadeMessage (true));
	}

	protected override void OnLose ()
	{
		base.OnLose ();
		StartCoroutine (fadeMessage (false));
	}

	protected IEnumerator fadeMessage(bool isWin)
	{
		blackFadeSprite.gameObject.SetActive (true);
		float blackFadeSpd = 1;
		float messageFadeSpd = 1;

		float a = 0;
		while (a < 0.75f) {
			blackFadeSprite.alpha = a;
			a += Time.deltaTime * blackFadeSpd;
			yield return 0;
		}
		blackFadeSprite.alpha = 0.75f;
		a = 0;
		if (isWin)
		{
			messageWinSprite.gameObject.SetActive (true);
			while (a < 1) {
				messageWinSprite.alpha = a;
				a += Time.deltaTime * messageFadeSpd;
				yield return 0;
			}
			messageWinSprite.alpha = 1;
		} else {
			messageLoseSprite.gameObject.SetActive (true);
			while (a < 1) {
				messageLoseSprite.alpha = a;
				a += Time.deltaTime * messageFadeSpd;
				yield return 0;
			}
			messageLoseSprite.alpha = 1;
		}
	}

	protected override void OnCharacterAttack (Character owner, CharacterAction attack, int damage, Character target)
	{
		base.OnCharacterAttack (owner, attack, damage, target);
		if (damageText != null)
		{
			UISimpleDamageText _damageText = (UISimpleDamageText)Instantiate(damageText, target.actor.transform.position, Quaternion.identity);
			_damageText.delay = attack.actionDuration;
			_damageText.damage = damage;
			_damageText.gameObject.SetActive(true);
		}
	}

	protected override void OnCharacterSkill (Character owner, CharacterSkill skill, int damage, Character target)
	{
		base.OnCharacterSkill (owner, skill, damage, target);
	}
	
	protected override void OnCharacterSpawn (Character owner)
	{
		base.OnCharacterSpawn (owner);
		owner.attackAction = defaultAttackAction;
	}

	protected override void OnCharacterDead (Character owner)
	{
		base.OnCharacterDead (owner);
	}
	
	protected override void OnCharacterTurnCountDown (Character owner)
	{
		base.OnCharacterTurnCountDown (owner);
	}
	
	protected override void OnCharacterSkillTurnCountDown (Character owner)
	{
		base.OnCharacterSkillTurnCountDown (owner);
        if (owner.skill != null)
        {
            Debug.Log("Player Character: " + owner.name + " skill turn: " + owner.skill.turn_count);
        }
	}

	protected override void OnEnemyCharacterAttack (Character owner, CharacterAction attack, int damage, Character target)
	{
		base.OnEnemyCharacterAttack (owner, attack, damage, target);
		if (damageText != null)
		{
			UISimpleDamageText _damageText = (UISimpleDamageText)Instantiate(damageText, Vector3.zero, Quaternion.identity);
			_damageText.delay = attack.actionDuration;
			_damageText.damage = damage;
			_damageText.gameObject.SetActive(true);
		}
	}

	protected override void OnEnemyCharacterSkill (Character owner, CharacterSkill skill, int damage, Character target)
	{
		base.OnEnemyCharacterSkill (owner, skill, damage, target);
	}

	protected override void OnEnemyCharacterSpawn (Character owner)
	{
		base.OnEnemyCharacterSpawn (owner);
		owner.attackAction = defaultAttackAction;
		StartCoroutine(fadeSpawnCharacter (owner));
	}

	protected override void OnEnemyCharacterDead (Character owner)
	{
		base.OnEnemyCharacterDead (owner);
		if (owner.hpBar != null)
		{
			owner.hpBar.gameObject.SetActive(false);
		}
		if (owner.turnCountLabel != null)
		{
			owner.turnCountLabel.gameObject.SetActive(false);
		}
		StartCoroutine(fadeDeadCharacter (owner));
	}

	protected override void OnEnemyCharacterTurnCountDown (Character owner)
	{
		base.OnEnemyCharacterTurnCountDown (owner);
		Debug.Log ("Enemy Character: " + owner.name + " turn: " + owner.turnCount);
	}
	
	protected override void OnEnemyCharacterSkillTurnCountDown (Character owner)
	{
        base.OnEnemyCharacterSkillTurnCountDown(owner);
        if (owner.skill != null)
        {
            Debug.Log("Enemy Character: " + owner.name + " skill turn: " + owner.skill.turn_count);
        }
	}
	
	protected IEnumerator fadeSpawnCharacter(Character character)
	{
		float fadeSpd = 1.5f;
		float a = 0;
		if (character.actor != null)
		{
			SpriteRenderer _spr = character.actor.GetComponent<SpriteRenderer>();
			if (_spr != null)
			{
				_spr.color = new Color(1, 1, 1, 0);
				while (a < 1)
				{
					_spr.color = new Color(1, 1, 1, a);
					a += Time.deltaTime * fadeSpd;
					yield return 0;
				}
				_spr.color = new Color(1, 1, 1, 1);
			}
		}
	}

	protected IEnumerator fadeDeadCharacter(Character character)
	{
		float fadeSpd = 1.5f;
		float a = 1;
        if (character.actor != null)
        {
			SpriteRenderer _spr = character.actor.GetComponent<SpriteRenderer>();
            if (_spr != null)
			{
				_spr.color = new Color(1, 1, 1, 1);
                while (a > 0)
                {
                    _spr.color = new Color(1, 1, 1, a);
                    a -= Time.deltaTime * fadeSpd;
                    yield return 0;
                }
                _spr.color = new Color(1, 1, 1, 0);
            }
        }
	}
}
