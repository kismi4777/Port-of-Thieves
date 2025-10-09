# CursorTagDetector - Zone 3 Tracking System

## Описание
Система отслеживания удаленных объектов в zone 3 для скрипта `CursorTagDetector`. Автоматически записывает информацию о всех объектах, которые были удалены в третьей зоне drop zone.

## 🎯 Основные функции

### 📊 Автоматическое отслеживание
- **Запись информации** - автоматически записывает данные об удаленных объектах
- **Детальная статистика** - отслеживает имя, тег, позицию, время удаления, редкость
- **Интеграция с RandomRarityOnSpawn** - автоматически определяет редкость объектов

### ⚙️ Настройки в Inspector

#### Zone 3 Tracking
- **Enable Zone 3 Tracking** - включить/выключить систему отслеживания (по умолчанию включено)
- **Show Tracking Debug Info** - показывать отладочную информацию в консоли (по умолчанию включено)

## 📈 API методы

### Основные методы статистики
```csharp
// Получить общее количество удаленных объектов
int GetTotalDestroyedCount()

// Получить количество удаленных объектов за последние N секунд
int GetDestroyedCountInLastSeconds(float seconds)

// Получить список всех удаленных объектов
List<DestroyedObjectInfo> GetAllDestroyedObjects()

// Получить список удаленных объектов за последние N секунд
List<DestroyedObjectInfo> GetDestroyedObjectsInLastSeconds(float seconds)
```

### Статистика по категориям
```csharp
// Получить статистику по типам объектов
Dictionary<string, int> GetDestroyedObjectsByType()

// Получить статистику по редкости объектов
Dictionary<string, int> GetDestroyedObjectsByRarity()

// Получить среднее время между удалениями объектов
float GetAverageTimeBetweenDestructions()
```

### Управление данными
```csharp
// Очистить историю удаленных объектов
void ClearDestroyedObjectsHistory()

// Показать полную статистику в консоли
void ShowDestroyedObjectsStats()
```

## 📋 Структура данных DestroyedObjectInfo

```csharp
public struct DestroyedObjectInfo
{
    public string objectName;        // Имя удаленного объекта
    public string objectTag;         // Тег удаленного объекта
    public Vector3 destroyPosition; // Позиция где был удален объект
    public float destroyTime;        // Время удаления (Time.time)
    public string destroyReason;     // Причина удаления
    public bool hadRandomRarityScript; // Был ли у объекта скрипт RandomRarityOnSpawn
    public string rarity;            // Редкость объекта (если была)
}
```

## 🎮 Контекстные меню для тестирования

Доступны через правый клик на компоненте CursorTagDetector в Inspector:

- **"Показать статистику удаленных объектов"** - выводит полную статистику в консоль
- **"Очистить историю удаленных объектов"** - очищает все данные отслеживания
- **"Показать последние 5 удаленных объектов"** - показывает последние 5 удаленных объектов

## 🔧 Примеры использования

### Получение базовой статистики
```csharp
CursorTagDetector detector = FindObjectOfType<CursorTagDetector>();

// Общее количество
int totalDestroyed = detector.GetTotalDestroyedCount();

// За последние 30 секунд
int recentDestroyed = detector.GetDestroyedCountInLastSeconds(30f);

// Среднее время между удалениями
float avgTime = detector.GetAverageTimeBetweenDestructions();
```

### Анализ по типам объектов
```csharp
var typeStats = detector.GetDestroyedObjectsByType();
foreach (var kvp in typeStats)
{
    Debug.Log($"Объект {kvp.Key} удален {kvp.Value} раз");
}
```

### Анализ по редкости
```csharp
var rarityStats = detector.GetDestroyedObjectsByRarity();
foreach (var kvp in rarityStats)
{
    Debug.Log($"Редкость {kvp.Key}: {kvp.Value} объектов");
}
```

### Получение детальной информации
```csharp
var allDestroyed = detector.GetAllDestroyedObjects();
foreach (var info in allDestroyed)
{
    Debug.Log($"Удален: {info.objectName} (тег: {info.objectTag}, редкость: {info.rarity})");
}
```

## 🎯 Интеграция с игровыми системами

### Связь с PrefabSpawner
Система автоматически интегрируется с существующей системой уведомлений PrefabSpawner при удалении объектов.

### Связь с RandomRarityOnSpawn
Автоматически определяет редкость объектов через компонент RandomRarityOnSpawn и записывает эту информацию.

## 📊 Отладочная информация

При включенной опции "Show Tracking Debug Info" в консоль выводятся сообщения:
- При записи каждого удаленного объекта
- При очистке истории
- При вызове статистических методов

Формат сообщений: `📊 Zone 3 Tracking: [описание]`

## ⚡ Производительность

- **Минимальное влияние** - система использует только необходимые данные
- **Эффективное хранение** - данные хранятся в структурах для минимального потребления памяти
- **Оптимизированные запросы** - методы статистики оптимизированы для быстрого выполнения

## 🔄 Совместимость

- **Обратная совместимость** - все существующие функции CursorTagDetector работают без изменений
- **Необязательная функциональность** - систему можно отключить без влияния на основную работу
- **Гибкая настройка** - все параметры настраиваются через Inspector
