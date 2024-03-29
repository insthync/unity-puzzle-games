using UnityEngine;
using System.Collections;

public class GameDatabase : MonoBehaviour {
    public CharacterSkill[] skills;
    public CharacterBuff[] buffs;
    public Character[] characters;
    public static GameDatabase instance { get; protected set; }
	void Awake () {
        if (instance != null)
        {
            Destroy(instance.gameObject);
        }
        instance = this;
	}
}
