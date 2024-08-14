using System;
using UnityEngine;

public class CarpetData
{
    private CarpetView m_View;
    private Tuple<NodeData, NodeData> m_Nodes;
    public void AttachView(CarpetView view)
    {
        m_View = view;
        m_View.AttachData(this);
    }

    public CarpetData(NodeData first, NodeData second)
    {
        m_Nodes = new Tuple<NodeData, NodeData>(first, second);
    }

    public bool IsCarpetOnNodes(NodeData first, NodeData second)
    {
        return (first == m_Nodes.Item1 && second == m_Nodes.Item2) || (first == m_Nodes.Item2 && second == m_Nodes.Item1);
    }
}
