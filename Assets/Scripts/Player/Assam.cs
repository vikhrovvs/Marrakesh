using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    Up,
    Down,
    Left,
    Right

}

public class Assam : MonoBehaviour
{
    [SerializeField] private float m_Speed = 5f;

    private Vector3 m_TargetPoint;

    private Vector3 m_Direction;

    private bool m_IsMoving = false;

    public void TryMoveToPoint(Vector3 target)
    {
        if (m_IsMoving)
        {
            Debug.Log("Trying to move when moving");
            return;
        }
        m_IsMoving = true;
        m_TargetPoint = new Vector3(target.x, transform.position.y, target.z);
        m_Direction = (m_TargetPoint - transform.position).normalized;
    }

    public bool IsMoving()
    {
        return m_IsMoving;
    }

    public void RotateLeft()
    {
        
        transform.Rotate(0, -90, 0);
    }

    public void RotateRight()
    {
        
        transform.Rotate(0, 90, 0);
    }

    void Update()
    {
        if (m_IsMoving)
        // we do this instead of returning cause we might want to so smth else
        // anyway should switch to controller in the future
        {
            Vector3 movement = m_Speed * Time.deltaTime * m_Direction;
            transform.Translate(movement, Space.World);
            if (Vector3.Distance(transform.position, m_TargetPoint) < 0.1f)
            {
                transform.position = m_TargetPoint;
                m_IsMoving = false;
            }
        }
    }

    public Direction GetDirection()
    {
        Vector3 euler = transform.rotation.eulerAngles;
        float y = euler.y;
        // Debug.Log(y);
        y = Mathf.Round(y / 90) * 90;
        // Debug.Log(y);
        switch(y)
        {
            case 0:
                return Direction.Up;
            case 90:
                return Direction.Right;
            case 180:
                return Direction.Down;
            case 270:
                return Direction.Left;
            default:
                throw new System.Exception("Bad y value");
        }
    }
}
