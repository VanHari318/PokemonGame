using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Pokemon", menuName = "Pokemon/Create new Pokemon")]
public class PokemonBase : ScriptableObject
{
    [SerializeField] string name;
    [TextArea]
    [SerializeField] string description;
    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite;
    [SerializeField] PokemonType type1;
    //[SerializeField] PokemonType type2;

    //Base Stats
    [SerializeField] int maxHp;
    [SerializeField] int attack;
    [SerializeField] int defense;
    [SerializeField] int spAttack;
    [SerializeField] int spDefense;
    [SerializeField] int speed;
    [SerializeField] List<LearnableMove> LearnableMoves;
    //Methods to access the private fields
    public string Name
    {
        get { return name; }
    }
    public string Description
    {
        get { return description; }
    }
    public Sprite FrontSprite
    {
        get { return frontSprite; }
    }
    public Sprite BackSprite
    {
        get { return backSprite; }
    }
    public PokemonType Type1
    {
        get { return type1; }
    }
    //public PokemonType Type2
    //{
    //    get { return type2; }
    //}
    public int MaxHp
    {
        get { return maxHp; }
    }
    public int Attack
    {
        get { return attack; }
    }
    public int Defense
    {
        get { return defense; }
    }
    public int SpAttack
    {
        get { return spAttack; }
    }
    public int SpDefense
    {
        get { return spDefense; }
    }
    public int Speed
    {
        get { return speed; }
    }
    public List<LearnableMove> LearnableMovesList
    {
        get { return LearnableMoves; }
    }


}
[System.Serializable]
public class LearnableMove
{
    [SerializeField] MoveBase moveBase;
    [SerializeField] int level;
    public MoveBase Base
    {
        get { return moveBase; }
    }
    public int Level
    {
        get { return level; }
    }
}
public enum PokemonType
{
    Normal,
    Fire,
    Water,
    Grass,
    Ground,
    Metal

}
public class TypeChart
{
    float[][] chart =
    {
        //                  NOR    FIR      WAT     GRA     GRO     MET
        /*NOR*/ new float[] { 1f,   1f,     1f,     1f,     1f,     0.5f },
        /*FIR*/ new float[] { 1f,   0.5f,   0.5f,   2f,     1f,     2f },
        /*WAT*/ new float[] { 1f,   2f,     0.5f,   0.5f,   2f,     1f },
        /*GRA*/ new float[] { 1f,   0.5f,   2f,     0.5f,   0.5f,   1f },
        /*GRO*/ new float[] { 1f,   2f,     1f,     2f,     1f,     1f },
        /*MET*/ new float[] { 1f,   0.5f,   1f,     1f,     2f,     0.5f }
    };
    public static float GetEffectiveness(PokemonType attackType, PokemonType defenseType)
    {
        int row = (int)attackType;
        int col = (int)defenseType;
        return new TypeChart().chart[row][col];
    }
}
