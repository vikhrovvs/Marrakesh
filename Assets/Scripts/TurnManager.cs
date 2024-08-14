using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Runtime.Serialization;


public enum TurnPhase
{
    NotMyTurn,
    DirectionChoosing,
    DiceRolling,
    Moving,
    CarpetPlacement
}


public class TurnManager : MonoBehaviourPunCallbacks
{
    public static TurnManager Instance;

    private int currentPlayerIndex;
    private Player[] players;
    private Game m_Game;

    private TurnPhase m_TurnPhase = TurnPhase.NotMyTurn;

    [SerializeField] private Text rollResultText;
    [SerializeField] private Text carpetColorText;


    [SerializeField] private GameObject m_DirectionChoosing;
    [SerializeField] private GameObject m_DiceRolling;
    [SerializeField] private GameObject m_Movement;
    [SerializeField] private GameObject m_CarpetPlacement;


    [SerializeField] private Text m_IncorrectCarpetPlacementText;
    [SerializeField] private float m_IncorrectCarpetPlacementTextFadeoutDuration = 1f;


    void Awake()
    {
        Instance = this;
        // TODO: not utilizing. Maybe should
    }

    public void SetGame(Game game) 
    {
        m_Game = game;
    }

    void Start()
    {
        Debug.Log("starting");
        Debug.Log($"I am {PhotonNetwork.LocalPlayer.ActorNumber} player");
        InitializeUI();

        currentPlayerIndex = 0;
        players = PhotonNetwork.PlayerList;
        m_Game.InitPlayers(players.Length);


        // Точно не нужно на всех? Короче нужно раздебажить
        if (PhotonNetwork.IsMasterClient)
        {
            // Debug.Log($"isConnected: {PhotonNetwork.IsConnected}");
            // Debug.Log($"inRoom: {PhotonNetwork.InRoom}");
            // Debug.Log(players);
            // Debug.Log(players.Length);
            // Debug.Log(players[0].ActorNumber);
            // Debug.Log($"");

            photonView.RPC("StartTurn", RpcTarget.All, players[currentPlayerIndex].ActorNumber);
        }
        Debug.Log("started");



        // TODO fix
        // StartTurn(0);

    }

    private void InitializeUI()
    {
        m_DirectionChoosing.SetActive(false);
        m_DiceRolling.SetActive(false);
        m_Movement.SetActive(false);
        m_CarpetPlacement.SetActive(false);
        MakeIncorrectCarpetPlacementMessageTransparent();
    }

    [PunRPC]
    void StartTurn(int playerActorNumber)
    {
        Debug.Log("It's player " + playerActorNumber + "'s turn");

        // Notify all players whose turn it is
        if (PhotonNetwork.LocalPlayer.ActorNumber == playerActorNumber)
        {
            // Debug.Log("Setting active");
            // Enable UI for the current player to take their turn
            m_DirectionChoosing.SetActive(true);
            m_TurnPhase = TurnPhase.DirectionChoosing;
        }
        else
        {
            // Disable UI for other players
        }
    }

    public void EndTurn()
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == players[currentPlayerIndex].ActorNumber)
        {
            photonView.RPC("NextTurn", RpcTarget.All);
        }
    }

    private bool GameShouldBeFinished()
    {
        Debug.Log("Checking if the game should be finished");
        Debug.Log($"Current player index is? {currentPlayerIndex}");
        Debug.Log($"Carpet value is? {m_Game.GetCarpetValue(0)}");
        return (currentPlayerIndex == 0) && (m_Game.GetCarpetValue(0) == 0);
    }

    [PunRPC]
    void NextTurn()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Length;
        if (GameShouldBeFinished())
        {
            m_Game.FinishGame();
        }
        else
        {
            photonView.RPC("StartTurn", RpcTarget.All, players[currentPlayerIndex].ActorNumber);
        }
    }

    // doesn't it send StartTurn call once each time for all the players?

    public bool IsMyTurn()
    {
        // TODO: use m_TurnPhase if confirmed to work properly?
        return PhotonNetwork.LocalPlayer.ActorNumber == players[currentPlayerIndex].ActorNumber;
    }

    public TurnPhase GetTurnPhase()
    {
        return m_TurnPhase;
    }

    void UpdateTurnUI(bool isMyTurn)
    {
        // Enable or disable UI elements based on the current turn
        // Example: actionButton.SetActive(isMyTurn);


        // храним фазу, тут апдейтим UI - потом
        // сейчас пока просто делаем, чтобы работало
    }

    public void RotateLeft()
    {
        m_Game.photonView.RPC("RotateLeft", RpcTarget.All);
        // m_Game.RotateLeft();
        BeginRollingPhase();

    }

    public void RotateRight()
    {
        m_Game.photonView.RPC("RotateRight", RpcTarget.All);
        BeginRollingPhase();

    }

    public void GoForward()
    {
        BeginRollingPhase();
    }

    private void BeginRollingPhase()
    {
        m_TurnPhase = TurnPhase.DiceRolling;
        m_DirectionChoosing.SetActive(false);
        m_DiceRolling.SetActive(true);
    }

    public void RollDice()
    {
        int rollResult = UnityEngine.Random.Range(1, 4);
        // Debug.Log($"rolled {rollResult}");
        m_Game.photonView.RPC("AcceptMoveResult", RpcTarget.All, rollResult);
        // m_Game.AcceptMoveResult(rollResult);
        rollResultText.text = $"Rolled {rollResult}";


        m_Game.StartMovingPhase(); 
        // Executed only for the local game! (But we are ok with that)

        m_TurnPhase = TurnPhase.Moving;
        m_DiceRolling.SetActive(false);
        m_Movement.SetActive(true);
    }

    public void Move()
    {
        StartCoroutine(MakeMove());
    }

    private IEnumerator MakeMove()
    {
        // m_Game.Move();
        m_Game.StartMovingPhase();
        m_Game.photonView.RPC("Move", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber - 1);
        while (m_Game.IsMovingPhase())
        {
            yield return null;
        }

        m_TurnPhase = TurnPhase.CarpetPlacement;
        m_Movement.SetActive(false);
        m_CarpetPlacement.SetActive(true);
    }


    public void ChangeOrientation()
    {
        // m_Game.ChangeOrientation();
        m_Game.photonView.RPC("ChangeOrientation", RpcTarget.All);
    }

    private void Update()
    {
        // TODO make controller
        if (!(Input.GetMouseButtonDown(0) && m_TurnPhase == TurnPhase.CarpetPlacement))
        {
            return;
        }
        if (!m_Game.HasSelectedNode())
        {
            return;
        }
        if (!m_Game.IsCurrentCarpetPlacementCorrect())
        {
            ShowIncorrectCarpetPlacementMessage();
            return;
        }

        Tuple<Vector2Int, Vector2Int> selectedNodes =  m_Game.GetSelectedNodesCoordinates();
        Vector2Int firstNodeCoordinates = selectedNodes.Item1;
        Vector2Int secondNodeCoordinates = selectedNodes.Item2;

        // might also serialize 
        // TODO maybe later 
        int firstNodeX = firstNodeCoordinates.x;
        int firstNodeY = firstNodeCoordinates.y;

        int secondNodeX = secondNodeCoordinates.x;
        int secondNodeY = secondNodeCoordinates.y;

        m_Game.photonView.RPC("SpawnCarpet", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber,
            firstNodeX, firstNodeY, secondNodeX, secondNodeY);
    }

    private void ShowIncorrectCarpetPlacementMessage()
    {
        MakeIncorrectCarpetPlacementMessageOpaque();
        StartCoroutine(FadeOutIncorrectCarpetPlacementMessage());
    }

    private IEnumerator FadeOutIncorrectCarpetPlacementMessage()
    {
        float startAlpha = m_IncorrectCarpetPlacementText.color.a;
        float rate = 1.0f / m_IncorrectCarpetPlacementTextFadeoutDuration;
        float progress = 0.0f;

        while (progress < 1.0f)
        {
            Color tmpColor = m_IncorrectCarpetPlacementText.color;
            tmpColor.a = Mathf.Lerp(startAlpha, 0, progress);
            m_IncorrectCarpetPlacementText.color = tmpColor;
            progress += rate * Time.deltaTime;

            yield return null;
        }
        
        MakeIncorrectCarpetPlacementMessageTransparent();
    }

    private void MakeIncorrectCarpetPlacementMessageTransparent()
    {
        Color tmpColor = m_IncorrectCarpetPlacementText.color;
        tmpColor.a = 0;
        m_IncorrectCarpetPlacementText.color = tmpColor;
    }

    private void MakeIncorrectCarpetPlacementMessageOpaque()
    {
        Color tmpColor = m_IncorrectCarpetPlacementText.color;
        tmpColor.a = 1;
        m_IncorrectCarpetPlacementText.color = tmpColor;
    }


    public void IncreaseCarpetColor()
    {
        // int carpetColor = m_Game.IncreaseCarpetColor();
    // int carpetColor = m_Game.photonView.RPC("IncreaseCarpetColor", RpcTarget.All);
    // carpetColorText.text = $"Change Color ({carpetColor})";
        m_Game.photonView.RPC("IncreaseCarpetColor", RpcTarget.All);
        Debug.Log("changed color");

    }

    public void EndCarpetPlacement()
    {
        m_CarpetPlacement.SetActive(false);
        EndTurn();
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.LoadLevel("Lobby");
    }
}
