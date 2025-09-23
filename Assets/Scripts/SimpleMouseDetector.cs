using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class SimpleMouseDetector : MonoBehaviour
{
    [Header("Object Detection")]
    [SerializeField] public string detectedObjectName = "None";
    [SerializeField] public bool hasRandomRarityScript = false;
    
    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 0.3f;
    [SerializeField] private float hideDelayMs = 100f; // Задержка в миллисекундах
    
    [Header("UI Display")]
    [SerializeField] private TextMeshProUGUI displayText;
    [SerializeField] private TextMeshProUGUI rarityText;
    
    private Camera mainCamera;
    private Mouse mouse;
    private float hideTimer = 0f;
    private bool isObjectDetected = false;
    private string lastDisplayedName = "None";
    private string lastRarity = "";
    
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
            
            // Проверяем наличие скрипта RandomRarityOnSpawn
            RandomRarityOnSpawn rarityScript = hitCollider.GetComponent<RandomRarityOnSpawn>();
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
            if (hasRandomRarityScript && detectedObjectName != "None")
            {
                // Получаем редкость из скрипта RandomRarityOnSpawn
                GameObject detectedObject = GameObject.Find(detectedObjectName);
                if (detectedObject != null)
                {
                    RandomRarityOnSpawn rarityScript = detectedObject.GetComponent<RandomRarityOnSpawn>();
                    if (rarityScript != null)
                    {
                        rarityText.text = rarityScript.AssignedRarity.ToString();
                        
                        // Устанавливаем цвет текста на основе цвета редкости
                        Color rarityColor = rarityScript.AssignedColor;
                        displayText.color = rarityColor;
                        rarityText.color = rarityColor;
                    }
                    else
                    {
                        rarityText.text = "";
                        // Сбрасываем цвет на белый если скрипт не найден
                        displayText.color = Color.white;
                        rarityText.color = Color.white;
                    }
                }
                else
                {
                    rarityText.text = "";
                    // Сбрасываем цвет на белый если объект не найден
                    displayText.color = Color.white;
                    rarityText.color = Color.white;
                }
            }
            else
            {
                rarityText.text = "";
                // Сбрасываем цвет на белый если нет скрипта или объект не обнаружен
                displayText.color = Color.white;
                rarityText.color = Color.white;
            }
        }
    }
}
