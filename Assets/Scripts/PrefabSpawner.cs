using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PrefabSpawner : MonoBehaviour
{
    [Header("Префабы для спавна")]
    public GameObject[] prefabsToSpawn;
    
    [Header("Настройки спавна")]
    public Transform[] spawnPoints;
    public SpawnPointMode spawnPointMode = SpawnPointMode.Random;
    public bool spawnOnStart = false;
    public float spawnDelay = 1f;
    public bool continuousSpawning = false;
    public float spawnInterval = 2f;
    
    [Header("Управление спавн поинтами")]
    [SerializeField] private bool showSpawnPointControls = true;
    
    public enum SpawnPointMode
    {
        Random,     // Случайный спавн поинт
        Sequential  // По порядку
    }
    
    private List<GameObject> spawnedObjects = new List<GameObject>();
    private int currentSpawnIndex = 0;
    
    void Start()
    {
        if (spawnOnStart)
        {
            if (spawnDelay > 0)
                StartCoroutine(SpawnWithDelay());
            else
                SpawnPrefab();
        }
        
        if (continuousSpawning)
        {
            StartCoroutine(ContinuousSpawning());
        }
    }
    
    IEnumerator SpawnWithDelay()
    {
        yield return new WaitForSeconds(spawnDelay);
        SpawnPrefab();
    }
    
    IEnumerator ContinuousSpawning()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnPrefab();
        }
    }
    
    public void SpawnPrefab()
    {
        if (prefabsToSpawn == null || prefabsToSpawn.Length == 0)
        {
            Debug.LogWarning("Нет префабов для спавна!");
            return;
        }
        
        GameObject prefabToSpawn = prefabsToSpawn[Random.Range(0, prefabsToSpawn.Length)];
        Vector3 spawnPosition = GetSpawnPosition();
        GameObject spawnedObject = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
        
        spawnedObjects.Add(spawnedObject);
        
        Debug.Log("Заспавнен объект: " + spawnedObject.name + " в позиции " + spawnPosition);
    }
    
    Vector3 GetSpawnPosition()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            return transform.position;
        }
        
        switch (spawnPointMode)
        {
            case SpawnPointMode.Random:
                Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
                return randomPoint != null ? randomPoint.position : transform.position;
                
            case SpawnPointMode.Sequential:
                Transform sequentialPoint = spawnPoints[currentSpawnIndex];
                currentSpawnIndex = (currentSpawnIndex + 1) % spawnPoints.Length;
                return sequentialPoint != null ? sequentialPoint.position : transform.position;
                
            default:
                return transform.position;
        }
    }
    
    
    public void SpawnPrefabAtPosition(Vector3 position)
    {
        if (prefabsToSpawn == null || prefabsToSpawn.Length == 0)
        {
            Debug.LogWarning("Нет префабов для спавна!");
            return;
        }
        
        GameObject prefabToSpawn = prefabsToSpawn[Random.Range(0, prefabsToSpawn.Length)];
        GameObject spawnedObject = Instantiate(prefabToSpawn, position, Quaternion.identity);
        
        spawnedObjects.Add(spawnedObject);
    }
    
    public void SpawnSpecificPrefab(int prefabIndex)
    {
        if (prefabsToSpawn == null || prefabIndex < 0 || prefabIndex >= prefabsToSpawn.Length)
        {
            Debug.LogWarning("Неверный индекс префаба!");
            return;
        }
        
        Vector3 spawnPosition = GetSpawnPosition();
        GameObject spawnedObject = Instantiate(prefabsToSpawn[prefabIndex], spawnPosition, Quaternion.identity);
        
        spawnedObjects.Add(spawnedObject);
    }
    
    public void ClearAllSpawnedObjects()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null)
                Destroy(obj);
        }
        spawnedObjects.Clear();
    }
    
    public int GetSpawnedObjectsCount()
    {
        return spawnedObjects.Count;
    }
    
    public void SetSpawnPointMode(SpawnPointMode mode)
    {
        spawnPointMode = mode;
        currentSpawnIndex = 0; // Сбрасываем индекс при смене режима
    }
    
    public void AddSpawnPoint(Transform spawnPoint)
    {
        if (spawnPoints == null)
        {
            spawnPoints = new Transform[1];
            spawnPoints[0] = spawnPoint;
        }
        else
        {
            Transform[] newSpawnPoints = new Transform[spawnPoints.Length + 1];
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                newSpawnPoints[i] = spawnPoints[i];
            }
            newSpawnPoints[spawnPoints.Length] = spawnPoint;
            spawnPoints = newSpawnPoints;
        }
    }
    
    public void RemoveSpawnPoint(int index)
    {
        if (spawnPoints != null && index >= 0 && index < spawnPoints.Length)
        {
            Transform[] newSpawnPoints = new Transform[spawnPoints.Length - 1];
            int newIndex = 0;
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                if (i != index)
                {
                    newSpawnPoints[newIndex] = spawnPoints[i];
                    newIndex++;
                }
            }
            spawnPoints = newSpawnPoints;
            
            // Корректируем текущий индекс
            if (currentSpawnIndex >= spawnPoints.Length)
            {
                currentSpawnIndex = 0;
            }
        }
    }
    
    public int GetSpawnPointsCount()
    {
        return spawnPoints != null ? spawnPoints.Length : 0;
    }
    
    [ContextMenu("Создать спавн поинт")]
    public void CreateSpawnPoint()
    {
        GameObject spawnPointObj = new GameObject("SpawnPoint_" + GetSpawnPointsCount());
        spawnPointObj.transform.position = transform.position + Vector3.right * GetSpawnPointsCount();
        
        // Добавляем компонент для визуализации
        SpawnPointVisualizer visualizer = spawnPointObj.AddComponent<SpawnPointVisualizer>();
        visualizer.SetSpawner(this);
        
        AddSpawnPoint(spawnPointObj.transform);
        
        #if UNITY_EDITOR
        Selection.activeGameObject = spawnPointObj;
        EditorGUIUtility.PingObject(spawnPointObj);
        #endif
        
        Debug.Log("Создан новый спавн поинт: " + spawnPointObj.name);
    }
    
    [ContextMenu("Удалить все спавн поинты")]
    public void ClearAllSpawnPoints()
    {
        if (spawnPoints != null)
        {
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                if (spawnPoints[i] != null)
                {
                    #if UNITY_EDITOR
                    DestroyImmediate(spawnPoints[i].gameObject);
                    #else
                    Destroy(spawnPoints[i].gameObject);
                    #endif
                }
            }
        }
        
        spawnPoints = new Transform[0];
        currentSpawnIndex = 0;
        
        Debug.Log("Все спавн поинты удалены");
    }
    
    void OnDrawGizmosSelected()
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                if (spawnPoints[i] != null)
                {
                    Vector3 center = spawnPoints[i].position;
                    Gizmos.DrawWireSphere(center, 0.5f);
                    
                    // Рисуем номер спавн поинта
                    #if UNITY_EDITOR
                    UnityEditor.Handles.Label(center + Vector3.up * 0.7f, i.ToString());
                    #endif
                }
            }
        }
        else
        {
            Gizmos.color = Color.green;
            Vector3 center = transform.position;
            Gizmos.DrawWireSphere(center, 0.5f);
        }
    }
}