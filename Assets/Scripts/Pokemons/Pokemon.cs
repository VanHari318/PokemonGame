using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class Pokemon
{
    public PokemonBase Base { get; set; }
    public int Level { get; set; }
    public int Hp { get; set; }
    public List<Move> Moves { get; set; }

    public Pokemon(PokemonBase pBase, int pLevel)
    {
        Base = pBase;
        Level = pLevel;
        Hp = MaxHp;
        Moves = new List<Move>();
        //Generate move
        foreach (var move in Base.LearnableMovesList)
        {
            if (move.Level <= Level)
                Moves.Add(new Move(move.Base));
            if (Moves.Count >= 4)
                break;
        }
    }
    public int Attack
    {
        get { return Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5; }
    }
    public int SpAttack
    {
        get { return Mathf.FloorToInt((Base.SpAttack * Level) / 100f) + 5; }
    }
    public int Defense
    {
        get { return Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5; }
    }
    public int SpDefense
    {
        get { return Mathf.FloorToInt((Base.SpDefense * Level) / 100f) + 5; }
    }
    public int Speed
    {
        get { return Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5; }
    }
    public int MaxHp
    {
        get { return Mathf.FloorToInt((Base.MaxHp * Level) / 100f) + 20; }
    }
    public DamegeDetails TakeDamage(Move move, Pokemon attacker)
    {
        float critical = 1f;
        if(Random.Range(1,100) <= 50f)
            critical = 2f;
        float type = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1);
        var damageDetails = new DamegeDetails()
        {
            Critical = critical,
            Type = type,
            Die = false
        };
        float modifier = Random.Range(8.5f, 1f) * type * critical;
        float a = 2 * attacker.Level;
        float d = a * move.Base.Power * ((float)attacker.Attack / Defense) + 2;
        int damage = Mathf.FloorToInt(d * modifier);
        Debug.Log(damage);
        Hp -= damage;
        if (Hp <= 0)
        {
            Hp = 0;
            damageDetails.Die = true;
        }
        else
        {
            
        }
        return damageDetails;
    }
    public Move GetRandomMove()
    {
        int r = Random.Range(0, Moves.Count);
        return Moves[r];
    }
}
public class DamegeDetails
{
    public bool Die { get; set; }
    public float Critical { get; set; }
    public float Type { get; set; }
}