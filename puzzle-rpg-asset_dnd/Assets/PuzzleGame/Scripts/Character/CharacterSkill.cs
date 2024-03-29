using UnityEngine;
using System.Collections;

public class CharacterSkill : CharacterAction {
	public enum Type
    {
        NONE,
        ATK_ONE,
        ATK_ALL,
        ATK_PC_ONE,
        ATK_PC_ALL
	}
	public string title;
	public string description;
    public Type type;
    public GameObject icon;
    [HideInInspector]
    public int currentLevel;
    public int maxLevel;
    public bool isChangeNodeElement;
    public int changeNodeFrom;
    public int changeNodeTo;
    public CharacterBuffDataList[] buffs;
    public CharacterBuffDataList[] teamBuffs;
    public CharacterBuffDataList[] enemyBuffs;
    public CharacterBuffDataList[] enemyTeamBuffs;
    public int[] turnCost;
    public int[] values;
    public int value
    {
        get
        {
            return values[currentLevel - 1];
        }
    }
    public int turn_cost
    {
        get
        {
            return turnCost[currentLevel - 1];
        }
    }
    public int turn_count { get; protected set; }
    protected override void Awake()
    {
        if (turnCost == null || turnCost.Length == 0)
        {
            turnCost = new int[maxLevel];
            for (int i = 0; i < maxLevel; ++i)
            {
                turnCost[i] = 0;
            }
        }
        if (values == null || values.Length == 0)
        {
            values = new int[maxLevel];
            for (int i = 0; i < maxLevel; ++i)
            {
                values[i] = 0;
            }
        }
		base.Awake();
    }

    public void ResetTurn()
    {
        turn_count = turn_cost;
    }

    public void DecreaseTurn(int count = 1)
    {
        if (turn_count == 0)
        {
            return;
        }
        turn_count -= count;
        if (turn_count < 0)
        {
            turn_count = 0;
        }
    }
}
