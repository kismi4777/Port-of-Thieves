# ClientManager - Deception Tracking System (Упрощенная версия)

## Описание
Упрощенная система отслеживания состояния "Is Deception Active" из ObjectDataExtractor в ClientManager. ClientManager автоматически отслеживает и предоставляет публичный доступ к состоянию обмана без необходимости ручной настройки связей.

## 🎯 Основные функции

### 📊 Автоматическое отслеживание Deception
- **Публичное поле** - `isDeceptionActive` доступно для чтения из других скриптов
- **Автоматическое обновление** - состояние обновляется в реальном времени из ObjectDataExtractor
- **Автопоиск компонентов** - автоматически находит ObjectDataExtractor на сцене каждый кадр

### ⚙️ Настройки в Inspector

#### Deception Tracking
- **Track Deception State** - включить/выключить отслеживание состояния Deception (по умолчанию включено)

#### Public Deception State
- **Is Deception Active** - публичное поле для отслеживания состояния Deception (только для чтения)

## 📈 API методы

### Основные методы отслеживания
```csharp
// Получить текущее состояние Deception
bool GetDeceptionState()

// Принудительно обновить состояние Deception
void ForceUpdateDeceptionState()

// Включить/выключить отслеживание состояния Deception
void SetDeceptionTrackingEnabled(bool enabled)
```

## 🔄 Как работает упрощенная система

### 1. Автоматическое отслеживание в реальном времени
```csharp
void Update()
{
    // Отслеживаем состояние Deception если включено
    if (trackDeceptionState)
    {
        UpdateDeceptionState();
    }
}
```

### 2. Поиск ObjectDataExtractor каждый кадр
```csharp
private void UpdateDeceptionState()
{
    // Ищем ObjectDataExtractor на сцене
    ObjectDataExtractor objectDataExtractor = FindObjectOfType<ObjectDataExtractor>();
    
    if (objectDataExtractor == null) return;
    
    // Получаем текущее состояние Deception из ObjectDataExtractor
    bool currentDeceptionState = objectDataExtractor.isDeceptionActive;
    
    // Обновляем публичное поле если состояние изменилось
    if (isDeceptionActive != currentDeceptionState)
    {
        isDeceptionActive = currentDeceptionState;
        
        if (showDebugInfo)
        {
            Debug.Log($"ClientManager: Состояние Deception обновлено: {isDeceptionActive}");
        }
    }
}
```

## 🎮 Контекстные меню для тестирования

Доступны через правый клик на компоненте ClientManager в Inspector:

- **"Обновить состояние Deception"** - принудительно обновляет состояние
- **"Включить отслеживание Deception"** - включает отслеживание
- **"Выключить отслеживание Deception"** - выключает отслеживание
- **"Показать статус"** - показывает полный статус включая состояние Deception

## 📋 Примеры использования

### Получение состояния Deception
```csharp
ClientManager clientManager = FindObjectOfType<ClientManager>();

// Через публичное поле
bool deceptionState = clientManager.isDeceptionActive;

// Через метод
bool deceptionState2 = clientManager.GetDeceptionState();

Debug.Log($"Deception активен: {deceptionState}");
```

### Интеграция с другими системами
```csharp
// Проверка состояния Deception в другом скрипте
public class SomeOtherScript : MonoBehaviour
{
    private ClientManager clientManager;
    
    void Start()
    {
        clientManager = FindObjectOfType<ClientManager>();
    }
    
    void Update()
    {
        if (clientManager != null && clientManager.isDeceptionActive)
        {
            // Логика когда обман активен
            Debug.Log("Обман активен!");
        }
    }
}
```

## 🔧 Настройка через Inspector

### Единственная настройка:
- **Track Deception State** - убедитесь что включено (галочка)

### Автоматические настройки:
- **Show Debug Info** - покажет отладочную информацию об обновлениях
- **Is Deception Active** - отображается как read-only поле

## ⚡ Производительность

**Оптимизации:**
- Обновление только при изменении состояния
- Условное выполнение отладочного вывода
- Минимальное влияние на производительность

**Совместимость:**
- Полная обратная совместимость с существующими функциями ClientManager
- Необязательная функциональность - можно отключить
- Гибкая настройка через Inspector

## 🎯 Ключевые особенности упрощенной версии

**Публичное поле:**
- `isDeceptionActive` - доступно для чтения из любых других скриптов
- Автоматически обновляется при изменении состояния в ObjectDataExtractor
- Отображается в Inspector как read-only поле

**Автоматическая интеграция:**
- Автоматически находит ObjectDataExtractor на сцене каждый кадр
- Не требует ручной настройки связей
- Работает "из коробки" без дополнительной конфигурации

**Упрощенная архитектура:**
- Удалены ненужные настройки "Object Data Extractor" и "Auto Find Object Data Extractor"
- Упрощена логика поиска компонентов
- Минимальный код для максимальной функциональности

## 📊 Статус системы

**Статус:** ✅ **УПРОЩЕННАЯ СИСТЕМА ПОЛНОСТЬЮ РЕАЛИЗОВАНА И ПРОТЕСТИРОВАНА**

**Тестирование:**
- ✅ Публичное поле `isDeceptionActive` доступно
- ✅ Автоматическое обновление состояния работает
- ✅ Методы управления функционируют корректно
- ✅ Автоматический поиск ObjectDataExtractor работает
- ✅ Контекстные меню для тестирования доступны
- ✅ Упрощенная архитектура без лишних настроек
