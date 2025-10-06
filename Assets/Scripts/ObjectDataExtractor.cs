using UnityEngine;

public class ObjectDataExtractor : MonoBehaviour
{
    [Header("Random Detection Settings")]
    [SerializeField] private string targetTag = "obj"; // Тег для поиска
    [SerializeField] private float detectionInterval = 1f; // Интервал поиска в секундах
    [SerializeField] private bool showDebugInfo = true;
    
    
    [Header("Extracted Data")]
    [SerializeField] private string foundObjectName = "None";
    [SerializeField] private string objectRarity = "";
    
    // Данные характеристик объекта (как в SimpleMouseDetector)
    [SerializeField] private string stat1 = "";
    [SerializeField] private int stat1Value = 0;
    [SerializeField] private string stat2 = "";
    [SerializeField] private int stat2Value = 0;
    [SerializeField] private string stat3 = "";
    [SerializeField] private int stat3Value = 0;
    [SerializeField] private string stat4 = "";
    [SerializeField] private int stat4Value = 0;
    [SerializeField] private string stat5 = "";
    [SerializeField] private int stat5Value = 0;
    
    
    private float lastDetectionTime = 0f;
    private GameObject currentFoundObject;
    private Collider2D currentFoundCollider;
    
    void Start()
    {
        if (showDebugInfo)
        {
            Debug.Log($"ObjectDataExtractor initialized. Target Tag: {targetTag}");
        }
    }
    
    void Update()
    {
        // Проверяем, прошло ли достаточно времени с последнего поиска
        if (Time.time - lastDetectionTime >= detectionInterval)
        {
            FindRandomObjectOnScene();
            lastDetectionTime = Time.time;
        }
    }
    
    // Поиск случайного объекта с тегом на сцене
    public void FindRandomObjectOnScene()
    {
        // Находим все объекты с нужным тегом
        GameObject[] allObjects = GameObject.FindGameObjectsWithTag(targetTag);
        
        if (allObjects.Length > 0)
        {
            // Выбираем случайный объект
            int randomIndex = Random.Range(0, allObjects.Length);
            GameObject randomObject = allObjects[randomIndex];
            
            // Получаем коллайдер объекта
            Collider2D collider = randomObject.GetComponent<Collider2D>();
            
            // Извлекаем данные
            ExtractObjectData(randomObject, collider);
            
            if (showDebugInfo)
            {
                Debug.Log($"Found random object: {randomObject.name} (index {randomIndex} of {allObjects.Length})");
            }
        }
        else
        {
            ClearObjectData();
            foundObjectName = "No objects found";
            
            if (showDebugInfo)
            {
                Debug.Log($"No objects with tag '{targetTag}' found on scene");
            }
        }
    }
    
    // Извлечение всех данных объекта (как в SimpleMouseDetector)
    private void ExtractObjectData(GameObject obj, Collider2D collider)
    {
        currentFoundObject = obj;
        currentFoundCollider = collider;
        
        // Основные данные объекта
        foundObjectName = obj.name;
        
        // Проверяем наличие скрипта RandomRarityOnSpawn
        RandomRarityOnSpawn rarityScript = obj.GetComponent<RandomRarityOnSpawn>();
        
        if (rarityScript != null)
        {
            // Извлекаем данные редкости
            objectRarity = rarityScript.AssignedRarity.ToString();
            
            // Извлекаем характеристики (как в SimpleMouseDetector)
            stat1 = rarityScript.stat1;
            stat1Value = rarityScript.stat1Value;
            stat2 = rarityScript.stat2;
            stat2Value = rarityScript.stat2Value;
            stat3 = rarityScript.stat3;
            stat3Value = rarityScript.stat3Value;
            stat4 = rarityScript.stat4;
            stat4Value = rarityScript.stat4Value;
            stat5 = rarityScript.stat5;
            stat5Value = rarityScript.stat5Value;
        }
        else
        {
            // Сбрасываем данные редкости если скрипт не найден
            objectRarity = "";
            ClearStatsData();
        }
        
        
        if (showDebugInfo)
        {
            Debug.Log($"Extracted data from object '{foundObjectName}': Rarity={objectRarity}, Stats={stat1}(+{stat1Value}), {stat2}(+{stat2Value}), {stat3}(+{stat3Value}), {stat4}(+{stat4Value}), {stat5}(+{stat5Value})");
        }
    }
    
    
    // Очистка данных объекта
    private void ClearObjectData()
    {
        currentFoundObject = null;
        currentFoundCollider = null;
        foundObjectName = "None";
        ClearStatsData();
    }
    
    // Очистка данных характеристик
    private void ClearStatsData()
    {
        stat1 = ""; stat1Value = 0;
        stat2 = ""; stat2Value = 0;
        stat3 = ""; stat3Value = 0;
        stat4 = ""; stat4Value = 0;
        stat5 = ""; stat5Value = 0;
    }
    
    // Публичные методы для доступа к данным
    
    // Поиск случайного объекта с тегом на сцене
    public GameObject FindRandomObjectWithTag()
    {
        GameObject[] allObjects = GameObject.FindGameObjectsWithTag(targetTag);
        
        if (allObjects.Length > 0)
        {
            int randomIndex = Random.Range(0, allObjects.Length);
            GameObject randomObject = allObjects[randomIndex];
            
            Collider2D collider = randomObject.GetComponent<Collider2D>();
            ExtractObjectData(randomObject, collider);
            
            return randomObject;
        }
        
        ClearObjectData();
        return null;
    }
    
    // Поиск всех объектов с тегом "obj" на сцене
    public GameObject[] FindAllObjectsWithTag()
    {
        GameObject[] allObjects = GameObject.FindGameObjectsWithTag(targetTag);
        return allObjects;
    }
    
    
    // Получение всех извлеченных данных в виде структуры
    public ObjectData GetExtractedData()
    {
        return new ObjectData
        {
            Name = foundObjectName,
            Stat1 = stat1,
            Stat1Value = stat1Value,
            Stat2 = stat2,
            Stat2Value = stat2Value,
            Stat3 = stat3,
            Stat3Value = stat3Value,
            Stat4 = stat4,
            Stat4Value = stat4Value,
            Stat5 = stat5,
            Stat5Value = stat5Value,
            GameObject = currentFoundObject,
            Collider = currentFoundCollider
        };
    }
    
    // Структура для хранения всех данных объекта
    [System.Serializable]
    public struct ObjectData
    {
        public string Name;
        public string Stat1;
        public int Stat1Value;
        public string Stat2;
        public int Stat2Value;
        public string Stat3;
        public int Stat3Value;
        public string Stat4;
        public int Stat4Value;
        public string Stat5;
        public int Stat5Value;
        public GameObject GameObject;
        public Collider2D Collider;
    }
    
    // Методы для управления обнаружением
    public void SetDetectionInterval(float interval)
    {
        detectionInterval = interval;
    }
    
    
    
    // Контекстные меню для тестирования
    [ContextMenu("Find Random Object with Tag")]
    public void TestFindRandomObject()
    {
        GameObject obj = FindRandomObjectWithTag();
        if (obj != null)
        {
            Debug.Log($"Found random object: {obj.name}");
        }
        else
        {
            Debug.Log("No objects with target tag found");
        }
    }
    
    [ContextMenu("Find All Objects with Tag")]
    public void TestFindAllObjects()
    {
        GameObject[] objects = FindAllObjectsWithTag();
        Debug.Log($"Found {objects.Length} objects with target tag");
        foreach (GameObject obj in objects)
        {
            Debug.Log($"- {obj.name}");
        }
    }
    
    [ContextMenu("Show Current Data")]
    public void ShowCurrentData()
    {
        ObjectData data = GetExtractedData();
        Debug.Log($"Current Object Data:\n" +
                 $"Name: {data.Name}\n" +
                 $"Stats: {data.Stat1}(+{data.Stat1Value}), {data.Stat2}(+{data.Stat2Value}), {data.Stat3}(+{data.Stat3Value}), {data.Stat4}(+{data.Stat4Value}), {data.Stat5}(+{data.Stat5Value})");
    }
}
