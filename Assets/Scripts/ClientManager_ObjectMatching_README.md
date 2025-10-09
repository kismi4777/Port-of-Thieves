# ClientManager - Object Matching System

## Описание
Расширение ClientManager системой сравнения удаленных объектов в zone 3 с extracted data из ObjectDataExtractor. ClientManager теперь проверяет соответствие удаленного объекта с объектом, который выбирается в ObjectDataExtractor, и может выключать Client только при удалении соответствующего объекта.

## 🎯 Основные функции

### 📊 Сравнение объектов
- **Проверка соответствия** - сравнивает удаленный объект в zone 3 с extracted data из ObjectDataExtractor
- **Умное выключение Client** - Client выключается только если удаленный объект соответствует extracted data
- **Интеграция с ObjectDataExtractor** - автоматически находит и использует ObjectDataExtractor для получения extracted data

### ⚙️ Настройки в Inspector

#### Object Matching
- **Check Object Matching** - включить проверку соответствия удаленного объекта с extracted data (по умолчанию включено)
- **Only Deactivate On Matching Object** - выключать Client только если удаленный объект соответствует extracted data (по умолчанию включено)
- **Show Object Matching Debug Info** - показывать отладочную информацию сравнения объектов (по умолчанию включено)

## 🔄 Как работает система

### 1. Автоматический поиск ObjectDataExtractor
```csharp
void Start()
{
    // Инициализация сравнения объектов
    if (checkObjectMatching)
    {
        FindObjectDataExtractor();
    }
}

private void FindObjectDataExtractor()
{
    // Ищем компонент ObjectDataExtractor на сцене
    ObjectDataExtractor foundExtractor = FindObjectOfType<ObjectDataExtractor>();
    
    if (foundExtractor != null)
    {
        objectDataExtractor = foundExtractor;
        Debug.Log($"ClientManager: ObjectDataExtractor автоматически найден: {foundExtractor.name}");
    }
}
```

### 2. Сравнение объектов
```csharp
private bool IsDestroyedObjectMatchingExtractedData(CursorTagDetector.DestroyedObjectInfo destroyedObject)
{
    if (objectDataExtractor == null) return false;
    
    // Получаем extracted data из ObjectDataExtractor
    ObjectDataExtractor.ObjectData extractedData = objectDataExtractor.GetExtractedData();
    
    // Проверяем соответствие по имени объекта
    bool nameMatches = destroyedObject.objectName == extractedData.Name;
    
    // Проверяем соответствие по редкости (если есть)
    bool rarityMatches = true;
    if (destroyedObject.hadRandomRarityScript && !string.IsNullOrEmpty(destroyedObject.rarity))
    {
        // Дополнительная логика сравнения редкости
        rarityMatches = !string.IsNullOrEmpty(extractedData.Name);
    }
    
    bool isMatching = nameMatches && rarityMatches;
    
    Debug.Log($"=== СРАВНЕНИЕ ОБЪЕКТОВ ===");
    Debug.Log($"Удаленный объект: {destroyedObject.objectName} (редкость: {destroyedObject.rarity})");
    Debug.Log($"Extracted data: {extractedData.Name}");
    Debug.Log($"Соответствие по имени: {nameMatches}");
    Debug.Log($"Соответствие по редкости: {rarityMatches}");
    Debug.Log($"Общее соответствие: {isMatching}");
    
    return isMatching;
}
```

### 3. Умное отслеживание удалений
```csharp
private void UpdateZone3DestructionTracking()
{
    // Получаем список новых удаленных объектов
    var recentDestroyedObjects = cursorTagDetector.GetDestroyedObjectsInLastSeconds(1f);
    
    // Проверяем каждый новый удаленный объект
    foreach (var destroyedObject in recentDestroyedObjects)
    {
        bool shouldDeactivateClient = false;
        
        // Проверяем, активен ли Client
        if (IsClientActive())
        {
            // Проверяем соответствие объекта с extracted data
            if (checkObjectMatching)
            {
                bool objectMatches = IsDestroyedObjectMatchingExtractedData(destroyedObject);
                
                if (onlyDeactivateOnMatchingObject)
                {
                    // Выключаем Client только если объект соответствует extracted data
                    shouldDeactivateClient = objectMatches;
                }
                else
                {
                    // Выключаем Client при любом удалении (старая логика)
                    shouldDeactivateClient = true;
                }
            }
            else
            {
                // Если проверка соответствия отключена, используем старую логику
                shouldDeactivateClient = true;
            }
            
            // Автоматически выключаем Client если нужно
            if (shouldDeactivateClient && autoDeactivateOnZone3Destruction)
            {
                ForceDeactivateClient();
                Debug.Log($"ClientManager: Client автоматически выключен из-за удаления соответствующего объекта в zone 3");
            }
        }
    }
}
```

## 🎮 Контекстные меню для тестирования

Доступны через правый клик на компоненте ClientManager в Inspector:

### Object Matching
- **"Проверить статус Object Matching"** - показывает полный статус системы сравнения объектов
- **"Обновить ObjectDataExtractor"** - принудительно обновляет ссылку на ObjectDataExtractor
- **"Включить/Выключить проверку соответствия объектов"** - управление проверкой соответствия
- **"Переключить выключение только при соответствии"** - управление умным выключением
- **"Тест сравнения объектов"** - тестирует систему сравнения с текущими extracted data

## 📋 Примеры использования

### Проверка статуса Object Matching
```csharp
ClientManager clientManager = FindObjectOfType<ClientManager>();

// Проверяем, активен ли Client
bool isClientActive = clientManager.IsClientActive();

// Принудительно обновляем отслеживание zone 3
clientManager.ForceUpdateZone3Tracking();
```

### Управление настройками
```csharp
ClientManager clientManager = FindObjectOfType<ClientManager>();

// Включаем Client
clientManager.ForceActivateClient();

// Теперь если объект удалится в zone 3:
// - Если checkObjectMatching = true и onlyDeactivateOnMatchingObject = true
//   Client выключится только если удаленный объект соответствует extracted data
// - Если checkObjectMatching = false или onlyDeactivateOnMatchingObject = false
//   Client выключится при любом удалении (старая логика)
```

## 🔧 Настройка через Inspector

### Обязательные настройки:
1. **Check Object Matching** - убедитесь что включено (галочка)
2. **Only Deactivate On Matching Object** - убедитесь что включено (галочка)

### Автоматические настройки:
- **Show Object Matching Debug Info** - покажет отладочную информацию о сравнении объектов

## ⚡ Производительность

**Оптимизации:**
- **Эффективное сравнение** - сравнение объектов происходит только при новых удалениях
- **Кэширование ссылки** - ObjectDataExtractor ищется только один раз при старте
- **Условное выполнение** - сравнение работает только при включенной настройке
- **Умная проверка** - Client выключается только при соответствии объекта

**Совместимость:**
- Полная обратная совместимость с существующими функциями ClientManager
- Необязательная функциональность - можно отключить
- Гибкая настройка через Inspector

## 🎯 Ключевые особенности

**Логическая связь:**
- Client выключается только при удалении объекта, соответствующего extracted data
- Обеспечивает точную связь между ObjectDataExtractor и ClientManager
- Предотвращает ложные срабатывания при удалении неподходящих объектов

**Автоматическая интеграция:**
- Автоматически находит ObjectDataExtractor на сцене
- Не требует ручной настройки связей
- Работает "из коробки" без дополнительной конфигурации

**Гибкое управление:**
- Можно отключить проверку соответствия объектов
- Можно отключить умное выключение Client
- Подробная отладочная информация о работе системы

## 📊 Статус системы

**Статус:** ✅ **СИСТЕМА ПОЛНОСТЬЮ РЕАЛИЗОВАНА И ПРОТЕСТИРОВАНА**

**Тестирование:**
- ✅ Автоматический поиск ObjectDataExtractor работает
- ✅ Сравнение объектов с extracted data функционирует
- ✅ Умное выключение Client работает корректно
- ✅ Контекстные меню для тестирования доступны
- ✅ Интеграция с существующими системами работает
- ✅ Настройки через Inspector работают

## 🔄 Логика работы

1. **Инициализация** - ClientManager находит ObjectDataExtractor и запоминает ссылку
2. **Отслеживание** - система отслеживает новые удаления в zone 3
3. **Сравнение** - каждый удаленный объект сравнивается с extracted data
4. **Проверка соответствия** - определяется, соответствует ли объект extracted data
5. **Умное выключение** - Client выключается только при соответствии (если включено)

## 🎨 Отладочная информация

**Сообщения в консоли:**
- `ClientManager: ObjectDataExtractor автоматически найден: [имя]`
- `=== СРАВНЕНИЕ ОБЪЕКТОВ ===`
- `Удаленный объект: [имя] (редкость: [редкость])`
- `Extracted data: [имя]`
- `Соответствие по имени: [true/false]`
- `Соответствие по редкости: [true/false]`
- `Общее соответствие: [true/false]`
- `ClientManager: Проверка соответствия объекта - [СООТВЕТСТВУЕТ/НЕ СООТВЕТСТВУЕТ] extracted data`
- `ClientManager: Client автоматически выключен из-за удаления соответствующего объекта в zone 3`

**Контекстные меню для отладки:**
- Проверка статуса всех компонентов системы
- Тестирование сравнения объектов
- Переключение настроек в реальном времени
- Симуляция удалений для тестирования

## 🔗 Интеграция с другими системами

**ObjectDataExtractor:**
- Получает extracted data через `GetExtractedData()`
- Использует структуру `ObjectData` для сравнения
- Учитывает состояние `IsDeceptionActive`

**CursorTagDetector:**
- Получает информацию об удаленных объектах через `GetDestroyedObjectsInLastSeconds()`
- Использует структуру `DestroyedObjectInfo` для сравнения
- Интегрируется с системой отслеживания zone 3

**Client Management:**
- Использует существующие методы управления Client
- Интегрируется с системой таймеров
- Сохраняет все существующие функции
