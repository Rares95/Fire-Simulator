using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Plant : MonoBehaviour
{
    public static readonly float PHI = 1.61803398875f;

    public enum PlantState
    {
        Normal,
        Burning,
        Burnt
    }

    [SerializeField] private PlantState m_PlantState;
    public PlantState plantState => m_PlantState;

    [SerializeField] public Material PlantBaseMaterial;
    [SerializeField] public Material PlantBurningMaterial;
    [SerializeField] public Material PlantBurntMaterial;

    [Header("Plant Parameters")]
    [Space(10)]

    [Tooltip("How many seconds should the plant burn for.")]
    [SerializeField] private float m_BaseHealth = 10;
    [Tooltip("The radius of the area around the plant that can be set on fire.")]
    [SerializeField] private float m_BaseFireSpreadRadius = 7;
    [Tooltip("The 'size' of the plant. Affects the other parameters.")]
    [SerializeField] private float m_SizeMultiplier = 1;

    [Header("Fire Spread Parameters")]
    [Space(10)]
    [Tooltip("How fast the fire spreads when no wind is involved.")]
    [SerializeField] private float m_BaseFireChance = 3;
    [Tooltip("How much does wind affect the chance of the fire spreading. (0.5 = 1.5x ,1 = 2x, 2 = 3x etc.)")]
    [SerializeField] private float m_WindFireChanceMultiplier = 1;
    [Tooltip("Depending on which state of burning the plant is in, change the fire spread chance.")]
    [SerializeField] private AnimationCurve m_FireLifetimeSpreadMultiplier;

    public float sizeMultiplier => m_SizeMultiplier;
    private float totalHealth => m_BaseHealth * sizeMultiplier;
    private float totalFireSpreadRadius => m_BaseFireSpreadRadius * m_SizeMultiplier + windSpeedRadius;
    private float windSpeedRadius => m_BaseFireSpreadRadius * m_SizeMultiplier * UIManager.s_Instance.windSpeed;
    private float maxPotentialRadius => totalFireSpreadRadius + windSpeedRadius * PHI;
    private Vector3 fireSphereCenter => transform.position + UIManager.s_Instance.windDirectionVector * windSpeedRadius * PHI;

    private float m_CurrentHealth;
    private Plant[] m_NeighborPlants;
    private List<Transform> m_PlantsSetOnFire = new List<Transform>();

    private void Awake()
    {
        m_SizeMultiplier = Random.Range(0.8f, 1.2f);
        transform.localScale = Vector3.one * m_SizeMultiplier;
        m_CurrentHealth = totalHealth;
    }

    private void Update()
    {
        switch (plantState)
        {
            case PlantState.Normal:
            {
                // Heal plant slowly when in the Normal state
                if (m_CurrentHealth != totalHealth)
                {
                    m_CurrentHealth += Time.deltaTime / 2f * PlantManager.s_SimulationSpeed;
                }
                break;
            }
            case PlantState.Burning:
            {
                if (m_CurrentHealth < 0)
                {
                    ChangePlantState(PlantState.Burnt);
                }
                else
                {
                    m_CurrentHealth -= Time.deltaTime * PlantManager.s_SimulationSpeed;
                }

                float sqrPotentialRadius = maxPotentialRadius * maxPotentialRadius;
                for (int i = 0; i < m_NeighborPlants.Length; ++i)
                {
                    Plant target = m_NeighborPlants[i];
                    if (target)
                    {
                        var sqrDistance = (target.transform.position - transform.position).sqrMagnitude;
                        var normalizedDistance = (sqrDistance / sqrPotentialRadius);
                        var sizeDelta = sizeMultiplier - target.sizeMultiplier;
                        var fireSpreadEvaluator = m_CurrentHealth / totalHealth;

                        var chance = m_BaseFireChance + (m_BaseFireChance * m_WindFireChanceMultiplier) + sizeDelta;

                        chance *= PlantManager.s_SimulationSpeed;
                        chance *= PHI * Mathf.Cos(normalizedDistance * Mathf.PI);
                        chance *= sizeMultiplier;
                        chance *= Time.deltaTime;
                        chance *= m_FireLifetimeSpreadMultiplier.Evaluate(1 - fireSpreadEvaluator);
                        float randomChance = Random.Range(0f, 100f);

                        if (randomChance < chance)
                        {
                            SetTargetOnFire(target);
                        }
                    }
                }

                break;
            }
            case PlantState.Burnt:
            {
                break;
            }
        }
        if (m_CurrentHealth > totalHealth)
        {
            m_CurrentHealth = totalHealth;
        }
    }

    public void SetOnFire()
    {
        ChangePlantState(PlantState.Burning);
    }

    public void SetTargetOnFire(Plant target)
    {
        m_PlantsSetOnFire.Add(target.transform);
        target.ChangePlantState(PlantState.Burning);
    }

    public void TogglePlantFire()
    {
        switch (plantState)
        {
            case PlantState.Normal:
                ChangePlantState(PlantState.Burning);
                break;
            case PlantState.Burning:
                ChangePlantState(PlantState.Normal);
                break;
        }
    }

    private void ChangePlantState(PlantState state)
    {
        m_PlantState = state;
        switch (plantState)
        {
            case PlantState.Normal:
                m_PlantsSetOnFire.Clear();
                GetComponent<Renderer>().material = PlantBaseMaterial;
                break;
            case PlantState.Burning:
                GetComponent<Renderer>().material = PlantBurningMaterial;
                ComputeNormalNeighbors();
                break;
            case PlantState.Burnt:
                m_CurrentHealth = 0;
                GetComponent<Renderer>().material = PlantBurntMaterial;
                break;
        }
    }

    public void ComputeNormalNeighbors()
    {
        var colliders = Physics.OverlapSphere(fireSphereCenter, totalFireSpreadRadius);
        List<Plant> plantList = new List<Plant>(colliders.Length);
        for (int i = 0; i < colliders.Length; ++i)
        {
            var plant = colliders[i].GetComponent<Plant>();
            if (plant && plant.plantState == PlantState.Normal)
            {
                plantList.Add(plant);
            }
        }
        m_NeighborPlants = plantList.ToArray();
    }

    private void OnDrawGizmos()
    {
        if (UIManager.s_Instance.debugMode == UIManager.DebugInfo.None)
        {
            return;
        }
#if UNITY_EDITOR
        if (!EditorApplication.isPlaying)
        {
            return;
        }
#endif
        var heightOffset = Vector3.up * 5;

        if (UIManager.s_Instance.debugMode != UIManager.DebugInfo.Wind && UIManager.s_Instance.debugMode != UIManager.DebugInfo.Fire)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawLine(transform.position + heightOffset + Vector3.up * 0.5f, transform.position + heightOffset + Vector3.up * 0.5f + Vector3.up * 4 * (m_CurrentHealth / totalHealth));
        }

        if (UIManager.s_Instance.debugMode == UIManager.DebugInfo.Complex || UIManager.s_Instance.debugMode == UIManager.DebugInfo.Wind || UIManager.s_Instance.debugMode == UIManager.DebugInfo.Simple)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position + heightOffset, transform.position + UIManager.s_Instance.windDirectionVector * 4 + UIManager.s_Instance.windDirectionVector * UIManager.s_Instance.windSpeed * 4 + heightOffset);
        }

        if (UIManager.s_Instance.debugMode == UIManager.DebugInfo.Complex)
        {
            switch (plantState)
            {
                case PlantState.Normal:
                    Gizmos.color = Color.gray;
                    break;
                case PlantState.Burning:
                    Gizmos.color = Color.red;
                    break;
                case PlantState.Burnt:
                    Gizmos.color = Color.black;
                    break;
            }
            Gizmos.DrawWireSphere(fireSphereCenter, totalFireSpreadRadius);

            Gizmos.color = new Color(1, 1, 0, 0.33f);
            Gizmos.DrawWireSphere(transform.position, maxPotentialRadius);
        }

        if (UIManager.s_Instance.debugMode == UIManager.DebugInfo.Complex || UIManager.s_Instance.debugMode == UIManager.DebugInfo.Fire)
        {
            Gizmos.color = new Color(1, 1, 1, 0.66f);
            foreach (Transform t in m_PlantsSetOnFire)
            {
                Debug.DrawLine(transform.position, t.position);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (UIManager.s_Instance.debugMode == UIManager.DebugInfo.None)
        {
            return;
        }
        var heightOffset = Vector3.up * 5;

        if (UIManager.s_Instance.debugMode == UIManager.DebugInfo.Complex)
        {
            switch (plantState)
            {
                case PlantState.Normal:
                    Gizmos.color = Color.green;
                    break;
                case PlantState.Burning:
                    Gizmos.color = Color.red;
                    break;
                case PlantState.Burnt:
                    Gizmos.color = Color.black;
                    break;
            }

            Gizmos.DrawSphere(transform.position + heightOffset, 0.5f);
        }
    }
}
