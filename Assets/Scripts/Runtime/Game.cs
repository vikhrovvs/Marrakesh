using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;


public class Game : MonoBehaviourPunCallbacks
{
    private Runner m_Runner;
    private GridHolder m_GridHolder;
    private Assam m_Assam;
    private TurnManager m_TurnManager;
    private int m_RollResult = 1;
    [SerializeField] private float m_DelayBetweenSteps = 0.1f;
    [SerializeField] private GameObject PlayerUIParent;
    [SerializeField] private PlayerUI playerUIPrefab;

    [SerializeField] private GameObject m_GameFinishUI;
    [SerializeField] private Text m_GameFinishedText;

    private Players m_Players;
    private int m_PlayerCount;
    private bool m_IsMovingPhase = false;

    void Awake()
    {
        m_Runner = FindObjectOfType<Runner>();
        m_Runner.StartRunning();

        m_GridHolder = FindObjectOfType<GridHolder>();
        m_GridHolder.SetGame(this);
        m_GridHolder.CreateGrid();

        m_Assam = FindObjectOfType<Assam>();
        m_TurnManager = FindObjectOfType<TurnManager>();
        m_TurnManager.SetGame(this);
    }


    public void InitPlayers(Player[] players)
    {
        int n_players = players.Length;
        m_Players = new Players(n_players, this);
        m_PlayerCount= n_players;
        CreatePlayersUI(players);
    }

    public void CreatePlayersUI(Player[] players)
    {
        List<int> moneyValues = m_Players.GetMoneyValues();
        List<int> carpetValues = m_Players.GetCarpetValues();

        List<PlayerUI> playerUIs = new List<PlayerUI>();
        for (int i = 0; i < m_PlayerCount; ++i)
        {
            string nickname = players[i].NickName;

            PlayerUI playerUI = Instantiate(playerUIPrefab);
            playerUI.Initialize(i, moneyValues[i], carpetValues[i], nickname);
            playerUI.gameObject.transform.SetParent(PlayerUIParent.transform, false);

            playerUIs.Add(playerUI);
        }

        m_Players.SetPlayerUIs(playerUIs);
    }

    public void HighlightActivePlayer(int playerIdx)
    {
        m_Players.HighlightActivePlayer(playerIdx);
    }

    public void RemoveInactivePlayerHighlight(int playerIdx)
    {
        m_Players.RemoveInactivePlayerHighlight(playerIdx);
    }

    public void TryToMoveAssam(Vector3 target)
    {
        m_Assam.TryMoveToPoint(target);
    }

    [PunRPC]
    public void RotateLeft()
    {
        m_Assam.RotateLeft();
    }


    [PunRPC]
    public void RotateRight()
    {
        m_Assam.RotateRight();
    }

    public void Rotate180()
    {
        // not very useful rn, might be useful if added animation
        m_Assam.RotateRight();
        m_Assam.RotateRight();
    }

    [PunRPC]
    public void AcceptMoveResult(int result) 
    {
        m_RollResult = result;
        // Debug.Log($"accepted result{result}");
    }

    public void StartMovingPhase()
    {
        m_IsMovingPhase = true;
    }

    private void FinishMovingPhase()
    {
        m_IsMovingPhase = false;
    }

    public bool IsMovingPhase()
    {
        return m_IsMovingPhase;
    }
    
    [PunRPC]
    public void Move(int playerNumber)
    {
        StartCoroutine(StartMovingAlongPath(playerNumber));
    }

    private IEnumerator StartMovingAlongPath(int playerNumber)
    {
        yield return StartCoroutine(MoveAlongPath());

        FinishMovingPhase();

        Tuple<int, int> ColorAndArea = m_GridHolder.GetCurrentColorAndArea();
        int color = ColorAndArea.Item1;
        int area = ColorAndArea.Item2;
        Debug.Log($"Finished on a carpet cluster of color {color} and area {area}");

        m_Players.FinishMovementOnColor(color, area, playerNumber);
    }

    public Vector3 GetAssamPosition()
    {
        return m_Assam.transform.position;
    }

    private IEnumerator MoveAlongPath()
    {
        int idx = 0;
        int rollResult = m_RollResult;
        while (idx < rollResult)
        {
            if (!m_Assam.IsMoving())
            {
                if (idx > 0)
                {
                    yield return new WaitForSeconds(m_DelayBetweenSteps);
                }
                Direction direction = m_Assam.GetDirection();
                NodeData currentNodeData = m_GridHolder.GetNextNodeInDirectionAndRotate(direction);
                Vector3 target = currentNodeData.Position;
                TryToMoveAssam(target);
                ++idx;
            }
            yield return null;
        }
        // wait so the movement 100% begins
        float delayBetweenStart = 2 * Time.deltaTime; 
        yield return new WaitForSeconds(delayBetweenStart);
        // wait for Assam to stop
        while (m_Assam.IsMoving())
        {
            yield return null;
        }
        Debug.Log("finished movement");

    }

    [PunRPC]
    public void ChangeOrientation()
    {
        m_GridHolder.ChangeOrientation();
    }

    public int IncreaseCarpetColor()
    {
        m_GridHolder.CarpetColor += 1;
        return m_GridHolder.CarpetColor;
    }

    public TurnPhase GetTurnPhase()
    {
        return m_TurnManager.GetTurnPhase();
    }


    public bool IsCurrentCarpetPlacementCorrect()
    {
        return m_GridHolder.IsCurrentCarpetPlacementCorrect();
    }
    public bool HasSelectedNode()
    {
        return m_GridHolder.HasSelectedNode();
    }

    public Tuple<Vector2Int, Vector2Int> GetSelectedNodesCoordinates()
    {
        return m_GridHolder.GetSelectedNodesCoordinates();
    }

    [PunRPC]
    public void SpawnCarpet(int playerNumber, int firstNodeX, int firstNodeY, int secondNodeX, int secondNodeY)
    {
        int carpetIdx = playerNumber - 1;
        Vector2Int firstNodeCoordinates = new Vector2Int(firstNodeX, firstNodeY);
        Vector2Int secondNodeCoordinates = new Vector2Int(secondNodeX, secondNodeY);
        m_Players.DecreaseCarpetAmount(carpetIdx);
        m_GridHolder.SpawnCarpet(carpetIdx, firstNodeCoordinates, secondNodeCoordinates);
    }

    public void EndCarpetPlacement()
    {
        m_TurnManager.EndCarpetPlacement();
    }

    public int GetCarpetValue(int playerIdx)
    {
        List<int> carpetValues = m_Players.GetCarpetValues();
        return carpetValues[playerIdx];
    }

    public int GetPlayerCount()
    {
        return m_PlayerCount;
    }

    public void FinishGame()
    {
        List<int> monetCount = m_Players.GetMoneyValues();
        List<int> activeCarpetCount = m_GridHolder.CountColors();

        int maxIndex = 0;
        int maxValue = monetCount[0] + activeCarpetCount[0];
        for (int i = 1; i < monetCount.Count; ++i)
        {
            int value = monetCount[i] + activeCarpetCount[i];
            if ((value == maxValue) && (monetCount[i] > monetCount[maxIndex]))
            {
                maxIndex = i;
                continue;
            }
            if (value > maxValue)
            {
                maxValue = value;
                maxIndex = i;
                continue;
            }
        }

        PlayerUIParent.SetActive(false);
        m_GameFinishUI.SetActive(true);
        m_GameFinishedText.text = $"Game finished!\nWinner: {maxIndex}";

        // count money
        // count carpets
        // choose winner
    }
}