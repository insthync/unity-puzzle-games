using UnityEngine;
using System.Collections;

public class CharacterBuff : MonoBehaviour {
	public enum Type
	{
		RECOVERY,
		INC_ATK,
		INC_RCV,
		INC_DEF,
		CHANGE_ELEMENT,
		REMOVE_RAND,
		REMOVE_ALL
    }
    public string title;
    public string description;
	public Type type;
    public GameObject icon;
    [HideInInspector]
    public int currentLevel;
    public int maxLevel;
    public int[] turnLength;
    public int[] values;
    public int value
    {
        get
        {
            return values[currentLevel - 1];
        }
    }
    public int turn_length
    {
        get
        {
            return turnLength[currentLevel - 1];
        }
    }
	public int turn_count { get; protected set; }
    void Awake()
    {
        if (turnLength == null || turnLength.Length == 0)
        {
            turnLength = new int[maxLevel];
            for (int i = 0; i < maxLevel; ++i)
            {
                turnLength[i] = 0;
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
    }

	public void ResetTurn()
	{
		turn_count = turn_length;
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
