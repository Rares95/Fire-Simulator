using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager s_Instance;

    public Action<int> OnPlantGenerate;
    public Action OnPlantDestroy;
    public Action OnSimulationStateToggle;

    [SerializeField] private InputField m_PlantNumber;
    [SerializeField] private Dropdown m_MouseMode;
    [SerializeField] private Slider m_WindSpeed;
    [SerializeField] private Slider m_WindDirection;

    public enum MouseMode
    {
        None = 0,
        Add = 1,
        Remove = 2,
        ToggleFire = 3
    }

    public int plantNumber { get { return Convert.ToInt32(m_PlantNumber.text); } }
    public MouseMode mouseMode { get { return (MouseMode)m_MouseMode.value; } }
    public float windSpeed
    { get { return m_WindSpeed.value; } }
    public float windDirection { get { return m_WindDirection.value; } }

    private void Awake()
    {
        if (s_Instance == null)
        {
            s_Instance = this;
        }
        else
        {
            DestroyImmediate(this);
        }
    }

    public void RegeneratePlants()
    {
        OnPlantDestroy.Invoke();
        OnPlantGenerate.Invoke(plantNumber);
    }

    public void DestroyPlants()
    {
        OnPlantDestroy.Invoke();
    }

    public void SimulationStateToggle()
    {
        OnSimulationStateToggle.Invoke();
    }

    public void QuitApplication()
    {
        Debug.Log("User quit the application.");
        Application.Quit();
    }
}
