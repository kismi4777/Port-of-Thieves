# ClientManager - Ограничения для Zone 3

## 🎯 Задача
Добавить ограничения для zone 3 в ClientManager, чтобы можно было помещать и удалять только объекты, выбранные ObjectDataExtractor.

## ✅ Реализованные изменения

### 1. CursorTagDetector - Новые ограничения Zone 3
- **Добавлены новые поля:**
  - `useZone3ObjectRestrictions` - включить ограничения для объектов в zone 3
  - `objectDataExtractor` - ссылка на ObjectDataExtractor для проверки соответствия
  - `autoFindObjectDataExtractor` - автоматический поиск ObjectDataExtractor
  - `showZone3RestrictionDebugInfo` - отладочная информация об ограничениях

### 2. Новые методы проверки
- `FindObjectDataExtractor()` - автоматический поиск ObjectDataExtractor на сцене
- `IsObjectMatchingExtractedData(GameObject obj)` - проверка соответствия объекта с extracted data

### 3. Обновленная логика размещения объектов
- Метод `CanDropAtPosition()` теперь принимает GameObject и проверяет соответствие для zone 3
- Объекты, не соответствующие extracted data, блокируются для размещения в zone 3

### 4. Обновленная логика удаления объектов
- Перед удалением объекта в zone 3 проверяется его соответствие с extracted data
- Несоответствующие объекты возвращаются на исходную позицию вместо удаления

### 5. Контекстные меню для тестирования
- "Принудительно найти ObjectDataExtractor" - поиск компонента
- "Показать текущие extracted data" - отображение данных из ObjectDataExtractor
- "Тест соответствия объекта" - проверка соответствия перетаскиваемого объекта

## 🔧 Рекомендуемые настройки в Inspector

### CursorTagDetector:
- **Use Zone 3 Object Restrictions**: ✅ true
- **Auto Find Object Data Extractor**: ✅ true
- **Show Zone 3 Restriction Debug Info**: ✅ true
- **Use Drop Zone 3 Destroy**: ✅ true

### ClientManager:
- **Track Zone 3 Destructions**: ✅ true
- **Auto Deactivate On Zone 3 Destruction**: ✅ true  
- **Show Zone 3 Debug Info**: ✅ true
- **Check Object Matching**: ✅ true
- **Only Deactivate On Matching Object**: ✅ true (только для соответствующих объектов)

## 📊 Ожидаемое поведение

### При размещении объекта в Zone 3:
```
CursorTagDetector: Проверка соответствия объекта 'Sword(Clone)' с extracted data 'Sword(Clone)' - СООТВЕТСТВУЕТ
Debug.Log: Объект помещен в третью дроп зону
```

### При попытке разместить неподходящий объект:
```
CursorTagDetector: Проверка соответствия объекта 'Shield(Clone)' с extracted data 'Sword(Clone)' - НЕ СООТВЕТСТВУЕТ
CursorTagDetector: Zone 3: Объект 'Shield(Clone)' не соответствует extracted data - доступ запрещен
```

### При попытке удалить неподходящий объект:
```
CursorTagDetector: Zone 3: Объект 'Shield(Clone)' не соответствует extracted data - удаление запрещено
```

## ✅ Статус
**ЗАДАЧА ПОЛНОСТЬЮ ВЫПОЛНЕНА** - Zone 3 теперь принимает только объекты, выбранные ObjectDataExtractor.