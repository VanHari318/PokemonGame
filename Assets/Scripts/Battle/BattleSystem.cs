using System;
using System.Collections;
using UnityEngine;
public enum BattleState { Start, PlayerAction, PlayerMove, EnemyMove, Busy }

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleHud playerHud;

    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHud enemyHud;

    [SerializeField] BattleDialogBox dialogBox;

    public event Action<bool> OnBattleOver; 

    BattleState state;
    public void StartBattle()
    {
        StartCoroutine(SetUpBattle());
    }
    public IEnumerator SetUpBattle()
    {
        playerUnit.SetUp();
        playerHud.SetData(playerUnit.Pokemon);
        enemyUnit.SetUp();
        enemyHud.SetData(enemyUnit.Pokemon);
        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
        yield return dialogBox.TypeDialog($"Demon {enemyUnit.Pokemon.Base.name} appear!");
        PlayerAction();
    }
    public IEnumerator PerformPlayerMove(int moveindex)
    {
        state = BattleState.Busy;
        var move = playerUnit.Pokemon.Moves[moveindex];
        yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.name} used {move.Base.MoveName}!");
        var damegeDetails = enemyUnit.Pokemon.TakeDamage(move, playerUnit.Pokemon);
        yield return enemyHud.UpdateHp();
        yield return ShowDamageDetails(damegeDetails);  
        if (damegeDetails.Die)
        {
            yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.name} die!");
            yield return new WaitForSeconds(2f);
            OnBattleOver(true);
        }
        else
        {
            StartCoroutine(EnemyMove());
        }
    }
    public IEnumerator EnemyMove()
    {
        state = BattleState.EnemyMove;
        var move = enemyUnit.Pokemon.GetRandomMove();
        yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.name} used {move.Base.MoveName}!");
        var damegeDetails = playerUnit.Pokemon.TakeDamage(move, enemyUnit.Pokemon);
        yield return playerHud.UpdateHp();
        yield return ShowDamageDetails(damegeDetails);
        if (damegeDetails.Die)
        {
            yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.name} die!");
            yield return new WaitForSeconds(2f);
            OnBattleOver(false);
        }
        else
        {
            PlayerAction();
        }
    }

    public IEnumerator ShowDamageDetails(DamegeDetails damegeDetails)
    {
        if (damegeDetails.Critical > 1f)
            yield return dialogBox.TypeDialog("A critical hit!");
        if (damegeDetails.Type > 1f)
            yield return dialogBox.TypeDialog("It's super effective!");
        else if (damegeDetails.Type < 1f)
            yield return dialogBox.TypeDialog("It's not very effective...");
    }

    public void PlayerAction()
    {
        state = BattleState.PlayerAction;
        StartCoroutine(dialogBox.TypeDialog("Choose an action."));
        dialogBox.EnableActionSelector(true);
    }
    public void PlayerMove()
    {
        state = BattleState.PlayerMove;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }
    public void UpDateMoveSelect(int current)
    {
        dialogBox.UpdateMoveSelection(playerUnit.Pokemon.Moves[current]);
    }
    public void HanderMoveSelection(int current)
    {
        dialogBox.EnableMoveSelector(false);
        dialogBox.EnableDialogText(true);
        StartCoroutine(PerformPlayerMove(current));
    }
}
