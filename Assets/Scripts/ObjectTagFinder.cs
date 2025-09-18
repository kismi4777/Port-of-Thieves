using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectTagFinder : MonoBehaviour
{
    [Header("Object Tag Detection")]
    public string targetTag = "Draggable"; // Тег для поиска
    public string foundObjectName = "None"; // Имя найденного объекта
    
    [Header("Detection Settings")]
    public float detectionRadius = 0.1f; // Радиус поиска
    public bool continuousDetection = true; // Непрерывный поиск
    
    private Camera mainCamera;
    private Mouse mouse;
    
    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
        
        // Инициализируем Input System
        mouse = Mouse.current;
        
        Debug.Log($"ObjectTagFinder initialized. Camera: {mainCamera.name}, Target Tag: {targetTag}");
    }
    
    void Update()
    {
        if (continuousDetection && mainCamera != null)
        {
            DetectObjectAtMousePosition();
        }
    }
    
    // Основная механика поиска объекта по тегу (взята из CursorTagDetector)
    public void DetectObjectAtMousePosition()
    {
        if (mouse == null) return;
        
        Vector2 mousePosition = mouse.position.ReadValue();
        
        // Конвертируем позицию мыши в мировые координаты (механика из CursorTagDetector)
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
        
        // Ищем коллайдеры в радиусе (механика из CursorTagDetector)
        Collider2D[] colliders = Physics2D.OverlapCircleAll(new Vector2(worldPosition.x, worldPosition.y), detectionRadius);
        
        if (colliders.Length > 0)
        {
            // Проверяем каждый найденный коллайдер на соответствие тегу
            foreach (Collider2D collider in colliders)
            {
                if (collider.CompareTag(targetTag))
                {
                    foundObjectName = collider.name;
                    Debug.Log($"Found object with tag '{targetTag}': {foundObjectName}");
                    return; // Найден первый объект с нужным тегом
                }
            }
            
            // Если объекты найдены, но ни один не соответствует тегу
            foundObjectName = "No matching tag";
        }
        else
        {
            foundObjectName = "No objects found";
        }
    }
    
    // Публичный метод для поиска объекта в определенной позиции
    public string FindObjectAtPosition(Vector3 worldPosition)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(new Vector2(worldPosition.x, worldPosition.y), detectionRadius);
        
        if (colliders.Length > 0)
        {
            foreach (Collider2D collider in colliders)
            {
                if (collider.CompareTag(targetTag))
                {
                    return collider.name;
                }
            }
        }
        
        return "None";
    }
    
    // Публичный метод для поиска всех объектов с нужным тегом на сцене
    public GameObject[] FindAllObjectsWithTag()
    {
        GameObject[] allObjects = GameObject.FindGameObjectsWithTag(targetTag);
        return allObjects;
    }
    
    // Публичный метод для поиска первого объекта с нужным тегом на сцене
    public GameObject FindFirstObjectWithTag()
    {
        GameObject foundObject = GameObject.FindGameObjectWithTag(targetTag);
        if (foundObject != null)
        {
            foundObjectName = foundObject.name;
        }
        else
        {
            foundObjectName = "None";
        }
        return foundObject;
    }
}
