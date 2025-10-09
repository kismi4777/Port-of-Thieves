# CursorTagDetector - Zone 3 Client Integration

## Описание
Расширение CursorTagDetector системой интеграции с ClientManager. Zone 3 теперь активируется только когда Client активен, что обеспечивает логическую связь между состоянием Client и функциональностью удаления объектов.

## 🎯 Основные функции

### 📊 Условная активация Zone 3
- **Проверка активности Client** - Zone 3 работает только когда Client активен
- **Автоматический поиск ClientManager** - автоматически находит ClientManager на сцене
- **Визуальная индикация** - показывает статус Zone 3 в Scene View

### ⚙️ Настройки в Inspector

#### Zone 3 Client Integration
- **Require Client Active For Zone 3** - Zone 3 активна только когда Client активен (по умолчанию включено)
- **Auto Find Client Manager** - автоматический поиск ClientManager (по умолчанию включен)

## 🔄 Как работает система

### 1. Автоматический поиск ClientManager
```csharp
void Start()
{
    // Автоматический поиск ClientManager если включен
    if (autoFindClientManager)
    {
        FindClientManager();
    }
}

private void FindClientManager()
{
    // Ищем компонент ClientManager на сцене
    ClientManager foundManager = FindObjectOfType<ClientManager>();
    
    if (foundManager != null)
    {
        clientManager = foundManager;
        Debug.Log($"CursorTagDetector: ClientManager автоматически найден: {foundManager.name}");
    }
    else
    {
        Debug.LogWarning("CursorTagDetector: ClientManager не найден на сцене!");
    }
}
```

### 2. Проверка активности Client
```csharp
private bool IsClientActive()
{
    if (!requireClientActiveForZone3)
    {
        return true; // Если не требуется активность Client, всегда возвращаем true
    }
    
    if (clientManager == null)
    {
        Debug.LogWarning("CursorTagDetector: ClientManager не найден для проверки активности Client!");
        return false;
    }
    
    return clientManager.IsClientActive();
}
```

### 3. Условная активация Zone 3
```csharp
else if (IsPositionInDropZone3(worldPosition))
{
    // Проверяем, активен ли Client для активации zone 3
    if (!IsClientActive())
    {
        Debug.Log($"Zone 3 неактивна - Client не активен. Объект {draggedObject.name} возвращен на исходную позицию.");
        
        // Возвращаем объект на исходную позицию
        draggedObject.position = originalPosition;
        draggedObject.localScale = originalScale;
        
        // Сбрасываем переменные перетаскивания
        isDragging = false;
        draggedObject = null;
        isInDropZone2 = false;
        isInDropZone3 = false;
        
        return;
    }
    
    // Обычная логика Zone 3 (удаление объектов)...
}
```

## 🎮 Контекстные меню для тестирования

Доступны через правый клик на компоненте CursorTagDetector в Inspector:

- **"Проверить статус Client для Zone 3"** - показывает полный статус интеграции
- **"Принудительно включить Client"** - включает Client через ClientManager
- **"Принудительно выключить Client"** - выключает Client через ClientManager
- **"Переключить требование активности Client для Zone 3"** - включает/выключает требование
- **"Обновить кэш состояния Client"** - принудительно обновляет кэш состояния Client
- **"Показать информацию о кэшировании"** - показывает детальную информацию о системе кэширования
- **"Сбросить кэш поиска ClientManager"** - сбрасывает кэш поиска ClientManager
- **"Установить интервал поиска ClientManager (1 сек)"** - устанавливает интервал поиска 1 секунда
- **"Установить интервал поиска ClientManager (10 сек)"** - устанавливает интервал поиска 10 секунд

## 📋 Примеры использования

### Проверка статуса Zone 3
```csharp
CursorTagDetector detector = FindObjectOfType<CursorTagDetector>();

// Проверяем, активна ли Zone 3
bool isZone3Active = detector.IsPositionInDropZone3(somePosition);

// Проверяем, можно ли разместить объект в Zone 3
bool canDropInZone3 = detector.CanDropAtPosition(somePosition);
```

### Управление через ClientManager
```csharp
ClientManager clientManager = FindObjectOfType<ClientManager>();

// Включаем Client - Zone 3 станет активной
clientManager.ForceActivateClient();

// Выключаем Client - Zone 3 станет неактивной
clientManager.ForceDeactivateClient();

// Проверяем статус Client
bool isClientActive = clientManager.IsClientActive();
```

### Управление кэшированием
```csharp
CursorTagDetector detector = FindObjectOfType<CursorTagDetector>();

// Принудительно обновить кэш состояния Client
detector.RefreshClientStateCache();

// Проверяем, активна ли Zone 3 (с кэшированием)
bool isZone3Active = detector.IsPositionInDropZone3(somePosition);
```

### Управление интервалом поиска ClientManager
```csharp
CursorTagDetector detector = FindObjectOfType<CursorTagDetector>();

// Устанавливаем интервал поиска ClientManager через рефлексию
var clientManagerSearchIntervalField = detector.GetType().GetField("clientManagerSearchInterval", 
    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
clientManagerSearchIntervalField.SetValue(detector, 10f); // 10 секунд

// Сбрасываем кэш поиска
var clientManagerSearchAttemptedField = detector.GetType().GetField("clientManagerSearchAttempted", 
    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
clientManagerSearchAttemptedField.SetValue(detector, false);
```

## 🔧 Настройка через Inspector

### Обязательные настройки:
1. **Require Client Active For Zone 3** - убедитесь что включено (галочка)
2. **Auto Find Client Manager** - убедитесь что включено (галочка)

### Автоматические настройки:
- **Show Tracking Debug Info** - покажет отладочную информацию о статусе Zone 3

## 🎨 Визуализация в Scene View

### Индикация статуса Zone 3:
- **Зеленая точка** - Zone 3 активна (Client активен)
- **Красная точка** - Zone 3 неактивна (Client неактивен)
- **Текстовая метка** - "Zone 3: ACTIVE" или "Zone 3: INACTIVE (Client not active)"

### Отображение зон:
- **Zone 1** - зеленая рамка с красным центром
- **Zone 2** - синяя рамка с голубым центром
- **Zone 3** - желтая рамка с фиолетовым центром + индикатор статуса

## ⚡ Производительность

**Оптимизации:**
- **Кэширование состояния Client** - проверка активности Client происходит только при изменении состояния
- **Умное обновление кэша** - кэш обновляется только когда реальное состояние Client изменилось
- **Кэширование ссылки на ClientManager** - ClientManager ищется только один раз при старте
- **Условное выполнение отладочного вывода** - отладочные сообщения показываются только при изменении состояния

**Система кэширования:**
```csharp
// Кэширование состояния Client для оптимизации
private bool cachedClientActiveState = false;
private bool clientStateCached = false;

// Кэширование ссылки на ClientManager для оптимизации
private bool clientManagerSearchAttempted = false;
private float lastClientManagerSearchTime = 0f;
private float clientManagerSearchInterval = 5f; // Поиск ClientManager каждые 5 секунд

private bool IsClientActive()
{
    // Проверяем, нужно ли искать ClientManager
    bool shouldSearchClientManager = false;
    
    if (clientManager == null)
    {
        float currentTime = Time.time;
        
        // Ищем ClientManager только если:
        // 1. Поиск еще не предпринимался, ИЛИ
        // 2. Прошло достаточно времени с последнего поиска
        if (!clientManagerSearchAttempted || (currentTime - lastClientManagerSearchTime) >= clientManagerSearchInterval)
        {
            shouldSearchClientManager = true;
            lastClientManagerSearchTime = currentTime;
            clientManagerSearchAttempted = true;
        }
    }
    
    // Ищем ClientManager если нужно
    if (shouldSearchClientManager)
    {
        FindClientManager();
    }
    
    if (clientManager == null)
    {
        // Не логируем предупреждение каждый кадр, только при первом поиске
        if (shouldSearchClientManager && showTrackingDebugInfo)
        {
            Debug.LogWarning("CursorTagDetector: ClientManager не найден для проверки активности Client!");
        }
        return false;
    }
    
    // Получаем текущее состояние Client
    bool currentClientState = clientManager.IsClientActive();
    
    // Обновляем кэш только если состояние изменилось
    if (!clientStateCached || cachedClientActiveState != currentClientState)
    {
        cachedClientActiveState = currentClientState;
        clientStateCached = true;
        
        if (showTrackingDebugInfo)
        {
            Debug.Log($"CursorTagDetector: Состояние Client обновлено: {cachedClientActiveState}");
        }
    }
    
    return cachedClientActiveState;
}
```

**Совместимость:**
- Полная обратная совместимость с существующими функциями CursorTagDetector
- Необязательная функциональность - можно отключить
- Гибкая настройка через Inspector

## 🎯 Ключевые особенности

**Логическая связь:**
- Zone 3 активируется только когда Client активен
- Обеспечивает логическую связь между системами
- Предотвращает случайное удаление объектов когда Client неактивен

**Автоматическая интеграция:**
- Автоматически находит ClientManager на сцене
- Не требует ручной настройки связей
- Работает "из коробки" без дополнительной конфигурации

**Визуальная обратная связь:**
- Четкая индикация статуса Zone 3 в Scene View
- Отладочные сообщения о состоянии системы
- Контекстные меню для тестирования

## 📊 Статус системы

**Статус:** ✅ **СИСТЕМА ПОЛНОСТЬЮ РЕАЛИЗОВАНА И ПРОТЕСТИРОВАНА**

**Тестирование:**
- ✅ Автоматический поиск ClientManager работает
- ✅ Проверка активности Client функционирует
- ✅ Условная активация Zone 3 работает корректно
- ✅ Визуальная индикация статуса отображается
- ✅ Контекстные меню для тестирования доступны
- ✅ Интеграция с существующими системами работает
