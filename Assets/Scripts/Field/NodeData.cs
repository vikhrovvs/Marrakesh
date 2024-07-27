using UnityEngine;

public class NodeData
{
    public Vector2Int Coordinates;
    public Vector3 Position;
    // and current carpet color
    private int color = -1;
    private NodeView m_View;

    public NodeData(Vector3 position, Vector2Int coordinates)
    {
        Position = position;
        Coordinates = coordinates;
    }

    public void AttachView(NodeView view)
    {
        m_View = view;
        m_View.AttachData(this);
    }

    public NodeView GetView()
    {
        return m_View;
    }

    public void SetColor(int Color)
    {
        color = Color;
    }

    public int GetColor()
    {
        return color;
    }
}