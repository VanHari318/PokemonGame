using UnityEngine;
[CreateAssetMenu(fileName = "Move", menuName = "Pokemon/Creat new Move")]
public class MoveBase : ScriptableObject
{
    [SerializeField] string moveName;
    [TextArea]
    [SerializeField] string description;
    [SerializeField] int power;
    [SerializeField] int accuracy;
    [SerializeField] int pp;
    [SerializeField] PokemonType type;

    public string MoveName
    {
        get { return moveName; }
    }
    public string Description
    {
        get { return description; }
    }
    public int Power
    {
        get { return power; }
    }
    public int Accuracy
    {
        get { return accuracy; }
    }
    public int PP
    {
        get { return pp; }
    }
    public PokemonType Type
    {
        get { return type; }
    }

}
