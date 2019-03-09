using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCameraMovement : MonoBehaviour
{
    private bool m_WasKeyPressed;
    private float m_Speed;

    void Update()
    {
        m_WasKeyPressed = false;
        if (Input.GetKey(KeyCode.W))
        {
            transform.position += transform.GetChild(0).forward * m_Speed;
            m_WasKeyPressed = true;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            transform.position -= transform.GetChild(0).forward * m_Speed;
            m_WasKeyPressed = true;
        }

        if (Input.GetKey(KeyCode.A))
        {
            transform.position -= transform.GetChild(0).right * m_Speed;
            m_WasKeyPressed = true;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            transform.position += transform.GetChild(0).right * m_Speed;
            m_WasKeyPressed = true;
        }

        if (m_WasKeyPressed && m_Speed < 10)
        {
            m_Speed += Time.deltaTime;
        }
        else
        {
            m_Speed = 1;
        }
    }
}
