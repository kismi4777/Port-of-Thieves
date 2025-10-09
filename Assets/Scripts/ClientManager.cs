using UnityEngine;
using System.Collections;

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
    
    private Coroutine activationTimerCoroutine; // Корутина таймера
    private bool isTimerRunning = false; // Флаг работы таймера
    
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
    }
    
    void OnDestroy()
    {
        // Останавливаем таймер при уничтожении объекта
        StopActivationTimer();
    }
}
