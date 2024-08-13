using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.EventSystems;

public class GridHolder: MonoBehaviour
{
    [SerializeField] private int m_GridWidth;
    [SerializeField] private int m_GridHeight;


    [SerializeField]
    private float m_NodeSize;

    [SerializeField] private NodeView m_NodeViewPrefab;
    [SerializeField] private CarpetView[] m_CarpetViewPrefabs = new CarpetView[4]; // TODO maybe accept whole asset?
    [SerializeField] private float CarpetHeight = 0.001f;
    
    private Grid m_Grid;
    private int m_CarpetColor;

    public int CarpetColor
    {
        get { return m_CarpetColor; }
        set { m_CarpetColor = value % 4; }
    }

    private Camera m_Camera;

    private Vector3 m_Offset;

    public Grid Grid => m_Grid;
    private Game m_Game;

    private List<CarpetData> carpets = new List<CarpetData>();

    private float maxCarpetHeight = 0.001f;

    private GameObject m_CarpetParent;
    private Vector3 m_CarpetScale = new Vector3(0.08f, 1, 0.16f);


    public void CreateGrid()
    {
        m_Camera = Camera.main;
        float width = m_GridWidth * m_NodeSize;
        float height = m_GridHeight * m_NodeSize;
        transform.localScale = new Vector3(width * 0.1f, 1f, height * 0.1f);

        m_Offset = transform.position - (new Vector3(width, 0f, height) * 0.5f);
        m_Grid = new Grid(m_GridWidth, m_GridHeight, m_Offset, m_NodeSize, m_NodeViewPrefab, this);

        m_CarpetParent = new GameObject("CarpetParent");
        m_CarpetParent.transform.parent = transform;
    }
    public void SetGame(Game game) 
    {
        m_Game = game;
    }

    public TurnPhase GetTurnPhase()
    {
        return m_Game.GetTurnPhase();
    }

    public void ChangeOrientation()
    {
        m_Grid.ChangeOrientation();
    }

    
    private void OnValidate()
    {
        float width = m_GridWidth * m_NodeSize;
        float height = m_GridHeight * m_NodeSize;
        transform.localScale = new Vector3(width * 0.1f, 1f, height * 0.1f);

        m_Offset = transform.position - (new Vector3(width, 0f, height) * 0.5f);
        // m_Grid = new Grid(m_GridWidth, m_GridHeight, m_Offset, m_NodeSize, m_NodeViewPrefab, this);
    }

    void Update()
    {
        // TODO switch to conroller
        RaycastInGrid();
    }

    public void RaycastInGrid()
    {
        if (m_Grid == null || m_Camera == null)
        {
            return;
        }

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        
        Vector3 mousePosition = Input.mousePosition;

        Ray ray = m_Camera.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))

        {
            // if (hit.transform != transform)
            // {
            //     m_Grid.UnselectNode();
            //     return;
            // }

            Vector3 hitPosition = hit.point;
            Vector3 difference = hitPosition - m_Offset;

            int x = (int)(difference.x / m_NodeSize);
            int y = (int)(difference.z / m_NodeSize);


            Vector2Int selectedNode = new Vector2Int(x, y); // TODO maybe get an actual node here

            m_Grid.SwitchSelectedNode(selectedNode);


            // if (Input.GetMouseButtonDown(0))
            // {
            //     SpawnCarpet(m_CarpetColor);
            // }

                // Debug.Log("logging click");
                // m_Grid.SetColorToNode(1, selectedNode);

                // NodeData selectedNodeObject = m_Grid.GetNode(x, y);
                // m_Game.TryToMoveAssam(selectedNodeObject.Position);

            if (Input.GetKeyDown("c"))
            {
                NodeData SelectedNode = m_Grid.GetSelectedNode();
                Tuple<int, int> colorAndArea = m_Grid.GetCurrentColorAndArea(SelectedNode);
                int Color = colorAndArea.Item1;
                int Area = colorAndArea.Item2;
                Debug.Log($"Selected node is inside of an area of color {Color} and size {Area}");



            }

            // m_Grid.SelectCoordinate(new Vector2Int(x, y));
        }
        else
        {
            // m_Grid.UnselectNode();
        }
    }

    public Tuple<int, int> GetCurrentColorAndArea()
    {
        Vector2Int assamCoordinates = GetAssamNodeCoordinates();
        NodeData assamCurrentNode = m_Grid.GetNode(assamCoordinates.x, assamCoordinates.y);

        // NodeData SelectedNode = m_Grid.GetSelectedNode();
        Tuple<int, int> colorAndArea = m_Grid.GetCurrentColorAndArea(assamCurrentNode);
        return colorAndArea;
    }

    public Tuple<Vector2Int, Vector2Int> GetSelectedNodesCoordinates()
    {
        NodeData firstNode = m_Grid.GetSelectedNode();
        NodeData secondNode = m_Grid.GetSecondSelectedNode();

        Vector2Int firstNodeCoordinates = firstNode.Coordinates;
        Vector2Int secondNodeCoordinates = secondNode.Coordinates;

        return new Tuple<Vector2Int, Vector2Int>(firstNodeCoordinates, secondNodeCoordinates);
    }

    public void SpawnCarpet(int carpetIdx, Vector2Int firstNodeCoordinates, Vector2Int secondNodeCoordinates)
    {
        NodeData firstNode = m_Grid.GetNode(firstNodeCoordinates.x, firstNodeCoordinates.y);
        NodeData secondNode = m_Grid.GetNode(secondNodeCoordinates.x, secondNodeCoordinates.y);


        // TODO probably rn nodes don't get selected on non-master clients

        Vector3 firstPositon = firstNode.GetView().gameObject.transform.position;
        Vector3 secondPosition = secondNode.GetView().gameObject.transform.position;

        Vector3 carpetPosition = (firstPositon + secondPosition) / 2;
        maxCarpetHeight += CarpetHeight; // yes, elevate first; probably change later
        carpetPosition.y = maxCarpetHeight;

        // TODO rotation
        CarpetData data = new CarpetData(firstNode, secondNode);
        firstNode.SetColor(carpetIdx);
        secondNode.SetColor(carpetIdx);

        CarpetView view = Instantiate(m_CarpetViewPrefabs[carpetIdx]);
        view.transform.position = carpetPosition;
        view.transform.localScale = m_CarpetScale;

        Orientation orientation = m_Grid.GetOrientation();
        if (orientation == Orientation.Vertical)
        {
            view.transform.eulerAngles = new Vector3(0, 90, 0);
        }

        view.transform.parent = m_CarpetParent.transform;

        data.AttachView(view);
        carpets.Add(data);

        m_Game.EndCarpetPlacement();
    }

    private void OnDrawGizmos() //debug method
    {
        if (m_Grid == null)
        {
            return;
        }

        // int i = 0;
        foreach (NodeData node in m_Grid.EnumerateAllNodes())
        {
            // if (node.NextNode == null)
            // {
            //     continue;
            // }
            /*
            if (node.IsOccupied)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(node.Position, 0.2f*m_NodeSize);
                continue;
            }
            */


            // switch (node.GetColor())
            // {
            //     case -1:
            //     {
            //         Gizmos.color = Color.yellow;
            //         return;
            //     }
            //     case 0:
            //     {
            //         Gizmos.color = Color.white;
            //         return;
            //     }
            //     case 1:
            //     {
            //         Gizmos.color = Color.red;
            //         return;
            //     }
            //     case 2:
            //     {
            //         Gizmos.color = Color.green;
            //         return;
            //     }
            //     case 3:
            //     {
            //         Gizmos.color = Color.blue;
            //         return;
            //     }
            // }



            // if (i == 0) {
            //     Gizmos.color = Color.red;
            //     i += 1;
            // }

            if (node == m_Grid.GetSelectedNode())
            // Important to be all the color assignments 
            {
                Color currentColor = Gizmos.color;
                Gizmos.color = new Color(currentColor.r, currentColor.g, currentColor.b, 0.5f);
            }

            Gizmos.DrawSphere(node.Position, 0.2f*m_NodeSize);
        }
    }


    private Vector2Int GetAssamNodeCoordinates()
    {
        Vector3 difference = m_Game.GetAssamPosition() - m_Offset;

        int x = (int)(difference.x / m_NodeSize);
        int y = (int)(difference.z / m_NodeSize);
        return new Vector2Int(x, y);
    }
    

    public NodeData GetNextNodeInDirectionAndRotate(Direction direction)
    {
        Vector2Int AssamPosition = GetAssamNodeCoordinates();
        int x = AssamPosition.x;
        int y = AssamPosition.y;

        // Debug.Log($"Assam position: {x}, {y}");
        switch (direction)
        {
            case Direction.Up:
                if (x != 6)
                {
                    // simple case, go up
                    x = x + 1;
                    break;
                }
                if (y == 6)
                {
                    // (6, 6) -> (6, 6)
                    m_Game.RotateRight();
                    break;
                }
                if (y % 2 == 1)
                {
                    y = y - 1;
                    m_Game.Rotate180();
                }
                else
                {
                    y = y + 1;
                    m_Game.Rotate180();
                }
                break;
                
            case Direction.Down:
                if (x != 0)
                {
                    // simple case, go down
                    x = x - 1;
                    break;
                }
                if (y == 0)
                {
                    // (0, 0) -> (0, 0)
                    m_Game.RotateRight();
                    break;
                }
                if (y % 2 == 1)
                {
                    y = y + 1;
                    m_Game.Rotate180();
                }
                else
                {
                    y = y - 1;
                    m_Game.Rotate180();
                }
                break;

                    // todo rotate assam

            case Direction.Left:
                if (y != 6)
                {
                    y = y + 1;
                    break;
                }
                if (x == 6)
                {
                    m_Game.RotateLeft();
                    break;
                }
                if (x % 2 == 1)
                {
                    x = x - 1;
                    m_Game.Rotate180();
                }
                else
                {
                    x = x + 1;
                    m_Game.Rotate180();
                }
                break;
            case Direction.Right:
                if (y != 0)
                {
                    y = y - 1;
                    break;
                }
                if (x == 0)
                {
                    m_Game.RotateLeft();
                    break;
                }
                if (x % 2 == 1)
                {
                    m_Game.Rotate180();
                    x = x + 1;
                }
                else
                {
                    m_Game.Rotate180();
                    x = x - 1;
                }
                break;
                
            default:
                Debug.Log("Bad direction");
                break;
        }
        
        
        NodeData node = m_Grid.GetNode(x, y);
        return node;
    }

    public List<int> CountColors()
    {
        int n_players = m_Game.GetPlayerCount();
        List<int> colorCount = new List<int>();
        for (int i = 0; i < n_players; ++i)
        {
            colorCount.Add(0);
        }

        foreach(NodeData nodeData in m_Grid.EnumerateAllNodes())
        {
            int color = nodeData.GetColor();
            if (color == -1)
            {
                continue;
            }
            ++colorCount[color];
        }

        return colorCount;
    }
}