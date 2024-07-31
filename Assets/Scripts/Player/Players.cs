using System.Collections.Generic;
using System.Linq; 
using UnityEngine;


public class Players
{
    private int n_players;
    private int defaultMoneyNumber = 30;

    private Dictionary<int, int> defaultCarpetNumber = new Dictionary<int, int> 
    {
        {1, 5},  // debug purposes
        {2, 12},
        {3, 15},
        {4, 12},
    };


    private List<int> moneyAmount = new List<int>();
    private List<int> carpetAmount = new List<int>();
    public Players (int N_players)
    {
        n_players = N_players;
        int carpetNumber = defaultCarpetNumber[n_players];
        for (int i = 0; i < n_players; ++i)
        {
            moneyAmount.Add(defaultMoneyNumber);
            carpetAmount.Add(carpetNumber);
        }
    }

    public List<int> GetMoneyValues()
    {
        return moneyAmount;
    }

    public List<int> GetCarpetValues()
    {
        return carpetAmount;
    }

    public void DecreaseCarpetAmount(int player)
    {
        carpetAmount[player] -= 1;
    }

    private void DeductMoney(int player, int moneyToDeduct)
    {
        // TODO this player ends game
        // пока что берем микрозайм
        moneyAmount[player] -= moneyToDeduct;
    }

    private void AddMoney(int player, int moneyToAdd)
    {
        moneyAmount[player] += moneyToAdd;

    }

    private void TransferMoney(int fromPlayer, int toPlayer, int amount)
    {
        AddMoney(toPlayer, amount);
        DeductMoney(fromPlayer, amount);
    }

    public void FinishMovementOnColor(int color, int area, int player)
    {
        if (color == -1 || color == player)
        {
            return;
        }
        Debug.Log($"Transferring {area} from {player} to {color}");
        TransferMoney(player, color, area);
        LogMoney();
    }

    public void LogMoney()
    {
        string logMessage = "Logging money: ";
        for (int i = 0; i < moneyAmount.Count; i++)
        {
            logMessage += "Player " + i + ": " + moneyAmount[i] + "; ";
        }
        Debug.Log(logMessage);
    }
}
