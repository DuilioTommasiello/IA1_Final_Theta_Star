using System.Collections.Generic;
using UnityEngine;

public class FlockManager : MonoBehaviour
{
    [Header("Global Flocking Settings")]
    [SerializeField] private float globalSeparationWeight = 1.5f;
    [SerializeField] private float globalAlignmentWeight = 1.0f;
    [SerializeField] private float globalCohesionWeight = 1.2f;
    [SerializeField] private float globalNeighborRadius = 8.0f;
    [SerializeField] private float globalSeparationRadius = 2.5f;

    private Dictionary<Agent, List<Minion>> leaderGroups = new Dictionary<Agent, List<Minion>>();

    void Update()
    {
        // Actualizar grupos periódicamente
        UpdateLeaderGroups();
    }

    private void UpdateLeaderGroups()
    {
        // Limpiar grupos antiguos
        var keysToRemove = new List<Agent>();
        foreach (var group in leaderGroups)
        {
            if (group.Key == null)
            {
                keysToRemove.Add(group.Key);
                continue;
            }

            // Limpiar minions nulos
            group.Value.RemoveAll(m => m == null);

            if (group.Value.Count == 0)
            {
                keysToRemove.Add(group.Key);
            }
        }

        foreach (var key in keysToRemove)
        {
            leaderGroups.Remove(key);
        }
    }

    public void RegisterMinion(Minion minion, Agent leader)
    {
        if (leader == null) return;

        if (!leaderGroups.ContainsKey(leader))
        {
            leaderGroups[leader] = new List<Minion>();
        }

        if (!leaderGroups[leader].Contains(minion))
        {
            leaderGroups[leader].Add(minion);
            Debug.Log($"Registered minion {minion.name} under leader {leader.name}");
        }
    }

    public void UnregisterMinion(Minion minion, Agent leader)
    {
        if (leader != null && leaderGroups.ContainsKey(leader))
        {
            leaderGroups[leader].Remove(minion);

            if (leaderGroups[leader].Count == 0)
            {
                leaderGroups.Remove(leader);
            }
        }
    }

    public List<Minion> GetNearbyMates(Minion minion, float customRadius = -1)
    {
        float radius = customRadius > 0 ? customRadius : globalNeighborRadius;
        List<Minion> nearbyMates = new List<Minion>();

        if (minion.target == null) return nearbyMates;

        // Buscar minions del mismo líder
        if (leaderGroups.ContainsKey(minion.target))
        {
            foreach (Minion mate in leaderGroups[minion.target])
            {
                if (mate == null || mate == minion || mate.target != minion.target) continue;

                float distance = Vector3.Distance(minion.transform.position, mate.transform.position);
                if (distance <= radius)
                {
                    nearbyMates.Add(mate);
                }
            }
        }

        return nearbyMates;
    }

    public int GetFormationIndex(Minion minion)
    {
        if (minion.target == null || !leaderGroups.ContainsKey(minion.target)) return -1;

        var group = leaderGroups[minion.target];
        for (int i = 0; i < group.Count; i++)
        {
            if (group[i] == minion) return i;
        }

        return -1;
    }
}