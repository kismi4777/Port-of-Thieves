using UnityEngine;
using UnityEngine.InputSystem;

public class CursorTagDetector : MonoBehaviour
{
    [Header("Cursor Tag Detection")]
    public string currentTag = "None";
    
    [Header("Drag Settings")]
    public string draggableTag = "Draggable"; // Тег объектов, которые можно перетаскивать
    
    [Header("Drop Zones")]
    public bool useDropZone = true; // Включить зоны для отпускания
    public Vector2 zone1Center = Vector2.zero; // Центр первой зоны
    public Vector2 zone1Size = new Vector2(10f, 10f); // Размер первой зоны
    public Color zone1Color = Color.green; // Цвет первой зоны (для отладки)
    
    public Vector2 zone2Center = new Vector2(5f, 5f); // Центр второй зоны
    public Vector2 zone2Size = new Vector2(8f, 8f); // Размер второй зоны
    public Color zone2Color = Color.blue; // Цвет второй зоны (для отладки)
    
    public Vector2 zone3Center = new Vector2(-5f, 5f); // Центр третьей зоны
    public Vector2 zone3Size = new Vector2(6f, 6f); // Размер третьей зоны
    public Color zone3Color = Color.yellow; // Цвет третьей зоны (для отладки)
    
    [Header("Scale Effect")]
    public bool useScaleEffect = true; // Включить эффект масштаба
    public float scaleMultiplier = 1.2f; // Множитель масштаба (1.2 = +20%)
    
    [Header("Drop Zone 2 Scale Effect")]
    public bool useDropZone2ScaleEffect = true; // Включить эффект масштаба для drop zone 2
    public float dropZone2ScaleMultiplier = 1.5f; // Множитель масштаба для drop zone 2 (1.5 = +50%)
    
    [Header("Drop Zone 3 Scale Effect")]
    public bool useDropZone3ScaleEffect = true; // Включить эффект масштаба для drop zone 3
    public float dropZone3ScaleMultiplier = 1.8f; // Множитель масштаба для drop zone 3 (1.8 = +80%)
    
    [Header("Drop Zone 3 Destroy Effect")]
    public bool useDropZone3Destroy = true; // Включить удаление объектов в drop zone 3
    public float destroyDelay = 0.1f; // Задержка перед удалением (в секундах)
    
    [Header("Zone 3 Object Restrictions")]
    public bool useZone3ObjectRestrictions = true; // Включить ограничения для объектов в zone 3
    public ObjectDataExtractor objectDataExtractor; // Ссылка на ObjectDataExtractor для проверки соответствия
    public bool autoFindObjectDataExtractor = true; // Автоматический поиск ObjectDataExtractor
    public bool showZone3RestrictionDebugInfo = true; // Показывать отладочную информацию об ограничениях zone 3
    
    [Header("Sound Effects")]
    public bool useSoundEffects = true; // Включить звуковые эффекты
    public AudioClip pickupSound; // Звук при поднятии объекта
    public AudioClip dropSound; // Звук при опускании объекта
    public AudioClip destroySound; // Звук при удалении объекта
    public float soundVolume = 1.0f; // Громкость звуков
    
    [Header("Particle Effects")]
    public bool useParticleEffects = true; // Включить эффекты частиц
    public GameObject dropParticlePrefab; // Префаб частиц при отпускании
    public GameObject destroyParticlePrefab; // Префаб частиц при удалении
    public float particleDuration = 2.0f; // Время до удаления частиц (в секундах)
    
    [Header("Zone 3 Particle Effects")]
    public GameObject zone3SuccessParticlePrefab; // Префаб партиклов при успешном удалении правильного объекта в zone 3
    public GameObject zone3BlockedParticlePrefab; // Префаб партиклов при блокировке неправильного объекта в zone 3
    public bool useZone3ParticleEffects = true; // Включить партиклы для zone 3
    
    [Header("PrefabSpawner Integration")]
    public PrefabSpawner prefabSpawner; // Ссылка на PrefabSpawner для уведомлений
    
    [Header("Zone 3 Tracking")]
    public bool enableZone3Tracking = true; // Включить отслеживание удаленных объектов в zone 3
    public bool showTrackingDebugInfo = true; // Показывать отладочную информацию отслеживания
    
    [Header("Zone 3 Client Integration")]
    public bool requireClientActiveForZone3 = true; // Zone 3 активна только когда Client активен
    public bool autoFindClientManager = true; // Автоматический поиск ClientManager
    
    
    private Camera mainCamera;
    private Mouse mouse;
    private bool isDragging = false;
    private Transform draggedObject = null;
    private Vector3 originalPosition; // Исходная позиция объекта
    private Vector3 originalScale; // Исходный масштаб объекта
    private Vector3 trueOriginalScale; // Истинно исходный масштаб объекта (без эффектов)
    private bool isInDropZone2 = false; // Флаг нахождения в drop zone 2
    private bool isInDropZone3 = false; // Флаг нахождения в drop zone 3
    private AudioSource audioSource; // Компонент для воспроизведения звуков
    
    // Система отслеживания удаленных объектов в zone 3
    private System.Collections.Generic.List<DestroyedObjectInfo> destroyedObjects = new System.Collections.Generic.List<DestroyedObjectInfo>();
    private int totalDestroyedCount = 0; // Общее количество удаленных объектов
    
    // Ссылка на ClientManager для проверки активности Client
    private ClientManager clientManager;
    
    // Кэширование состояния Client для оптимизации
    private bool cachedClientActiveState = false;
    private bool clientStateCached = false;
    
    // Кэширование ссылки на ClientManager для оптимизации
    private bool clientManagerSearchAttempted = false;
    private float lastClientManagerSearchTime = 0f;
    private float clientManagerSearchInterval = 5f; // Поиск ClientManager каждые 5 секунд
    
    
    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
        
        mouse = Mouse.current;
        
        // Настраиваем AudioSource для воспроизведения звуков
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.volume = soundVolume;
        audioSource.loop = false;
        audioSource.priority = 0; // Высокий приоритет для быстрого воспроизведения
        audioSource.bypassEffects = true; // Отключаем эффекты для быстрого воспроизведения
        audioSource.bypassListenerEffects = true;
        audioSource.bypassReverbZones = true;
        
        // Автоматический поиск ClientManager если включен
        if (autoFindClientManager)
        {
            FindClientManager();
        }
        
        // Автоматический поиск ObjectDataExtractor если включен
        if (autoFindObjectDataExtractor)
        {
            FindObjectDataExtractor();
        }
        
        Debug.Log($"Camera found: {mainCamera.name}, Position: {mainCamera.transform.position}");
    }
    
    /// <summary>
    /// Автоматический поиск ClientManager на сцене
    /// </summary>
    private void FindClientManager()
    {
        // Ищем компонент ClientManager на сцене
        ClientManager foundManager = FindObjectOfType<ClientManager>();
        
        if (foundManager != null)
        {
            clientManager = foundManager;
            if (showTrackingDebugInfo)
            {
                Debug.Log($"CursorTagDetector: ClientManager автоматически найден: {foundManager.name}");
            }
        }
        else
        {
            Debug.LogWarning("CursorTagDetector: ClientManager не найден на сцене!");
        }
    }
    
    /// <summary>
    /// Автоматический поиск ObjectDataExtractor на сцене
    /// </summary>
    private void FindObjectDataExtractor()
    {
        // Ищем компонент ObjectDataExtractor на сцене
        ObjectDataExtractor foundExtractor = FindObjectOfType<ObjectDataExtractor>();
        
        if (foundExtractor != null)
        {
            objectDataExtractor = foundExtractor;
            if (showZone3RestrictionDebugInfo)
            {
                Debug.Log($"CursorTagDetector: ObjectDataExtractor автоматически найден: {foundExtractor.name}");
            }
        }
        else
        {
            Debug.LogWarning("CursorTagDetector: ObjectDataExtractor не найден на сцене!");
        }
    }
    
    /// <summary>
    /// Проверяет, соответствует ли объект данным из ObjectDataExtractor
    /// </summary>
    private bool IsObjectMatchingExtractedData(GameObject obj)
    {
        if (!useZone3ObjectRestrictions)
        {
            return true; // Если ограничения отключены, разрешаем все объекты
        }
        
        if (objectDataExtractor == null)
        {
            if (showZone3RestrictionDebugInfo)
            {
                Debug.LogWarning("CursorTagDetector: ObjectDataExtractor не найден! Объект будет заблокирован для zone 3");
            }
            return false;
        }
        
        // Получаем данные из ObjectDataExtractor
        ObjectDataExtractor.ObjectData extractedData = objectDataExtractor.GetExtractedData();
        
        // Получаем компонент RandomRarityOnSpawn у проверяемого объекта
        RandomRarityOnSpawn objRarityScript = obj.GetComponent<RandomRarityOnSpawn>();
        RandomRarityOnSpawn extractedRarityScript = extractedData.GameObject?.GetComponent<RandomRarityOnSpawn>();
        
        // Удаляем "(Clone)" из имени объекта для корректного сравнения
        string objNameClean = obj.name.Replace("(Clone)", "").Trim();
        string extractedNameClean = extractedData.Name.Replace("(Clone)", "").Trim();
        
        // Проверяем соответствие по имени объекта
        bool nameMatches = objNameClean == extractedNameClean;
        
        // Проверяем соответствие по редкости
        bool rarityMatches = true;
        if (objRarityScript != null && extractedRarityScript != null)
        {
            rarityMatches = objRarityScript.AssignedRarity == extractedRarityScript.AssignedRarity;
        }
        else if (objRarityScript != null || extractedRarityScript != null)
        {
            // Если у одного объекта есть скрипт редкости, а у другого нет - не совпадают
            rarityMatches = false;
        }
        
        // Проверяем соответствие по характеристикам
        bool statsMatch = true;
        if (objRarityScript != null && extractedRarityScript != null)
        {
            statsMatch = CompareObjectStats(objRarityScript, extractedRarityScript);
        }
        else if (objRarityScript != null || extractedRarityScript != null)
        {
            // Если у одного объекта есть характеристики, а у другого нет - не совпадают
            statsMatch = false;
        }
        
        // Проверяем режим обмана (deception)
        bool isDeceptionActive = extractedData.IsDeceptionActive;
        
        // Объект считается соответствующим в зависимости от режима
        bool overallMatch;
        if (isDeceptionActive)
        {
            // В режиме обмана проверяем ТОЛЬКО имя
            overallMatch = nameMatches;
        }
        else
        {
            // В обычном режиме проверяем ВСЕ критерии
            overallMatch = nameMatches && rarityMatches && statsMatch;
        }
        
        if (showZone3RestrictionDebugInfo)
        {
            Debug.Log($"=== ПРОВЕРКА СООТВЕТСТВИЯ ОБЪЕКТА ===");
            Debug.Log($"🎭 Режим обмана (Deception): {(isDeceptionActive ? "✅ АКТИВЕН" : "❌ НЕ АКТИВЕН")}");
            Debug.Log($"Имя объекта: '{obj.name}' → '{objNameClean}'");
            Debug.Log($"Имя эталона: '{extractedData.Name}' → '{extractedNameClean}'");
            Debug.Log($"✅ Соответствие по имени: {nameMatches}");
            
            if (isDeceptionActive)
            {
                Debug.Log($"🎭 В режиме обмана проверяется ТОЛЬКО имя объекта");
            }
            else
            {
                if (objRarityScript != null && extractedRarityScript != null)
                {
                    Debug.Log($"Редкость объекта: {objRarityScript.AssignedRarity}");
                    Debug.Log($"Редкость эталона: {extractedRarityScript.AssignedRarity}");
                    Debug.Log($"✅ Соответствие по редкости: {rarityMatches}");
                    
                    Debug.Log($"✅ Соответствие по характеристикам: {statsMatch}");
                }
                else
                {
                    Debug.Log($"⚠️ Скрипт редкости: объект={objRarityScript != null}, эталон={extractedRarityScript != null}");
                }
            }
            
            Debug.Log($"🎯 ОБЩИЙ РЕЗУЛЬТАТ: {(overallMatch ? "✅ СООТВЕТСТВУЕТ" : "❌ НЕ СООТВЕТСТВУЕТ")}");
        }
        
        return overallMatch;
    }
    
    /// <summary>
    /// Сравнивает характеристики двух объектов
    /// </summary>
    private bool CompareObjectStats(RandomRarityOnSpawn obj1, RandomRarityOnSpawn obj2)
    {
        // Создаем списки характеристик для обоих объектов
        var stats1 = GetObjectStatsList(obj1);
        var stats2 = GetObjectStatsList(obj2);
        
        // Если количество характеристик разное - не совпадают
        if (stats1.Count != stats2.Count)
        {
            return false;
        }
        
        // Сравниваем каждую характеристику
        for (int i = 0; i < stats1.Count; i++)
        {
            if (stats1[i].stat != stats2[i].stat || stats1[i].value != stats2[i].value)
            {
                return false;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Получает список характеристик объекта в виде структуры
    /// </summary>
    private System.Collections.Generic.List<(string stat, int value)> GetObjectStatsList(RandomRarityOnSpawn rarityScript)
    {
        var stats = new System.Collections.Generic.List<(string stat, int value)>();
        
        if (!string.IsNullOrEmpty(rarityScript.stat1) && rarityScript.stat1Value > 0)
            stats.Add((rarityScript.stat1, rarityScript.stat1Value));
        if (!string.IsNullOrEmpty(rarityScript.stat2) && rarityScript.stat2Value > 0)
            stats.Add((rarityScript.stat2, rarityScript.stat2Value));
        if (!string.IsNullOrEmpty(rarityScript.stat3) && rarityScript.stat3Value > 0)
            stats.Add((rarityScript.stat3, rarityScript.stat3Value));
        if (!string.IsNullOrEmpty(rarityScript.stat4) && rarityScript.stat4Value > 0)
            stats.Add((rarityScript.stat4, rarityScript.stat4Value));
        if (!string.IsNullOrEmpty(rarityScript.stat5) && rarityScript.stat5Value > 0)
            stats.Add((rarityScript.stat5, rarityScript.stat5Value));
        
        return stats;
    }
    
    /// <summary>
    private bool IsClientActive()
    {
        if (!requireClientActiveForZone3)
        {
            return true; // Если не требуется активность Client, всегда возвращаем true
        }
        
        // Проверяем, нужно ли искать ClientManager
        bool shouldSearchClientManager = false;
        
        if (clientManager == null)
        {
            float currentTime = Time.time;
            
            // Ищем ClientManager только если:
            // 1. Поиск еще не предпринимался, ИЛИ
            // 2. Прошло достаточно времени с последнего поиска
            if (!clientManagerSearchAttempted || (currentTime - lastClientManagerSearchTime) >= clientManagerSearchInterval)
            {
                shouldSearchClientManager = true;
                lastClientManagerSearchTime = currentTime;
                clientManagerSearchAttempted = true;
            }
        }
        
        // Ищем ClientManager если нужно
        if (shouldSearchClientManager)
        {
            FindClientManager();
        }
        
        if (clientManager == null)
        {
            // Не логируем предупреждение каждый кадр, только при первом поиске
            if (shouldSearchClientManager && showTrackingDebugInfo)
            {
                Debug.LogWarning("CursorTagDetector: ClientManager не найден для проверки активности Client!");
            }
            return false;
        }
        
        // Получаем текущее состояние Client
        bool currentClientState = clientManager.IsClientActive();
        
        // Обновляем кэш только если состояние изменилось
        if (!clientStateCached || cachedClientActiveState != currentClientState)
        {
            cachedClientActiveState = currentClientState;
            clientStateCached = true;
            
            if (showTrackingDebugInfo)
            {
                Debug.Log($"CursorTagDetector: Состояние Client обновлено: {cachedClientActiveState}");
            }
        }
        
        return cachedClientActiveState;
    }
    
    /// <summary>
    /// Принудительно обновляет кэш состояния Client
    /// </summary>
    public void RefreshClientStateCache()
    {
        clientStateCached = false;
        clientManagerSearchAttempted = false; // Сбрасываем кэш поиска ClientManager
        IsClientActive(); // Принудительно обновляем кэш
    }
    
    void Update()
    {
        if (mainCamera != null && mouse != null)
        {
            Vector2 mousePosition = mouse.position.ReadValue();
            
            // Для ортогональной камеры используем другой подход
            Vector3 worldPosition;
            if (mainCamera.orthographic)
            {
                // Для ортогональной камеры
                float distance = Mathf.Abs(mainCamera.transform.position.z);
                worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, distance));
            }
            else
            {
                // Для перспективной камеры
                worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 10f));
            }
            
            // Проверяем все коллайдеры в небольшом радиусе
            Collider2D[] colliders = Physics2D.OverlapCircleAll(new Vector2(worldPosition.x, worldPosition.y), 0.1f);
            
            if (colliders.Length > 0)
            {
                currentTag = colliders[0].tag;
                Debug.Log($"Found collider: {colliders[0].name}, Tag: {colliders[0].tag}");
            }
            else
            {
                currentTag = "No Hit";
                // Показываем отладку только раз в секунду
                if (Time.time % 1f < 0.1f)
                {
                    Debug.Log($"No hit. Mouse: {mousePosition}, World: {worldPosition}, Camera ortho: {mainCamera.orthographic}");
                }
            }
            
            // Обработка перетаскивания
            HandleDragging(worldPosition);
        }
        else
        {
            currentTag = "No Camera/Mouse";
        }
    }
    
    void HandleDragging(Vector3 worldPosition)
    {
        // Проверяем нажатие левой кнопки мыши
        if (mouse.leftButton.wasPressedThisFrame)
        {
            // Проверяем коллайдеры СРАЗУ при нажатии (не полагаемся на currentTag)
            Collider2D[] colliders = Physics2D.OverlapCircleAll(new Vector2(worldPosition.x, worldPosition.y), 0.1f);
            if (colliders.Length > 0 && colliders[0].tag == draggableTag)
            {
                // Воспроизводим звук поднятия СРАЗУ при подборе объекта
                PlayPickupSound();
                
                isDragging = true;
                draggedObject = colliders[0].transform;
                // Запоминаем исходную позицию и масштаб объекта
                originalPosition = draggedObject.position;
                originalScale = draggedObject.localScale;
                
                // Проверяем, был ли объект в drop zone 2 или 3 перед взятием
                bool wasInDropZone2 = IsPositionInDropZone2(draggedObject.position);
                bool wasInDropZone3 = IsPositionInDropZone3(draggedObject.position);
                isInDropZone2 = false; // Сбрасываем флаг, так как объект взят
                isInDropZone3 = false; // Сбрасываем флаг, так как объект взят
                
                // Вычисляем истинно исходный масштаб
                if (wasInDropZone2 && useDropZone2ScaleEffect)
                {
                    // Если объект был в drop zone 2, восстанавливаем истинно исходный масштаб
                    trueOriginalScale = draggedObject.localScale / dropZone2ScaleMultiplier;
                    Debug.Log($"Объект был в drop zone 2, истинно исходный масштаб: {draggedObject.localScale} / {dropZone2ScaleMultiplier} = {trueOriginalScale}");
                }
                else if (wasInDropZone3 && useDropZone3ScaleEffect)
                {
                    // Если объект был в drop zone 3, восстанавливаем истинно исходный масштаб
                    trueOriginalScale = draggedObject.localScale / dropZone3ScaleMultiplier;
                    Debug.Log($"Объект был в drop zone 3, истинно исходный масштаб: {draggedObject.localScale} / {dropZone3ScaleMultiplier} = {trueOriginalScale}");
                }
                else
                {
                    // Если объект не был в специальных зонах, текущий масштаб и есть исходный
                    trueOriginalScale = draggedObject.localScale;
                    Debug.Log($"Объект не был в специальных зонах, истинно исходный масштаб: {trueOriginalScale}");
                }
                
                // Уведомляем PrefabSpawner о том, что объект был забран и начал перетаскивание
                if (prefabSpawner != null)
                {
                    prefabSpawner.MarkObjectAsDragging(draggedObject.gameObject);
                    Debug.Log($"Объект {draggedObject.name} взят для перетаскивания");
                }
                
                // Устанавливаем масштаб при взятии объекта
                if (useScaleEffect)
                {
                    // Всегда используем истинно исходный масштаб для эффекта перетаскивания
                    draggedObject.localScale = trueOriginalScale * scaleMultiplier;
                    
                    if (wasInDropZone2 && useDropZone2ScaleEffect)
                    {
                        Debug.Log($"Объект {draggedObject.name} взят из drop zone 2, масштаб восстановлен к обычному перетаскиванию: {trueOriginalScale} * {scaleMultiplier} = {draggedObject.localScale}");
                    }
                    else
                    {
                        Debug.Log($"Объект {draggedObject.name} взят, применен эффект перетаскивания: {trueOriginalScale} * {scaleMultiplier} = {draggedObject.localScale}");
                    }
                }
                
                Debug.Log($"Started dragging: {draggedObject.name} from {originalPosition}");
            }
        }
        
        // Если кнопка мыши отпущена
        if (mouse.leftButton.wasReleasedThisFrame)
        {
            if (isDragging && draggedObject != null)
            {
                // Воспроизводим звук опускания СРАЗУ при отпускании перетаскиваемого объекта
                PlayDropSound();
                
                // Восстанавливаем исходный масштаб
                if (useScaleEffect)
                {
                    draggedObject.localScale = originalScale;
                }
                
                // Проверяем, можно ли отпустить объект в этой позиции
                if (CanDropAtPosition(worldPosition, draggedObject.gameObject))
                {
                    // Объект остается в новой позиции
                    draggedObject.position = worldPosition;
                    
                    // Проверяем, находится ли объект в drop zone 2 или 3 и изменяем масштаб
                    if (useDropZone2ScaleEffect && IsPositionInDropZone2(worldPosition))
                    {
                        draggedObject.localScale = trueOriginalScale * dropZone2ScaleMultiplier;
                        isInDropZone2 = true;
                        isInDropZone3 = false;
                        Debug.Log($"Объект {draggedObject.name} помещен в drop zone 2, масштаб увеличен: {trueOriginalScale} * {dropZone2ScaleMultiplier} = {draggedObject.localScale}");
                    }
                    else if (IsPositionInDropZone3(worldPosition))
                    {
                        // Проверяем, активен ли Client для активации zone 3
                        if (!IsClientActive())
                        {
                            if (showTrackingDebugInfo)
                            {
                                Debug.Log($"Zone 3 неактивна - Client не активен. Объект {draggedObject.name} возвращен на исходную позицию.");
                            }
                            
                            // Возвращаем объект на исходную позицию
                            draggedObject.position = originalPosition;
                            draggedObject.localScale = originalScale;
                            
                            // Сбрасываем переменные перетаскивания
                            isDragging = false;
                            draggedObject = null;
                            isInDropZone2 = false;
                            isInDropZone3 = false;
                            
                            return;
                        }
                        
                        // Проверяем, нужно ли удалить объект в третьей зоне
                        if (useDropZone3Destroy)
                        {
                            // Проверяем соответствие объекта с данными из ObjectDataExtractor
                            if (useZone3ObjectRestrictions && !IsObjectMatchingExtractedData(draggedObject.gameObject))
                            {
                                if (showZone3RestrictionDebugInfo)
                                {
                                    Debug.Log($"Zone 3: Объект '{draggedObject.name}' не соответствует extracted data - удаление запрещено");
                                }
                                
                                // Воспроизводим партиклы блокировки неправильного объекта
                                PlayZone3BlockedParticles(worldPosition);
                                
                                // Возвращаем объект на исходную позицию
                                draggedObject.position = originalPosition;
                                draggedObject.localScale = originalScale;
                                
                                // Сбрасываем переменные перетаскивания
                                isDragging = false;
                                draggedObject = null;
                                isInDropZone2 = false;
                                isInDropZone3 = false;
                                
                                return;
                            }
                            
                            // Записываем информацию об объекте перед удалением
                            if (enableZone3Tracking)
                            {
                                RecordDestroyedObject(draggedObject.gameObject, worldPosition, "Zone 3 Destroy");
                            }
                            
                            // Уведомляем PrefabSpawner об удалении объекта
                            if (prefabSpawner != null)
                            {
                                prefabSpawner.MarkObjectAsDestroyed(draggedObject.gameObject);
                                Debug.Log($"Объект {draggedObject.name} помещен в drop zone 3 для удаления");
                            }
                            
                            // Запускаем корутину удаления с задержкой
                            StartCoroutine(DestroyObjectWithDelay(draggedObject.gameObject));
                            
                            // Воспроизводим партиклы успешного удаления правильного объекта
                            PlayZone3SuccessParticles(worldPosition);
                            
                            // Сохраняем имя объекта для лога
                            string objectName = draggedObject.name;
                            
                            // Сбрасываем переменные перетаскивания
                            isDragging = false;
                            draggedObject = null;
                            isInDropZone2 = false;
                            isInDropZone3 = false;
                            
                            Debug.Log($"Объект {objectName} будет удален через {destroyDelay} секунд");
                        }
                        else if (useDropZone3ScaleEffect)
                        {
                            // Если удаление отключено, применяем только масштабирование
                            draggedObject.localScale = trueOriginalScale * dropZone3ScaleMultiplier;
                            isInDropZone2 = false;
                            isInDropZone3 = true;
                            Debug.Log($"Объект {draggedObject.name} помещен в drop zone 3, масштаб увеличен: {trueOriginalScale} * {dropZone3ScaleMultiplier} = {draggedObject.localScale}");
                        }
                    }
                    else
                    {
                        // Если не в специальных зонах, восстанавливаем исходный масштаб
                        draggedObject.localScale = trueOriginalScale;
                        isInDropZone2 = false;
                        isInDropZone3 = false;
                        Debug.Log($"Объект {draggedObject.name} помещен в drop zone 1, масштаб восстановлен к исходному: {trueOriginalScale}");
                    }
                    
                    // Воспроизводим частицы при успешном отпускании
                    PlayDropParticles(worldPosition);
                    
                    // Уведомляем PrefabSpawner о том, что объект помещен в drop zone
                    if (prefabSpawner != null && draggedObject != null)
                    {
                        prefabSpawner.MarkObjectAsInDropZone(draggedObject.gameObject);
                        Debug.Log($"Объект {draggedObject.name} помещен в drop zone");
                    }
                    
                    if (draggedObject != null)
                    {
                        Debug.Log($"Dropped: {draggedObject.name} at {worldPosition}");
                    }
                }
                else
                {
                    if (draggedObject != null)
                    {
                        // Проверяем, был ли объект заблокирован из-за ограничений zone 3
                        bool wasBlockedByZone3Restrictions = false;
                        if (useDropZone && IsPositionInZone(worldPosition, zone3Center, zone3Size))
                        {
                            if (IsClientActive() && useZone3ObjectRestrictions && !IsObjectMatchingExtractedData(draggedObject.gameObject))
                            {
                                wasBlockedByZone3Restrictions = true;
                            }
                        }
                        
                        // Возвращаем объект на исходную позицию
                        draggedObject.position = originalPosition;
                        
                        // Восстанавливаем исходный масштаб при возврате
                        if (useDropZone2ScaleEffect || useDropZone3ScaleEffect)
                        {
                            draggedObject.localScale = trueOriginalScale;
                            isInDropZone2 = false;
                            isInDropZone3 = false;
                            Debug.Log($"Объект {draggedObject.name} возвращен, масштаб восстановлен к исходному: {trueOriginalScale}");
                        }
                        
                        // Воспроизводим соответствующие партиклы
                        if (wasBlockedByZone3Restrictions)
                        {
                            PlayZone3BlockedParticles(worldPosition);
                        }
                        else
                        {
                            PlayDropParticles(draggedObject.position);
                        }
                        
                        // Уведомляем PrefabSpawner о том, что объект вне drop zone
                        if (prefabSpawner != null)
                        {
                            prefabSpawner.MarkObjectAsOutOfDropZone(draggedObject.gameObject);
                        }
                        
                        Debug.Log($"Returned: {draggedObject.name} to spawn point (outside drop zone)");
                    }
                }
                
                // Уведомляем PrefabSpawner о завершении перетаскивания
                if (prefabSpawner != null && draggedObject != null)
                {
                    prefabSpawner.MarkObjectAsDropped(draggedObject.gameObject);
                    Debug.Log($"Перетаскивание объекта {draggedObject.name} завершено");
                }
                
                // Сбрасываем флаги нахождения в специальных зонах
                isInDropZone2 = false;
                isInDropZone3 = false;
                isDragging = false;
                draggedObject = null;
            }
            else if (isDragging && draggedObject == null)
            {
                // Объект был уничтожен во время перетаскивания
                Debug.LogWarning("Перетаскиваемый объект был уничтожен во время перетаскивания");
                isDragging = false;
            }
        }
        
        // Если перетаскиваем объект - можно двигать везде
        if (isDragging && draggedObject != null)
        {
            // Обновляем позицию объекта - центр объекта следует за курсором
            draggedObject.position = worldPosition;
        }
        else if (isDragging && draggedObject == null)
        {
            // Объект был удален, прекращаем перетаскивание
            Debug.LogWarning("Перетаскиваемый объект был уничтожен во время перетаскивания");
            isDragging = false;
        }
    }
    
    // Проверяет, можно ли отпустить объект в данной позиции
    public bool CanDropAtPosition(Vector3 position, GameObject obj = null)
    {
        if (!useDropZone)
            return true; // Если зона отключена, можно отпускать везде
        
        // Проверяем первую дроп зону
        if (IsPositionInZone(position, zone1Center, zone1Size))
        {
            Debug.Log("Объект помещен в первую дроп зону");
            return true;
        }
        
        // Проверяем вторую дроп зону
        if (IsPositionInZone(position, zone2Center, zone2Size))
        {
            Debug.Log("Объект помещен во вторую дроп зону");
            return true;
        }
        
        // Проверяем третью дроп зону
        if (IsPositionInZone(position, zone3Center, zone3Size))
        {
            // Проверяем, активен ли Client для zone 3
            if (!IsClientActive())
            {
                if (showTrackingDebugInfo)
                {
                    Debug.Log("Zone 3 неактивна - Client не активен");
                }
                return false;
            }
            
            // Проверяем соответствие объекта с данными из ObjectDataExtractor
            if (obj != null && !IsObjectMatchingExtractedData(obj))
            {
                if (showZone3RestrictionDebugInfo)
                {
                    Debug.Log($"Zone 3: Объект '{obj.name}' не соответствует extracted data - доступ запрещен");
                }
                return false;
            }
            
            Debug.Log("Объект помещен в третью дроп зону");
            return true;
        }
        
        return false; // Позиция не входит ни в одну из дроп зон
    }
    
    // Проверяет, находится ли позиция в указанной зоне
    private bool IsPositionInZone(Vector3 position, Vector2 zoneCenter, Vector2 zoneSize)
    {
        float minX = zoneCenter.x - zoneSize.x / 2f;
        float maxX = zoneCenter.x + zoneSize.x / 2f;
        float minY = zoneCenter.y - zoneSize.y / 2f;
        float maxY = zoneCenter.y + zoneSize.y / 2f;
        
        return position.x >= minX && position.x <= maxX && 
               position.y >= minY && position.y <= maxY;
    }
    
    // Проверяет, находится ли позиция в drop zone 2
    public bool IsPositionInDropZone2(Vector3 position)
    {
        return IsPositionInZone(position, zone2Center, zone2Size);
    }
    
    // Проверяет, находится ли позиция в drop zone 3
    public bool IsPositionInDropZone3(Vector3 position)
    {
        // Проверяем, активен ли Client для zone 3
        if (!IsClientActive())
        {
            return false;
        }
        
        return IsPositionInZone(position, zone3Center, zone3Size);
    }
    
    
    
    // Воспроизводит звук поднятия объекта
    void PlayPickupSound()
    {
        if (useSoundEffects && pickupSound != null && audioSource != null)
        {
            // Останавливаем текущий звук
            audioSource.Stop();
            // Устанавливаем звук и воспроизводим напрямую (быстрее чем PlayOneShot)
            audioSource.clip = pickupSound;
            audioSource.volume = soundVolume;
            audioSource.Play();
        }
    }
    
    // Воспроизводит звук опускания объекта
    void PlayDropSound()
    {
        if (useSoundEffects && dropSound != null && audioSource != null)
        {
            // Останавливаем текущий звук
            audioSource.Stop();
            // Устанавливаем звук и воспроизводим напрямую (быстрее чем PlayOneShot)
            audioSource.clip = dropSound;
            audioSource.volume = soundVolume;
            audioSource.Play();
        }
    }
    
    // Воспроизводит звук удаления объекта
    void PlayDestroySound()
    {
        if (useSoundEffects && destroySound != null && audioSource != null)
        {
            // Останавливаем текущий звук
            audioSource.Stop();
            // Устанавливаем звук и воспроизводим напрямую (быстрее чем PlayOneShot)
            audioSource.clip = destroySound;
            audioSource.volume = soundVolume;
            audioSource.Play();
        }
    }
    
    // Воспроизводит частицы при отпускании объекта
    void PlayDropParticles(Vector3 position)
    {
        if (useParticleEffects && dropParticlePrefab != null)
        {
            // Создаем экземпляр префаба частиц в позиции отпускания
            GameObject particleInstance = Instantiate(dropParticlePrefab, position, Quaternion.identity);
            
            // Принудительно запускаем частицы
            ParticleSystem[] particleSystems = particleInstance.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in particleSystems)
            {
                ps.Play();
            }
            
            // Уничтожаем частицы через заданное время
            StartCoroutine(DestroyParticlesAfterDelay(particleInstance));
        }
    }
    
    // Воспроизводит партиклы при успешном удалении правильного объекта в zone 3
    void PlayZone3SuccessParticles(Vector3 position)
    {
        if (useZone3ParticleEffects && zone3SuccessParticlePrefab != null)
        {
            // Создаем экземпляр префаба партиклов в позиции отпускания
            GameObject particleInstance = Instantiate(zone3SuccessParticlePrefab, position, Quaternion.identity);
            
            // Принудительно запускаем партиклы
            ParticleSystem[] particleSystems = particleInstance.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in particleSystems)
            {
                ps.Play();
            }
            
            // Уничтожаем партиклы через заданное время
            StartCoroutine(DestroyParticlesAfterDelay(particleInstance));
            
            if (showZone3RestrictionDebugInfo)
            {
                Debug.Log($"Zone 3: Воспроизведены партиклы успешного удаления в позиции {position}");
            }
        }
    }
    
    // Воспроизводит партиклы при блокировке неправильного объекта в zone 3
    void PlayZone3BlockedParticles(Vector3 position)
    {
        if (useZone3ParticleEffects && zone3BlockedParticlePrefab != null)
        {
            // Создаем экземпляр префаба партиклов в позиции отпускания
            GameObject particleInstance = Instantiate(zone3BlockedParticlePrefab, position, Quaternion.identity);
            
            // Принудительно запускаем партиклы
            ParticleSystem[] particleSystems = particleInstance.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in particleSystems)
            {
                ps.Play();
            }
            
            // Уничтожаем партиклы через заданное время
            StartCoroutine(DestroyParticlesAfterDelay(particleInstance));
            
            if (showZone3RestrictionDebugInfo)
            {
                Debug.Log($"Zone 3: Воспроизведены партиклы блокировки в позиции {position}");
            }
        }
    }
    
    // Воспроизводит эффекты при удалении объекта
    void PlayDestroyEffects(Vector3 position)
    {
        // Воспроизводим звук удаления
        PlayDestroySound();
        
        // Воспроизводим частицы удаления
        if (useParticleEffects && destroyParticlePrefab != null)
        {
            // Создаем экземпляр префаба частиц в позиции удаления
            GameObject particleInstance = Instantiate(destroyParticlePrefab, position, Quaternion.identity);
            
            // Принудительно запускаем частицы
            ParticleSystem[] particleSystems = particleInstance.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in particleSystems)
            {
                ps.Play();
            }
            
            // Уничтожаем частицы через заданное время
            StartCoroutine(DestroyParticlesAfterDelay(particleInstance));
        }
    }
    
    // Уничтожает частицы через заданное время
    System.Collections.IEnumerator DestroyParticlesAfterDelay(GameObject particleInstance)
    {
        // Ждем заданное время
        yield return new WaitForSeconds(particleDuration);
        
        if (particleInstance != null)
        {
            // Останавливаем все частицы перед удалением
            ParticleSystem[] particleSystems = particleInstance.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in particleSystems)
            {
                ps.Stop();
            }
            
            // Уничтожаем объект
            Destroy(particleInstance);
        }
    }
    
    // Уничтожает объект с задержкой
    System.Collections.IEnumerator DestroyObjectWithDelay(GameObject objectToDestroy)
    {
        // Ждем заданное время
        yield return new WaitForSeconds(destroyDelay);
        
        if (objectToDestroy != null)
        {
            // Воспроизводим эффекты удаления
            PlayDestroyEffects(objectToDestroy.transform.position);
            
            // Уничтожаем объект
            Destroy(objectToDestroy);
            Debug.Log($"Объект {objectToDestroy.name} удален из сцены");
        }
    }
    
    
    // Визуализация зон в Scene View (только в редакторе)
    void OnDrawGizmos()
    {
        if (useDropZone)
        {
            // Рисуем первую дроп зону
            Gizmos.color = zone1Color;
            Gizmos.DrawWireCube(new Vector3(zone1Center.x, zone1Center.y, 0), new Vector3(zone1Size.x, zone1Size.y, 0));
            
            // Показываем центр первой зоны
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(new Vector3(zone1Center.x, zone1Center.y, 0), 0.2f);
            
            // Рисуем вторую дроп зону
            Gizmos.color = zone2Color;
            Gizmos.DrawWireCube(new Vector3(zone2Center.x, zone2Center.y, 0), new Vector3(zone2Size.x, zone2Size.y, 0));
            
            // Показываем центр второй зоны
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(new Vector3(zone2Center.x, zone2Center.y, 0), 0.2f);
            
            // Рисуем третью дроп зону
            Gizmos.color = zone3Color;
            Gizmos.DrawWireCube(new Vector3(zone3Center.x, zone3Center.y, 0), new Vector3(zone3Size.x, zone3Size.y, 0));
            
            // Показываем центр третьей зоны
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(new Vector3(zone3Center.x, zone3Center.y, 0), 0.2f);
            
            // Показываем статус активности zone 3
            if (requireClientActiveForZone3)
            {
                bool isClientActive = IsClientActive();
                Gizmos.color = isClientActive ? Color.green : Color.red;
                Gizmos.DrawWireSphere(new Vector3(zone3Center.x, zone3Center.y + 1f, 0), 0.1f);
                
                #if UNITY_EDITOR
                UnityEditor.Handles.Label(new Vector3(zone3Center.x, zone3Center.y + 1.5f, 0), 
                    isClientActive ? "Zone 3: ACTIVE" : "Zone 3: INACTIVE (Client not active)");
                #endif
            }
        }
        
    }
    
    // ========== КОНТЕКСТНЫЕ МЕНЮ ДЛЯ ТЕСТИРОВАНИЯ ZONE 3 CLIENT INTEGRATION ==========
    
    [ContextMenu("Проверить статус Client для Zone 3")]
    private void TestClientStatusForZone3()
    {
        bool isClientActive = IsClientActive();
        Debug.Log($"=== СТАТУС CLIENT ДЛЯ ZONE 3 ===");
        Debug.Log($"Client активен: {isClientActive}");
        Debug.Log($"Требуется активность Client для Zone 3: {requireClientActiveForZone3}");
        Debug.Log($"ClientManager найден: {(clientManager != null ? "Да" : "Нет")}");
        
        if (clientManager != null)
        {
            Debug.Log($"ClientManager: {clientManager.name}");
            Debug.Log($"Client объект активен: {clientManager.IsClientActive()}");
        }
        
        Debug.Log($"Zone 3 будет активна: {isClientActive || !requireClientActiveForZone3}");
    }
    
    [ContextMenu("Принудительно включить Client")]
    private void TestForceActivateClient()
    {
        if (clientManager != null)
        {
            clientManager.ForceActivateClient();
            Debug.Log("Client принудительно включен!");
        }
        else
        {
            Debug.LogError("ClientManager не найден!");
        }
    }
    
    [ContextMenu("Принудительно выключить Client")]
    private void TestForceDeactivateClient()
    {
        if (clientManager != null)
        {
            clientManager.ForceDeactivateClient();
            Debug.Log("Client принудительно выключен!");
        }
        else
        {
            Debug.LogError("ClientManager не найден!");
        }
    }
    
    [ContextMenu("Переключить требование активности Client для Zone 3")]
    private void TestToggleRequireClientActive()
    {
        requireClientActiveForZone3 = !requireClientActiveForZone3;
        Debug.Log($"Требование активности Client для Zone 3: {(requireClientActiveForZone3 ? "Включено" : "Выключено")}");
    }
    
    [ContextMenu("Обновить кэш состояния Client")]
    private void TestRefreshClientCache()
    {
        RefreshClientStateCache();
        Debug.Log($"Кэш состояния Client обновлен. Текущее состояние: {cachedClientActiveState}");
    }
    
    [ContextMenu("Показать информацию о кэшировании")]
    private void TestShowCacheInfo()
    {
        Debug.Log($"=== ИНФОРМАЦИЯ О КЭШИРОВАНИИ CLIENT ===");
        Debug.Log($"Кэш инициализирован: {clientStateCached}");
        Debug.Log($"Кэшированное состояние Client: {cachedClientActiveState}");
        Debug.Log($"ClientManager найден: {(clientManager != null ? "Да" : "Нет")}");
        Debug.Log($"Поиск ClientManager предпринимался: {clientManagerSearchAttempted}");
        Debug.Log($"Время последнего поиска: {lastClientManagerSearchTime:F2}");
        Debug.Log($"Интервал поиска: {clientManagerSearchInterval} секунд");
        
        if (clientManager != null)
        {
            bool realClientState = clientManager.IsClientActive();
            Debug.Log($"Реальное состояние Client: {realClientState}");
            Debug.Log($"Кэш синхронизирован: {cachedClientActiveState == realClientState}");
        }
    }
    
    [ContextMenu("Сбросить кэш поиска ClientManager")]
    private void TestResetClientManagerCache()
    {
        clientManagerSearchAttempted = false;
        lastClientManagerSearchTime = 0f;
        Debug.Log("Кэш поиска ClientManager сброшен!");
    }
    
    [ContextMenu("Установить интервал поиска ClientManager (1 сек)")]
    private void TestSetSearchInterval1Sec()
    {
        clientManagerSearchInterval = 1f;
        Debug.Log($"Интервал поиска ClientManager установлен: {clientManagerSearchInterval} секунд");
    }
    
    [ContextMenu("Установить интервал поиска ClientManager (10 сек)")]
    private void TestSetSearchInterval10Sec()
    {
        clientManagerSearchInterval = 10f;
        Debug.Log($"Интервал поиска ClientManager установлен: {clientManagerSearchInterval} секунд");
    }
    
    // ========== СИСТЕМА ОТСЛЕЖИВАНИЯ УДАЛЕННЫХ ОБЪЕКТОВ В ZONE 3 ==========
    
    /// <summary>
    /// Записывает информацию об удаленном объекте
    /// </summary>
    private void RecordDestroyedObject(GameObject obj, Vector3 destroyPosition, string reason)
    {
        if (obj == null) return;
        
        // Получаем информацию об объекте
        string objectName = obj.name;
        string objectTag = obj.tag;
        
        // Проверяем наличие скрипта RandomRarityOnSpawn
        bool hadRandomRarityScript = false;
        string rarity = "";
        int goldAmount = 0;
        
        RandomRarityOnSpawn rarityScript = obj.GetComponent<RandomRarityOnSpawn>();
        if (rarityScript != null)
        {
            hadRandomRarityScript = true;
            rarity = rarityScript.AssignedRarity.ToString();
            goldAmount = rarityScript.gold; // Извлекаем количество золота
        }
        
        // Создаем запись об удаленном объекте
        DestroyedObjectInfo destroyedInfo = new DestroyedObjectInfo(
            objectName,
            objectTag,
            destroyPosition,
            Time.time,
            reason,
            hadRandomRarityScript,
            rarity,
            goldAmount
        );
        
        // Добавляем в список
        destroyedObjects.Add(destroyedInfo);
        totalDestroyedCount++;
        
        if (showTrackingDebugInfo)
        {
            Debug.Log($"📊 Zone 3 Tracking: Записан удаленный объект '{objectName}' (тег: {objectTag}, редкость: {rarity}, золото: {goldAmount}, позиция: {destroyPosition})");
            Debug.Log($"📊 Zone 3 Tracking: Всего удалено объектов: {totalDestroyedCount}");
        }
    }
    
    /// <summary>
    /// Получить общее количество удаленных объектов в zone 3
    /// </summary>
    public int GetTotalDestroyedCount()
    {
        return totalDestroyedCount;
    }
    
    /// <summary>
    /// Получить количество удаленных объектов за последние N секунд
    /// </summary>
    public int GetDestroyedCountInLastSeconds(float seconds)
    {
        float currentTime = Time.time;
        int count = 0;
        
        foreach (DestroyedObjectInfo info in destroyedObjects)
        {
            if (currentTime - info.destroyTime <= seconds)
            {
                count++;
            }
        }
        
        return count;
    }
    
    /// <summary>
    /// Получить список всех удаленных объектов
    /// </summary>
    public System.Collections.Generic.List<DestroyedObjectInfo> GetAllDestroyedObjects()
    {
        return new System.Collections.Generic.List<DestroyedObjectInfo>(destroyedObjects);
    }
    
    /// <summary>
    /// Получить список удаленных объектов за последние N секунд
    /// </summary>
    public System.Collections.Generic.List<DestroyedObjectInfo> GetDestroyedObjectsInLastSeconds(float seconds)
    {
        float currentTime = Time.time;
        System.Collections.Generic.List<DestroyedObjectInfo> recentObjects = new System.Collections.Generic.List<DestroyedObjectInfo>();
        
        foreach (DestroyedObjectInfo info in destroyedObjects)
        {
            if (currentTime - info.destroyTime <= seconds)
            {
                recentObjects.Add(info);
            }
        }
        
        return recentObjects;
    }
    
    /// <summary>
    /// Получить статистику по типам объектов
    /// </summary>
    public System.Collections.Generic.Dictionary<string, int> GetDestroyedObjectsByType()
    {
        System.Collections.Generic.Dictionary<string, int> typeStats = new System.Collections.Generic.Dictionary<string, int>();
        
        foreach (DestroyedObjectInfo info in destroyedObjects)
        {
            if (typeStats.ContainsKey(info.objectName))
            {
                typeStats[info.objectName]++;
            }
            else
            {
                typeStats[info.objectName] = 1;
            }
        }
        
        return typeStats;
    }
    
    /// <summary>
    /// Получить статистику по редкости объектов
    /// </summary>
    public System.Collections.Generic.Dictionary<string, int> GetDestroyedObjectsByRarity()
    {
        System.Collections.Generic.Dictionary<string, int> rarityStats = new System.Collections.Generic.Dictionary<string, int>();
        
        foreach (DestroyedObjectInfo info in destroyedObjects)
        {
            string rarityKey = string.IsNullOrEmpty(info.rarity) ? "Без редкости" : info.rarity;
            
            if (rarityStats.ContainsKey(rarityKey))
            {
                rarityStats[rarityKey]++;
            }
            else
            {
                rarityStats[rarityKey] = 1;
            }
        }
        
        return rarityStats;
    }
    
    /// <summary>
    /// Очистить историю удаленных объектов
    /// </summary>
    public void ClearDestroyedObjectsHistory()
    {
        destroyedObjects.Clear();
        totalDestroyedCount = 0;
        
        if (showTrackingDebugInfo)
        {
            Debug.Log("📊 Zone 3 Tracking: История удаленных объектов очищена");
        }
    }
    
    /// <summary>
    /// Получить среднее время между удалениями объектов
    /// </summary>
    public float GetAverageTimeBetweenDestructions()
    {
        if (destroyedObjects.Count < 2)
        {
            return 0f;
        }
        
        float totalTime = 0f;
        for (int i = 1; i < destroyedObjects.Count; i++)
        {
            totalTime += destroyedObjects[i].destroyTime - destroyedObjects[i - 1].destroyTime;
        }
        
        return totalTime / (destroyedObjects.Count - 1);
    }
    
    /// <summary>
    /// Показать полную статистику удаленных объектов
    /// </summary>
    public void ShowDestroyedObjectsStats()
    {
        Debug.Log("📊 ========== СТАТИСТИКА УДАЛЕННЫХ ОБЪЕКТОВ В ZONE 3 ==========");
        Debug.Log($"📊 Общее количество удаленных объектов: {totalDestroyedCount}");
        Debug.Log($"📊 Удалено за последние 10 секунд: {GetDestroyedCountInLastSeconds(10f)}");
        Debug.Log($"📊 Удалено за последние 60 секунд: {GetDestroyedCountInLastSeconds(60f)}");
        
        if (totalDestroyedCount > 0)
        {
            Debug.Log($"📊 Среднее время между удалениями: {GetAverageTimeBetweenDestructions():F2} секунд");
            
            // Статистика по типам
            var typeStats = GetDestroyedObjectsByType();
            Debug.Log("📊 Статистика по типам объектов:");
            foreach (var kvp in typeStats)
            {
                Debug.Log($"📊   - {kvp.Key}: {kvp.Value} раз");
            }
            
            // Статистика по редкости
            var rarityStats = GetDestroyedObjectsByRarity();
            Debug.Log("📊 Статистика по редкости:");
            foreach (var kvp in rarityStats)
            {
                Debug.Log($"📊   - {kvp.Key}: {kvp.Value} раз");
            }
        }
        
        Debug.Log("📊 ==========================================================");
    }
    
    // Контекстные меню для тестирования отслеживания
    [ContextMenu("Показать статистику удаленных объектов")]
    private void TestShowStats()
    {
        ShowDestroyedObjectsStats();
    }
    
    [ContextMenu("Очистить историю удаленных объектов")]
    private void TestClearHistory()
    {
        ClearDestroyedObjectsHistory();
    }
    
    [ContextMenu("Показать последние 5 удаленных объектов")]
    private void TestShowRecent()
    {
        var recentObjects = GetDestroyedObjectsInLastSeconds(300f); // За последние 5 минут
        Debug.Log($"📊 Последние {Mathf.Min(5, recentObjects.Count)} удаленных объектов:");
        
        for (int i = Mathf.Max(0, recentObjects.Count - 5); i < recentObjects.Count; i++)
        {
            var info = recentObjects[i];
            Debug.Log($"📊   {i + 1}. {info.objectName} (тег: {info.objectTag}, редкость: {info.rarity}, время: {info.destroyTime:F2})");
        }
    }
    
    // Структура для хранения информации об удаленных объектах
    [System.Serializable]
    public struct DestroyedObjectInfo
    {
        public string objectName; // Имя удаленного объекта
        public string objectTag; // Тег удаленного объекта
        public Vector3 destroyPosition; // Позиция где был удален объект
        public float destroyTime; // Время удаления (Time.time)
        public string destroyReason; // Причина удаления
        public bool hadRandomRarityScript; // Был ли у объекта скрипт RandomRarityOnSpawn
        public string rarity; // Редкость объекта (если была)
        public int gold; // Количество золота объекта
        
        public DestroyedObjectInfo(string name, string tag, Vector3 position, float time, string reason, bool hadRarityScript = false, string rarityType = "", int goldAmount = 0)
        {
            this.objectName = name;
            this.objectTag = tag;
            this.destroyPosition = position;
            this.destroyTime = time;
            this.destroyReason = reason;
            this.hadRandomRarityScript = hadRarityScript;
            this.rarity = rarityType;
            this.gold = goldAmount;
        }
    }
    
    [ContextMenu("Принудительно найти ObjectDataExtractor")]
    private void TestFindObjectDataExtractor()
    {
        FindObjectDataExtractor();
        if (objectDataExtractor != null)
        {
            Debug.Log($"✅ ObjectDataExtractor найден: {objectDataExtractor.name}");
        }
        else
        {
            Debug.LogWarning("❌ ObjectDataExtractor не найден на сцене!");
        }
    }
    
    [ContextMenu("Показать текущие extracted data")]
    private void TestShowExtractedData()
    {
        if (objectDataExtractor != null)
        {
            ObjectDataExtractor.ObjectData data = objectDataExtractor.GetExtractedData();
            Debug.Log($"📋 Extracted Data: Name='{data.Name}', Stat1='{data.Stat1Combined}', Stat2='{data.Stat2Combined}'");
        }
        else
        {
            Debug.LogWarning("❌ ObjectDataExtractor не найден!");
        }
    }
    
    [ContextMenu("Тест соответствия объекта")]
    private void TestObjectMatching()
    {
        if (draggedObject != null)
        {
            bool matches = IsObjectMatchingExtractedData(draggedObject.gameObject);
            Debug.Log($"🔍 Тест соответствия объекта '{draggedObject.name}': {(matches ? "✅ СООТВЕТСТВУЕТ" : "❌ НЕ СООТВЕТСТВУЕТ")} extracted data");
        }
        else
        {
            Debug.LogWarning("❌ Нет перетаскиваемого объекта для тестирования!");
        }
    }
    
    [ContextMenu("Тест партиклов Zone 3 - Успех")]
    private void TestZone3SuccessParticles()
    {
        Vector3 testPosition = transform.position;
        PlayZone3SuccessParticles(testPosition);
        Debug.Log($"🎉 Тест партиклов успешного удаления в позиции {testPosition}");
    }
    
    [ContextMenu("Тест партиклов Zone 3 - Блокировка")]
    private void TestZone3BlockedParticles()
    {
        Vector3 testPosition = transform.position;
        PlayZone3BlockedParticles(testPosition);
        Debug.Log($"🚫 Тест партиклов блокировки в позиции {testPosition}");
    }
    
    [ContextMenu("Тест расширенной проверки соответствия")]
    private void TestExtendedObjectMatching()
    {
        if (draggedObject != null)
        {
            bool matches = IsObjectMatchingExtractedData(draggedObject.gameObject);
            Debug.Log($"🔍 Расширенный тест соответствия объекта '{draggedObject.name}': {(matches ? "✅ СООТВЕТСТВУЕТ" : "❌ НЕ СООТВЕТСТВУЕТ")}");
            
            // Показываем детали проверки
            RandomRarityOnSpawn objScript = draggedObject.GetComponent<RandomRarityOnSpawn>();
            if (objScript != null)
            {
                Debug.Log($"📊 Детали объекта '{draggedObject.name}':");
                Debug.Log($"   Редкость: {objScript.AssignedRarity}");
                Debug.Log($"   Характеристики: {GetObjectStatsList(objScript).Count} шт.");
                var stats = GetObjectStatsList(objScript);
                for (int i = 0; i < stats.Count; i++)
                {
                    Debug.Log($"     {i + 1}. {stats[i].stat} +{stats[i].value}");
                }
            }
        }
        else
        {
            Debug.LogWarning("❌ Нет перетаскиваемого объекта для тестирования!");
        }
    }
    
    [ContextMenu("Показать эталонный объект")]
    private void TestShowReferenceObject()
    {
        if (objectDataExtractor != null)
        {
            ObjectDataExtractor.ObjectData data = objectDataExtractor.GetExtractedData();
            Debug.Log($"📋 ЭТАЛОННЫЙ ОБЪЕКТ:");
            Debug.Log($"   Имя: {data.Name}");
            Debug.Log($"   🎭 Режим обмана: {(data.IsDeceptionActive ? "✅ АКТИВЕН" : "❌ НЕ АКТИВЕН")}");
            
            RandomRarityOnSpawn refScript = data.GameObject?.GetComponent<RandomRarityOnSpawn>();
            if (refScript != null)
            {
                Debug.Log($"   Редкость: {refScript.AssignedRarity}");
                Debug.Log($"   Характеристики: {GetObjectStatsList(refScript).Count} шт.");
                var stats = GetObjectStatsList(refScript);
                for (int i = 0; i < stats.Count; i++)
                {
                    Debug.Log($"     {i + 1}. {stats[i].stat} +{stats[i].value}");
                }
            }
            else
            {
                Debug.LogWarning("   ⚠️ У эталонного объекта нет скрипта редкости!");
            }
        }
        else
        {
            Debug.LogError("❌ ObjectDataExtractor не найден!");
        }
    }
    
    [ContextMenu("Тест режима обмана - Включить")]
    private void TestEnableDeception()
    {
        if (objectDataExtractor != null)
        {
            objectDataExtractor.isDeceptionActive = true;
            Debug.Log("🎭 Режим обмана принудительно ВКЛЮЧЕН!");
            Debug.Log("Теперь проверяется ТОЛЬКО имя объекта для соответствия");
        }
        else
        {
            Debug.LogError("❌ ObjectDataExtractor не найден!");
        }
    }
    
    [ContextMenu("Тест режима обмана - Выключить")]
    private void TestDisableDeception()
    {
        if (objectDataExtractor != null)
        {
            objectDataExtractor.isDeceptionActive = false;
            Debug.Log("🎭 Режим обмана принудительно ВЫКЛЮЧЕН!");
            Debug.Log("Теперь проверяется имя, редкость И характеристики для соответствия");
        }
        else
        {
            Debug.LogError("❌ ObjectDataExtractor не найден!");
        }
    }
    
    [ContextMenu("Показать текущий режим проверки")]
    private void TestShowCurrentMode()
    {
        if (objectDataExtractor != null)
        {
            ObjectDataExtractor.ObjectData data = objectDataExtractor.GetExtractedData();
            bool isDeceptionActive = data.IsDeceptionActive;
            
            Debug.Log($"🎭 ТЕКУЩИЙ РЕЖИМ ПРОВЕРКИ:");
            Debug.Log($"   Режим обмана: {(isDeceptionActive ? "✅ АКТИВЕН" : "❌ НЕ АКТИВЕН")}");
            
            if (isDeceptionActive)
            {
                Debug.Log($"   📋 Проверяется: ТОЛЬКО имя объекта");
                Debug.Log($"   ✅ Объект пройдет если: имя совпадает с эталоном");
            }
            else
            {
                Debug.Log($"   📋 Проверяется: имя + редкость + характеристики");
                Debug.Log($"   ✅ Объект пройдет если: имя И редкость И характеристики совпадают");
            }
        }
        else
        {
            Debug.LogError("❌ ObjectDataExtractor не найден!");
        }
    }
}
