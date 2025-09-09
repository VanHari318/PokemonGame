using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleHud : MonoBehaviour
{
    [SerializeField] HpBar hpBar;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI levelText;

    Pokemon _pokemon;
    public void SetData(Pokemon pokemon)
    {
        _pokemon = pokemon;
        nameText.text = pokemon.Base.Name + "" + pokemon.Hp + "/" + pokemon.MaxHp;
        levelText.text = "Lvl " + pokemon.Level;
        hpBar.SetHp((float)pokemon.Hp / pokemon.MaxHp);
    }
    public IEnumerator UpdateHp()
    {
        yield return hpBar.SetHpSmooth((float)_pokemon.Hp / _pokemon.MaxHp);
    }
}
