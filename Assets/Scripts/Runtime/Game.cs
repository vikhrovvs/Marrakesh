using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
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

    private Players m_Players;

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


    public void InitPlayers(int n_players)
    {
        m_Players = new Players(n_players, this);
        CreatePlayersUI();
    }

    public void CreatePlayersUI()
    {
        List<int> moneyValues = m_Players.GetMoneyValues();
        List<int> carpetValues = m_Players.GetCarpetValues();
        int n_players = moneyValues.Count;

        List<PlayerUI> playerUIs = new List<PlayerUI>();
        for (int i = 0; i < n_players; ++i)
        {
            PlayerUI playerUI = Instantiate(playerUIPrefab);
            playerUI.Initialize(i, moneyValues[i], carpetValues[i]);
            playerUI.gameObject.transform.parent = PlayerUIParent.transform;

            playerUIs.Add(playerUI);
        }

        m_Players.SetPlayerUIs(playerUIs);
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
        Debug.Log($"accepted result{result}");
    }
    
    [PunRPC]
    public void Move(int playerNumber)
    {
        StartCoroutine(StartMovingAlongPath(playerNumber));
    }

    private IEnumerator StartMovingAlongPath(int playerNumber)
    {
        yield return StartCoroutine(MoveAlongPath());

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

        m_GridHolder.SpawnCarpet(carpetIdx, firstNodeCoordinates, secondNodeCoordinates);

        m_Players.DecreaseCarpetAmount(carpetIdx);
    }

    public void EndCarpetPlacement()
    {
        m_TurnManager.EndCarpetPlacement();
    }
}