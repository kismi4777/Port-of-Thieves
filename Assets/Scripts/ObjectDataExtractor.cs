using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ObjectDataExtractor : MonoBehaviour
{
    [Header("Random Detection Settings")]
    [SerializeField] private string targetTag = "obj"; // Тег для поиска
    [SerializeField] private bool showDebugInfo = true;
    
    [Header("Zone 1 Settings (ОБЯЗАТЕЛЬНО)")]
    [SerializeField] private CursorTagDetector cursorTagDetector; // Ссылка на CursorTagDetector для получения настроек зоны 1
    [Tooltip("Поиск объектов ТОЛЬКО с тегом 'obj' И внутри Zone 1")]
    [SerializeField] private bool showZone1Info = true; // Показывать информацию о зоне 1 в консоли
    
    [Header("Deception Settings")]
    [SerializeField] public bool isDeceptionActive = false; // Показатель обмана
    [SerializeField] [Range(0f, 100f)] public float deceptionChance = 5f; // Шанс активации обмана в процентах
    
    // Ложные данные для отображения при обмане
    private string fakeRarity = "";
    private string fakeStat1Combined = "";
    private string fakeStat2Combined = "";
    
    
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
    
    void Start()
    {
        // Автоматически находим CursorTagDetector в сцене, если он не назначен
        if (cursorTagDetector == null)
        {
            cursorTagDetector = FindObjectOfType<CursorTagDetector>();
            if (cursorTagDetector != null)
            {
                Debug.Log("CursorTagDetector автоматически найден в сцене");
            }
            else
            {
                Debug.LogWarning("CursorTagDetector не найден в сцене! Назначьте его вручную в Inspector.");
            }
        }
    }
    
    // Проверяет, находится ли объект в зоне 1 (ОБЯЗАТЕЛЬНОЕ УСЛОВИЕ)
    private bool IsObjectInZone1(GameObject obj)
    {
        if (cursorTagDetector == null)
        {
            Debug.LogError("CursorTagDetector не назначен! Невозможно определить Zone 1. Объект не может быть найден.");
            return false;
        }
        
        Vector3 position = obj.transform.position;
        Vector2 zone1Center = cursorTagDetector.zone1Center;
        Vector2 zone1Size = cursorTagDetector.zone1Size;
        
        float minX = zone1Center.x - zone1Size.x / 2f;
        float maxX = zone1Center.x + zone1Size.x / 2f;
        float minY = zone1Center.y - zone1Size.y / 2f;
        float maxY = zone1Center.y + zone1Size.y / 2f;
        
        bool isInZone = position.x >= minX && position.x <= maxX && 
                        position.y >= minY && position.y <= maxY;
        
        if (showZone1Info && showDebugInfo)
        {
            string bounds = $"Zone1 границы: X[{minX:F2} - {maxX:F2}], Y[{minY:F2} - {maxY:F2}]";
            string objPos = $"Объект '{obj.name}' позиция: ({position.x:F2}, {position.y:F2})";
            string result = isInZone ? "✓ В ЗОНЕ" : "✗ ВНЕ ЗОНЫ";
            Debug.Log($"{objPos} | {bounds} | {result}");
        }
        
        return isInZone;
    }
    
    // Генерация ложных данных для обмана
    private void GenerateFakeData()
    {
        // Массив возможных типов редкости (русские названия)
        string[] rarityTypes = { "Обычный", "Необычный", "Редкий", "Эпический", "Легендарный", "Мифический" };
        fakeRarity = rarityTypes[Random.Range(0, rarityTypes.Length)];
        
        // Массив возможных характеристик (русские названия)
        string[] statTypes = { 
            "Физический урон",
            "Магический урон", 
            "Скорость атаки",
            "Критический шанс",
            "Критический урон",
            "Физическая защита",
            "Магическая защита",
            "Сопротивление огню",
            "Сопротивление яду",
            "Блокирование",
            "Поглощение урона",
            "Скорость передвижения",
            "Вампиризм Жизни",
            "Вампиризм Маны",
            "Сопротивление оглушению",
            "Сопротивление замедлению",
            "Отражение урона",
            "Регенерация здоровья",
            "Регенерация маны"
        };
        
        // Генерируем две случайные характеристики
        string stat1 = statTypes[Random.Range(0, statTypes.Length)];
        int value1 = Random.Range(1, 100);
        fakeStat1Combined = $"{stat1} + {value1}";
        
        string stat2 = statTypes[Random.Range(0, statTypes.Length)];
        int value2 = Random.Range(1, 100);
        fakeStat2Combined = $"{stat2} + {value2}";
        
        if (showDebugInfo)
        {
            Debug.Log($"Generated fake data - Rarity: {fakeRarity}, Stat1: {fakeStat1Combined}, Stat2: {fakeStat2Combined}");
        }
    }
    
    void OnEnable()
    {
        // Активируем обман с настраиваемым шансом
        isDeceptionActive = Random.Range(0f, 100f) < deceptionChance;
        
        if (showDebugInfo)
        {
            Debug.Log($"Deception active: {isDeceptionActive} ({deceptionChance}% chance)");
        }
        
        // Генерируем ложные данные если обман активирован
        if (isDeceptionActive)
        {
            GenerateFakeData();
        }
        
        // Находим случайный объект с тегом при включении скрипта
        FindRandomObjectOnScene();
    }
    
    void OnDisable()
    {
        // Сбрасываем все данные при отключении скрипта
        ClearObjectData();
        isDeceptionActive = false;
        ClearFakeData();
        
        if (showDebugInfo)
        {
            Debug.Log("ObjectDataExtractor disabled, all data cleared");
        }
    }
    
    
    // Поиск случайного объекта с тегом на сцене (ТОЛЬКО в Zone 1, выполняется при включении)
    public void FindRandomObjectOnScene()
    {
        // ИСПОЛЬЗУЕМ МЕТОД С ФИЛЬТРАЦИЕЙ ПО ZONE 1
        GameObject randomObject = FindRandomObjectWithTag();
        
        if (randomObject != null && showDebugInfo)
        {
            Debug.Log($"OnEnable: Найден случайный объект в Zone 1: {randomObject.name}");
            
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
        else if (randomObject == null)
        {
            if (showDebugInfo)
            {
                Debug.LogWarning($"OnEnable: Не найдено объектов с тегом '{targetTag}' в Zone 1!");
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
            Debug.Log($"Deception Active: {isDeceptionActive}");
            
            if (isDeceptionActive)
            {
                Debug.Log($"REAL DATA - Stat1: {stat1Combined}, Stat2: {stat2Combined}");
                Debug.Log($"FAKE DATA - Rarity: {fakeRarity}, Stat1: {fakeStat1Combined}, Stat2: {fakeStat2Combined}");
            }
            else
            {
                Debug.Log($"Stat1: {stat1Combined}");
                Debug.Log($"Stat2: {stat2Combined}");
            }
            
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
    
    // Очистка ложных данных
    private void ClearFakeData()
    {
        fakeRarity = "";
        fakeStat1Combined = "";
        fakeStat2Combined = "";
    }
    
    // Обновление TextMeshPro полей данными объекта
    private void UpdateTextMeshProFields()
    {
        // nameText всегда содержит имя объекта
        if (nameText != null)
        {
            nameText.text = foundObjectName;
        }
        
        // rarityText содержит редкость (ложную или реальную в зависимости от обмана)
        if (rarityText != null)
        {
            if (isDeceptionActive && !string.IsNullOrEmpty(fakeRarity))
            {
                rarityText.text = fakeRarity;
            }
            else
            {
                rarityText.text = objectRarity;
            }
        }
        
        // stat1Text содержит первую характеристику (ложную или реальную в зависимости от обмана)
        if (stat1Text != null)
        {
            if (isDeceptionActive && !string.IsNullOrEmpty(fakeStat1Combined))
            {
                stat1Text.text = fakeStat1Combined;
            }
            else
            {
                stat1Text.text = IsValidStat(stat1Combined) ? stat1Combined : "";
            }
        }
        
        // stat2Text содержит вторую характеристику (ложную или реальную в зависимости от обмана)
        if (stat2Text != null)
        {
            if (isDeceptionActive && !string.IsNullOrEmpty(fakeStat2Combined))
            {
                stat2Text.text = fakeStat2Combined;
            }
            else
            {
                stat2Text.text = IsValidStat(stat2Combined) ? stat2Combined : "";
            }
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
    
    // Поиск случайного объекта с тегом на сцене (ТОЛЬКО в Zone 1)
    public GameObject FindRandomObjectWithTag()
    {
        GameObject[] allObjects = GameObject.FindGameObjectsWithTag(targetTag);
        
        if (showDebugInfo)
        {
            Debug.Log($"Найдено {allObjects.Length} объектов с тегом '{targetTag}'. Фильтрация по Zone 1...");
        }
        
        // ОБЯЗАТЕЛЬНАЯ фильтрация объектов по зоне 1
        List<GameObject> objectsInZone1 = new List<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (IsObjectInZone1(obj))
            {
                objectsInZone1.Add(obj);
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"Найдено {objectsInZone1.Count} объектов с тегом '{targetTag}' в Zone 1");
        }
        
        if (objectsInZone1.Count > 0)
        {
            int randomIndex = Random.Range(0, objectsInZone1.Count);
            GameObject randomObject = objectsInZone1[randomIndex];
            
            Collider2D collider = randomObject.GetComponent<Collider2D>();
            ExtractObjectData(randomObject, collider);
            
            if (showDebugInfo)
            {
                Debug.Log($"Выбран случайный объект: {randomObject.name} на позиции {randomObject.transform.position}");
            }
            
            return randomObject;
        }
        
        if (showDebugInfo)
        {
            Debug.LogWarning($"Не найдено объектов с тегом '{targetTag}' в Zone 1!");
        }
        
        ClearObjectData();
        return null;
    }
    
    // Поиск всех объектов с тегом "obj" на сцене (ТОЛЬКО в Zone 1)
    public GameObject[] FindAllObjectsWithTag()
    {
        GameObject[] allObjects = GameObject.FindGameObjectsWithTag(targetTag);
        
        if (showDebugInfo)
        {
            Debug.Log($"Поиск всех объектов с тегом '{targetTag}'. Всего найдено: {allObjects.Length}");
        }
        
        // ОБЯЗАТЕЛЬНАЯ фильтрация объектов по зоне 1
        List<GameObject> objectsInZone1 = new List<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (IsObjectInZone1(obj))
            {
                objectsInZone1.Add(obj);
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"Объектов с тегом '{targetTag}' в Zone 1: {objectsInZone1.Count}");
        }
        
        return objectsInZone1.ToArray();
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
            Collider = currentFoundCollider,
            IsDeceptionActive = isDeceptionActive
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
        public bool IsDeceptionActive;
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
                 $"Deception Active: {data.IsDeceptionActive}\n" +
                 $"Stats: {data.Stat1Combined}, {data.Stat2Combined}, {data.Stat3Combined}, {data.Stat4Combined}, {data.Stat5Combined}");
    }
    
    [ContextMenu("Test Deception - Generate Fake Data")]
    public void TestDeception()
    {
        isDeceptionActive = true;
        GenerateFakeData();
        UpdateTextMeshProFields();
        Debug.Log("Deception activated and fake data generated!");
    }
    
    [ContextMenu("Test Deception - Disable")]
    public void TestDeceptionDisable()
    {
        isDeceptionActive = false;
        UpdateTextMeshProFields();
        Debug.Log("Deception disabled!");
    }
    
    [ContextMenu("Find All Objects in Zone 1")]
    public void TestFindObjectsInZone1()
    {
        GameObject[] objects = FindAllObjectsWithTag();
        Debug.Log($"===== Поиск объектов с тегом '{targetTag}' в Zone 1 =====");
        Debug.Log($"Найдено: {objects.Length} объектов");
        
        foreach (GameObject obj in objects)
        {
            Debug.Log($"✓ {obj.name} на позиции {obj.transform.position}");
        }
        
        if (objects.Length == 0)
        {
            Debug.LogWarning("Объектов не найдено! Проверьте:\n" +
                           "1. Есть ли объекты с тегом 'obj' на сцене\n" +
                           "2. Находятся ли они внутри Zone 1\n" +
                           "3. Назначен ли CursorTagDetector");
        }
    }
    
    [ContextMenu("DEBUG: Show ALL Objects (In & Out of Zone)")]
    public void DebugShowAllObjectsWithStatus()
    {
        GameObject[] allObjects = GameObject.FindGameObjectsWithTag(targetTag);
        
        Debug.Log($"========== ДИАГНОСТИКА ВСЕХ ОБЪЕКТОВ С ТЕГОМ '{targetTag}' ==========");
        Debug.Log($"Всего объектов с тегом: {allObjects.Length}");
        
        if (cursorTagDetector == null)
        {
            Debug.LogError("CursorTagDetector не назначен! Проверка зоны невозможна.");
            return;
        }
        
        Vector2 zone1Center = cursorTagDetector.zone1Center;
        Vector2 zone1Size = cursorTagDetector.zone1Size;
        Debug.Log($"Zone 1: центр ({zone1Center.x}, {zone1Center.y}), размер ({zone1Size.x}, {zone1Size.y})");
        
        int inZoneCount = 0;
        int outZoneCount = 0;
        
        foreach (GameObject obj in allObjects)
        {
            bool isInZone = IsObjectInZone1(obj);
            if (isInZone)
            {
                Debug.Log($"✓ В ЗОНЕ: {obj.name} на позиции {obj.transform.position}");
                inZoneCount++;
            }
            else
            {
                Debug.LogWarning($"✗ ВНЕ ЗОНЫ: {obj.name} на позиции {obj.transform.position}");
                outZoneCount++;
            }
        }
        
        Debug.Log($"========== ИТОГО: В зоне {inZoneCount} | Вне зоны {outZoneCount} ==========");
    }
    
    [ContextMenu("Show Zone 1 Settings")]
    public void ShowZone1Settings()
    {
        Debug.Log("===== Настройки Zone 1 (ОБЯЗАТЕЛЬНОЕ УСЛОВИЕ) =====");
        
        if (cursorTagDetector != null)
        {
            Debug.Log($"✓ CursorTagDetector: Назначен\n" +
                     $"✓ Zone 1 Center: {cursorTagDetector.zone1Center}\n" +
                     $"✓ Zone 1 Size: {cursorTagDetector.zone1Size}\n" +
                     $"✓ Target Tag: '{targetTag}'\n" +
                     $"Условия поиска: Объект ДОЛЖЕН иметь тег '{targetTag}' И находиться в Zone 1");
        }
        else
        {
            Debug.LogError("✗ CursorTagDetector НЕ НАЗНАЧЕН!\n" +
                          "Поиск объектов невозможен без Zone 1.\n" +
                          "Назначьте CursorTagDetector в Inspector.");
        }
    }
    
    // Визуализация зоны 1 в Scene View (ОБЯЗАТЕЛЬНАЯ зона поиска)
    void OnDrawGizmos()
    {
        if (cursorTagDetector != null)
        {
            Vector2 zone1Center = cursorTagDetector.zone1Center;
            Vector2 zone1Size = cursorTagDetector.zone1Size;
            
            // Рисуем зону 1 желтым цветом (ОБЯЗАТЕЛЬНАЯ зона)
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(new Vector3(zone1Center.x, zone1Center.y, 0), new Vector3(zone1Size.x, zone1Size.y, 0));
            
            // Показываем центр зоны 1 красным
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(new Vector3(zone1Center.x, zone1Center.y, 0), 0.3f);
            
            // Полупрозрачное заполнение зоны
            Gizmos.color = new Color(1f, 1f, 0f, 0.1f); // Полупрозрачный желтый
            Gizmos.DrawCube(new Vector3(zone1Center.x, zone1Center.y, 0), new Vector3(zone1Size.x, zone1Size.y, 0.1f));
        }
        else
        {
            // Предупреждение если CursorTagDetector не назначен
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position, "⚠ CursorTagDetector не назначен!");
            #endif
        }
    }
    
    [ContextMenu("Test Deception - 100% Chance")]
    public void TestDeception100Percent()
    {
        deceptionChance = 100f;
        isDeceptionActive = true;
        GenerateFakeData();
        UpdateTextMeshProFields();
        Debug.Log("Deception set to 100% chance and activated!");
    }
    
    [ContextMenu("Test Deception - 0% Chance")]
    public void TestDeception0Percent()
    {
        deceptionChance = 0f;
        isDeceptionActive = false;
        UpdateTextMeshProFields();
        Debug.Log("Deception set to 0% chance and disabled!");
    }
}
