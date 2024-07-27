using System;
using System.Collections.Generic;
using UnityEngine;
#nullable enable


public enum Orientation
{
    Vertical = 0,
    Horizontal = 1
}

public class Grid
{
    private NodeData[,] m_Nodes;
    private int m_Width;
    private int m_Height;
    private Vector3 m_offset;
    private float m_nodeSize;
    private NodeData? SelectedNode = null;
    private NodeData? SecondSelectedNode = null;
    private Orientation m_CurrentOrientation = Orientation.Vertical;

    private GridHolder m_GridHolder;
    private GameObject m_NodeParent;
    private float m_TransparencyLevel = 0.8f;
    private float m_SecondNodeTransparencyLevel = 0.6f;
    private float m_NodeSizeModifier = 0.99f;


    public Grid(int width, int height, Vector3 offset, float nodeSize, NodeView nodeViewPrefab, GridHolder gridHolder)
    {
        m_Width = width;
        m_Height = height;

        m_offset = offset;
        m_nodeSize = nodeSize;

        m_GridHolder = gridHolder;
        
        m_Nodes = new NodeData[m_Width, m_Height];

        m_NodeParent = new GameObject("NodeParent");
        m_NodeParent.transform.parent = gridHolder.transform;


        for (int i = 0; i < m_Nodes.GetLength(0); i++)
        {
            for (int j = 0; j < m_Nodes.GetLength(1); j++)
            {
                Vector3 nodePosition = offset + new Vector3(i + .5f, 0, j + .5f) * nodeSize;
                Vector2Int coordinates = new Vector2Int(i, j);
                NodeData newNode = new NodeData(nodePosition, coordinates);
                m_Nodes[i, j] = newNode;

                NodeView view = UnityEngine.Object.Instantiate(nodeViewPrefab);
                view.transform.position = nodePosition;
                view.transform.localScale = Vector3.one * (m_NodeSizeModifier * nodeSize / 10f);
                view.transform.parent = m_NodeParent.transform;
                // TODO resize
                newNode.AttachView(view);


                /*
                EnemyView view = Object.Instantiate(asset.ViewPrefab);
        Vector3 startNodePosition = m_Grid.GetStartNode().Position;
        view.transform.position = new Vector3(startNodePosition.x, view.transform.position.y, startNodePosition.z);   ;
        EnemyData data = new EnemyData(asset);

        data.AttachView(view);
        */
                
            }
        }

    }

    public void ChangeOrientation()
    {
        
        m_CurrentOrientation = (Orientation)(1 - (int)m_CurrentOrientation);
    }

    public Orientation GetOrientation()
    {
        return m_CurrentOrientation;
    }

    public IEnumerable<NodeData?> EnumerateAllNodes()
    {
        for (int i = 0; i < m_Width; ++i)
        {
            for (int j = 0; j < m_Height; ++j)
            {
                yield return GetNode(i, j);
            }

        }
    }

    

    public NodeData? GetNode(int i, int j)
    {
        if (i < 0 || i >= m_Width)
        {
            return null;
        }

        if (j < 0 || j >= m_Height)
        {
            return null;
        }
        return m_Nodes[i, j];
    }

    public void SetColorToNode(int color, Vector2Int coordinates)
    {
        // was meant to be used instead of SpawnCarpet; now useless?
        NodeData? selectedNode = GetNode(coordinates.x, coordinates.y);
        if (selectedNode == null)
        {
            return;
        }
        selectedNode.SetColor(color);
    }

    public void SwitchSelectedNode(Vector2Int coordinates)
    {
        NodeData? prevSelectedNode = SelectedNode;
        MakeNodeOpaque(prevSelectedNode);

        NodeData? PrevSecondSelectedNode = SecondSelectedNode;
        MakeNodeOpaque(PrevSecondSelectedNode);


        NodeData? selectedNode = GetNode(coordinates.x, coordinates.y);
        SelectedNode = selectedNode;
        MakeNodeTransparent(selectedNode);

        NodeData? secondSelectedNode = GetSecondNodeInCorrespondingOrientation(coordinates);
        SecondSelectedNode = secondSelectedNode;
        MakeSecondNodeTransparent(SecondSelectedNode);
    }

    private void MakeNodeOpaque(NodeData? node)
    {
        SetTransparencyLevel(node, 1f);
    }

    public void MakeNodeTransparent(NodeData? node)
    {
        SetTransparencyLevel(node, m_TransparencyLevel);
    }

    public void MakeSecondNodeTransparent(NodeData? node)
    {
        SetTransparencyLevel(node, m_SecondNodeTransparencyLevel);
    }

    private void SetTransparencyLevel(NodeData? node, float transparencyLevel)
    {
        if (node == null)
        {
            return;
        }
        Renderer renderer = node.GetView().gameObject.GetComponent<Renderer>();
        foreach (Material mat in renderer.materials)
        {
            Color color = mat.color;
            color.a = transparencyLevel;
            mat.color = color;
            
            // Ensure the material is using the correct rendering mode
            SetMaterialRenderingMode(mat, transparencyLevel);
        }
    }
    

    public NodeData? GetSelectedNode()
    {
        return SelectedNode;
    }

    public NodeData? GetSecondSelectedNode()
    {
        return SecondSelectedNode;
    }

    private void SetMaterialRenderingMode(Material material, float alpha)
    {
        // ChatGPT-4o generated
        // tested to be working
        if (alpha < 1f)
        {
            material.SetFloat("_Mode", 3); // 3 corresponds to Transparent mode
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }
        else
        {
            material.SetFloat("_Mode", 0); // 0 corresponds to Opaque mode
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            material.SetInt("_ZWrite", 1);
            material.DisableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = -1;
        }
    }

    private NodeData? GetSecondNodeInCorrespondingOrientation(Vector2Int coordinates)
    {
        int x = coordinates.x;
        int y = coordinates.y;
        if (m_CurrentOrientation == Orientation.Vertical)
        {
            if (x != 6)
            {
                x += 1;
            }
            else
            {
                x -= 1;
            }
        }
        else  // horizontal
        {
            if (y != 6)
            {
                y += 1;
            }
            else
            {
                y -= 1;
            }
        }
        return GetNode(x, y);
    }

    public Tuple<int, int> GetCurrentColorAndArea(NodeData node)
    {
        int currentColor = node.GetColor();
        int area = DFSNodes(node);
        Tuple<int, int> result = new Tuple<int, int>(currentColor, area);
        return result;
    }

    private int DFSNodes(NodeData node) 
    {
        int color = node.GetColor();
        if (color == -1)
        {
            return 1;
        }

        int nodeCount = 0;

        List<NodeData> nodeQueue = new List<NodeData>{node};
        List<NodeData> visited = new List<NodeData>();

        while(nodeQueue.Count > 0)
        {
            NodeData current = nodeQueue[0];
            nodeQueue.RemoveAt(0);

            if (visited.Contains(current))
            {
                continue;
            }

            ++nodeCount;

            visited.Add(current);

            List<NodeData> neighbours = GetNeighboursOfTheSameColor(current);

            nodeQueue.AddRange(neighbours);
        }
        return nodeCount;
    }

    private List<NodeData> GetNeighboursOfTheSameColor(NodeData node)
    {
        int color = node.GetColor();
        List<NodeData> neighbours = new List<NodeData>();
        List<Vector2Int> deltaCoordinates = new List<Vector2Int>{
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };
        foreach(Vector2Int delta in deltaCoordinates)
        {
            Vector2Int newCoordinates = node.Coordinates + delta;
            if (newCoordinates.x > 6 || newCoordinates.x < 0 || newCoordinates.y > 6 || newCoordinates.y < 0)
            {
                continue;
            }
            NodeData? neighbour = GetNode(newCoordinates.x, newCoordinates.y);
            if (neighbour is null)
            {
                continue;
            }
            if (neighbour.GetColor() != color)
            {
                continue;
            }
            neighbours.Add(neighbour);
        }
        return neighbours;
    }
}
