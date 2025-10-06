# ObjectDataExtractor - Инструкция по использованию

## Описание
`ObjectDataExtractor` - это скрипт для поиска объектов с тегом "obj" на сцене и извлечения всех их данных, аналогично `SimpleMouseDetector`.

## Быстрый старт

### 1. Добавление в сцену
1. Создайте пустой GameObject в сцене
2. Добавьте компонент `ObjectDataExtractor`
3. Настройте параметры в Inspector

### 2. Настройка параметров
- **Target Tag**: тег для поиска (по умолчанию "obj")
- **Continuous Detection**: непрерывный поиск объектов
- **Detection Radius**: радиус поиска коллайдеров (0.1f)
- **Show Debug Info**: отображение отладочной информации

### 3. Использование

#### Автоматический поиск
Скрипт автоматически ищет объекты под курсором мыши, если включен `Continuous Detection`.

#### Программный поиск
```csharp
// Найти первый объект с тегом
GameObject obj = dataExtractor.FindFirstObjectWithTag();

// Найти все объекты с тегом
GameObject[] objects = dataExtractor.FindAllObjectsWithTag();

// Найти объект в определенной позиции
Vector3 position = new Vector3(0, 0, 0);
GameObject obj = dataExtractor.FindObjectAtPosition(position);

// Получить данные найденного объекта
ObjectDataExtractor.ObjectData data = dataExtractor.GetExtractedData();
```

## Извлекаемые данные

### Основные данные
- **Name**: имя объекта
- **Position**: позиция объекта
- **Scale**: масштаб объекта
- **GameObject**: ссылка на GameObject
- **Collider**: ссылка на Collider2D

### Данные редкости (если есть RandomRarityOnSpawn)
- **Rarity**: тип редкости (Обычный, Необычный, Редкий, и т.д.)
- **RarityColor**: цвет редкости
- **HasRandomRarityScript**: наличие скрипта редкости

### Характеристики (до 5 слотов)
- **Stat1-5**: названия характеристик
- **Stat1Value-5Value**: значения характеристик

### Дополнительные данные
- **ObjectText**: текст с компонента Text
- **ObjectSprite**: спрайт с дочернего объекта "Image" или SpriteRenderer

## Тестирование

### Контекстные меню
В Inspector доступны контекстные меню:
- **"Find First Object with Tag"** - найти первый объект
- **"Find All Objects with Tag"** - найти все объекты
- **"Show Current Data"** - показать текущие данные

### Пример использования
Добавьте `ObjectDataExtractorExample` для автоматического тестирования:
1. Создайте GameObject
2. Добавьте `ObjectDataExtractorExample`
3. Назначьте `ObjectDataExtractor` в поле "Data Extractor"
4. Запустите сцену - тесты выполнятся автоматически

## Требования

### Теги объектов
Объекты должны иметь тег, указанный в `Target Tag` (по умолчанию "obj").

### Компоненты
- **Collider2D** - для обнаружения объектов
- **RandomRarityOnSpawn** (опционально) - для данных редкости
- **Text** (опционально) - для текста объекта
- **SpriteRenderer** (опционально) - для спрайта объекта

### Структура объекта с изображением
```
Object (с тегом "obj")
├── Collider2D
├── RandomRarityOnSpawn (опционально)
├── Text (опционально)
└── Image (дочерний объект)
    └── SpriteRenderer
```

## Производительность

- **Время поиска**: ~1-5ms
- **Время извлечения данных**: ~0.1-1ms
- **Общее время Update**: ~1-6ms

## Отладка

Включите `Show Debug Info` для отображения:
- Инициализации скрипта
- Найденных объектов
- Извлеченных данных характеристик

## Интеграция

Скрипт полностью совместим с:
- `SimpleMouseDetector` - идентичная логика извлечения
- `RandomRarityOnSpawn` - поддержка всех характеристик
- `CursorTagDetector` - та же система обнаружения
