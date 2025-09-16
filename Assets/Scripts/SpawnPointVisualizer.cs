using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SpawnPointVisualizer : MonoBehaviour
{
    [Header("Настройки визуализации")]
    public Color gizmoColor = Color.green;
    public float gizmoSize = 0.5f;
    public bool showLabel = true;
    
    private PrefabSpawner spawner;
    private int spawnPointIndex = -1;
    
    public void SetSpawner(PrefabSpawner spawnerRef)
    {
        spawner = spawnerRef;
        UpdateSpawnPointIndex();
    }
    
    void UpdateSpawnPointIndex()
    {
        if (spawner != null && spawner.spawnPoints != null)
        {
            for (int i = 0; i < spawner.spawnPoints.Length; i++)
            {
                if (spawner.spawnPoints[i] == transform)
                {
                    spawnPointIndex = i;
                    break;
                }
            }
        }
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, gizmoSize);
        
        // Рисуем стрелку направления
        Gizmos.color = gizmoColor;
        Vector3 forward = transform.forward * gizmoSize * 2;
        Gizmos.DrawLine(transform.position, transform.position + forward);
        Gizmos.DrawWireCube(transform.position + forward, Vector3.one * gizmoSize * 0.3f);
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, gizmoSize * 1.2f);
        
        if (showLabel && spawnPointIndex >= 0)
        {
            #if UNITY_EDITOR
            Handles.Label(transform.position + Vector3.up * (gizmoSize + 0.5f), 
                         "Spawn Point " + spawnPointIndex.ToString());
            #endif
        }
    }
    
    void OnValidate()
    {
        UpdateSpawnPointIndex();
    }
    
    #if UNITY_EDITOR
    [ContextMenu("Удалить этот спавн поинт")]
    void RemoveThisSpawnPoint()
    {
        if (spawner != null)
        {
            spawner.RemoveSpawnPoint(spawnPointIndex);
            DestroyImmediate(gameObject);
        }
    }
    
    [ContextMenu("Создать спавн поинт рядом")]
    void CreateNearbySpawnPoint()
    {
        if (spawner != null)
        {
            spawner.CreateSpawnPoint();
        }
    }
    #endif
}
