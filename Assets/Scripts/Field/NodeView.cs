using UnityEngine;

public class NodeView : MonoBehaviour
{
    private NodeData m_Data;

    public void AttachData(NodeData data)
        {
            m_Data = data;
        }
}