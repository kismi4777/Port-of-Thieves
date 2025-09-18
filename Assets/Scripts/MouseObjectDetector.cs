using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class MouseObjectDetector : MonoBehaviour
{
    [Header("Object Detection")]
    [SerializeField] public string detectedObjectName = "None";
    
    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 0.3f; // Увеличиваем радиус для лучшего обнаружения
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private int detectionPoints = 3; // Количество точек для проверки при быстром движении
    [SerializeField] private float fastMovementThreshold = 5f; // Порог быстрого движения в пикселях
    [SerializeField] private float hideDelay = 0.05f; // Задержка исчезания имени в секундах (настройка в сотых)
    
    [Header("UI Display")]
    [SerializeField] private TextMeshProUGUI displayText;
    [SerializeField] private string displayFormat = "Объект: {0}";
    
    [Header("Prefab Control")]
    [SerializeField] private GameObject targetPrefab; // Префаб для включения/выключения
    [SerializeField] private bool showPrefabWhenObjectDetected = true; // Показывать префаб при обнаружении объекта
    
    private Camera mainCamera;
    private Mouse mouse;
    private Vector2 lastMousePosition;
    private string lastDisplayedName = "None";
    private float hideTimer = 0f; // Таймер для задержки исчезания
    private bool isObjectDetected = false; // Флаг обнаружения объекта
    private bool prefabWasActive = false; // Предыдущее состояние префаба
    
    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
        
        // Инициализируем Input System
        mouse = Mouse.current;
        
        // Если UI Text не назначен, попробуем найти его автоматически
        if (displayText == null)
        {
            displayText = FindObjectOfType<TextMeshProUGUI>();
        }
        
        Debug.Log($"MouseObjectDetector initialized. Camera: {mainCamera?.name}");
    }
    
    void Update()
    {
        DetectObjectUnderMouse();
    }
    
    private void DetectObjectUnderMouse()
    {
        if (mainCamera == null || mouse == null) return;
        
        Vector2 mousePosition = mouse.position.ReadValue();
        
        // Конвертируем позицию мыши в мировые координаты
        Vector3 worldPosition;
        if (mainCamera.orthographic)
        {
            float distance = Mathf.Abs(mainCamera.transform.position.z);
            worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, distance));
        }
        else
        {
            worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 10f));
        }
        
        // Ищем коллайдеры в радиусе
        Collider2D[] colliders = Physics2D.OverlapCircleAll(new Vector2(worldPosition.x, worldPosition.y), detectionRadius);
        
        if (colliders.Length > 0)
        {
            // Берем первый найденный коллайдер
            Collider2D hitCollider = colliders[0];
            detectedObjectName = hitCollider.name;
            isObjectDetected = true;
            hideTimer = 0f; // Сбрасываем таймер при обнаружении объекта
            
            if (showDebugInfo)
            {
                Debug.Log($"Найден объект: {detectedObjectName} (Tag: {hitCollider.tag})");
            }
        }
        else
        {
            // Если объект не найден, проверяем промежуточные точки между текущей и предыдущей позицией
            bool objectFound = false;
            
            if (lastMousePosition != Vector2.zero)
            {
                Vector2 mouseDelta = mousePosition - lastMousePosition;
                float distance = mouseDelta.magnitude;
                
                // Если мышь двигается быстро, проверяем промежуточные точки
                if (distance > fastMovementThreshold)
                {
                    for (int i = 1; i <= detectionPoints; i++)
                    {
                        float t = (float)i / (detectionPoints + 1);
                        Vector2 intermediatePos = lastMousePosition + mouseDelta * t;
                        
                        // Конвертируем промежуточную позицию в мировые координаты
                        Vector3 intermediateWorldPos;
                        if (mainCamera.orthographic)
                        {
                            float distance2 = Mathf.Abs(mainCamera.transform.position.z);
                            intermediateWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(intermediatePos.x, intermediatePos.y, distance2));
                        }
                        else
                        {
                            intermediateWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(intermediatePos.x, intermediatePos.y, 10f));
                        }
                        
                        // Проверяем коллайдеры в промежуточной точке
                        Collider2D[] intermediateColliders = Physics2D.OverlapCircleAll(new Vector2(intermediateWorldPos.x, intermediateWorldPos.y), detectionRadius);
                        
                        if (intermediateColliders.Length > 0)
                        {
                            Collider2D hitCollider = intermediateColliders[0];
                            detectedObjectName = hitCollider.name;
                            isObjectDetected = true;
                            hideTimer = 0f; // Сбрасываем таймер при обнаружении объекта
                            objectFound = true;
                            
                            if (showDebugInfo)
                            {
                                Debug.Log($"Найден объект в промежуточной точке: {detectedObjectName}");
                            }
                            break; // Найден объект, прекращаем поиск
                        }
                    }
                }
            }
            
            // Если объект не найден ни в текущей позиции, ни в промежуточных точках
            if (!objectFound)
            {
                isObjectDetected = false;
                // Увеличиваем таймер с использованием Time.deltaTime
                hideTimer += Time.deltaTime;
                
                // Если прошло достаточно времени, сбрасываем имя объекта
                if (hideTimer >= hideDelay)
                {
                    detectedObjectName = "None";
                }
            }
        }
        
        // Сохраняем текущую позицию мыши для следующего кадра
        lastMousePosition = mousePosition;
        
        // Обновляем UI отображение только если имя изменилось
        if (detectedObjectName != lastDisplayedName)
        {
            UpdateDisplay();
            lastDisplayedName = detectedObjectName;
        }
        
        // Выводим информацию в консоль для отладки
        if (showDebugInfo && detectedObjectName != "None")
        {
            Debug.Log($"Объект под курсором: {detectedObjectName}");
        }
    }
    
    // Публичные методы для доступа к данным
    public string GetDetectedObjectName()
    {
        return detectedObjectName;
    }
    
    public bool IsObjectDetected()
    {
        return detectedObjectName != "None";
    }
    
    public void SetDetectionRadius(float newRadius)
    {
        detectionRadius = newRadius;
    }
    
    public void SetDebugInfo(bool enabled)
    {
        showDebugInfo = enabled;
    }
    
    public void SetHideDelay(float delay)
    {
        hideDelay = delay;
    }
    
    public float GetHideDelay()
    {
        return hideDelay;
    }
    
    public void SetTargetPrefab(GameObject prefab)
    {
        targetPrefab = prefab;
    }
    
    public GameObject GetTargetPrefab()
    {
        return targetPrefab;
    }
    
    public void SetShowPrefabWhenObjectDetected(bool show)
    {
        showPrefabWhenObjectDetected = show;
    }
    
    public bool GetShowPrefabWhenObjectDetected()
    {
        return showPrefabWhenObjectDetected;
    }
    
    public void ForceUpdatePrefabVisibility()
    {
        UpdatePrefabVisibility();
    }
    
    private void UpdateDisplay()
    {
        if (displayText != null)
        {
            if (detectedObjectName == "None")
            {
                displayText.text = "Объект: None";
            }
            else
            {
                displayText.text = string.Format(displayFormat, detectedObjectName);
            }
        }
        
        // Управление префабом
        UpdatePrefabVisibility();
    }
    
    private void UpdatePrefabVisibility()
    {
        if (targetPrefab == null) return;
        
        bool shouldShowPrefab = detectedObjectName != "None" && showPrefabWhenObjectDetected;
        
        // Обновляем состояние префаба только если оно изменилось
        if (targetPrefab.activeInHierarchy != shouldShowPrefab)
        {
            targetPrefab.SetActive(shouldShowPrefab);
            prefabWasActive = shouldShowPrefab;
            
            if (showDebugInfo)
            {
                if (shouldShowPrefab)
                {
                    Debug.Log($"Префаб '{targetPrefab.name}' включен (объект обнаружен: {detectedObjectName})");
                }
                else
                {
                    Debug.Log($"Префаб '{targetPrefab.name}' выключен (объект не обнаружен)");
                }
            }
        }
    }
}
