using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {
	public BoardControllerBase board;
    // Characters
    public int maxMemberQuantity = 6;
    public bool isInitCharacterById;
	public List<Character> characters;
	public List<Character> enemyCharacters;
    public List<AreaData> areaDataList;
    public List<int> character_ids;
    public Transform characterContainer;
    public Transform enemyCharacterContainer;
	public Vector3 characterScale = Vector3.one;
	public Vector3 enemyCharacterScale = Vector3.one;
    public CharacterPositionList characterPositions;
    public CharacterPositionList[] enemyCharacterPositions;
	// UIs
	public Camera inputCamera;
	public UIAbstractBar hpBar;
	public UIAbstractBar timeBar;
	public UIAbstractBar characterHpBar;
	public Vector3 characterHpBarOffset;
	public UIAbstractLabel characterTurnCountLabel;
	public Vector3 characterTurnCountLabelOffset;
	// Timers
	public float matchingCountDownTime;
	public float changeAttackingTeamDelay;
	public float changeAttackingCharacterDelay = 0.15f;
	public float changeAreaDelay;
    // Character Attributes
	[HideInInspector]
	public float recoveryMultiply;
	public int elementRecovery;
	public ElementPair[] elementPairs;
	public float advantageElementDamageRate;
	public bool isUpdateInput;
	public int currentRcv { get; protected set; }
	public int currentMaxHp { get; protected set; }
	public int currentArea { get; protected set; }
	protected int hp;
	protected CountDownHelper boardCountDown;
	protected bool isAwaken = false;
	protected bool isGameEnd = false;
	protected bool canActiveTarget = false;
	public int currentHp
	{
		get {
			return hp;
		}
		set {
			if (hp != value) {
				hp = value;
				if (hp > currentMaxHp)
				{
					hp = currentMaxHp;
				}
				if (hp <= 0)
				{
					hp = 0;
				}
				if (isAwaken && hpBar != null)
					hpBar.SetRate((float)hp / (float)currentMaxHp);
			}
		}
	}
	protected Character _character;
	public Character selectedCharacter
	{
		get {
			return _character;
		}
		set {
			if (_character != value)
			{
				_character = value;
			} else {
				_character = null;
			}
		}
	}

	protected int combo;

	protected virtual void Awake()
	{
		if (inputCamera == null)
		{
			inputCamera = Camera.main;
		}
		isAwaken = true;
	}

	protected virtual void Start()
	{
		if (!isComponentsReady())
		{
			return;
		}
		currentArea = 0;
		isGameEnd = false;
		InitBoard ();
		InitCharacter ();
	}

	protected virtual void Update ()
	{
		if (!isComponentsReady())
		{
			return;
		}
	}

	#region Board
	protected void InitBoard()
	{
		board.onStartMoveNode = OnBoardStartMoveNode;
		board.onNodeMatch = OnBoardNodeMatch;
		board.onNoNodeMatches = OnBoardNoNodeMatches;
		board.onEndMovingNode = OnBoardEndMovingNode;

		if (timeBar != null)
			timeBar.gameObject.SetActive(false);
		if (hpBar != null)
			hpBar.gameObject.SetActive(true);

		combo = 0;
	}

	protected void OnBoardStartMoveNode()
	{
		boardCountDown = CountDownHelper.StartCountDown (matchingCountDownTime, 
			delegate(CountDownHelper owner) {
				board.EndMovingNode();
			},
			delegate(CountDownHelper owner) {
				// Set time length
				if (timeBar != null)
					timeBar.SetRate(1 - owner.timeRate);
			}
		);
		if (timeBar != null)
			timeBar.gameObject.SetActive(true);
		if (hpBar != null)
			hpBar.gameObject.SetActive(false);
		canActiveTarget = false;
	}
	
	protected void OnBoardNodeMatch(Node[] nodes, int type, int quantity, int currentCombo)
	{
		// Show combo, calculate character's attack damage
		StartCoroutine(NodeMatch(nodes, type, quantity, currentCombo));
	}

	protected IEnumerator NodeMatch(Node[] nodes, int element, int quantity, int currentCombo)
	{
		yield return 0;
		combo = currentCombo;
		if (element != elementRecovery)
		{
			Character[] anCharacters = findCharactersByElement (element);
			
			// Calculate damage by quantity {formula: 100%+(n-3)*25%, n=number of connected orbs }
			for (int i = 0; i < anCharacters.Length; ++i)
			{
				Character theCharacter = anCharacters[i];
				// When you match 5 or more adjacent orbs of a certain color
				// each monster with that attribute will perform a mass attack against all enemies.
				if (quantity >= 5)
				{
					theCharacter.atkSplash = true;
				}
				theCharacter.atkMultiply += (1 + ((quantity - 3) * 0.25f));
			}
		} else {
			// Recoverying hp
			recoveryMultiply += (1 + ((quantity - 3) * 0.25f));
		}
	}

	protected void OnBoardEndMovingNode()
	{
		if (boardCountDown != null)
		{
			boardCountDown.Destroy();
		}

		if (timeBar != null)
			timeBar.gameObject.SetActive(false);

		if (hpBar != null)
			hpBar.gameObject.SetActive(true);
	}
	
	protected void OnBoardNoNodeMatches()
	{
		// Let's character attacking, count enemy's turn down
		StartCoroutine (StartBattle ());
	}

	public void TouchedOnCharacter(Character character)
	{
		if (canActiveTarget)
		{
			if (character.controllerType == Character.ControllerType.NORMAL)
			{
				TouchedOnPlayerCharacter(character);
			}
			if (character.controllerType == Character.ControllerType.AI)
			{
				TouchedOnEnemyCharacter(character);
			}
		}
	}

	public virtual void TouchedOnPlayerCharacter(Character character)
	{
		character.DoSkill(selectedCharacter);
	}

	public virtual void TouchedOnEnemyCharacter(Character character)
	{
		SelectCharacter(character);
	}

	public virtual void SelectCharacter(Character character)
	{
		selectedCharacter = character;
	}

	protected IEnumerator StartBattle()
	{
		board.IsPause = true;
		yield return 0;

		bool isAttackOnce = false;
		Character theCharacter = null;
		Character target = null;
		// Calculate damage by combo {formula: 100%+(n-1)*25%, n=total combo count }
		for (int i = 0; i < characters.Count; ++i)
		{
			theCharacter = characters[i];
			if (theCharacter != null && theCharacter.atkMultiply > 0 && theCharacter.currentHp > 0)
			{
				theCharacter.atkMultiply *= (1 + ((combo - 1) * 0.25f));
				if (selectedCharacter != null && selectedCharacter.currentHp > 0)
				{
					target = selectedCharacter;
				} else {
					target = findAliveEnemyCharacter();
				}
				if (target != null)
				{
					yield return new WaitForSeconds(changeAttackingCharacterDelay);
					isAttackOnce = true;
					theCharacter.DoAttack(target);
				}
			}
		}

		recoveryMultiply *= (1 + ((combo - 1) * 0.25f));

		currentHp += Mathf.CeilToInt(currentRcv * recoveryMultiply);
        while (!isAllCharacterActionEnd())
        {
            yield return 0;
		}

		// Decrease turn count
		PrepareCharactersTurn ();
		PrepareEnemyCharactersTurn ();
		CheckEnemyCharactersDead();

		if (isAttackOnce)
			yield return new WaitForSeconds(changeAttackingTeamDelay);

		// Then let's enemy character attacking
		int diedCharacters = 0;
		for (int i = 0; i < enemyCharacters.Count; ++i)
		{
			theCharacter = enemyCharacters[i];
            if (theCharacter != null)
            {
                if (theCharacter.currentHp <= 0)
                {
                    ++diedCharacters;
                }
                else
                {
                    target = findAliveCharacter();
                    if (theCharacter.skill != null && theCharacter.skill.turn_count <= 0)
                    {
                        // Use some skill
                        if (target != null)
                        {
							yield return new WaitForSeconds(changeAttackingCharacterDelay);
                            theCharacter.DoSkill(target);
                        }
                        theCharacter.ResetSkillTurn();
                        theCharacter.ResetTurn();
                    }
                    else if (theCharacter.turnCount <= 0)
                    {
                        // Attacking
                        if (target != null)
                        {
							yield return new WaitForSeconds(changeAttackingCharacterDelay);
                            theCharacter.DoAttack(target);
                        }
                        theCharacter.ResetTurn();
                    }
                }
            }
            else
            {
                ++diedCharacters;
            }
        }

        while (!isAllCharacterActionEnd())
        {
            yield return 0;
        }
		
		ResetCharacters ();
		combo = 0;
		
		if (currentHp <= 0)
		{
			// Game over
			OnLose();
			isGameEnd = true;
		} else {
			if (diedCharacters >= enemyCharacters.Count)
			{
				if (currentArea >= areaDataList.Count)
				{
					OnWin();
					isGameEnd = true;
				} else {
					OnAreaChange(currentArea + 1);
					yield return new WaitForSeconds(changeAreaDelay);
					InitCharacter();
				}
			} else {
				canActiveTarget = true;
			}
		}

		if (!isGameEnd)
		{
			// Let player to start matching game again
			board.IsPause = false;
		}
	}
	#endregion

	#region Battle
	protected void InitCharacter()
	{
		selectedCharacter = null;
		StartCoroutine(_InitCharacter());
	}

	protected IEnumerator _InitCharacter()
	{
		yield return 0;
        // Characters
		if (currentArea == 0)
		{
            if (isInitCharacterById)
            {
                if (characters != null)
                {
                    for (int i = 0; i < characters.Count; ++i)
                    {
                        Character character = characters[i];
                        if (character != null)
                            Destroy(character.gameObject);
                    }
                    characters.Clear();
                }
                for (int i = 0; i < maxMemberQuantity; ++i)
                {
                    if (i >= character_ids.Count)
                    {
                        break;
                    }
                    Character character = null;
                    int character_id = character_ids[i];
                    if (character_id >= 1)
                    {
                        character = Instantiate(GameDatabase.instance.characters[character_id - 1]) as Character;
                        character.transform.parent = characterContainer;
                    }
                    characters.Add(character);
                }
            }

            for (int i = 0; i < maxMemberQuantity; ++i)
            {
                if (i >= characters.Count)
                {
                    break;
                }
				Character theCharacter = characters[i];
                if (theCharacter != null)
                {
                    theCharacter.MakeActorAsIcon();
                    theCharacter.controllerType = Character.ControllerType.NORMAL;
                    theCharacter.onAttack = OnCharacterAttack;
					theCharacter.onSkill = OnCharacterSkill;
					theCharacter.onSpawn = OnCharacterSpawn;
					theCharacter.onDead = OnCharacterDead;
					theCharacter.onTurnCountDown = OnCharacterTurnCountDown;
					theCharacter.onSkillTurnCountDown = OnCharacterSkillTurnCountDown;
					theCharacter.Spawn(this);
					currentMaxHp += theCharacter.currentMaxHp;
					try
					{
						theCharacter.transform.localPosition = characterPositions.positions[i];
					}
					catch
					{
						theCharacter.transform.localPosition = Vector3.zero;
					}
					theCharacter.transform.localScale = characterScale;
				}
			}

			if (currentMaxHp <= 0)
			{
				currentMaxHp = 1;
			}
			currentHp = currentMaxHp;
            hpBar.SetRate(1);
		}

        // Enemy characters
        if (enemyCharacters != null)
        {
            for (int i = 0; i < enemyCharacters.Count; ++i)
            {
                Character character = enemyCharacters[i];
                if (character != null)
                    Destroy(character.gameObject);
            }
            enemyCharacters.Clear();
        }

		if (currentArea >= 0 && currentArea < areaDataList.Count)
        {
            if (isInitCharacterById)
            {
                List<int> character_ids = areaDataList[currentArea].character_ids;
                for (int i = 0; i < maxMemberQuantity; ++i)
                {
                    if (i >= character_ids.Count)
                    {
                        break;
                    }
                    Character character = null;
                    int character_id = character_ids[i];
                    if (character_id >= 1)
                    {
                        character = Instantiate(GameDatabase.instance.characters[character_id - 1]) as Character;
                        character.transform.parent = enemyCharacterContainer;
                    }
                    enemyCharacters.Add(character);
                }
            }
            else
            {
                enemyCharacters = areaDataList[currentArea].characters;
            }

            int characterQuantity = enemyCharacters.Count;
            for (int i = 0; i < maxMemberQuantity; ++i)
            {
                if (i >= enemyCharacters.Count)
                {
                    break;
                }
				Character theCharacter = enemyCharacters[i];
				if (theCharacter != null)
                {
                    theCharacter.MakeActorAsPortrait();
                    theCharacter.controllerType = Character.ControllerType.AI;
                    theCharacter.onAttack = OnEnemyCharacterAttack;
                    theCharacter.onSkill = OnEnemyCharacterSkill;
					theCharacter.onSpawn = OnEnemyCharacterSpawn;
					theCharacter.onDead = OnEnemyCharacterDead;
					theCharacter.onTurnCountDown = OnEnemyCharacterTurnCountDown;
					theCharacter.onSkillTurnCountDown = OnEnemyCharacterSkillTurnCountDown;
					try
					{
						theCharacter.transform.localPosition = enemyCharacterPositions[characterQuantity - 1].positions[i];
					}
					catch
					{
						theCharacter.transform.localPosition = Vector3.zero;
					}
					theCharacter.transform.localScale = enemyCharacterScale;
					if (characterHpBar != null)
					{
						UIAbstractBar hpBar = Instantiate(characterHpBar) as UIAbstractBar;
						theCharacter.hpBar = hpBar;
						hpBar.transform.position = theCharacter.transform.position + characterHpBarOffset;
						hpBar.gameObject.SetActive(true);
					}
					if (characterTurnCountLabel != null)
					{
						UIAbstractLabel turnCountLabel = Instantiate(characterTurnCountLabel) as UIAbstractLabel;
						theCharacter.turnCountLabel = turnCountLabel;
						turnCountLabel.transform.position = theCharacter.transform.position + characterTurnCountLabelOffset;
						turnCountLabel.gameObject.SetActive(true);
					}
					theCharacter.Spawn(this);
				}
			}
		}
		currentArea++;
		canActiveTarget = true;
	}

	protected void ResetCharacters()
	{
		for (int i = 0; i < characters.Count; ++i)
		{
			Character theCharacter = characters[i];
			if (theCharacter != null) {
				theCharacter.atkMultiply = 0;
				theCharacter.atkSplash = false;
			}
		}
	}

	protected void PrepareCharactersTurn()
	{
		currentRcv = 0;
		for (int i = 0; i < characters.Count; ++i)
		{
			Character theCharacter = characters[i];
			if (theCharacter != null && theCharacter.currentHp > 0) {
				theCharacter.DecreaseTurn();
				theCharacter.DecreaseSkillTurn();
				currentRcv += theCharacter.totalRcv;
			}
		}
	}

	protected void PrepareEnemyCharactersTurn()
	{
		for (int i = 0; i < enemyCharacters.Count; ++i)
		{
			Character theCharacter = enemyCharacters[i];
			if (theCharacter != null && theCharacter.currentHp > 0) {
				theCharacter.DecreaseTurn();
				theCharacter.DecreaseSkillTurn();
			}
		}
	}

    public bool isAllCharacterActionEnd()
    {
        for (int i = 0; i < characters.Count; ++i)
        {
            Character character = characters[i];
            if (character == null || character.currentHp <= 0)
                continue;

            if (!character.isActionEnd)
            {
                return false;
            }
        }

        for (int i = 0; i < enemyCharacters.Count; ++i)
        {
            Character character = enemyCharacters[i];
            if (character == null || character.currentHp <= 0)
                continue;

            if (!character.isActionEnd)
            {
                return false;
            }
        }
        return true;
    }

    public void Attacked(int damage, float delay = 0)
	{
		currentHp -= damage;
	}

	public void Replay()
	{
		for (int i = 0; i < characters.Count; ++i)
		{
			Character theCharacter = characters[i];
			theCharacter.Rebirth();
		}
		board.IsPause = false;
		isGameEnd = false;
	}

	public void Dead()
	{
		for (int i = 0; i < characters.Count; ++i)
		{
			Character theCharacter = characters[i];
			theCharacter.Dead();
		}
	}

	protected void CheckEnemyCharactersDead()
	{
		int alive = 0;
		for (int i = 0; i < enemyCharacters.Count; ++i)
		{
			Character theCharacter = enemyCharacters[i];
			if (theCharacter != null && theCharacter.gameObject.activeSelf)
			{
				if (theCharacter.currentHp <= 0)
				{
					theCharacter.Dead();
				} else {
					++alive;
				}
			}
		}
	}
	#endregion
	
	
	#region Events 
	// Game events which developer could implements later.
	
	protected virtual void OnWin () {}
	
	protected virtual void OnLose () {}
	
	protected virtual void OnAreaChange (int area) {}
	
	protected virtual void OnCharacterAttack (Character owner, CharacterAction attack, int damage, Character target) {}
	
	protected virtual void OnCharacterSkill (Character owner, CharacterSkill skill, int damage, Character target) {}
	
	protected virtual void OnCharacterSpawn (Character owner) {}

	protected virtual void OnCharacterDead (Character owner) {}
	
	protected virtual void OnCharacterTurnCountDown (Character owner) {}
	
	protected virtual void OnCharacterSkillTurnCountDown (Character owner) {}
	
	protected virtual void OnEnemyCharacterAttack (Character owner, CharacterAction attack, int damage, Character target) {}
	
	protected virtual void OnEnemyCharacterSkill (Character owner, CharacterSkill skill, int damage, Character target) {}
	
	protected virtual void OnEnemyCharacterSpawn (Character owner) {}

	protected virtual void OnEnemyCharacterDead (Character owner) {}
	
	protected virtual void OnEnemyCharacterTurnCountDown (Character owner) {}
	
	protected virtual void OnEnemyCharacterSkillTurnCountDown (Character owner) {}
	
	#endregion

	public Character[] findCharactersByElement(int element)
	{
		List<Character> anCharacters = new List<Character> ();
		if (characters != null)
		{
			for (int i = 0; i < characters.Count; ++i)
			{
				Character theCharacter = characters[i];
				if (theCharacter != null && theCharacter.attr_element == element)
				{
					anCharacters.Add(theCharacter);
				}
			}
		}
		return anCharacters.ToArray();
	}

	public Character findAliveCharacter()
	{
		if (characters != null)
		{
			List<Character> anCharacters = new List<Character>(characters);
			anCharacters.Shuffle();
			for (int i = 0; i < anCharacters.Count; ++i)
			{
				Character theCharacter = anCharacters[i];
				if (theCharacter != null && theCharacter.currentHp > 0)
				{
					return theCharacter;
				}
			}
		}
		return null;
	}

	public Character findAliveEnemyCharacter()
	{
		if (enemyCharacters != null)
		{
			List<Character> anCharacters = new List<Character>(enemyCharacters);
			anCharacters.Shuffle();
			for (int i = 0; i < anCharacters.Count; ++i)
			{
				Character theCharacter = anCharacters[i];
				if (theCharacter != null && theCharacter.currentHp > 0)
				{
					return theCharacter;
				}
			}
		}
		return null;
	}
	
	public int calculateElementDamage(int damage, int element, int target_element)
	{
		int add_damage = Mathf.CeilToInt((float)damage * advantageElementDamageRate);
		for (int i = 0; i < elementPairs.Length; ++i)
		{
			ElementPair pair = elementPairs[i];
			if (pair.advantageElement == element && pair.disadvantageElement == target_element)
			{
				damage += add_damage;
			}
			if (pair.advantageElement == target_element && pair.disadvantageElement == element)
			{
				damage -= add_damage;
			}
		}
		return damage;
	}

	protected bool isComponentsReady()
	{
		return board != null && characters != null;
	}
}
