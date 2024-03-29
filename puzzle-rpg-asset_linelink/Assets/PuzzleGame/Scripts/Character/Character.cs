using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Character : MonoBehaviour {
	public enum ControllerType
	{
		NORMAL,
		AI
	}
	
	protected bool isAwaken = false;
	protected bool isMouseDownToActor = false;
	protected bool isDead = false;
	public GameManager manager { get; protected set; }
	public Transform _transform { get; protected set; }
    [HideInInspector]
	public ControllerType controllerType;
	[HideInInspector]
	public float atkMultiply;
	[HideInInspector]
	public bool atkSplash;
	// Price
	public int price_sell;
	public int price_upgrade;
	// General attributes
	public int attr_level;
	public int attr_max_level;
	public int attr_exp;
	public int attr_next_exp;
	public int attr_total_exp;
	public int attr_star;
	public int attr_cost;
	public int attr_element;
	// Min attributes
	public int attr_atk;
	public int attr_def;
	public int attr_rcv;
	public int attr_hp;
	// Max attributes
	public int attr_max_atk;
	public int attr_max_def;
	public int attr_max_rcv;
	public int attr_max_hp;
	// Fix attributes, while fixed use min attributes
	public bool fix_atk;
	public bool fix_def;
	public bool fix_rcv;
	public bool fix_hp;
	// Attribute calculate helper
	public float growth_atk;
	public float growth_def;
	public float growth_rcv;
	public float growth_hp;
	// Calculated attributes
	public int currentAtk { get; protected set; }
	public int currentDef { get; protected set; }
	public int currentRcv { get; protected set; }
	public int currentMaxHp { get; protected set; }
    // Events
	public System.Action<Character, CharacterAction, int, Character> onAttack = null;
	public System.Action<Character, CharacterSkill, int, Character> onSkill = null;
	public System.Action<Character> onSpawn = null;
	public System.Action<Character> onDead = null;
	public System.Action<Character> onTurnCountDown = null;
	public System.Action<Character> onSkillTurnCountDown = null;
    // UI Gadget
    public string title;
    public string description;
    public GameObject icon;
    public GameObject portrait;
    public GameObject actor { get; protected set; }
	[HideInInspector]
	public UIAbstractBar hpBar;
	[HideInInspector]
	public UIAbstractLabel turnCountLabel;

	// Skills & turns
    public int skillIndex;
    public int currentSkillLevel;
    public CharacterSkill skill;
	public int turn_length;
    protected List<CharacterBuff> buffs;

	// Action
    public CharacterAction attackAction;
    protected CharacterAction currentAction;
    public bool isActionEnd
    {
        get
        {
            if (currentAction == null)
                return true;

            return currentAction.isActionEnd(this);
        }
    }

    protected int hp;
    public int currentHp
    {
        get
        {
            return hp;
        }
        set
        {
			hp = value;
			if (hp > currentMaxHp)
			{
				hp = currentMaxHp;
			}
			if (hp <= 0)
			{
				hp = 0;
			}
			if (hpBar != null)
				hpBar.SetRate((float)hp / (float)currentMaxHp);
        }
	}

	protected int turn_count;
	public int turnCount
	{
		get
		{
			return turn_count;
		}
		set
		{
			turn_count = value;
			if (turn_count > turn_length)
			{
				turn_count = turn_length;
			}
			if (turn_count <= 0)
			{
				turn_count = 0;
			}
			if (turnCountLabel != null)
				turnCountLabel.SetText(turn_count.ToString("N0"));
		}
	}
	
	protected virtual void Awake() {
        if (isAwaken)
            return;

		_transform = transform;
		buffs = new List<CharacterBuff>();

		// Set current attribute
		if (this.attr_level < 1) {
			this.attr_level = 1;
		}
		if (this.attr_max_level < 1) {
			this.attr_max_level = 1;
		}
		int attr_level = this.attr_level >= 1 ? this.attr_level : 1;
		int attr_max_level = this.attr_max_level >= 2 ? this.attr_max_level : 2;
		if (fix_atk) {
			currentAtk = attr_atk;
		} else {
			currentAtk = attr_atk + ((attr_max_atk - attr_atk) * Mathf.CeilToInt(Mathf.Pow((attr_level - 1) / (attr_max_level - 1), growth_atk)));
		}

		if (fix_def) {
			currentDef = attr_def;
		} else {
			currentDef = attr_def + ((attr_max_def - attr_def) * Mathf.CeilToInt(Mathf.Pow((attr_level - 1) / (attr_max_level - 1), growth_def)));
		}

		if (fix_rcv) {
			currentRcv = attr_rcv;
		} else {
			currentRcv = attr_rcv + ((attr_max_rcv - attr_rcv) * Mathf.CeilToInt(Mathf.Pow((attr_level - 1) / (attr_max_level - 1), growth_rcv)));
		}

		if (fix_hp) {
			currentMaxHp = currentHp = attr_hp;
		} else {
			currentMaxHp = currentHp = attr_hp + ((attr_max_hp - attr_hp) * Mathf.CeilToInt(Mathf.Pow((attr_level - 1) / (attr_max_level - 1), growth_hp)));
		}

        if (skill == null)
        {
            if (skillIndex >= 1)
            {
                try
                {
                    skill = GameDatabase.instance.skills[skillIndex - 1];
                    skill.transform.parent = transform;
                    skill.transform.localPosition = Vector3.zero;
                }
                catch
                {
                    skill = null;
                }
            }
        }

		ResetTurn ();
		ResetSkillTurn ();
		isAwaken = true;
		gameObject.SetActive (false);
	}

	protected virtual void Update()
	{
		UpdateInput();
	}
	
	protected virtual void UpdateInput()
	{
		if (!manager.isUpdateInput)
		{
			return;
		}

		Ray ray = manager.inputCamera.ScreenPointToRay(Input.mousePosition);
		if (Input.GetMouseButtonDown(0))
		{
			if (!isMouseDownToActor && isTouchedActor(ray))
			{
				isMouseDownToActor = true;
			}
		} else if (Input.GetMouseButtonUp(0)) {
			if (isMouseDownToActor)
			{
				Touched();
				isMouseDownToActor = false;
			}
		}
	}
	
	protected bool isTouchedActor(Ray ray) {
		RaycastHit2D hit2d = Physics2D.Raycast(ray.origin, ray.direction);
		if (hit2d.collider != null)
		{
			if (hit2d.transform.gameObject.Equals(actor))
				return true;
		}
		
		RaycastHit hit3d;
		if (Physics.Raycast(ray, out hit3d))
		{
			if (hit3d.transform.gameObject.Equals(actor))
				return true;
		}
		
		return false;
	}

    protected virtual void OnDestroy()
    {
        if (hpBar != null)
        {
            Destroy(hpBar.gameObject);
		}

		if (turnCountLabel != null)
		{
			Destroy(turnCountLabel.gameObject);
		}

        if (actor != null)
        {
            Destroy(actor);
        }
	}
	
	public virtual void Touched()
	{
		manager.TouchedOnCharacter(this);
	}

	public void Spawn(GameManager manager)
	{
		if (onSpawn != null)
			onSpawn(this);

        if (!isAwaken)
            Awake();

        this.manager = manager;
		gameObject.SetActive (true);
		Rebirth();

		if (attackAction == null)
			attackAction = gameObject.AddComponent<CharacterAction>();
	}

    public void MakeActorAsIcon()
    {
        if (icon != null)
        {
            actor = Instantiate(icon) as GameObject;
            actor.transform.parent = transform;
            actor.transform.localPosition = Vector3.zero;
        }
    }

    public void MakeActorAsPortrait()
    {
        if (portrait != null)
        {
            actor = Instantiate(portrait) as GameObject;
            actor.transform.parent = transform;
            actor.transform.localPosition = Vector3.zero;
        }
    }

	public void DoAttack(Character target)
	{
		if (manager == null || attackAction == null || target == null)
		{
			return;
		}
		
		currentAction = null;
		
		if (controllerType == ControllerType.NORMAL && target.currentHp <= 0)
		{
			target = manager.findAliveEnemyCharacter();
		}
		if (target != null)
		{
			currentAction = attackAction;
			List<Character> actionTargets = new List<Character>();
			int damage = 0;
			if (controllerType == ControllerType.NORMAL)
			{
				if (atkSplash)
				{
					for (int i = 0; i < manager.enemyCharacters.Count; ++i)
					{
						Character _target = manager.enemyCharacters[i];
						if (_target != null && _target.currentHp > 0)
						{
							target = _target;
							damage = CalculateAttackDamage(this, target);
							if (damage <= 0)
								damage = 1;
							
							if (onAttack != null)
								onAttack(this, attackAction, damage, target);
							
							target.Attacked(damage, this, attackAction.actionDuration);
							actionTargets.Add(target);
						}
					}
				} else {
					damage = CalculateAttackDamage(this, target);
					if (damage <= 0)
						damage = 1;
					
					if (onAttack != null)
						onAttack(this, attackAction, damage, target);
					
					target.Attacked(damage, this, attackAction.actionDuration);
					actionTargets.Add(target);
				}
			}
			else if (controllerType == ControllerType.AI)
			{
				if (manager.currentHp > 0) {
					damage = CalculateAttackDamageAI(this);

					if (damage <= 0)
						damage = 1;
					
					if (onAttack != null)
						onAttack(this, attackAction, damage, target);
					
					manager.Attacked(damage, attackAction.actionDuration);
				}
			}
			attackAction.DoAction(this, actionTargets);
		}
	}
	
	public void DoSkill(Character target)
	{
		if (manager == null || skill == null || target == null)
		{
			return;
		}

		currentAction = null;
		
		if (controllerType == ControllerType.NORMAL && target.currentHp <= 0)
		{
			target = manager.findAliveEnemyCharacter();
		}
		if (target != null)
		{
			currentAction = skill;
			List<Character> actionTargets = new List<Character>();
			List<Character> characters = null, enemyCharacters = null;
			int damage = 0;
			if (controllerType == ControllerType.NORMAL)
			{
				characters = manager.characters;
				enemyCharacters = manager.enemyCharacters;
			}
			else if (controllerType == ControllerType.AI)
			{
				characters = manager.enemyCharacters;
				enemyCharacters = manager.characters;
			}
			// Attack characters
			if (controllerType == ControllerType.NORMAL)
			{
				if (skill.type == CharacterSkill.Type.ATK_ONE || skill.type == CharacterSkill.Type.ATK_PC_ONE)
				{
					damage = CalculateAttackDamage(this, target, skill);
					if (damage <= 0)
						damage = 1;

					if (onSkill != null)
						onSkill(this, skill, damage, target);

					target.Attacked(damage, this, skill.actionDuration);
					actionTargets.Add(target);
				}
				else if (skill.type == CharacterSkill.Type.ATK_ALL || skill.type == CharacterSkill.Type.ATK_PC_ALL)
				{
					for (int i = 0; i < enemyCharacters.Count; ++i)
					{
						Character _target = enemyCharacters[i];
						if (_target != null && _target.currentHp > 0)
						{
							target = _target;
							damage = CalculateAttackDamage(this, target, skill);
							if (damage <= 0)
								damage = 1;

							if (onSkill != null)
								onSkill(this, skill, damage, target);

							target.Attacked(damage, this, skill.actionDuration);
							actionTargets.Add(target);
						}
					}
				}
			}
			else if (controllerType == ControllerType.AI)
			{
				if (manager.currentHp > 0)
				{
					damage = CalculateAttackDamageAI(this, skill);

					if (damage <= 0)
						damage = 1;
					
					if (onSkill != null)
						onSkill(this, skill, damage, target);

					manager.Attacked(damage, skill.actionDuration);
				}
			}
			// Buffs active
			CharacterBuffData[] buffs = null;
			buffs = skill.buffs[currentSkillLevel - 1].buffs;
			if (buffs != null && buffs.Length > 0)
			{
				AddBuffsToCharacter(this, buffs);
			}
			buffs = skill.enemyBuffs[currentSkillLevel - 1].buffs;
			if (buffs != null && buffs.Length > 0)
			{
				AddBuffsToCharacter(target, buffs);
			}
			buffs = skill.teamBuffs[currentSkillLevel - 1].buffs;
			if (characters != null && buffs != null && buffs.Length > 0)
			{
				AddBuffsToCharacters(characters, buffs);
			}
			buffs = skill.enemyTeamBuffs[currentSkillLevel - 1].buffs;
			if (enemyCharacters != null && buffs != null && buffs.Length > 0)
			{
				AddBuffsToCharacters(enemyCharacters, buffs);
			}
			skill.DoAction(this, actionTargets);
		}
	}

	public int CalculateAttackDamageAI(Character attacker, CharacterSkill skill = null)
	{
		int damage = 0;
		if (skill == null)
		{
			damage = attacker.totalAtk;
		} else {
			damage = attacker.totalAtk;
			if (skill.type == CharacterSkill.Type.ATK_ONE || skill.type == CharacterSkill.Type.ATK_ALL)
			{
				damage = skill.value;
			}
			else if (skill.type == CharacterSkill.Type.ATK_PC_ONE || skill.type == CharacterSkill.Type.ATK_PC_ALL)
			{
				damage = damage * skill.value;
			}
		}
		return damage;
	}

	public int CalculateAttackDamage(Character attacker, Character target, CharacterSkill skill = null)
	{
		int damage = 0;
		if (skill == null)
		{
			damage = manager.calculateElementDamage (attacker.totalAtk, attacker.attr_element, target.attr_element);
			damage -= target.totalDef;
		} else {
			damage = manager.calculateElementDamage(attacker.totalAtk, attacker.attr_element, target.attr_element);
			if (skill.type == CharacterSkill.Type.ATK_ONE || skill.type == CharacterSkill.Type.ATK_ALL)
			{
				damage = skill.value;
			}
			else if (skill.type == CharacterSkill.Type.ATK_PC_ONE || skill.type == CharacterSkill.Type.ATK_PC_ALL)
			{
				damage = damage * skill.value;
			}
			damage -= target.totalDef;
		}
		return damage;
	}
	
	public void Attacked(int damage, Character from = null, float delay = 0)
	{
		if (delay > 0)
		{
			StartCoroutine(_Attacked(damage, from, delay));
		} else {	
			currentHp -= damage;
		}
	}

	private IEnumerator _Attacked(int damage, Character from, float delay)
	{
		yield return new WaitForSeconds(delay);
		currentHp -= damage;
	}
	
	public void Rebirth()
	{
		isDead = false;
		
		currentHp = currentMaxHp;
		ResetTurn();
		ResetSkillTurn();
	}
	
	public void Dead()
	{
		if (isDead)
			return;
		
		isDead = true;
		
		currentHp = 0;
		
		if (onDead != null)
			onDead(this);
	}
	
	public void ResetSkillTurn()
	{
		if (skill != null)
		{
			skill.ResetTurn();
		}
	}
	
	public void DecreaseSkillTurn(int count = 1)
	{
		if (skill != null)
		{
			skill.DecreaseTurn(count);
			if (onSkillTurnCountDown != null)
			{
				onSkillTurnCountDown(this);
			}
		}
	}
	
	public void ResetTurn()
	{
		turnCount = turn_length;
	}
	
	public void DecreaseTurn(int count = 1)
	{
		if (turnCount == 0)
		{
			return;
		}
		turnCount -= count;

		int i = 0;
		while (i < buffs.Count)
		{
			CharacterBuff buff = buffs[i];
			if (buff == null) {
				buffs.RemoveAt(i);
			}
			else
			{
				if (buff.type == CharacterBuff.Type.RECOVERY)
				{
					currentHp += buff.value;
				}
				buff.DecreaseTurn();
				if (buff.turn_count <= 0)
				{
					buffs.RemoveAt(i);
                    Destroy(buff);
				} else {
					i++;
				}
			}
		}

		if (onTurnCountDown != null)
		{
			onTurnCountDown(this);
		}
    }

    protected void AddBuffsToCharacter(Character character, CharacterBuffData[] buffs)
    {
        for (int i = 0; i < buffs.Length; ++i)
        {
            CharacterBuffData buff = buffs[i];
            character.AddBuff(buff.buff, buff.currentLevel);
        }
    }

    protected void AddBuffsToCharacters(List<Character> characters, CharacterBuffData[] buffs)
    {
        for (int i = 0; i < characters.Count; ++i)
        {
            for (int j = 0; j < buffs.Length; ++j)
            {
                CharacterBuffData buff = buffs[j];
                characters[i].AddBuff(buff.buff, buff.currentLevel);
            }
        }
    }

	public void AddBuff(CharacterBuff _buff, int _level)
	{
        CharacterBuff buff = Instantiate(_buff) as CharacterBuff;
        if (buff == null)
            return;
        buff.currentLevel = _level;
        buff.transform.parent = transform;
        buff.transform.localPosition = Vector3.zero;

		if (buff.type == CharacterBuff.Type.REMOVE_ALL)
		{
			buffs.Clear();
		}
		else if (buff.type == CharacterBuff.Type.REMOVE_RAND && buffs.Count > 0)
		{
			int removeIndex = Random.Range(0, buffs.Count);
			buffs.RemoveAt(removeIndex);
		}
		else if (buff.type == CharacterBuff.Type.RECOVERY)
		{
			if (buff.turn_count > 0)
			{
				buffs.Add(buff);
			} else {
				currentHp += buff.value;
			}
		} else {
			buffs.Add(buff);
		}
	}

	#region Sum attribute with buffs
	public int totalAtk {
		get {
			int atk = currentAtk;
			foreach (CharacterBuff buff in buffs)
			{
				if (buff != null && buff.type == CharacterBuff.Type.INC_ATK)
				{
					atk += buff.value;
				}
			}
			if (controllerType == ControllerType.NORMAL)
			{
				atk = Mathf.CeilToInt(atk * atkMultiply);
			}
			return atk;
		}
	}
	
	public int totalDef {
		get {
			int def = currentDef;
			foreach (CharacterBuff buff in buffs)
			{
				if (buff != null && buff.type == CharacterBuff.Type.INC_DEF)
				{
					def += buff.value;
				}
			}
			return def;
		}
	}
	
	public int totalRcv {
		get {
			int rcv = currentRcv;
			foreach (CharacterBuff buff in buffs)
			{
				if (buff != null && buff.type == CharacterBuff.Type.INC_RCV)
				{
					rcv += buff.value;
				}
			}
			return rcv;
		}
	}
	#endregion
}
