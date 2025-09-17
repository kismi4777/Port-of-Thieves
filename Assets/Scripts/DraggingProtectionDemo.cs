using UnityEngine;

public class DraggingProtectionDemo : MonoBehaviour
{
    [Header("Демонстрация защиты от удаления при перетаскивании")]
    public PrefabSpawner spawner;
    public GameObject testPrefab;
    
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
        spawner.SetDropZoneProtection(true);
        spawner.SetAutoCleanup(true);
        spawner.SetObjectLifetime(5f);
        
        Debug.Log("=== ДЕМОНСТРАЦИЯ ЗАЩИТЫ ОТ УДАЛЕНИЯ ПРИ ПЕРЕТАСКИВАНИИ ===");
        Debug.Log("1. Создаем тестовый объект");
        CreateTestObject();
        
        Debug.Log("2. Объект будет автоматически удален через 5 секунд, ЕСЛИ он не перетаскивается И не в drop zone");
        Debug.Log("3. Нажмите ПРОБЕЛ для симуляции взятия объекта (защита от удаления)");
        Debug.Log("4. Нажмите R для симуляции отпускания объекта (можно удалить)");
        Debug.Log("5. Нажмите D для симуляции помещения в drop zone (защита от удаления)");
        Debug.Log("6. Нажмите F для симуляции выхода из drop zone (можно удалить)");
        Debug.Log("7. Нажмите T для создания нового объекта");
    }
    
    void Update()
    {
        if (spawner == null) return;
        
        // Управление симуляцией перетаскивания
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SimulateObjectPickup();
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            SimulateObjectDrop();
        }
        
        if (Input.GetKeyDown(KeyCode.D))
        {
            SimulateObjectInDropZone();
        }
        
        if (Input.GetKeyDown(KeyCode.F))
        {
            SimulateObjectOutOfDropZone();
        }
        
        if (Input.GetKeyDown(KeyCode.T))
        {
            CreateTestObject();
        }
        
        // Показываем статистику каждые 2 секунды
        if (Time.frameCount % 120 == 0) // Примерно каждые 2 секунды при 60 FPS
        {
            int draggedCount = spawner.GetDraggedObjectsCount();
            int inDropZoneCount = spawner.GetObjectsInDropZoneCount();
            int totalCount = spawner.GetSpawnedObjectsCount();
            Debug.Log($"Статистика: Всего объектов: {totalCount}, Перетаскиваемых: {draggedCount}, В drop zone: {inDropZoneCount}");
        }
    }
    
    void CreateTestObject()
    {
        if (testPrefab != null)
        {
            GameObject testObj = Instantiate(testPrefab, transform.position, Quaternion.identity);
            testObj.name = "DraggingTestObject";
            spawner.SpawnPrefabAtPosition(testObj.transform.position);
        }
        else
        {
            // Создаем простой куб для тестирования
            GameObject testObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            testObj.transform.position = transform.position;
            testObj.name = "DraggingTestCube";
            spawner.SpawnPrefabAtPosition(testObj.transform.position);
        }
        
        Debug.Log("✅ Создан новый тестовый объект");
    }
    
    void SimulateObjectPickup()
    {
        // Находим последний созданный объект
        var spawnedObjects = FindObjectsOfType<GameObject>();
        GameObject lastObject = null;
        
        foreach (var obj in spawnedObjects)
        {
            if (obj.name.Contains("DraggingTest"))
            {
                lastObject = obj;
            }
        }
        
        if (lastObject != null)
        {
            spawner.MarkObjectAsDragging(lastObject);
            Debug.Log("🔄 ОБЪЕКТ ВЗЯТ ДЛЯ ПЕРЕТАСКИВАНИЯ - он НЕ будет удален автоматически!");
        }
        else
        {
            Debug.LogWarning("Нет объектов для взятия!");
        }
    }
    
    void SimulateObjectDrop()
    {
        // Находим последний созданный объект
        var spawnedObjects = FindObjectsOfType<GameObject>();
        GameObject lastObject = null;
        
        foreach (var obj in spawnedObjects)
        {
            if (obj.name.Contains("DraggingTest"))
            {
                lastObject = obj;
            }
        }
        
        if (lastObject != null)
        {
            spawner.MarkObjectAsDropped(lastObject);
            Debug.Log("⏹️ ОБЪЕКТ ОТПУЩЕН - теперь он может быть удален автоматически");
        }
        else
        {
            Debug.LogWarning("Нет объектов для отпускания!");
        }
    }
    
    [ContextMenu("Создать новый тестовый объект")]
    public void CreateNewTestObject()
    {
        CreateTestObject();
    }
    
    [ContextMenu("Симулировать взятие объекта")]
    public void SimulatePickupFromMenu()
    {
        SimulateObjectPickup();
    }
    
    [ContextMenu("Симулировать отпускание объекта")]
    public void SimulateDropFromMenu()
    {
        SimulateObjectDrop();
    }
    
    void SimulateObjectInDropZone()
    {
        // Находим последний созданный объект
        var spawnedObjects = FindObjectsOfType<GameObject>();
        GameObject lastObject = null;
        
        foreach (var obj in spawnedObjects)
        {
            if (obj.name.Contains("DraggingTest"))
            {
                lastObject = obj;
            }
        }
        
        if (lastObject != null)
        {
            spawner.MarkObjectAsInDropZone(lastObject);
            Debug.Log("🏠 ОБЪЕКТ ПОМЕЩЕН В DROP ZONE - он НЕ будет удален автоматически!");
        }
        else
        {
            Debug.LogWarning("Нет объектов для помещения в drop zone!");
        }
    }
    
    void SimulateObjectOutOfDropZone()
    {
        // Находим последний созданный объект
        var spawnedObjects = FindObjectsOfType<GameObject>();
        GameObject lastObject = null;
        
        foreach (var obj in spawnedObjects)
        {
            if (obj.name.Contains("DraggingTest"))
            {
                lastObject = obj;
            }
        }
        
        if (lastObject != null)
        {
            spawner.MarkObjectAsOutOfDropZone(lastObject);
            Debug.Log("🚪 ОБЪЕКТ ВЫНЕСЕН ИЗ DROP ZONE - теперь он может быть удален автоматически");
        }
        else
        {
            Debug.LogWarning("Нет объектов для вынесения из drop zone!");
        }
    }
    
    [ContextMenu("Симулировать помещение в drop zone")]
    public void SimulateInDropZoneFromMenu()
    {
        SimulateObjectInDropZone();
    }
    
    [ContextMenu("Симулировать выход из drop zone")]
    public void SimulateOutOfDropZoneFromMenu()
    {
        SimulateObjectOutOfDropZone();
    }
    
    [ContextMenu("Показать статистику")]
    public void ShowStats()
    {
        if (spawner != null)
        {
            int draggedCount = spawner.GetDraggedObjectsCount();
            int inDropZoneCount = spawner.GetObjectsInDropZoneCount();
            int totalCount = spawner.GetSpawnedObjectsCount();
            Debug.Log($"Статистика: Всего объектов: {totalCount}, Перетаскиваемых: {draggedCount}, В drop zone: {inDropZoneCount}");
        }
    }
}
