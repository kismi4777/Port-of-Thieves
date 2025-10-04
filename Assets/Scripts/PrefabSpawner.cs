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
    public bool enableDropZone2Cleanup = true; // Включить автоматическое удаление в drop zone 2
    
    [Header("Drop Zone 2 Cleanup Settings")]
    private float lastSpawnTime = 0f; // Время последнего спавна
    private bool isDropZone2CleanupScheduled = false; // Запланирована ли очистка drop zone 2
    
    
    [Header("Drop Zone Integration")]
    public CursorTagDetector cursorDetector; // Ссылка на CursorTagDetector для проверки drop zone
    
    [Header("Расширенная рандомизация")]
    public bool useAdvancedRandomization = true; // Включить расширенную рандомизацию
    public int maxRecentSpawns = 3; // Максимальное количество последних заспавненных объектов для отслеживания
    public bool prioritizeObjectsNotOnScene = true; // Приоритет для объектов которых нет на сцене
    public float sceneCheckRadius = 2f; // Радиус проверки наличия объектов на сцене
    
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
    
    // Система расширенной рандомизации
    private List<GameObject> recentSpawnedPrefabs = new List<GameObject>(); // Последние заспавненные префабы
    private List<GameObject> availablePrefabs = new List<GameObject>(); // Доступные для спавна префабы
    
    // Система приоритета объектов на сцене
    private Dictionary<GameObject, int> prefabsOnSceneCount = new Dictionary<GameObject, int>(); // Количество объектов каждого типа на сцене
    
    void Start()
    {
        // Инициализация системы рандомизации
        InitializeRandomizationSystem();
        
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
        
        if (enableDropZone2Cleanup)
        {
            StartCoroutine(DropZone2CleanupRoutine());
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
    
    // Инициализация системы расширенной рандомизации
    void InitializeRandomizationSystem()
    {
        if (prefabsToSpawn != null && prefabsToSpawn.Length > 0)
        {
            availablePrefabs.Clear();
            recentSpawnedPrefabs.Clear();
            prefabsOnSceneCount.Clear();
            
            // Копируем все префабы в список доступных
            foreach (GameObject prefab in prefabsToSpawn)
            {
                if (prefab != null)
                {
                    availablePrefabs.Add(prefab);
                    prefabsOnSceneCount[prefab] = 0; // Инициализируем счетчик объектов на сцене
                }
            }
            
            Debug.Log($"Система рандомизации инициализирована. Доступно префабов: {availablePrefabs.Count}");
        }
    }
    
    // Получение префаба с учетом расширенной рандомизации
    GameObject GetRandomPrefabWithAdvancedRandomization()
    {
        if (!useAdvancedRandomization || prefabsToSpawn == null || prefabsToSpawn.Length == 0)
        {
            // Если расширенная рандомизация отключена, используем обычный случайный выбор
            return prefabsToSpawn[Random.Range(0, prefabsToSpawn.Length)];
        }
        
        // Обновляем счетчики объектов на сцене
        UpdateSceneObjectCounts();
        
        // Обновляем список доступных префабов
        UpdateAvailablePrefabs();
        
        if (availablePrefabs.Count == 0)
        {
            // Если все префабы уже заспавнены, сбрасываем список и начинаем заново
            ResetAvailablePrefabs();
            Debug.Log("Все префабы заспавнены, сбрасываем список доступных");
        }
        
        GameObject selectedPrefab = null;
        
        // Приоритет для объектов которых нет на сцене
        if (prioritizeObjectsNotOnScene && HasPrefabsNotOnScene())
        {
            List<GameObject> notOnScenePrefabs = GetPrefabsNotOnScene();
            // Фильтруем только те, которые доступны для спавна И не в последних заспавненных
            List<GameObject> availableNotOnScene = new List<GameObject>();
            foreach (GameObject prefab in notOnScenePrefabs)
            {
                if (availablePrefabs.Contains(prefab) && !recentSpawnedPrefabs.Contains(prefab))
                {
                    availableNotOnScene.Add(prefab);
                }
            }
            
            if (availableNotOnScene.Count > 0)
            {
                selectedPrefab = availableNotOnScene[Random.Range(0, availableNotOnScene.Count)];
                Debug.Log($"Выбран приоритетный префаб (не на сцене): {selectedPrefab.name}");
            }
        }
        
        // Если приоритетный выбор не сработал, используем обычную логику
        if (selectedPrefab == null)
        {
            selectedPrefab = availablePrefabs[Random.Range(0, availablePrefabs.Count)];
            Debug.Log($"Выбран обычный префаб: {selectedPrefab.name}");
        }
        
        // Добавляем в список последних заспавненных
        AddToRecentSpawns(selectedPrefab);
        
        // Удаляем из доступных (чтобы не спавнить повторно)
        availablePrefabs.Remove(selectedPrefab);
        
        Debug.Log($"Выбран префаб: {selectedPrefab.name}. Осталось доступных: {availablePrefabs.Count}");
        
        return selectedPrefab;
    }
    
    // Обновление списка доступных префабов
    void UpdateAvailablePrefabs()
    {
        // Если все префабы уже заспавнены, ничего не делаем
        if (availablePrefabs.Count == 0)
            return;
            
        // Проверяем, есть ли неиспользованные префабы из исходного массива
        List<GameObject> unusedPrefabs = new List<GameObject>();
        foreach (GameObject prefab in prefabsToSpawn)
        {
            if (prefab != null && !availablePrefabs.Contains(prefab))
            {
                // Проверяем, что префаб не в списке последних заспавненных
                if (!recentSpawnedPrefabs.Contains(prefab))
                {
                    unusedPrefabs.Add(prefab);
                }
            }
        }
        
        // Если есть неиспользованные префабы, добавляем их в доступные
        if (unusedPrefabs.Count > 0)
        {
            availablePrefabs.AddRange(unusedPrefabs);
            Debug.Log($"Добавлено {unusedPrefabs.Count} неиспользованных префабов в доступные");
        }
    }
    
    // Сброс списка доступных префабов
    void ResetAvailablePrefabs()
    {
        availablePrefabs.Clear();
        foreach (GameObject prefab in prefabsToSpawn)
        {
            if (prefab != null)
            {
                availablePrefabs.Add(prefab);
            }
        }
    }
    
    // Добавление префаба в список последних заспавненных
    void AddToRecentSpawns(GameObject prefab)
    {
        recentSpawnedPrefabs.Add(prefab);
        
        // Ограничиваем размер списка
        if (recentSpawnedPrefabs.Count > maxRecentSpawns)
        {
            recentSpawnedPrefabs.RemoveAt(0);
        }
        
        Debug.Log($"Добавлен в последние заспавненные: {prefab.name}. Всего в списке: {recentSpawnedPrefabs.Count}");
    }
    
    // Проверка наличия объектов определенного типа на сцене
    int CountObjectsOnScene(GameObject prefab)
    {
        if (prefab == null) return 0;
        
        int count = 0;
        string prefabName = prefab.name;
        
        // Используем отслеживание через spawnedObjects для производительности
        foreach (SpawnedObjectInfo objInfo in spawnedObjects)
        {
            if (objInfo.obj != null && (objInfo.obj.name == prefabName || objInfo.obj.name == prefabName + "(Clone)"))
            {
                count++;
            }
        }
        
        return count;
    }
    
    // Обновление счетчика объектов на сцене
    void UpdateSceneObjectCounts()
    {
        if (!prioritizeObjectsNotOnScene) return;
        
        foreach (GameObject prefab in prefabsToSpawn)
        {
            if (prefab != null)
            {
                prefabsOnSceneCount[prefab] = CountObjectsOnScene(prefab);
            }
        }
    }
    
    // Получение префабов которых нет на сцене
    List<GameObject> GetPrefabsNotOnScene()
    {
        List<GameObject> notOnScene = new List<GameObject>();
        
        foreach (GameObject prefab in prefabsToSpawn)
        {
            if (prefab != null && prefabsOnSceneCount.ContainsKey(prefab) && prefabsOnSceneCount[prefab] == 0)
            {
                notOnScene.Add(prefab);
            }
        }
        
        return notOnScene;
    }
    
    // Проверка, есть ли объекты которых нет на сцене
    bool HasPrefabsNotOnScene()
    {
        return GetPrefabsNotOnScene().Count > 0;
    }
    
    // Получение уникальных префабов для спавна на всех точках
    List<GameObject> GetUniquePrefabsForAllPointsSpawn()
    {
        List<GameObject> result = new List<GameObject>();
        
        if (!useAdvancedRandomization || prefabsToSpawn == null || prefabsToSpawn.Length == 0)
        {
            // Если расширенная рандомизация отключена, используем обычный случайный выбор
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                result.Add(prefabsToSpawn[Random.Range(0, prefabsToSpawn.Length)]);
            }
            return result;
        }
        
        // Обновляем счетчики объектов на сцене
        UpdateSceneObjectCounts();
        
        // Обновляем список доступных префабов
        UpdateAvailablePrefabs();
        
        // Если все префабы уже заспавнены, сбрасываем список
        if (availablePrefabs.Count == 0)
        {
            ResetAvailablePrefabs();
            Debug.Log("Все префабы заспавнены, сбрасываем список доступных для AllPoints спавна");
        }
        
        // Создаем список префабов с приоритетом для объектов которых нет на сцене
        List<GameObject> prioritizedPrefabs = new List<GameObject>();
        
        if (prioritizeObjectsNotOnScene && HasPrefabsNotOnScene())
        {
            // Сначала добавляем префабы которых нет на сцене
            List<GameObject> notOnScenePrefabs = GetPrefabsNotOnScene();
            foreach (GameObject prefab in notOnScenePrefabs)
            {
                if (availablePrefabs.Contains(prefab) && !recentSpawnedPrefabs.Contains(prefab))
                {
                    prioritizedPrefabs.Add(prefab);
                }
            }
            
            // Затем добавляем остальные доступные префабы
            foreach (GameObject prefab in availablePrefabs)
            {
                if (!prioritizedPrefabs.Contains(prefab) && !recentSpawnedPrefabs.Contains(prefab))
                {
                    prioritizedPrefabs.Add(prefab);
                }
            }
        }
        else
        {
            // Если приоритет отключен, используем все доступные префабы (исключая последние заспавненные)
            foreach (GameObject prefab in availablePrefabs)
            {
                if (!recentSpawnedPrefabs.Contains(prefab))
                {
                    prioritizedPrefabs.Add(prefab);
                }
            }
        }
        
        // Перемешиваем список для случайности
        for (int i = 0; i < prioritizedPrefabs.Count; i++)
        {
            GameObject temp = prioritizedPrefabs[i];
            int randomIndex = Random.Range(i, prioritizedPrefabs.Count);
            prioritizedPrefabs[i] = prioritizedPrefabs[randomIndex];
            prioritizedPrefabs[randomIndex] = temp;
        }
        
        // Заполняем результат уникальными префабами
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (i < prioritizedPrefabs.Count)
            {
                // Используем уникальный префаб
                result.Add(prioritizedPrefabs[i]);
            }
            else
            {
                // Если точек спавна больше чем префабов, циклически повторяем
                result.Add(prioritizedPrefabs[i % prioritizedPrefabs.Count]);
            }
        }
        
        // Обновляем систему рандомизации - удаляем использованные префабы (без дублирования)
        HashSet<GameObject> usedPrefabsSet = new HashSet<GameObject>(result);
        foreach (GameObject usedPrefab in usedPrefabsSet)
        {
            if (availablePrefabs.Contains(usedPrefab))
            {
                availablePrefabs.Remove(usedPrefab);
                AddToRecentSpawns(usedPrefab);
            }
        }
        
        Debug.Log($"Подготовлено {result.Count} уникальных префабов для AllPoints спавна. Осталось доступных: {availablePrefabs.Count}");
        
        return result;
    }
    
    IEnumerator DropZone2CleanupRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f); // Проверяем каждые 0.5 секунды
            
            if (cursorDetector == null)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }
            
            // Проверяем, истекло ли время с последнего спавна
            if (lastSpawnTime > 0 && Time.time - lastSpawnTime >= objectLifetime && !isDropZone2CleanupScheduled)
            {
                Debug.Log("Время истекло! Удаляем все объекты в drop zone 2");
                CleanupAllObjectsInDropZone2();
                isDropZone2CleanupScheduled = true;
            }
        }
    }
    
    void CleanupAllObjectsInDropZone2()
    {
        if (cursorDetector == null) return;
        
        // Находим все объекты с тегом "Obj" в сцене
        GameObject[] allObjObjects = GameObject.FindGameObjectsWithTag("Obj");
        int deletedCount = 0;
        
        foreach (GameObject obj in allObjObjects)
        {
            if (obj == null) continue;
            
            // Проверяем, что объект еще существует и его transform не null
            if (obj.transform != null)
            {
                // Проверяем, находится ли объект в drop zone 2
                if (cursorDetector.IsPositionInDropZone2(obj.transform.position))
                {
                    Debug.Log("Удаляем объект " + obj.name + " из drop zone 2");
                    Destroy(obj);
                    deletedCount++;
                }
            }
        }
        
        Debug.Log($"Удалено {deletedCount} объектов из drop zone 2");
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
        
        GameObject prefabToSpawn = GetRandomPrefabWithAdvancedRandomization();
        Vector3 spawnPosition = GetSpawnPosition();
        GameObject spawnedObject = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
        
        // Убираем суффикс "(Clone)" из имени объекта
        if (spawnedObject.name.EndsWith("(Clone)"))
        {
            spawnedObject.name = spawnedObject.name.Replace("(Clone)", "");
        }
        
        // Устанавливаем правильный масштаб в зависимости от зоны спавна
        SetCorrectScaleForSpawnedObject(spawnedObject, spawnPosition);
        
        float lifetime = autoCleanup ? GetObjectLifetime() : float.MaxValue;
        spawnedObjects.Add(new SpawnedObjectInfo(spawnedObject, lifetime));
        
        // Запускаем таймер для drop zone 2 cleanup
        if (enableDropZone2Cleanup)
        {
            lastSpawnTime = Time.time;
            isDropZone2CleanupScheduled = false;
            Debug.Log("Запущен таймер для drop zone 2 cleanup: " + objectLifetime + "с");
        }
        
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
        
        // Для режима AllPoints используем специальную логику без дублирования
        List<GameObject> prefabsForThisSpawn = GetUniquePrefabsForAllPointsSpawn();
        
        // Спавним префаб на каждой точке спавна
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] != null)
            {
                // Выбираем префаб из подготовленного списка (с циклическим повторением если нужно)
                GameObject prefabToSpawn = prefabsForThisSpawn[i % prefabsForThisSpawn.Count];
                
                GameObject spawnedObject = Instantiate(prefabToSpawn, spawnPoints[i].position, Quaternion.identity);
                
                // Убираем суффикс "(Clone)" из имени объекта
                if (spawnedObject.name.EndsWith("(Clone)"))
                {
                    spawnedObject.name = spawnedObject.name.Replace("(Clone)", "");
                }
                
                // Устанавливаем правильный масштаб в зависимости от зоны спавна
                SetCorrectScaleForSpawnedObject(spawnedObject, spawnPoints[i].position);
                
                float lifetime = autoCleanup ? GetObjectLifetime() : float.MaxValue;
                spawnedObjects.Add(new SpawnedObjectInfo(spawnedObject, lifetime));
                
                Debug.Log("Заспавнен объект: " + spawnedObject.name + " в позиции " + spawnPoints[i].position + (autoCleanup ? " (время жизни: " + lifetime + "с)" : ""));
            }
        }
        
        
        // Запускаем таймер для drop zone 2 cleanup
        if (enableDropZone2Cleanup)
        {
            lastSpawnTime = Time.time;
            isDropZone2CleanupScheduled = false;
            Debug.Log("Запущен таймер для drop zone 2 cleanup: " + objectLifetime + "с");
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
        
        GameObject prefabToSpawn = GetRandomPrefabWithAdvancedRandomization();
        GameObject spawnedObject = Instantiate(prefabToSpawn, position, Quaternion.identity);
        
        // Убираем суффикс "(Clone)" из имени объекта
        if (spawnedObject.name.EndsWith("(Clone)"))
        {
            spawnedObject.name = spawnedObject.name.Replace("(Clone)", "");
        }
        
        // Устанавливаем правильный масштаб в зависимости от зоны спавна
        SetCorrectScaleForSpawnedObject(spawnedObject, position);
        
        float lifetime = autoCleanup ? GetObjectLifetime() : float.MaxValue;
        spawnedObjects.Add(new SpawnedObjectInfo(spawnedObject, lifetime));
        
        // Запускаем таймер для drop zone 2 cleanup
        if (enableDropZone2Cleanup)
        {
            lastSpawnTime = Time.time;
            isDropZone2CleanupScheduled = false;
            Debug.Log("Запущен таймер для drop zone 2 cleanup: " + objectLifetime + "с");
        }
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
        
        // Убираем суффикс "(Clone)" из имени объекта
        if (spawnedObject.name.EndsWith("(Clone)"))
        {
            spawnedObject.name = spawnedObject.name.Replace("(Clone)", "");
        }
        
        // Устанавливаем правильный масштаб в зависимости от зоны спавна
        SetCorrectScaleForSpawnedObject(spawnedObject, spawnPosition);
        
        float lifetime = autoCleanup ? GetObjectLifetime() : float.MaxValue;
        spawnedObjects.Add(new SpawnedObjectInfo(spawnedObject, lifetime));
        
        // Запускаем таймер для drop zone 2 cleanup
        if (enableDropZone2Cleanup)
        {
            lastSpawnTime = Time.time;
            isDropZone2CleanupScheduled = false;
            Debug.Log("Запущен таймер для drop zone 2 cleanup: " + objectLifetime + "с");
        }
        
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
                
                // Убираем суффикс "(Clone)" из имени объекта
                if (spawnedObject.name.EndsWith("(Clone)"))
                {
                    spawnedObject.name = spawnedObject.name.Replace("(Clone)", "");
                }
                
                // Устанавливаем правильный масштаб в зависимости от зоны спавна
                SetCorrectScaleForSpawnedObject(spawnedObject, spawnPoints[i].position);
                
                float lifetime = autoCleanup ? GetObjectLifetime() : float.MaxValue;
                spawnedObjects.Add(new SpawnedObjectInfo(spawnedObject, lifetime));
                
                Debug.Log("Заспавнен объект: " + spawnedObject.name + " в позиции " + spawnPoints[i].position + (autoCleanup ? " (время жизни: " + lifetime + "с)" : ""));
            }
        }
        
        // Обновляем систему рандомизации для конкретного префаба
        if (useAdvancedRandomization)
        {
            GameObject specificPrefab = prefabsToSpawn[prefabIndex];
            if (availablePrefabs.Contains(specificPrefab))
            {
                availablePrefabs.Remove(specificPrefab);
                AddToRecentSpawns(specificPrefab);
            }
        }
        
        // Запускаем таймер для drop zone 2 cleanup
        if (enableDropZone2Cleanup)
        {
            lastSpawnTime = Time.time;
            isDropZone2CleanupScheduled = false;
            Debug.Log("Запущен таймер для drop zone 2 cleanup: " + objectLifetime + "с");
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
        
        // Сбрасываем систему рандомизации при очистке всех объектов
        if (useAdvancedRandomization)
        {
            ResetAvailablePrefabs();
            recentSpawnedPrefabs.Clear();
            prefabsOnSceneCount.Clear();
            Debug.Log("Система рандомизации сброшена после очистки всех объектов");
        }
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
        if (obj.transform != null)
        {
            return cursorDetector.CanDropAtPosition(obj.transform.position);
        }
        return false;
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
    
    // Методы управления системой рандомизации
    public void SetAdvancedRandomization(bool enabled)
    {
        useAdvancedRandomization = enabled;
        if (enabled)
        {
            InitializeRandomizationSystem();
        }
        else
        {
            availablePrefabs.Clear();
            recentSpawnedPrefabs.Clear();
        }
        Debug.Log($"Расширенная рандомизация {(enabled ? "включена" : "отключена")}");
    }
    
    public void SetMaxRecentSpawns(int maxSpawns)
    {
        maxRecentSpawns = Mathf.Max(1, maxSpawns);
        Debug.Log($"Максимальное количество последних заспавненных объектов установлено: {maxRecentSpawns}");
    }
    
    public void ResetRandomizationSystem()
    {
        if (useAdvancedRandomization)
        {
            InitializeRandomizationSystem();
            Debug.Log("Система рандомизации сброшена");
        }
    }
    
    public int GetAvailablePrefabsCount()
    {
        return availablePrefabs.Count;
    }
    
    public int GetRecentSpawnsCount()
    {
        return recentSpawnedPrefabs.Count;
    }
    
    public List<GameObject> GetRecentSpawnedPrefabs()
    {
        return new List<GameObject>(recentSpawnedPrefabs);
    }
    
    // Методы управления приоритетом объектов на сцене
    public void SetScenePriority(bool enabled)
    {
        prioritizeObjectsNotOnScene = enabled;
        if (enabled)
        {
            UpdateSceneObjectCounts();
        }
        Debug.Log($"Приоритет объектов на сцене {(enabled ? "включен" : "отключен")}");
    }
    
    public void SetSceneCheckRadius(float radius)
    {
        sceneCheckRadius = Mathf.Max(0.1f, radius);
        Debug.Log($"Радиус проверки объектов на сцене установлен: {sceneCheckRadius}");
    }
    
    public void UpdateSceneCounts()
    {
        UpdateSceneObjectCounts();
        Debug.Log("Счетчики объектов на сцене обновлены");
    }
    
    public int GetObjectsOnSceneCount(GameObject prefab)
    {
        if (prefab == null || !prefabsOnSceneCount.ContainsKey(prefab))
            return 0;
        return prefabsOnSceneCount[prefab];
    }
    
    public List<GameObject> GetPrefabsNotOnSceneList()
    {
        return GetPrefabsNotOnScene();
    }
    
    public bool HasObjectsNotOnScene()
    {
        return HasPrefabsNotOnScene();
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
    
    [ContextMenu("Сбросить систему рандомизации")]
    public void ResetRandomizationSystemMenu()
    {
        ResetRandomizationSystem();
    }
    
    [ContextMenu("Показать статистику рандомизации")]
    public void ShowRandomizationStats()
    {
        Debug.Log($"=== Статистика системы рандомизации ===");
        Debug.Log($"Расширенная рандомизация: {(useAdvancedRandomization ? "Включена" : "Отключена")}");
        Debug.Log($"Максимум последних заспавненных: {maxRecentSpawns}");
        Debug.Log($"Доступных префабов: {GetAvailablePrefabsCount()}");
        Debug.Log($"Последних заспавненных: {GetRecentSpawnsCount()}");
        Debug.Log($"Всего префабов в массиве: {(prefabsToSpawn != null ? prefabsToSpawn.Length : 0)}");
        Debug.Log($"Приоритет объектов на сцене: {(prioritizeObjectsNotOnScene ? "Включен" : "Отключен")}");
        
        if (recentSpawnedPrefabs.Count > 0)
        {
            Debug.Log("Последние заспавненные префабы:");
            for (int i = 0; i < recentSpawnedPrefabs.Count; i++)
            {
                Debug.Log($"  {i + 1}. {recentSpawnedPrefabs[i].name}");
            }
        }
        
        if (prioritizeObjectsNotOnScene)
        {
            Debug.Log("=== Статистика объектов на сцене ===");
            UpdateSceneObjectCounts();
            foreach (GameObject prefab in prefabsToSpawn)
            {
                if (prefab != null)
                {
                    int count = GetObjectsOnSceneCount(prefab);
                    Debug.Log($"  {prefab.name}: {count} объектов на сцене");
                }
            }
            
            List<GameObject> notOnScene = GetPrefabsNotOnScene();
            if (notOnScene.Count > 0)
            {
                Debug.Log("Префабы которых нет на сцене:");
                foreach (GameObject prefab in notOnScene)
                {
                    Debug.Log($"  - {prefab.name}");
                }
            }
        }
    }
    
    [ContextMenu("Обновить счетчики объектов на сцене")]
    public void UpdateSceneCountsMenu()
    {
        UpdateSceneCounts();
    }
    
    // Устанавливает правильный масштаб для заспавненного объекта в зависимости от зоны
    private void SetCorrectScaleForSpawnedObject(GameObject spawnedObject, Vector3 spawnPosition)
    {
        if (spawnedObject == null || cursorDetector == null) return;
        
        // Получаем исходный масштаб объекта (масштаб префаба)
        Vector3 originalScale = spawnedObject.transform.localScale;
        
        // Проверяем, находится ли объект в drop zone 2
        if (cursorDetector.IsPositionInDropZone2(spawnPosition))
        {
            // Если в drop zone 2, применяем масштаб drop zone 2
            if (cursorDetector.useDropZone2ScaleEffect)
            {
                spawnedObject.transform.localScale = originalScale * cursorDetector.dropZone2ScaleMultiplier;
                Debug.Log($"Объект {spawnedObject.name} заспавнен в drop zone 2, масштаб установлен: {originalScale} * {cursorDetector.dropZone2ScaleMultiplier} = {spawnedObject.transform.localScale}");
            }
        }
        else
        {
            // Если не в drop zone 2, оставляем исходный масштаб
            Debug.Log($"Объект {spawnedObject.name} заспавнен вне drop zone 2, масштаб: {originalScale}");
        }
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