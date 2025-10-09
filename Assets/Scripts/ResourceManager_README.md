# Resource Manager System

Система управления ресурсами для проекта "Port of Thieves" с поддержкой расширяемости и интеграции с TextMeshPro.

## Компоненты системы

### 1. Resource.cs
Базовый класс для хранения информации о ресурсах:
- Название ресурса
- Текущее количество
- Максимальное количество
- Методы для добавления/удаления ресурсов

### 2. ResourceManager.cs
Простой менеджер ресурсов с фокусом на золоте:
- Управление золотом
- Интеграция с TextMeshPro для отображения
- Система событий для уведомлений об изменениях
- Debug методы для тестирования

### 3. ExtendedResourceManager.cs
Расширяемый менеджер ресурсов:
- Динамическое добавление новых типов ресурсов
- Поддержка множественных ресурсов
- Гибкая система UI для каждого ресурса
- Обратная совместимость с ResourceManager

## Использование

### Базовое использование (ResourceManager)

1. Добавьте компонент `ResourceManager` на GameObject
2. Перетащите TextMeshPro компонент в поле `Gold Text`
3. Используйте методы для управления золотом:

```csharp
ResourceManager resourceManager = GetComponent<ResourceManager>();

// Добавить золото
resourceManager.AddGold(100);

// Удалить золото
resourceManager.RemoveGold(50);

// Проверить наличие золота
if (resourceManager.HasEnoughGold(25))
{
    resourceManager.RemoveGold(25);
}

// Установить точное количество
resourceManager.SetGold(1000);
```

### Расширенное использование (ExtendedResourceManager)

1. Добавьте компонент `ExtendedResourceManager` на GameObject
2. Настройте ресурсы в инспекторе или добавьте программно:

```csharp
ExtendedResourceManager resourceManager = GetComponent<ExtendedResourceManager>();

// Добавить новый тип ресурса
resourceManager.AddResourceType("Wood", 0, 1000, woodTextComponent, "Wood: {0}");

// Работа с любыми ресурсами
resourceManager.AddResource("Wood", 50);
resourceManager.RemoveResource("Gold", 25);

// Проверка ресурсов
if (resourceManager.HasEnoughResource("Wood", 10))
{
    resourceManager.RemoveResource("Wood", 10);
}
```

### Подписка на события

```csharp
void Start()
{
    ResourceManager.OnGoldChanged += OnGoldChanged;
    ResourceManager.OnResourceChanged += OnResourceChanged;
}

void OnGoldChanged(Resource gold)
{
    Debug.Log($"Gold changed: {gold.CurrentAmount}");
}

void OnResourceChanged(string resourceName, int currentAmount, int maxAmount)
{
    Debug.Log($"{resourceName}: {currentAmount}/{maxAmount}");
}
```

## Настройка UI

### Для ResourceManager:
- Перетащите TextMeshPro компонент в поле `Gold Text`
- Измените `Gold Display Format` для кастомизации отображения

### Для ExtendedResourceManager:
- Настройте ресурсы в списке `Resource Data List`
- Для каждого ресурса укажите:
  - Название
  - Начальное количество
  - Максимальное количество
  - TextMeshPro компонент для отображения
  - Формат отображения

## Debug функции

Оба менеджера содержат контекстные меню для тестирования:
- Add 100 Gold
- Remove 50 Gold
- Reset Gold

ExtendedResourceManager дополнительно содержит:
- Add Wood Resource
- Add 50 Wood

## Расширение системы

Для добавления новых ресурсов в ExtendedResourceManager:

1. **Через инспектор**: Добавьте новый элемент в `Resource Data List`
2. **Программно**: Используйте `AddResourceType()`

```csharp
// Добавить камень
resourceManager.AddResourceType("Stone", 0, 500, stoneText, "Stones: {0}");

// Добавить еду
resourceManager.AddResourceType("Food", 10, 100, foodText, "Food: {0}");
```

## Особенности

- Автоматическое обновление UI при изменении ресурсов
- Система событий для интеграции с другими системами
- Проверка границ (нельзя уйти в минус или превысить максимум)
- Debug методы для быстрого тестирования
- Поддержка кастомных форматов отображения
- Расширяемость без изменения существующего кода
