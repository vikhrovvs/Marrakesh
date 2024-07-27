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
}