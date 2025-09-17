using UnityEngine;

public class AutoCleanupTester : MonoBehaviour
{
    [Header("Тестирование автоматического удаления")]
    public PrefabSpawner spawner;
    public float testInterval = 2f;
    public int maxTestObjects = 10;
    
    private float lastSpawnTime;
    private int spawnedCount = 0;
    
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
        
        // Настраиваем автоматическое удаление
        spawner.SetAutoCleanup(true);
        spawner.SetObjectLifetime(5f); // Объекты живут 5 секунд
        spawner.SetRandomLifetime(false);
        
        Debug.Log("Тест автоматического удаления запущен. Объекты будут удаляться через 5 секунд.");
    }
    
    void Update()
    {
        if (spawner == null) return;
        
        // Спавним объекты каждые testInterval секунд
        if (Time.time - lastSpawnTime >= testInterval && spawnedCount < maxTestObjects)
        {
            spawner.SpawnPrefab();
            spawnedCount++;
            lastSpawnTime = Time.time;
            
            Debug.Log($"Заспавнен тестовый объект #{spawnedCount}. Всего объектов: {spawner.GetSpawnedObjectsCount()}");
        }
        
        // Показываем статистику каждые 2 секунды
        if (Time.frameCount % 120 == 0) // Примерно каждые 2 секунды при 60 FPS
        {
            Debug.Log($"Статистика: Заспавнено {spawnedCount} объектов, Активных: {spawner.GetSpawnedObjectsCount()}");
        }
    }
    
    [ContextMenu("Очистить все объекты")]
    public void ClearAllObjects()
    {
        if (spawner != null)
        {
            spawner.ClearAllSpawnedObjects();
            spawnedCount = 0;
            Debug.Log("Все объекты очищены");
        }
    }
    
    [ContextMenu("Изменить время жизни на 3 секунды")]
    public void SetShortLifetime()
    {
        if (spawner != null)
        {
            spawner.SetObjectLifetime(3f);
            Debug.Log("Время жизни объектов изменено на 3 секунды");
        }
    }
    
    [ContextMenu("Включить случайное время жизни")]
    public void EnableRandomLifetime()
    {
        if (spawner != null)
        {
            spawner.SetRandomLifetime(true, 2f, 8f);
            Debug.Log("Включено случайное время жизни: 2-8 секунд");
        }
    }
}