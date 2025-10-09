using UnityEngine;
using System.Collections;
using PortOfThieves.Resources;

/// <summary>
/// Менеджер для управления объектом Client
/// Отвечает за включение объекта по таймеру
/// </summary>
public class ClientManager : MonoBehaviour
{
    [Header("Client Management")]
    [SerializeField] private GameObject clientObject; // Ссылка на объект Client
    [SerializeField] private bool autoFindClient = true; // Автоматический поиск объекта Client
    
    [Header("Timer Settings")]
    [SerializeField] private float activationDelay = 5f; // Задержка перед включением (в секундах)
    [SerializeField] private bool enableTimer = true; // Включить/выключить таймер
    [SerializeField] private bool startTimerOnAwake = true; // Запускать таймер при старте
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true; // Показывать отладочную информацию
    
    [Header("Deception Tracking")]
    [SerializeField] private bool trackDeceptionState = true; // Включить отслеживание состояния Deception
    
    [Header("Zone 3 Object Tracking")]
    [SerializeField] private bool trackZone3Destructions = true; // Включить отслеживание удалений в zone 3
    [SerializeField] private bool autoDeactivateOnZone3Destruction = true; // Автоматически выключать Client при удалении в zone 3
    [SerializeField] private bool showZone3DebugInfo = true; // Показывать отладочную информацию zone 3
    
    [Header("Object Matching")]
    [SerializeField] private bool checkObjectMatching = true; // Проверять соответствие удаленного объекта с extracted data
    [SerializeField] private bool onlyDeactivateOnMatchingObject = false; // Выключать Client только если удаленный объект соответствует extracted data
    [SerializeField] private bool showObjectMatchingDebugInfo = true; // Показывать отладочную информацию сравнения объектов
    
    [Header("Resource Management")]
    [SerializeField] private ResourceManager resourceManager; // Ссылка на ResourceManager для передачи золота
    [SerializeField] private bool autoFindResourceManager = true; // Автоматический поиск ResourceManager
    [SerializeField] private bool transferGoldOnZone3Destruction = true; // Передавать золото при удалении в zone 3
    
    
    [Header("Public Deception State")]
    public bool isDeceptionActive = false; // Публичное поле для отслеживания состояния Deception
    
    private Coroutine activationTimerCoroutine; // Корутина таймера
    private bool isTimerRunning = false; // Флаг работы таймера
    
    // Zone 3 Object Tracking
    private CursorTagDetector cursorTagDetector; // Ссылка на CursorTagDetector для отслеживания удалений
    private int lastZone3DestructionCount = 0; // Последнее количество удаленных объектов в zone 3
    
    // Object Matching
    private ObjectDataExtractor objectDataExtractor; // Ссылка на ObjectDataExtractor для сравнения объектов
    
    // Resource Management
    private bool resourceManagerSearchAttempted = false; // Флаг попытки поиска ResourceManager
    
    void Start()
    {
        // Автоматический поиск объекта Client если не назначен
        if (clientObject == null && autoFindClient)
        {
            FindClientObject();
        }
        
        // Проверяем наличие объекта Client
        if (clientObject == null)
        {
            Debug.LogError("ClientManager: Объект Client не найден! Назначьте его в Inspector или убедитесь что объект с именем 'Client' существует на сцене.");
            return;
        }
        
        // Запускаем таймер если включен и объект выключен
        if (startTimerOnAwake && enableTimer && !clientObject.activeInHierarchy)
        {
            StartActivationTimer();
        }
        
        // Инициализация отслеживания zone 3
        if (trackZone3Destructions)
        {
            FindCursorTagDetector();
        }
        
        // Инициализация сравнения объектов
        if (checkObjectMatching)
        {
            FindObjectDataExtractor();
        }
        
        // Инициализация ResourceManager
        if (transferGoldOnZone3Destruction && autoFindResourceManager)
        {
            FindResourceManager();
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"ClientManager инициализирован. Client объект: {clientObject.name}, Активен: {clientObject.activeInHierarchy}");
        }
    }
    
    void Update()
    {
        // Проверяем состояние объекта Client
        if (clientObject != null)
        {
            // Если объект выключен и таймер не запущен - запускаем таймер
            if (!clientObject.activeInHierarchy && !isTimerRunning && enableTimer)
            {
                StartActivationTimer();
            }
            // Если объект включен и таймер запущен - останавливаем таймер
            else if (clientObject.activeInHierarchy && isTimerRunning)
            {
                StopActivationTimer();
            }
        }
        
        // Отслеживаем состояние Deception если включено
        if (trackDeceptionState)
        {
            UpdateDeceptionState();
        }
        
        // Отслеживаем удаления в zone 3
        if (trackZone3Destructions)
        {
            UpdateZone3DestructionTracking();
        }
    }
    
    /// <summary>
    /// Автоматический поиск объекта Client на сцене
    /// </summary>
    private void FindClientObject()
    {
        // Ищем объект с именем "Client"
        GameObject foundClient = GameObject.Find("Client");
        
        if (foundClient != null)
        {
            clientObject = foundClient;
            if (showDebugInfo)
            {
                Debug.Log($"ClientManager: Объект Client автоматически найден: {foundClient.name}");
            }
        }
        else
        {
            Debug.LogWarning("ClientManager: Объект с именем 'Client' не найден на сцене!");
        }
    }
    
    /// <summary>
    /// Обновление состояния Deception из ObjectDataExtractor
    /// </summary>
    private void UpdateDeceptionState()
    {
        // Ищем ObjectDataExtractor на сцене
        ObjectDataExtractor objectDataExtractor = FindObjectOfType<ObjectDataExtractor>();
        
        if (objectDataExtractor == null) return;
        
        // Получаем текущее состояние Deception из ObjectDataExtractor
        bool currentDeceptionState = objectDataExtractor.isDeceptionActive;
        
        // Обновляем публичное поле если состояние изменилось
        if (isDeceptionActive != currentDeceptionState)
        {
            isDeceptionActive = currentDeceptionState;
            
            if (showDebugInfo)
            {
                Debug.Log($"ClientManager: Состояние Deception обновлено: {isDeceptionActive}");
            }
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
            
            if (showObjectMatchingDebugInfo)
            {
                Debug.Log($"ClientManager: ObjectDataExtractor автоматически найден: {foundExtractor.name}");
            }
        }
        else
        {
            Debug.LogWarning("ClientManager: ObjectDataExtractor не найден на сцене!");
            
            if (showObjectMatchingDebugInfo)
            {
                Debug.LogWarning("ClientManager: Проверка соответствия объектов будет отключена. Убедитесь что ObjectDataExtractor присутствует на сцене.");
            }
        }
    }
    
    /// <summary>
    /// Автоматический поиск ResourceManager на сцене
    /// </summary>
    private void FindResourceManager()
    {
        if (resourceManagerSearchAttempted) return;
        resourceManagerSearchAttempted = true;
        
        // Ищем компонент ResourceManager на сцене
        ResourceManager foundResourceManager = FindObjectOfType<ResourceManager>();
        
        if (foundResourceManager != null)
        {
            resourceManager = foundResourceManager;
            
            if (showZone3DebugInfo)
            {
                Debug.Log($"ClientManager: ResourceManager автоматически найден: {foundResourceManager.name}");
            }
        }
        else
        {
            Debug.LogWarning("ClientManager: ResourceManager не найден на сцене!");
        }
    }
    
    /// <summary>
    /// Проверяет, соответствует ли удаленный объект extracted data
    /// </summary>
    private bool IsDestroyedObjectMatchingExtractedData(CursorTagDetector.DestroyedObjectInfo destroyedObject)
    {
        if (objectDataExtractor == null) return false;
        
        // Получаем extracted data из ObjectDataExtractor
        ObjectDataExtractor.ObjectData extractedData = objectDataExtractor.GetExtractedData();
        
        // Проверяем соответствие по имени объекта
        bool nameMatches = destroyedObject.objectName == extractedData.Name;
        
        // Проверяем соответствие по редкости (если есть)
        bool rarityMatches = true;
        if (destroyedObject.hadRandomRarityScript && !string.IsNullOrEmpty(destroyedObject.rarity))
        {
            // Здесь можно добавить дополнительную логику сравнения редкости
            // Пока просто проверяем что у обоих объектов есть редкость
            rarityMatches = !string.IsNullOrEmpty(extractedData.Name); // Упрощенная проверка
        }
        
        bool isMatching = nameMatches && rarityMatches;
        
        if (showObjectMatchingDebugInfo)
        {
            Debug.Log($"=== СРАВНЕНИЕ ОБЪЕКТОВ ===");
            Debug.Log($"Удаленный объект: {destroyedObject.objectName} (редкость: {destroyedObject.rarity})");
            Debug.Log($"Extracted data: {extractedData.Name}");
            Debug.Log($"Соответствие по имени: {nameMatches}");
            Debug.Log($"Соответствие по редкости: {rarityMatches}");
            Debug.Log($"Общее соответствие: {isMatching}");
        }
        
        return isMatching;
    }
    
    /// <summary>
    /// Автоматический поиск CursorTagDetector на сцене
    /// </summary>
    private void FindCursorTagDetector()
    {
        // Ищем компонент CursorTagDetector на сцене
        CursorTagDetector foundDetector = FindObjectOfType<CursorTagDetector>();
        
        if (foundDetector != null)
        {
            cursorTagDetector = foundDetector;
            lastZone3DestructionCount = foundDetector.GetTotalDestroyedCount();
            
            if (showZone3DebugInfo)
            {
                Debug.Log($"ClientManager: CursorTagDetector автоматически найден: {foundDetector.name}");
                Debug.Log($"ClientManager: Начальное количество удаленных объектов в zone 3: {lastZone3DestructionCount}");
            }
        }
        else
        {
            Debug.LogWarning("ClientManager: CursorTagDetector не найден на сцене!");
        }
    }
    
    /// <summary>
    /// Отслеживание удалений объектов в zone 3
    /// </summary>
    private void UpdateZone3DestructionTracking()
    {
        if (cursorTagDetector == null) return;
        
        // Получаем текущее количество удаленных объектов
        int currentDestructionCount = cursorTagDetector.GetTotalDestroyedCount();
        
        // Проверяем, увеличилось ли количество удалений
        if (currentDestructionCount > lastZone3DestructionCount)
        {
            // Получаем список новых удаленных объектов
            var recentDestroyedObjects = cursorTagDetector.GetDestroyedObjectsInLastSeconds(1f);
            
            // Обновляем счетчик
            int newDestructions = currentDestructionCount - lastZone3DestructionCount;
            lastZone3DestructionCount = currentDestructionCount;
            
            if (showZone3DebugInfo)
            {
                Debug.Log($"ClientManager: Обнаружено {newDestructions} новых удалений в zone 3. Всего: {currentDestructionCount}");
            }
            
            // Проверяем каждый новый удаленный объект
            foreach (var destroyedObject in recentDestroyedObjects)
            {
                bool shouldDeactivateClient = false;
                
                // Проверяем, активен ли Client
                if (IsClientActive())
                {
                    if (showZone3DebugInfo)
                    {
                        Debug.Log($"ClientManager: Client активен во время удаления объекта '{destroyedObject.objectName}' в zone 3!");
                    }
                    
                    // Проверяем соответствие объекта с extracted data
                    if (checkObjectMatching)
                    {
                        if (objectDataExtractor == null)
                        {
                            // Если ObjectDataExtractor не найден, используем старую логику
                            shouldDeactivateClient = true;
                            
                            if (showZone3DebugInfo)
                            {
                                Debug.LogWarning("ClientManager: ObjectDataExtractor не найден! Используется старая логика - Client будет выключен при любом удалении в zone 3");
                            }
                        }
                        else
                        {
                            bool objectMatches = IsDestroyedObjectMatchingExtractedData(destroyedObject);
                            
                            if (onlyDeactivateOnMatchingObject)
                            {
                                // Выключаем Client только если объект соответствует extracted data
                                shouldDeactivateClient = objectMatches;
                                
                                if (showZone3DebugInfo)
                                {
                                    Debug.Log($"ClientManager: Проверка соответствия объекта - {(objectMatches ? "СООТВЕТСТВУЕТ" : "НЕ СООТВЕТСТВУЕТ")} extracted data");
                                }
                            }
                            else
                            {
                                // Выключаем Client при любом удалении (старая логика)
                                shouldDeactivateClient = true;
                                
                                if (showZone3DebugInfo)
                                {
                                    Debug.Log("ClientManager: Проверка соответствия включена, но выключение только при соответствии отключено - Client будет выключен при любом удалении");
                                }
                            }
                        }
                    }
                    else
                    {
                        // Если проверка соответствия отключена, используем старую логику
                        shouldDeactivateClient = true;
                        
                        if (showZone3DebugInfo)
                        {
                            Debug.Log("ClientManager: Проверка соответствия отключена - Client будет выключен при любом удалении в zone 3");
                        }
                    }
                    
                    // Автоматически выключаем Client если нужно
                    if (shouldDeactivateClient && autoDeactivateOnZone3Destruction)
                    {
                        ForceDeactivateClient();
                        
                        if (showZone3DebugInfo)
                        {
                            string reason = checkObjectMatching && onlyDeactivateOnMatchingObject ? 
                                "из-за удаления соответствующего объекта в zone 3" : 
                                "из-за удаления объекта в zone 3";
                            Debug.Log($"ClientManager: Client автоматически выключен {reason}");
                        }
                    }
                }
                
                // Передаем золото в ResourceManager при удалении объекта в zone 3
                if (transferGoldOnZone3Destruction && destroyedObject.gold > 0)
                {
                    TransferGoldToResourceManager(destroyedObject);
                }
            }
        }
    }
    
    /// <summary>
    /// Принудительное обновление отслеживания zone 3
    /// </summary>
    public void ForceUpdateZone3Tracking()
    {
        if (cursorTagDetector == null)
        {
            FindCursorTagDetector();
        }
        
        if (cursorTagDetector != null)
        {
            lastZone3DestructionCount = cursorTagDetector.GetTotalDestroyedCount();
            
            if (showZone3DebugInfo)
            {
                Debug.Log($"ClientManager: Принудительное обновление отслеживания zone 3. Текущий счетчик: {lastZone3DestructionCount}");
            }
        }
    }
    
    /// <summary>
    /// Передает золото удаленного объекта в ResourceManager
    /// </summary>
    private void TransferGoldToResourceManager(CursorTagDetector.DestroyedObjectInfo destroyedObject)
    {
        if (resourceManager == null)
        {
            // Попробуем найти ResourceManager если он не найден
            if (autoFindResourceManager && !resourceManagerSearchAttempted)
            {
                FindResourceManager();
            }
            
            if (resourceManager == null)
            {
                Debug.LogWarning($"ClientManager: ResourceManager не найден! Золото ({destroyedObject.gold}) от объекта '{destroyedObject.objectName}' не передано.");
                return;
            }
        }
        
        // Добавляем золото в ResourceManager
        bool success = resourceManager.AddGold(destroyedObject.gold);
        
        if (showZone3DebugInfo)
        {
            if (success)
            {
                Debug.Log($"💰 ClientManager: Золото ({destroyedObject.gold}) от объекта '{destroyedObject.objectName}' успешно добавлено в ResourceManager!");
                Debug.Log($"💰 ClientManager: Текущее количество золота: {resourceManager.GoldAmount}");
            }
            else
            {
                Debug.LogWarning($"💰 ClientManager: Не удалось добавить золото ({destroyedObject.gold}) от объекта '{destroyedObject.objectName}' в ResourceManager (достигнут максимум)!");
            }
        }
    }
    
    /// <summary>
    /// Запуск таймера включения объекта Client
    /// </summary>
    public void StartActivationTimer()
    {
        if (clientObject == null)
        {
            Debug.LogError("ClientManager: Нельзя запустить таймер - объект Client не назначен!");
            return;
        }
        
        if (isTimerRunning)
        {
            if (showDebugInfo)
            {
                Debug.Log("ClientManager: Таймер уже запущен!");
            }
            return;
        }
        
        if (clientObject.activeInHierarchy)
        {
            if (showDebugInfo)
            {
                Debug.Log("ClientManager: Объект Client уже активен, таймер не нужен!");
            }
            return;
        }
        
        // Останавливаем предыдущий таймер если он был
        StopActivationTimer();
        
        // Запускаем новый таймер
        activationTimerCoroutine = StartCoroutine(ActivationTimerCoroutine());
        isTimerRunning = true;
        
        if (showDebugInfo)
        {
            Debug.Log($"ClientManager: Таймер включения запущен. Время до активации: {activationDelay} секунд");
        }
    }
    
    /// <summary>
    /// Остановка таймера включения
    /// </summary>
    public void StopActivationTimer()
    {
        if (activationTimerCoroutine != null)
        {
            StopCoroutine(activationTimerCoroutine);
            activationTimerCoroutine = null;
        }
        
        isTimerRunning = false;
        
        if (showDebugInfo)
        {
            Debug.Log("ClientManager: Таймер включения остановлен");
        }
    }
    
    /// <summary>
    /// Корутина таймера включения
    /// </summary>
    private IEnumerator ActivationTimerCoroutine()
    {
        if (showDebugInfo)
        {
            Debug.Log($"ClientManager: Ожидание {activationDelay} секунд перед включением объекта Client...");
        }
        
        // Ждем указанное время
        yield return new WaitForSeconds(activationDelay);
        
        // Проверяем что объект все еще существует и не активен
        if (clientObject != null && !clientObject.activeInHierarchy)
        {
            // Включаем объект Client
            clientObject.SetActive(true);
            
            if (showDebugInfo)
            {
                Debug.Log($"ClientManager: Объект Client включен после {activationDelay} секунд ожидания!");
            }
        }
        else if (clientObject == null)
        {
            Debug.LogError("ClientManager: Объект Client был уничтожен во время ожидания!");
        }
        else if (clientObject.activeInHierarchy)
        {
            if (showDebugInfo)
            {
                Debug.Log("ClientManager: Объект Client уже был включен во время ожидания!");
            }
        }
        
        // Сбрасываем флаг работы таймера
        isTimerRunning = false;
        activationTimerCoroutine = null;
    }
    
    /// <summary>
    /// Принудительное включение объекта Client
    /// </summary>
    public void ForceActivateClient()
    {
        if (clientObject == null)
        {
            Debug.LogError("ClientManager: Нельзя включить объект - Client не назначен!");
            return;
        }
        
        // Останавливаем таймер если он запущен
        StopActivationTimer();
        
        // Включаем объект
        clientObject.SetActive(true);
        
        if (showDebugInfo)
        {
            Debug.Log("ClientManager: Объект Client принудительно включен!");
        }
    }
    
    /// <summary>
    /// Принудительное выключение объекта Client
    /// </summary>
    public void ForceDeactivateClient()
    {
        if (clientObject == null)
        {
            Debug.LogError("ClientManager: Нельзя выключить объект - Client не назначен!");
            return;
        }
        
        // Останавливаем таймер если он запущен
        StopActivationTimer();
        
        // Выключаем объект
        clientObject.SetActive(false);
        
        if (showDebugInfo)
        {
            Debug.Log("ClientManager: Объект Client принудительно выключен!");
        }
    }
    
    /// <summary>
    /// Установка задержки таймера
    /// </summary>
    public void SetActivationDelay(float delay)
    {
        activationDelay = Mathf.Max(0f, delay);
        
        if (showDebugInfo)
        {
            Debug.Log($"ClientManager: Задержка таймера установлена: {activationDelay} секунд");
        }
    }
    
    /// <summary>
    /// Включение/выключение таймера
    /// </summary>
    public void SetTimerEnabled(bool enabled)
    {
        enableTimer = enabled;
        
        if (!enabled && isTimerRunning)
        {
            StopActivationTimer();
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"ClientManager: Таймер {(enabled ? "включен" : "выключен")}");
        }
    }
    
    /// <summary>
    /// Получение состояния объекта Client
    /// </summary>
    public bool IsClientActive()
    {
        return clientObject != null && clientObject.activeInHierarchy;
    }
    
    /// <summary>
    /// Получение состояния таймера
    /// </summary>
    public bool IsTimerRunning()
    {
        return isTimerRunning;
    }
    
    /// <summary>
    /// Получение оставшегося времени таймера
    /// </summary>
    public float GetRemainingTime()
    {
        if (!isTimerRunning || activationTimerCoroutine == null)
        {
            return 0f;
        }
        
        // Возвращаем полное время задержки (точное время сложно отследить без дополнительной логики)
        return activationDelay;
    }
    
    /// <summary>
    /// Получение текущего состояния Deception
    /// </summary>
    public bool GetDeceptionState()
    {
        return isDeceptionActive;
    }
    
    /// <summary>
    /// Включение/выключение отслеживания состояния Deception
    /// </summary>
    public void SetDeceptionTrackingEnabled(bool enabled)
    {
        trackDeceptionState = enabled;
        
        if (showDebugInfo)
        {
            Debug.Log($"ClientManager: Отслеживание Deception {(enabled ? "включено" : "выключено")}");
        }
    }
    
    /// <summary>
    /// Принудительное обновление состояния Deception
    /// </summary>
    public void ForceUpdateDeceptionState()
    {
        UpdateDeceptionState();
        
        if (showDebugInfo)
        {
            Debug.Log($"ClientManager: Принудительное обновление состояния Deception: {isDeceptionActive}");
        }
    }
    
    /// <summary>
    /// Принудительный поиск ObjectDataExtractor во время выполнения
    /// </summary>
    public void ForceFindObjectDataExtractor()
    {
        FindObjectDataExtractor();
        
        if (showObjectMatchingDebugInfo)
        {
            Debug.Log($"ClientManager: Принудительный поиск ObjectDataExtractor - {(objectDataExtractor != null ? "НАЙДЕН" : "НЕ НАЙДЕН")}");
        }
    }
    
    // Контекстные меню для тестирования
    [ContextMenu("Запустить таймер включения")]
    private void TestStartTimer()
    {
        StartActivationTimer();
    }
    
    [ContextMenu("Остановить таймер")]
    private void TestStopTimer()
    {
        StopActivationTimer();
    }
    
    [ContextMenu("Принудительно включить Client")]
    private void TestForceActivate()
    {
        ForceActivateClient();
    }
    
    [ContextMenu("Принудительно выключить Client")]
    private void TestForceDeactivate()
    {
        ForceDeactivateClient();
    }
    
    [ContextMenu("Показать статус")]
    private void ShowStatus()
    {
        Debug.Log($"=== Статус ClientManager ===");
        Debug.Log($"Client объект: {(clientObject != null ? clientObject.name : "НЕ НАЗНАЧЕН")}");
        Debug.Log($"Client активен: {IsClientActive()}");
        Debug.Log($"Таймер запущен: {IsTimerRunning()}");
        Debug.Log($"Задержка таймера: {activationDelay} секунд");
        Debug.Log($"Таймер включен: {enableTimer}");
        Debug.Log($"Отслеживание Deception: {trackDeceptionState}");
        Debug.Log($"Состояние Deception: {isDeceptionActive}");
    }
    
    [ContextMenu("Обновить состояние Deception")]
    private void TestUpdateDeception()
    {
        ForceUpdateDeceptionState();
    }
    
    [ContextMenu("Включить отслеживание Deception")]
    private void TestEnableDeceptionTracking()
    {
        SetDeceptionTrackingEnabled(true);
    }
    
    [ContextMenu("Выключить отслеживание Deception")]
    private void TestDisableDeceptionTracking()
    {
        SetDeceptionTrackingEnabled(false);
    }
    
    // ========== КОНТЕКСТНЫЕ МЕНЮ ДЛЯ ТЕСТИРОВАНИЯ ZONE 3 TRACKING ==========
    
    [ContextMenu("Проверить статус Zone 3 Tracking")]
    private void TestZone3TrackingStatus()
    {
        Debug.Log($"=== СТАТУС ZONE 3 TRACKING ===");
        Debug.Log($"Отслеживание Zone 3 включено: {trackZone3Destructions}");
        Debug.Log($"Автовыключение Client при удалении: {autoDeactivateOnZone3Destruction}");
        Debug.Log($"Отладочная информация Zone 3: {showZone3DebugInfo}");
        Debug.Log($"CursorTagDetector найден: {(cursorTagDetector != null ? "Да" : "Нет")}");
        
        if (cursorTagDetector != null)
        {
            Debug.Log($"CursorTagDetector: {cursorTagDetector.name}");
            Debug.Log($"Текущий счетчик удалений: {cursorTagDetector.GetTotalDestroyedCount()}");
            Debug.Log($"Последний отслеживаемый счетчик: {lastZone3DestructionCount}");
        }
        
        Debug.Log($"Client активен: {IsClientActive()}");
    }
    
    [ContextMenu("Обновить отслеживание Zone 3")]
    private void TestUpdateZone3Tracking()
    {
        ForceUpdateZone3Tracking();
    }
    
    [ContextMenu("Включить отслеживание Zone 3")]
    private void TestEnableZone3Tracking()
    {
        trackZone3Destructions = true;
        Debug.Log("Отслеживание Zone 3 включено!");
    }
    
    [ContextMenu("Выключить отслеживание Zone 3")]
    private void TestDisableZone3Tracking()
    {
        trackZone3Destructions = false;
        Debug.Log("Отслеживание Zone 3 выключено!");
    }
    
    [ContextMenu("Переключить автовыключение Client при удалении")]
    private void TestToggleAutoDeactivateOnDestruction()
    {
        autoDeactivateOnZone3Destruction = !autoDeactivateOnZone3Destruction;
        Debug.Log($"Автовыключение Client при удалении в Zone 3: {(autoDeactivateOnZone3Destruction ? "Включено" : "Выключено")}");
    }
    
    [ContextMenu("Симулировать удаление в Zone 3")]
    private void TestSimulateZone3Destruction()
    {
        if (cursorTagDetector != null)
        {
            // Увеличиваем счетчик для симуляции
            lastZone3DestructionCount = cursorTagDetector.GetTotalDestroyedCount() - 1;
            Debug.Log("Симуляция удаления в Zone 3 - счетчик уменьшен на 1");
        }
        else
        {
            Debug.LogError("CursorTagDetector не найден для симуляции!");
        }
    }
    
    // ========== КОНТЕКСТНЫЕ МЕНЮ ДЛЯ ТЕСТИРОВАНИЯ OBJECT MATCHING ==========
    
    [ContextMenu("Проверить статус Object Matching")]
    private void TestObjectMatchingStatus()
    {
        Debug.Log($"=== СТАТУС OBJECT MATCHING ===");
        Debug.Log($"Проверка соответствия объектов включена: {checkObjectMatching}");
        Debug.Log($"Выключение только при соответствии: {onlyDeactivateOnMatchingObject}");
        Debug.Log($"Отладочная информация Object Matching: {showObjectMatchingDebugInfo}");
        Debug.Log($"ObjectDataExtractor найден: {(objectDataExtractor != null ? "Да" : "Нет")}");
        
        if (objectDataExtractor != null)
        {
            Debug.Log($"ObjectDataExtractor: {objectDataExtractor.name}");
            
            // Получаем текущие extracted data
            ObjectDataExtractor.ObjectData extractedData = objectDataExtractor.GetExtractedData();
            Debug.Log($"Текущий extracted объект: {extractedData.Name}");
            Debug.Log($"Deception активен: {extractedData.IsDeceptionActive}");
        }
        
        Debug.Log($"Client активен: {IsClientActive()}");
    }
    
    [ContextMenu("Обновить ObjectDataExtractor")]
    private void TestUpdateObjectDataExtractor()
    {
        FindObjectDataExtractor();
    }
    
    [ContextMenu("Принудительно найти ObjectDataExtractor")]
    private void TestForceFindObjectDataExtractor()
    {
        ForceFindObjectDataExtractor();
    }
    
    [ContextMenu("Включить проверку соответствия объектов")]
    private void TestEnableObjectMatching()
    {
        checkObjectMatching = true;
        Debug.Log("Проверка соответствия объектов включена!");
    }
    
    [ContextMenu("Выключить проверку соответствия объектов")]
    private void TestDisableObjectMatching()
    {
        checkObjectMatching = false;
        Debug.Log("Проверка соответствия объектов выключена!");
    }
    
    [ContextMenu("Переключить выключение только при соответствии")]
    private void TestToggleOnlyDeactivateOnMatching()
    {
        onlyDeactivateOnMatchingObject = !onlyDeactivateOnMatchingObject;
        Debug.Log($"Выключение Client только при соответствии объекта: {(onlyDeactivateOnMatchingObject ? "Включено" : "Выключено")}");
    }
    
    [ContextMenu("Тест сравнения объектов")]
    private void TestObjectComparison()
    {
        if (objectDataExtractor == null)
        {
            Debug.LogError("ObjectDataExtractor не найден для тестирования!");
            return;
        }
        
        // Получаем extracted data
        ObjectDataExtractor.ObjectData extractedData = objectDataExtractor.GetExtractedData();
        
        Debug.Log($"=== ТЕСТ СРАВНЕНИЯ ОБЪЕКТОВ ===");
        Debug.Log($"Extracted data: {extractedData.Name}");
        Debug.Log($"Deception активен: {extractedData.IsDeceptionActive}");
        
        // Создаем тестовый объект для сравнения
        CursorTagDetector.DestroyedObjectInfo testObject = new CursorTagDetector.DestroyedObjectInfo(
            extractedData.Name, // Используем то же имя что и в extracted data
            "obj",
            Vector3.zero,
            Time.time,
            "Test",
            true,
            "Common"
        );
        
        bool matches = IsDestroyedObjectMatchingExtractedData(testObject);
        Debug.Log($"Тестовый объект соответствует extracted data: {matches}");
    }
    
    void OnDestroy()
    {
        // Останавливаем таймер при уничтожении объекта
        StopActivationTimer();
    }
}
