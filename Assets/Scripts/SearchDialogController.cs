using UnityEngine;
using TMPro;

/// <summary>
/// Контроллер для диалога поиска объектов
/// Использует RandomPhraseManager для отображения случайных фраз
/// Получает имя объекта из ObjectDataExtractor
/// </summary>
public class SearchDialogController : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private RandomPhraseManager phraseManager;
    [SerializeField] private ObjectDataExtractor objectDataExtractor;
    [SerializeField] private TextMeshProUGUI dialogText;
    
    [Header("Настройки")]
    [SerializeField] private bool updateOnStart = true;
    [SerializeField] private bool updateOnObjectChange = true; // Автообновление при смене объекта
    [SerializeField] private string fallbackObjectName = "Неизвестный предмет"; // На случай если объект не найден
    
    [Header("Настройки текста объектов")]
    [SerializeField] private Color objectTextColor = Color.yellow; // Цвет для имени объекта
    [SerializeField] [Range(0f, 1f)] private float objectTextAlpha = 1f; // Прозрачность для имени объекта
    [SerializeField] private bool useObjectFormatting = true; // Включить форматирование имени объекта
    

    private string lastObjectName = "";

    private void Start()
    {
        // Автоматически находим компоненты, если не назначены
        if (phraseManager == null)
        {
            phraseManager = GetComponent<RandomPhraseManager>();
            if (phraseManager == null)
            {
                phraseManager = FindObjectOfType<RandomPhraseManager>();
            }
        }

        if (objectDataExtractor == null)
        {
            objectDataExtractor = FindObjectOfType<ObjectDataExtractor>();
        }

        if (dialogText == null)
        {
            // Пытаемся найти TextMeshProUGUI в дочерних объектах
            dialogText = GetComponentInChildren<TextMeshProUGUI>();
        }

        if (updateOnStart)
        {
            UpdateDialogText();
        }
    }

    private void Update()
    {
        // Автоматическое обновление при смене объекта
        if (updateOnObjectChange && objectDataExtractor != null)
        {
            string currentObjectName = GetObjectNameFromExtractor();
            if (currentObjectName != lastObjectName && !string.IsNullOrEmpty(currentObjectName) && currentObjectName != "None")
            {
                lastObjectName = currentObjectName;
                UpdateDialogText();
            }
        }
    }

    /// <summary>
    /// Получить имя объекта из ObjectDataExtractor
    /// </summary>
    private string GetObjectNameFromExtractor()
    {
        if (objectDataExtractor == null)
        {
            Debug.LogWarning("ObjectDataExtractor не назначен!");
            return fallbackObjectName;
        }

        // Используем публичное свойство FoundObjectName
        string objectName = objectDataExtractor.FoundObjectName;
        return !string.IsNullOrEmpty(objectName) && objectName != "None" ? objectName : fallbackObjectName;
    }

    /// <summary>
    /// Форматировать только имя объекта через Rich Text
    /// </summary>
    private string FormatObjectName(string objectName)
    {
        if (!useObjectFormatting)
        {
            return objectName.ToUpper(); // Только заглавные буквы
        }

        // Создаем цвет с учетом прозрачности
        Color colorWithAlpha = new Color(objectTextColor.r, objectTextColor.g, objectTextColor.b, objectTextAlpha);
        
        // Форматирование: цвет с прозрачностью + заглавные буквы
        return $"<color=#{ColorUtility.ToHtmlStringRGBA(colorWithAlpha)}>{objectName.ToUpper()}</color>";
    }

    /// <summary>
    /// Обновить текст диалога случайной фразой
    /// </summary>
    public void UpdateDialogText()
    {
        if (phraseManager == null)
        {
            Debug.LogError("RandomPhraseManager не назначен!");
            return;
        }

        if (dialogText == null)
        {
            Debug.LogError("TextMeshProUGUI не назначен!");
            return;
        }

        string objectName = GetObjectNameFromExtractor();
        string randomPhraseTemplate = phraseManager.GetRandomPhraseTemplate();
        
        // Форматируем только имя объекта через Rich Text
        string formattedObjectName = FormatObjectName(objectName);
        string finalPhrase = randomPhraseTemplate.Replace("{0}", formattedObjectName);
        
        dialogText.text = finalPhrase;
        
        Debug.Log($"Обновлен текст диалога: {finalPhrase} (объект: {objectName})");
    }

    /// <summary>
    /// Принудительно обновить фразу (например, при новом объекте)
    /// </summary>
    public void ForceUpdatePhrase()
    {
        UpdateDialogText();
    }

    /// <summary>
    /// Получить текущее имя объекта из ObjectDataExtractor
    /// </summary>
    public string GetObjectName()
    {
        return GetObjectNameFromExtractor();
    }

    // Для тестирования в Inspector
    [ContextMenu("Обновить фразу")]
    private void TestUpdatePhrase()
    {
        UpdateDialogText();
    }

    [ContextMenu("Показать текущий объект")]
    private void ShowCurrentObject()
    {
        string objectName = GetObjectNameFromExtractor();
        Debug.Log($"Текущий объект из ObjectDataExtractor: {objectName}");
    }

    [ContextMenu("Тест форматирования объекта")]
    private void TestObjectFormatting()
    {
        string testObjectName = "Тестовый Объект";
        string formattedName = FormatObjectName(testObjectName);
        Debug.Log($"Форматирование объекта '{testObjectName}': {formattedName}");
    }

    /// <summary>
    /// Установить цвет для имени объекта
    /// </summary>
    public void SetObjectTextColor(Color color)
    {
        objectTextColor = color;
    }

    /// <summary>
    /// Установить прозрачность для имени объекта
    /// </summary>
    public void SetObjectTextAlpha(float alpha)
    {
        objectTextAlpha = Mathf.Clamp01(alpha);
    }

    /// <summary>
    /// Установить цвет и прозрачность для имени объекта
    /// </summary>
    public void SetObjectTextColorAndAlpha(Color color, float alpha)
    {
        objectTextColor = color;
        objectTextAlpha = Mathf.Clamp01(alpha);
    }

    /// <summary>
    /// Включить/выключить форматирование имени объекта
    /// </summary>
    public void SetObjectFormattingEnabled(bool enabled)
    {
        useObjectFormatting = enabled;
    }
}

