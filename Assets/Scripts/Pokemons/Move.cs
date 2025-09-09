using UnityEngine;

public class Move 
{
    public MoveBase Base { get; set; }
    public int pp { get; set; }
    public Move(MoveBase moveBase)
    {
        Base = moveBase;
        pp = moveBase.PP;
    }   

}
