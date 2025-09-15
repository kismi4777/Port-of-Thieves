using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PrefabSpawner : MonoBehaviour
{
    [Header("Префабы для спавна")]
    public GameObject[] prefabsToSpawn;
    
    [Header("Настройки спавна")]
    public Transform spawnPoint;
    public bool spawnOnStart = false;
    public float spawnDelay = 1f;
    public bool continuousSpawning = false;
    public float spawnInterval = 2f;
    
    private List<GameObject> spawnedObjects = new List<GameObject>();
    
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
        return spawnPoint != null ? spawnPoint.position : transform.position;
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
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 center = spawnPoint != null ? spawnPoint.position : transform.position;
        Gizmos.DrawWireSphere(center, 0.5f);
    }
}