using System;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class UIManager : MonoBehaviour
{
    public static UIManager s_Instance;

    public Action<int> OnPlantsRegenerate;
    public Action OnPlantsDestroy;
    public Action<int> OnPlantsRandomFire;
    public Action OnRecomputeBurningPlantNeighbors;

    [SerializeField] private InputField m_PlantNumber;
    [SerializeField] private Dropdown m_DebugInfoMode;
    [SerializeField] private Dropdown m_MouseMode;
    [SerializeField] private Slider m_SimulationSpeed;
    [SerializeField] private Slider m_WindSpeed;
    [SerializeField] private Slider m_WindDirection;

    public enum MouseMode
    {
        None = 0,
        Add = 1,
        Remove = 2,
        ToggleFire = 3
    }

    public enum DebugInfo
    {
        None = 0,
        Simple = 1,
        Complex = 2,
        Fire = 3,
        Wind = 4
    }

    public int plantNumber
    {
        get
        {
            if (!string.IsNullOrEmpty(m_PlantNumber.text))
            {
                return Convert.ToInt32(m_PlantNumber.text);
            }
            return 1000;
        }
    }
    public MouseMode mouseMode { get { return (MouseMode)m_MouseMode.value; } }
    public DebugInfo debugMode { get { return (DebugInfo)m_DebugInfoMode.value; } }
    public float windSpeed { get { return m_WindSpeed.value; } }
    public float windDirectionAngle { get { return m_WindDirection.value; } }

    private Vector3 m_WindDirectionVector;
    public Vector3 windDirectionVector => m_WindDirectionVector;


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

        RecomputeWindDirection();
    }

    public void RegeneratePlants()
    {
        OnPlantsRegenerate.Invoke(plantNumber);
    }

    public void DestroyPlants()
    {
        OnPlantsDestroy.Invoke();
    }

    public void RandomFire()
    {
        OnPlantsRandomFire.Invoke(Random.Range(4, 16));
    }

    public void SimulationStateToggle()
    {
        if(m_SimulationSpeed.value== 0)
        {
            m_SimulationSpeed.value = 1;
        }
        else
        {
            m_SimulationSpeed.value = 0;
        }
        UpdateSimulationSpeed();
    }

    public void UpdateSimulationSpeed()
    {
        PlantManager.ChangeSimulationSpeed(m_SimulationSpeed.value);
    }

    public void RecomputeWindDirection()
    {
        m_WindDirectionVector = new Vector3(Mathf.Sin(windDirectionAngle * Mathf.Deg2Rad), 0, Mathf.Cos(windDirectionAngle * Mathf.Deg2Rad)).normalized;
    }

    public void RecomputeBurningPlantNeighbors()
    {
        OnRecomputeBurningPlantNeighbors.Invoke();
    }

    public void QuitApplication()
    {
        Debug.Log("User quit the application.");
        Application.Quit();
    }
}
