using UnityEngine;

public class MovementTestDemo : MonoBehaviour
{
    [Header("Демонстрация системы отслеживания движения")]
    public PrefabSpawner spawner;
    public GameObject testPrefab;
    public float testDuration = 10f;
    
    private GameObject testObject;
    private bool isMoving = false;
    private Vector3 startPosition;
    
    void Start()
    {
        if (spawner == null)
        {
            spawner = FindObjectOfType<PrefabSpawner>();
        }
        
        if (spawner == null)
        {
            Debug.LogError("PrefabSpawner не найден!");
            enabled = false;
            return;
        }
        
        // Настраиваем spawner для демонстрации
        spawner.SetDraggingProtection(true);
        spawner.SetAutoCleanup(true);
        spawner.SetObjectLifetime(5f);
        
        Debug.Log("=== ДЕМОНСТРАЦИЯ СИСТЕМЫ ОТСЛЕЖИВАНИЯ ДВИЖЕНИЯ ===");
        Debug.Log("1. Создаем тестовый объект");
        CreateTestObject();
        
        Debug.Log("2. Объект будет автоматически удален через 5 секунд, ЕСЛИ он не перетаскивается");
        Debug.Log("3. Нажмите ПРОБЕЛ для симуляции взятия объекта");
        Debug.Log("4. Нажмите R для симуляции отпускания объекта");
    }
    
    void Update()
    {
        if (testObject == null) return;
        
        // Управление симуляцией перетаскивания
        if (Input.GetKeyDown(KeyCode.Space) && !isMoving)
        {
            StartDragging();
        }
        
        if (Input.GetKeyDown(KeyCode.R) && isMoving)
        {
            StopDragging();
        }
        
        // Движение объекта при симуляции перетаскивания
        if (isMoving)
        {
            testObject.transform.position += Vector3.right * Time.deltaTime * 2f;
        }
        
        // Показываем статистику
        if (Time.frameCount % 60 == 0) // Каждую секунду
        {
            int draggedCount = spawner.GetDraggedObjectsCount();
            int totalCount = spawner.GetSpawnedObjectsCount();
            Debug.Log($"Статистика: Всего объектов: {totalCount}, Перетаскиваемых: {draggedCount}");
        }
    }
    
    void CreateTestObject()
    {
        if (testPrefab != null)
        {
            testObject = Instantiate(testPrefab, transform.position, Quaternion.identity);
            testObject.name = "MovementTestObject";
            startPosition = testObject.transform.position;
            
            // Добавляем объект в spawner для отслеживания
            spawner.SpawnPrefabAtPosition(testObject.transform.position);
        }
        else
        {
            // Создаем простой куб для тестирования
            testObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            testObject.transform.position = transform.position;
            testObject.name = "MovementTestCube";
            startPosition = testObject.transform.position;
            
            // Добавляем объект в spawner для отслеживания
            spawner.SpawnPrefabAtPosition(testObject.transform.position);
        }
    }
    
    void StartDragging()
    {
        isMoving = true;
        if (spawner != null)
        {
            spawner.MarkObjectAsDragging(testObject);
        }
        Debug.Log("🔄 ОБЪЕКТ ВЗЯТ ДЛЯ ПЕРЕТАСКИВАНИЯ - он НЕ будет удален автоматически!");
    }
    
    void StopDragging()
    {
        isMoving = false;
        if (spawner != null)
        {
            spawner.MarkObjectAsDropped(testObject);
        }
        Debug.Log("⏹️ ОБЪЕКТ ОТПУЩЕН - теперь он может быть удален автоматически");
    }
    
    [ContextMenu("Создать новый тестовый объект")]
    public void CreateNewTestObject()
    {
        if (testObject != null)
        {
            DestroyImmediate(testObject);
        }
        CreateTestObject();
    }
    
    [ContextMenu("Начать перетаскивание")]
    public void StartDraggingFromMenu()
    {
        StartDragging();
    }
    
    [ContextMenu("Остановить перетаскивание")]
    public void StopDraggingFromMenu()
    {
        StopDragging();
    }
}
