# ClientManager - Zone 3 Object Tracking

## Описание
Расширение ClientManager системой отслеживания удаления объектов в zone 3. ClientManager теперь автоматически отслеживает удаления объектов в zone 3 и может автоматически выключать Client при удалении объектов во время его активности.

## 🎯 Основные функции

### 📊 Отслеживание удалений в Zone 3
- **Автоматическое отслеживание** - ClientManager отслеживает количество удаленных объектов в zone 3
- **Автоматическое выключение Client** - Client выключается при удалении объекта в zone 3 во время его активности
- **Интеграция с CursorTagDetector** - автоматически находит и использует CursorTagDetector для получения статистики

### ⚙️ Настройки в Inspector

#### Zone 3 Object Tracking
- **Track Zone 3 Destructions** - включить отслеживание удалений в zone 3 (по умолчанию включено)
- **Auto Deactivate On Zone 3 Destruction** - автоматически выключать Client при удалении в zone 3 (по умолчанию включено)
- **Show Zone 3 Debug Info** - показывать отладочную информацию zone 3 (по умолчанию включено)

## 🔄 Как работает система

### 1. Автоматический поиск CursorTagDetector
```csharp
void Start()
{
    // Инициализация отслеживания zone 3
    if (trackZone3Destructions)
    {
        FindCursorTagDetector();
    }
}

private void FindCursorTagDetector()
{
    // Ищем компонент CursorTagDetector на сцене
    CursorTagDetector foundDetector = FindObjectOfType<CursorTagDetector>();
    
    if (foundDetector != null)
    {
        cursorTagDetector = foundDetector;
        lastZone3DestructionCount = foundDetector.GetTotalDestroyedCount();
        
        Debug.Log($"ClientManager: CursorTagDetector автоматически найден: {foundDetector.name}");
    }
}
```

### 2. Отслеживание удалений в Update
```csharp
void Update()
{
    // Отслеживаем удаления в zone 3
    if (trackZone3Destructions)
    {
        UpdateZone3DestructionTracking();
    }
}

private void UpdateZone3DestructionTracking()
{
    if (cursorTagDetector == null) return;
    
    // Получаем текущее количество удаленных объектов
    int currentDestructionCount = cursorTagDetector.GetTotalDestroyedCount();
    
    // Проверяем, увеличилось ли количество удалений
    if (currentDestructionCount > lastZone3DestructionCount)
    {
        // Обновляем счетчик
        int newDestructions = currentDestructionCount - lastZone3DestructionCount;
        lastZone3DestructionCount = currentDestructionCount;
        
        Debug.Log($"ClientManager: Обнаружено {newDestructions} новых удалений в zone 3. Всего: {currentDestructionCount}");
        
        // Проверяем, активен ли Client
        if (IsClientActive())
        {
            Debug.Log("ClientManager: Client активен во время удаления в zone 3!");
            
            // Автоматически выключаем Client если включено
            if (autoDeactivateOnZone3Destruction)
            {
                ForceDeactivateClient();
                Debug.Log("ClientManager: Client автоматически выключен из-за удаления в zone 3");
            }
        }
    }
}
```

## 🎮 Контекстные меню для тестирования

Доступны через правый клик на компоненте ClientManager в Inspector:

### Zone 3 Tracking
- **"Проверить статус Zone 3 Tracking"** - показывает полный статус системы отслеживания
- **"Обновить отслеживание Zone 3"** - принудительно обновляет отслеживание
- **"Включить отслеживание Zone 3"** - включает отслеживание zone 3
- **"Выключить отслеживание Zone 3"** - выключает отслеживание zone 3
- **"Переключить автовыключение Client при удалении"** - включает/выключает автовыключение
- **"Симулировать удаление в Zone 3"** - симулирует удаление для тестирования

## 📋 Примеры использования

### Проверка статуса Zone 3 Tracking
```csharp
ClientManager clientManager = FindObjectOfType<ClientManager>();

// Принудительно обновить отслеживание zone 3
clientManager.ForceUpdateZone3Tracking();

// Проверяем, активен ли Client
bool isClientActive = clientManager.IsClientActive();
```

### Управление настройками
```csharp
ClientManager clientManager = FindObjectOfType<ClientManager>();

// Включаем Client
clientManager.ForceActivateClient();

// Теперь если объект удалится в zone 3, Client автоматически выключится
// (если включено autoDeactivateOnZone3Destruction)
```

## 🔧 Настройка через Inspector

### Обязательные настройки:
1. **Track Zone 3 Destructions** - убедитесь что включено (галочка)
2. **Auto Deactivate On Zone 3 Destruction** - убедитесь что включено (галочка)

### Автоматические настройки:
- **Show Zone 3 Debug Info** - покажет отладочную информацию о работе системы

## ⚡ Производительность

**Оптимизации:**
- **Эффективное отслеживание** - проверка изменений только при увеличении счетчика
- **Кэширование ссылки** - CursorTagDetector ищется только один раз при старте
- **Условное выполнение** - отслеживание работает только при включенной настройке
- **Умная проверка** - Client выключается только если он был активен во время удаления

**Совместимость:**
- Полная обратная совместимость с существующими функциями ClientManager
- Необязательная функциональность - можно отключить
- Гибкая настройка через Inspector

## 🎯 Ключевые особенности

**Логическая связь:**
- Client выключается при удалении объекта в zone 3 во время его активности
- Обеспечивает логическую связь между системами
- Предотвращает дальнейшие действия с Client после удаления объекта

**Автоматическая интеграция:**
- Автоматически находит CursorTagDetector на сцене
- Не требует ручной настройки связей
- Работает "из коробки" без дополнительной конфигурации

**Гибкое управление:**
- Можно отключить автовыключение Client
- Можно отключить отслеживание zone 3 полностью
- Подробная отладочная информация о работе системы

## 📊 Статус системы

**Статус:** ✅ **СИСТЕМА ПОЛНОСТЬЮ РЕАЛИЗОВАНА И ПРОТЕСТИРОВАНА**

**Тестирование:**
- ✅ Автоматический поиск CursorTagDetector работает
- ✅ Отслеживание удалений в zone 3 функционирует
- ✅ Автоматическое выключение Client работает корректно
- ✅ Контекстные меню для тестирования доступны
- ✅ Интеграция с существующими системами работает
- ✅ Настройки через Inspector работают

## 🔄 Логика работы

1. **Инициализация** - ClientManager находит CursorTagDetector и запоминает начальный счетчик удалений
2. **Отслеживание** - каждый кадр проверяется, увеличился ли счетчик удалений
3. **Обнаружение** - при увеличении счетчика система определяет количество новых удалений
4. **Проверка Client** - если Client активен во время удаления, срабатывает автовыключение
5. **Автовыключение** - Client автоматически выключается (если включена настройка)

## 🎨 Отладочная информация

**Сообщения в консоли:**
- `ClientManager: CursorTagDetector автоматически найден: [имя]`
- `ClientManager: Начальное количество удаленных объектов в zone 3: [число]`
- `ClientManager: Обнаружено [число] новых удалений в zone 3. Всего: [число]`
- `ClientManager: Client активен во время удаления в zone 3!`
- `ClientManager: Client автоматически выключен из-за удаления в zone 3`

**Контекстные меню для отладки:**
- Проверка статуса всех компонентов системы
- Симуляция удалений для тестирования
- Переключение настроек в реальном времени
