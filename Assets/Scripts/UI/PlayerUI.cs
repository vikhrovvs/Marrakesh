using Photon.Pun;
using UnityEngine.UI;
using UnityEngine;


public class PlayerUI: MonoBehaviourPunCallbacks
{
    [SerializeField] private Image background;
    [SerializeField] private float m_InactivePlayerOpacity = 0.8f;
    [SerializeField] private Text playerText;  // TODO material?
    [SerializeField] private Text moneyText;
    [SerializeField] private Text carpetCountText;

    public void Initialize(int playerNumber, int moneyNumber, int carpetNumber, string nickname)
    {
        playerText.text = $"Player {playerNumber}\n{nickname}";
        moneyText.text = $"Money: {moneyNumber}";
        carpetCountText.text = $"Carpets: {carpetNumber}";
        RemoveHighlight();
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
        Color tmpColor = background.color;
        tmpColor.a = 1;
        background.color = tmpColor;
    }

    public void RemoveHighlight()
    {
        Color tmpColor = background.color;
        tmpColor.a = m_InactivePlayerOpacity;
        background.color = tmpColor;
    }
}