using Photon.Pun;
using UnityEngine.UI;
using UnityEngine;


public class PlayerUI: MonoBehaviourPunCallbacks
{
    [SerializeField] private Text playerText;  // TODO material?
    [SerializeField] private Text moneyText;
    [SerializeField] private Text carpetCountText;

    public void Initialize(int playerNumber, int moneyNumber, int carpetNumber)
    {
        playerText.text = $"Player {playerNumber}";
        moneyText.text = $"Money: {moneyNumber}";
        carpetCountText.text = $"Carpets: {carpetNumber}";
    }

    public void SetMoney(int money)
    {
        moneyText.text = $"Money: {money}";
    }

    public void SetCarpetCount(int carpetCount)
    {
        carpetCountText.text = $"Carpets: {carpetCount}";
    }

    public void Highlight()
    {

    }

    public void RemoveHighlight()
    {

    }
}