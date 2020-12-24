using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public class PlantManager : MonoBehaviour
{
    public static PlantManager s_Instance;
    public static float s_SimulationSpeed = 1;

    [SerializeField] private Terrain m_Terrain;
    [SerializeField] private GameObject m_PlantPrefab;

    private GameObject m_PlantContainer;

    private float terrainWidth => m_Terrain.terrainData.size.x;
    private float terrainLength => m_Terrain.terrainData.size.z;
    private float terrainPosX => m_Terrain.transform.position.x;
    private float terrainPosZ => m_Terrain.transform.position.z;

    ulong PlantCounter = 1;

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

    private void Start()
    {
        RegeneratePlants(UIManager.s_Instance.plantNumber);
    }

    private void OnEnable()
    {
        UIManager.s_Instance.OnPlantsRegenerate += RegeneratePlants;
        UIManager.s_Instance.OnPlantsDestroy += DestroyPlants;
        UIManager.s_Instance.OnPlantsRandomFire += SetRandomFires;
        UIManager.s_Instance.OnRecomputeBurningPlantNeighbors += RecomputeBurningPlantNeighbors;
    }

    private void OnDisable()
    {
        UIManager.s_Instance.OnPlantsRegenerate -= RegeneratePlants;
        UIManager.s_Instance.OnPlantsDestroy -= DestroyPlants;
        UIManager.s_Instance.OnPlantsRandomFire -= SetRandomFires;
        UIManager.s_Instance.OnRecomputeBurningPlantNeighbors -= RecomputeBurningPlantNeighbors;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            MouseClick();
        }
    }

    public void RegeneratePlants(int num)
    {
        DestroyPlants();

        m_PlantContainer = new GameObject("Plant Container");
        m_PlantContainer.transform.parent = transform;

        for (int i = 0; i < num; ++i)
        {
            GeneratePlant(m_PlantContainer.transform);
        }
    }

    public GameObject GeneratePlant(Transform parent, Vector3 position)
    {
        var plant = Instantiate(m_PlantPrefab, parent);
        plant.name = "Plant_" + PlantCounter;
        ++PlantCounter;

        var plantPosY = m_Terrain.SampleHeight(new Vector3(position.x, 0, position.z));

        plant.transform.position = new Vector3(position.x, plantPosY, position.z);

        return plant;
    }

    public void SetRandomFires(int firesToSet)
    {
        var firesSet = 0;

        List<Plant> unburntPlants = new List<Plant>(m_PlantContainer.transform.childCount);

        foreach (Transform t in m_PlantContainer.transform)
        {
            Plant plant = t.GetComponent<Plant>();
            if (plant)
            {
                if (plant.plantState == Plant.PlantState.Normal)
                {
                    unburntPlants.Add(plant);
                }
            }
        }

        if (unburntPlants.Count < firesToSet)
        {
            foreach (Plant p in unburntPlants)
            {
                p.SetOnFire();
                ++firesSet;
            }
        }
        else
        {
            while (firesSet != firesToSet)
            {
                var index = Random.Range(0, unburntPlants.Count);
                if (unburntPlants[index].plantState == Plant.PlantState.Normal)
                {
                    ++firesSet;
                    unburntPlants[index].SetOnFire();
                }
            }
        }
    }

    public GameObject GeneratePlant(Transform parent)
    {
        var plantPosX = Random.Range(terrainPosX, terrainPosX + terrainWidth);
        var plantPosZ = Random.Range(terrainPosZ, terrainPosZ + terrainLength);

        return GeneratePlant(parent, new Vector3(plantPosX, 0, plantPosZ));
    }

    public GameObject GeneratePlant(Vector3 position)
    {
        return GeneratePlant(m_PlantContainer.transform, new Vector3(position.x, 0, position.z));
    }

    public void DestroyPlants()
    {
        // Should probably pool objects
        if (m_PlantContainer)
        {
            Destroy(m_PlantContainer);
        }
    }

    public static void ChangeSimulationSpeed(float speed)
    {
        s_SimulationSpeed = speed;
    }

    public void RecomputeBurningPlantNeighbors()
    {
        foreach (Transform t in m_PlantContainer.transform)
        {
            var tmpPlant = t.GetComponent<Plant>();
            if (tmpPlant.plantState == Plant.PlantState.Burning)
            {
                tmpPlant.ComputeNormalNeighbors();
            }
        }
    }

    private void MouseClick()
    {
        if (!EventSystem.current.currentSelectedGameObject)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Transform objectHit = hit.transform;

                switch (UIManager.s_Instance.mouseMode)
                {
                    case UIManager.MouseMode.Remove:
                    {
                        Plant plant = objectHit.GetComponent<Plant>();
                        if (plant)
                        {
                            Destroy(plant.gameObject);
                        }
                        break;
                    }
                    case UIManager.MouseMode.ToggleFire:
                    {
                        Plant plant = objectHit.GetComponent<Plant>();
                        if (plant)
                        {
                            plant.TogglePlantFire();
                        }
                        break;
                    }
                    case UIManager.MouseMode.Add:
                    {
                        Terrain terrain = objectHit.GetComponent<Terrain>();
                        if (terrain)
                        {
                            var plant = GeneratePlant(hit.point).GetComponent<Plant>();

                            RecomputeBurningPlantNeighbors();
                        }
                        break;
                    }
                }
            }
        }

    }
}
