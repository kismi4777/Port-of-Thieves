using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

public class SimpleMouseDetector : MonoBehaviour
{
    [Header("Object Detection")]
    [SerializeField] public string detectedObjectName = "None";
    [SerializeField] public bool hasRandomRarityScript = false;
    
    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 0.3f;
    [SerializeField] private float hideDelayMs = 100f; // Задержка в миллисекундах
    [SerializeField] private bool usePreciseDetection = true; // Точное обнаружение через Raycast
    
    [Header("UI Display")]
    [SerializeField] private TextMeshProUGUI displayText;
    [SerializeField] private TextMeshProUGUI rarityText;
    [SerializeField] private TextMeshProUGUI objectTextDisplay;
    
    [Header("Stats Display")]
    [SerializeField] private TextMeshProUGUI stat1Text;
    [SerializeField] private TextMeshProUGUI stat2Text;
    [SerializeField] private TextMeshProUGUI stat3Text;
    [SerializeField] private TextMeshProUGUI stat4Text;
    [SerializeField] private TextMeshProUGUI stat5Text;
    
    [Header("Stats Values Display")]
    [SerializeField] private TextMeshProUGUI stat1ValueText;
    [SerializeField] private TextMeshProUGUI stat2ValueText;
    [SerializeField] private TextMeshProUGUI stat3ValueText;
    [SerializeField] private TextMeshProUGUI stat4ValueText;
    [SerializeField] private TextMeshProUGUI stat5ValueText;
    
    [Header("Mouse Control")]
    [SerializeField] private GameObject controlledObject;
    [SerializeField] private float holdTimeRequired = 0.5f; // Время удержания в секундах
    
    private Camera mainCamera;
    private Mouse mouse;
    private float hideTimer = 0f;
    private bool isObjectDetected = false;
    private string lastDisplayedName = "None";
    private string lastRarity = "";
    private Collider2D currentDetectedCollider;
    
    // Переменные для управления объектом
    private float mouseHoldTimer = 0f;
    private bool isMouseHeld = false;
    
    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
        
        mouse = Mouse.current;
    }
    
    void Update()
    {
        DetectObjectUnderMouse();
        HandleMouseControl();
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
        
        Collider2D detectedCollider = null;
        
        if (usePreciseDetection)
        {
            // Точное обнаружение через Raycast - проверяем что именно находится под курсором
            RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero, 0f);
            if (hit.collider != null)
            {
                detectedCollider = hit.collider;
            }
        }
        else
        {
            // Старый метод - поиск в радиусе
            Collider2D[] colliders = Physics2D.OverlapCircleAll(new Vector2(worldPosition.x, worldPosition.y), detectionRadius);
            
            if (colliders.Length > 0)
            {
                // Находим ближайший коллайдер к позиции мыши
                Collider2D closestCollider = null;
                float closestDistance = float.MaxValue;
                
                foreach (Collider2D collider in colliders)
                {
                    if (collider != null)
                    {
                        float distance = Vector2.Distance(worldPosition, collider.bounds.center);
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestCollider = collider;
                        }
                    }
                }
                
                detectedCollider = closestCollider;
            }
        }
        
        if (detectedCollider != null)
        {
            detectedObjectName = detectedCollider.name;
            currentDetectedCollider = detectedCollider; // Сохраняем ссылку на конкретный коллайдер
            
            // Проверяем наличие скрипта RandomRarityOnSpawn
            RandomRarityOnSpawn rarityScript = detectedCollider.GetComponent<RandomRarityOnSpawn>();
            hasRandomRarityScript = rarityScript != null;
            
            isObjectDetected = true;
            hideTimer = 0f; // Сбрасываем таймер при обнаружении объекта
        }
        else
        {
            isObjectDetected = false;
            // Увеличиваем таймер
            hideTimer += Time.deltaTime * 1000f; // Конвертируем в миллисекунды
            
            // Если прошло достаточно времени, сбрасываем имя объекта
            if (hideTimer >= hideDelayMs)
            {
                detectedObjectName = "None";
                hasRandomRarityScript = false;
                currentDetectedCollider = null; // Сбрасываем ссылку на коллайдер
            }
        }
        
        // Обновляем UI отображение только если имя или редкость изменились
        if (detectedObjectName != lastDisplayedName)
        {
            UpdateDisplay();
            lastDisplayedName = detectedObjectName;
        }
    }
    
    private void UpdateDisplay()
    {
        if (displayText != null)
        {
            displayText.text = detectedObjectName;
        }
        
        if (rarityText != null)
        {
            if (hasRandomRarityScript && detectedObjectName != "None" && currentDetectedCollider != null)
            {
                // Получаем редкость из скрипта RandomRarityOnSpawn напрямую из коллайдера
                RandomRarityOnSpawn rarityScript = currentDetectedCollider.GetComponent<RandomRarityOnSpawn>();
                if (rarityScript != null)
                {
                    rarityText.text = rarityScript.AssignedRarity.ToString();
                    
                    // Устанавливаем цвет текста на основе цвета редкости
                    Color rarityColor = rarityScript.AssignedColor;
                    displayText.color = rarityColor;
                    rarityText.color = rarityColor;
                    
                    // Отображаем статы объекта
                    UpdateStatsDisplay(rarityScript);
                    
                    // Отображаем текст с компонента Text объекта
                    UpdateObjectTextDisplay();
                }
                else
                {
                    rarityText.text = "";
                    // Сбрасываем цвет на белый если скрипт не найден
                    displayText.color = Color.white;
                    rarityText.color = Color.white;
                    ClearStatsDisplay();
                    ClearObjectTextDisplay();
                }
            }
            else
            {
                rarityText.text = "";
                // Сбрасываем цвет на белый если нет скрипта или объект не обнаружен
                displayText.color = Color.white;
                rarityText.color = Color.white;
                ClearStatsDisplay();
                ClearObjectTextDisplay();
            }
        }
    }
    
    private void UpdateStatsDisplay(RandomRarityOnSpawn rarityScript)
    {
        // Обновляем стат 1
        if (stat1Text != null)
        {
            if (!string.IsNullOrEmpty(rarityScript.stat1))
            {
                stat1Text.text = rarityScript.stat1;
            }
            else
            {
                stat1Text.text = "";
            }
        }
        
        // Обновляем значение стата 1
        if (stat1ValueText != null)
        {
            if (!string.IsNullOrEmpty(rarityScript.stat1))
            {
                stat1ValueText.text = $"+{rarityScript.stat1Value}";
            }
            else
            {
                stat1ValueText.text = "";
            }
        }
        
        // Обновляем стат 2
        if (stat2Text != null)
        {
            if (!string.IsNullOrEmpty(rarityScript.stat2))
            {
                stat2Text.text = rarityScript.stat2;
            }
            else
            {
                stat2Text.text = "";
            }
        }
        
        // Обновляем значение стата 2
        if (stat2ValueText != null)
        {
            if (!string.IsNullOrEmpty(rarityScript.stat2))
            {
                stat2ValueText.text = $"+{rarityScript.stat2Value}";
            }
            else
            {
                stat2ValueText.text = "";
            }
        }
        
        // Обновляем стат 3
        if (stat3Text != null)
        {
            if (!string.IsNullOrEmpty(rarityScript.stat3))
            {
                stat3Text.text = rarityScript.stat3;
            }
            else
            {
                stat3Text.text = "";
            }
        }
        
        // Обновляем значение стата 3
        if (stat3ValueText != null)
        {
            if (!string.IsNullOrEmpty(rarityScript.stat3))
            {
                stat3ValueText.text = $"+{rarityScript.stat3Value}";
            }
            else
            {
                stat3ValueText.text = "";
            }
        }
        
        // Обновляем стат 4
        if (stat4Text != null)
        {
            if (!string.IsNullOrEmpty(rarityScript.stat4))
            {
                stat4Text.text = rarityScript.stat4;
            }
            else
            {
                stat4Text.text = "";
            }
        }
        
        // Обновляем значение стата 4
        if (stat4ValueText != null)
        {
            if (!string.IsNullOrEmpty(rarityScript.stat4))
            {
                stat4ValueText.text = $"+{rarityScript.stat4Value}";
            }
            else
            {
                stat4ValueText.text = "";
            }
        }
        
        // Обновляем стат 5
        if (stat5Text != null)
        {
            if (!string.IsNullOrEmpty(rarityScript.stat5))
            {
                stat5Text.text = rarityScript.stat5;
            }
            else
            {
                stat5Text.text = "";
            }
        }
        
        // Обновляем значение стата 5
        if (stat5ValueText != null)
        {
            if (!string.IsNullOrEmpty(rarityScript.stat5))
            {
                stat5ValueText.text = $"+{rarityScript.stat5Value}";
            }
            else
            {
                stat5ValueText.text = "";
            }
        }
    }
    
    private void ClearStatsDisplay()
    {
        if (stat1Text != null) stat1Text.text = "";
        if (stat2Text != null) stat2Text.text = "";
        if (stat3Text != null) stat3Text.text = "";
        if (stat4Text != null) stat4Text.text = "";
        if (stat5Text != null) stat5Text.text = "";
        
        if (stat1ValueText != null) stat1ValueText.text = "";
        if (stat2ValueText != null) stat2ValueText.text = "";
        if (stat3ValueText != null) stat3ValueText.text = "";
        if (stat4ValueText != null) stat4ValueText.text = "";
        if (stat5ValueText != null) stat5ValueText.text = "";
    }
    
    private void UpdateObjectTextDisplay()
    {
        if (objectTextDisplay != null && currentDetectedCollider != null)
        {
            // Ищем компонент Text на обнаруженном объекте
            Text objectText = currentDetectedCollider.GetComponent<Text>();
            if (objectText != null)
            {
                objectTextDisplay.text = objectText.text;
            }
            else
            {
                objectTextDisplay.text = "";
            }
        }
    }
    
    private void ClearObjectTextDisplay()
    {
        if (objectTextDisplay != null)
        {
            objectTextDisplay.text = "";
        }
    }
    
    private void HandleMouseControl()
    {
        if (mouse == null || controlledObject == null) return;
        
        bool leftMousePressed = mouse.leftButton.isPressed;
        
        if (leftMousePressed)
        {
            // Левая кнопка мыши зажата
            if (!isMouseHeld)
            {
                // Начинаем отсчет времени удержания
                isMouseHeld = true;
                mouseHoldTimer = 0f;
            }
            else
            {
                // Увеличиваем таймер удержания
                mouseHoldTimer += Time.deltaTime;
                
                // Если удержали достаточно долго - включаем объект
                if (mouseHoldTimer >= holdTimeRequired)
                {
                    controlledObject.SetActive(true);
                }
            }
        }
        else
        {
            // Левая кнопка мыши отпущена
            if (isMouseHeld)
            {
                // Сбрасываем состояние и выключаем объект
                isMouseHeld = false;
                mouseHoldTimer = 0f;
                controlledObject.SetActive(false);
            }
        }
    }
}
