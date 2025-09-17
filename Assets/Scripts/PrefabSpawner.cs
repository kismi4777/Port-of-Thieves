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
    
    [Header("Настройки автоматического удаления")]
    public bool autoCleanup = true;
    public float objectLifetime = 10f;
    public bool preventCleanupOnDragging = true;
    public bool preventCleanupInDropZone = true;
    
    
    [Header("Drop Zone Integration")]
    public CursorTagDetector cursorDetector; // Ссылка на CursorTagDetector для проверки drop zone
    
    public enum SpawnPointMode
    {
        Random,     // Случайный спавн поинт
        Sequential, // По порядку
        AllPoints   // На всех точках одновременно
    }
    
    [System.Serializable]
    public class SpawnedObjectInfo
    {
        public GameObject obj;
        public float lifetime;
        public float spawnTime;
        public bool isBeingDragged;
        public bool isInDropZone;
        
        public SpawnedObjectInfo(GameObject obj, float lifetime)
        {
            this.obj = obj;
            this.lifetime = lifetime;
            this.spawnTime = Time.time;
            this.isBeingDragged = false;
            this.isInDropZone = false;
        }
    }
    
    private List<SpawnedObjectInfo> spawnedObjects = new List<SpawnedObjectInfo>();
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
        
        if (autoCleanup)
        {
            StartCoroutine(AutoCleanupRoutine());
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
    
    
    IEnumerator AutoCleanupRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f); // Проверяем каждые 0.5 секунды
            
            for (int i = spawnedObjects.Count - 1; i >= 0; i--)
            {
                if (spawnedObjects[i].obj == null)
                {
                    // Объект уже удален, убираем из списка
                    spawnedObjects.RemoveAt(i);
                    continue;
                }
                
                // Проверяем, истекло ли время жизни объекта
                bool shouldDelete = Time.time - spawnedObjects[i].spawnTime >= spawnedObjects[i].lifetime;
                
                // Если включена защита от удаления при перетаскивании, проверяем состояние перетаскивания
                if (shouldDelete && preventCleanupOnDragging && spawnedObjects[i].isBeingDragged)
                {
                    Debug.Log("Объект " + spawnedObjects[i].obj.name + " не удален - он перетаскивается!");
                    shouldDelete = false;
                }
                
                // Если включена защита от удаления в drop zone, проверяем нахождение в drop zone
                if (shouldDelete && preventCleanupInDropZone && IsObjectInDropZone(spawnedObjects[i].obj))
                {
                    Debug.Log("Объект " + spawnedObjects[i].obj.name + " не удален - он находится в drop zone!");
                    shouldDelete = false;
                }
                
                if (shouldDelete)
                {
                    Debug.Log("Автоматически удален объект: " + spawnedObjects[i].obj.name + " (время жизни: " + spawnedObjects[i].lifetime + "с)");
                    Destroy(spawnedObjects[i].obj);
                    spawnedObjects.RemoveAt(i);
                }
            }
        }
    }
    
    float GetObjectLifetime()
    {
        return objectLifetime;
    }
    
    public void SpawnPrefab()
    {
        if (prefabsToSpawn == null || prefabsToSpawn.Length == 0)
        {
            Debug.LogWarning("Нет префабов для спавна!");
            return;
        }
        
        // Если режим AllPoints, спавним на всех точках
        if (spawnPointMode == SpawnPointMode.AllPoints)
        {
            SpawnAllPrefabs();
            return;
        }
        
        GameObject prefabToSpawn = prefabsToSpawn[Random.Range(0, prefabsToSpawn.Length)];
        Vector3 spawnPosition = GetSpawnPosition();
        GameObject spawnedObject = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
        
        float lifetime = autoCleanup ? GetObjectLifetime() : float.MaxValue;
        spawnedObjects.Add(new SpawnedObjectInfo(spawnedObject, lifetime));
        
        Debug.Log("Заспавнен объект: " + spawnedObject.name + " в позиции " + spawnPosition + (autoCleanup ? " (время жизни: " + lifetime + "с)" : ""));
    }
    
    public void SpawnAllPrefabs()
    {
        if (prefabsToSpawn == null || prefabsToSpawn.Length == 0)
        {
            Debug.LogWarning("Нет префабов для спавна!");
            return;
        }
        
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("Нет точек спавна!");
            return;
        }
        
        // Спавним префаб на каждой точке спавна
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] != null)
            {
                GameObject prefabToSpawn = prefabsToSpawn[Random.Range(0, prefabsToSpawn.Length)];
                GameObject spawnedObject = Instantiate(prefabToSpawn, spawnPoints[i].position, Quaternion.identity);
                
                float lifetime = autoCleanup ? GetObjectLifetime() : float.MaxValue;
                spawnedObjects.Add(new SpawnedObjectInfo(spawnedObject, lifetime));
                
                Debug.Log("Заспавнен объект: " + spawnedObject.name + " в позиции " + spawnPoints[i].position + (autoCleanup ? " (время жизни: " + lifetime + "с)" : ""));
            }
        }
        
        Debug.Log("Заспавнено " + spawnPoints.Length + " объектов на всех точках");
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
        
        float lifetime = autoCleanup ? GetObjectLifetime() : float.MaxValue;
        spawnedObjects.Add(new SpawnedObjectInfo(spawnedObject, lifetime));
    }
    
    public void SpawnSpecificPrefab(int prefabIndex)
    {
        if (prefabsToSpawn == null || prefabIndex < 0 || prefabIndex >= prefabsToSpawn.Length)
        {
            Debug.LogWarning("Неверный индекс префаба!");
            return;
        }
        
        // Если режим AllPoints, спавним конкретный префаб на всех точках
        if (spawnPointMode == SpawnPointMode.AllPoints)
        {
            SpawnSpecificPrefabOnAllPoints(prefabIndex);
            return;
        }
        
        Vector3 spawnPosition = GetSpawnPosition();
        GameObject spawnedObject = Instantiate(prefabsToSpawn[prefabIndex], spawnPosition, Quaternion.identity);
        
        float lifetime = autoCleanup ? GetObjectLifetime() : float.MaxValue;
        spawnedObjects.Add(new SpawnedObjectInfo(spawnedObject, lifetime));
    }
    
    public void SpawnSpecificPrefabOnAllPoints(int prefabIndex)
    {
        if (prefabsToSpawn == null || prefabIndex < 0 || prefabIndex >= prefabsToSpawn.Length)
        {
            Debug.LogWarning("Неверный индекс префаба!");
            return;
        }
        
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("Нет точек спавна!");
            return;
        }
        
        // Спавним конкретный префаб на каждой точке спавна
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] != null)
            {
                GameObject spawnedObject = Instantiate(prefabsToSpawn[prefabIndex], spawnPoints[i].position, Quaternion.identity);
                
                float lifetime = autoCleanup ? GetObjectLifetime() : float.MaxValue;
                spawnedObjects.Add(new SpawnedObjectInfo(spawnedObject, lifetime));
                
                Debug.Log("Заспавнен объект: " + spawnedObject.name + " в позиции " + spawnPoints[i].position + (autoCleanup ? " (время жизни: " + lifetime + "с)" : ""));
            }
        }
        
        Debug.Log("Заспавнено " + spawnPoints.Length + " объектов типа " + prefabsToSpawn[prefabIndex].name + " на всех точках");
    }
    
    public void ClearAllSpawnedObjects()
    {
        foreach (SpawnedObjectInfo objInfo in spawnedObjects)
        {
            if (objInfo.obj != null)
                Destroy(objInfo.obj);
        }
        spawnedObjects.Clear();
    }
    
    public int GetSpawnedObjectsCount()
    {
        return spawnedObjects.Count;
    }
    
    public void SetAutoCleanup(bool enabled)
    {
        autoCleanup = enabled;
        if (enabled && !IsInvoking("AutoCleanupRoutine"))
        {
            StartCoroutine(AutoCleanupRoutine());
        }
    }
    
    public void SetObjectLifetime(float lifetime)
    {
        objectLifetime = lifetime;
    }
    
    public void SetDraggingProtection(bool enabled)
    {
        preventCleanupOnDragging = enabled;
    }
    
    public void MarkObjectAsDragging(GameObject obj)
    {
        for (int i = 0; i < spawnedObjects.Count; i++)
        {
            if (spawnedObjects[i].obj == obj)
            {
                spawnedObjects[i].isBeingDragged = true;
                Debug.Log("Объект " + obj.name + " помечен как перетаскиваемый");
                return;
            }
        }
    }
    
    public void MarkObjectAsDropped(GameObject obj)
    {
        for (int i = 0; i < spawnedObjects.Count; i++)
        {
            if (spawnedObjects[i].obj == obj)
            {
                spawnedObjects[i].isBeingDragged = false;
                Debug.Log("Объект " + obj.name + " помечен как отпущенный");
                return;
            }
        }
    }
    
    public bool IsObjectBeingDragged(GameObject obj)
    {
        for (int i = 0; i < spawnedObjects.Count; i++)
        {
            if (spawnedObjects[i].obj == obj)
            {
                return spawnedObjects[i].isBeingDragged;
            }
        }
        return false;
    }
    
    public int GetDraggedObjectsCount()
    {
        int count = 0;
        foreach (var objInfo in spawnedObjects)
        {
            if (objInfo.isBeingDragged)
                count++;
        }
        return count;
    }
    
    public void SetDropZoneProtection(bool enabled)
    {
        preventCleanupInDropZone = enabled;
    }
    
    public void MarkObjectAsInDropZone(GameObject obj)
    {
        for (int i = 0; i < spawnedObjects.Count; i++)
        {
            if (spawnedObjects[i].obj == obj)
            {
                spawnedObjects[i].isInDropZone = true;
                Debug.Log("Объект " + obj.name + " помечен как находящийся в drop zone");
                return;
            }
        }
    }
    
    public void MarkObjectAsOutOfDropZone(GameObject obj)
    {
        for (int i = 0; i < spawnedObjects.Count; i++)
        {
            if (spawnedObjects[i].obj == obj)
            {
                spawnedObjects[i].isInDropZone = false;
                Debug.Log("Объект " + obj.name + " помечен как находящийся вне drop zone");
                return;
            }
        }
    }
    
    public bool IsObjectInDropZone(GameObject obj)
    {
        // Если нет ссылки на CursorTagDetector, используем внутренний флаг
        if (cursorDetector == null)
        {
            for (int i = 0; i < spawnedObjects.Count; i++)
            {
                if (spawnedObjects[i].obj == obj)
                {
                    return spawnedObjects[i].isInDropZone;
                }
            }
            return false;
        }
        
        // Используем CursorTagDetector для проверки drop zone
        return cursorDetector.CanDropAtPosition(obj.transform.position);
    }
    
    public int GetObjectsInDropZoneCount()
    {
        int count = 0;
        foreach (var objInfo in spawnedObjects)
        {
            if (objInfo.isInDropZone)
                count++;
        }
        return count;
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