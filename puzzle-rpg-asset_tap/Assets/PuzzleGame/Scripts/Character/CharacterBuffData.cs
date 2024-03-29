using UnityEngine;
using System.Collections;

[System.Serializable]
public class CharacterBuffData {
    public int buffIndex;
    public int currentLevel;
    public CharacterBuff buff
    {
        get
        {
            if (buffIndex >= 1)
            {
                try
                {
                    return GameDatabase.instance.buffs[buffIndex - 1];
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }
    }
}
