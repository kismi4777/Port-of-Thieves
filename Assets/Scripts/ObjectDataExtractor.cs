using UnityEngine;
using TMPro;

public class ObjectDataExtractor : MonoBehaviour
{
    [Header("Random Detection Settings")]
    [SerializeField] private string targetTag = "obj"; // Тег для поиска
    [SerializeField] private bool showDebugInfo = true;
    
    
    [Header("Extracted Data")]
    [SerializeField] private string foundObjectName = "None";
    [SerializeField] private string objectRarity = "";
    
    // Данные характеристик объекта (объединенные stat + value)
    [SerializeField] private string stat1Combined = "";
    [SerializeField] private string stat2Combined = "";
    [SerializeField] private string stat3Combined = "";
    [SerializeField] private string stat4Combined = "";
    [SerializeField] private string stat5Combined = "";
    
    [Header("TextMeshPro References")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI rarityText;
    public TextMeshProUGUI stat1Text;
    public TextMeshProUGUI stat2Text;
    
    private GameObject currentFoundObject;
    private Collider2D currentFoundCollider;
    
    // Публичное свойство для доступа к имени найденного объекта
    public string FoundObjectName => foundObjectName;
    
    void OnEnable()
    {
        // Находим случайный объект с тегом при включении скрипта
        FindRandomObjectOnScene();
    }
    
    void OnDisable()
    {
        // Сбрасываем все данные при отключении скрипта
        ClearObjectData();
        
        if (showDebugInfo)
        {
            Debug.Log("ObjectDataExtractor disabled, all data cleared");
        }
    }
    
    
    // Поиск случайного объекта с тегом на сцене (выполняется только один раз)
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
                
                // Проверяем наличие RandomRarityOnSpawn компонента
                RandomRarityOnSpawn rarityScript = randomObject.GetComponent<RandomRarityOnSpawn>();
                if (rarityScript != null)
                {
                    Debug.Log($"RandomRarityOnSpawn component found on {randomObject.name}");
                    Debug.Log($"Rarity: {rarityScript.AssignedRarity}");
                    Debug.Log($"Stat1: {rarityScript.stat1} + {rarityScript.stat1Value}");
                }
                else
                {
                    Debug.LogWarning($"RandomRarityOnSpawn component NOT found on {randomObject.name}");
                }
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
            
            // Извлекаем характеристики (объединяем stat + value)
            stat1Combined = $"{rarityScript.stat1} + {rarityScript.stat1Value}";
            stat2Combined = $"{rarityScript.stat2} + {rarityScript.stat2Value}";
            stat3Combined = $"{rarityScript.stat3} + {rarityScript.stat3Value}";
            stat4Combined = $"{rarityScript.stat4} + {rarityScript.stat4Value}";
            stat5Combined = $"{rarityScript.stat5} + {rarityScript.stat5Value}";
        }
        else
        {
            // Сбрасываем данные редкости если скрипт не найден
            objectRarity = "";
            ClearStatsData();
        }
        
        
        if (showDebugInfo)
        {
            Debug.Log($"Extracted data from object '{foundObjectName}': Rarity={objectRarity}");
            Debug.Log($"Stat1: {stat1Combined}");
            Debug.Log($"Stat2: {stat2Combined}");
            Debug.Log($"Stat3: {stat3Combined}");
            Debug.Log($"Stat4: {stat4Combined}");
            Debug.Log($"Stat5: {stat5Combined}");
        }
        
        // Обновляем TextMeshPro поля
        UpdateTextMeshProFields();
        
        // Принудительно обновляем Inspector
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
    
    
    // Очистка данных объекта
    private void ClearObjectData()
    {
        currentFoundObject = null;
        currentFoundCollider = null;
        foundObjectName = "None";
        objectRarity = "";
        ClearStatsData();
        
        // Очищаем TextMeshPro поля
        ClearTextMeshProFields();
    }
    
    // Очистка данных характеристик
    private void ClearStatsData()
    {
        stat1Combined = "";
        stat2Combined = "";
        stat3Combined = "";
        stat4Combined = "";
        stat5Combined = "";
    }
    
    // Обновление TextMeshPro полей данными объекта
    private void UpdateTextMeshProFields()
    {
        // nameText всегда содержит имя объекта
        if (nameText != null)
        {
            nameText.text = foundObjectName;
        }
        
        // rarityText содержит редкость (после имени)
        if (rarityText != null)
        {
            rarityText.text = objectRarity;
        }
        
        // stat1Text содержит первую характеристику (только если не пустая и не "+0")
        if (stat1Text != null)
        {
            stat1Text.text = IsValidStat(stat1Combined) ? stat1Combined : "";
        }
        
        // stat2Text содержит вторую характеристику (только если не пустая и не "+0")
        if (stat2Text != null)
        {
            stat2Text.text = IsValidStat(stat2Combined) ? stat2Combined : "";
        }
    }
    
    // Проверка валидности стата (не пустой и не "+0")
    private bool IsValidStat(string stat)
    {
        return !string.IsNullOrEmpty(stat) && stat != "+0" && stat != " + 0";
    }
    
    // Очистка TextMeshPro полей
    private void ClearTextMeshProFields()
    {
        if (nameText != null) nameText.text = "";
        if (rarityText != null) rarityText.text = "";
        if (stat1Text != null) stat1Text.text = "";
        if (stat2Text != null) stat2Text.text = "";
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
            Stat1Combined = stat1Combined,
            Stat2Combined = stat2Combined,
            Stat3Combined = stat3Combined,
            Stat4Combined = stat4Combined,
            Stat5Combined = stat5Combined,
            GameObject = currentFoundObject,
            Collider = currentFoundCollider
        };
    }
    
    // Структура для хранения всех данных объекта
    [System.Serializable]
    public struct ObjectData
    {
        public string Name;
        public string Stat1Combined;
        public string Stat2Combined;
        public string Stat3Combined;
        public string Stat4Combined;
        public string Stat5Combined;
        public GameObject GameObject;
        public Collider2D Collider;
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
                 $"Stats: {data.Stat1Combined}, {data.Stat2Combined}, {data.Stat3Combined}, {data.Stat4Combined}, {data.Stat5Combined}");
    }
}
