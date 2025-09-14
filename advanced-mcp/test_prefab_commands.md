# Unity Prefab Manager - Команды для тестирования

## 🧪 Тестирование новых возможностей

### 1. Создание префаба из объекта на сцене

```javascript
// Создать префаб из объекта JumpingBall
{
  "object_name": "JumpingBall",
  "prefab_path": "Assets/Prefabs/JumpingBall.prefab"
}
```

**MCP команда:**
```bash
mcp unity create_prefab --object_name "JumpingBall" --prefab_path "Assets/Prefabs/JumpingBall.prefab"
```

### 2. Создание экземпляра префаба

```javascript
// Создать экземпляр префаба JumpingBall в позиции (2, 1, 0)
{
  "prefab_path": "Assets/Prefabs/JumpingBall.prefab",
  "position": [2, 1, 0],
  "rotation": [0, 0, 0],
  "scale": [1, 1, 1]
}
```

**MCP команда:**
```bash
mcp unity instantiate_prefab --prefab_path "Assets/Prefabs/JumpingBall.prefab" --position "[2, 1, 0]"
```

### 3. Поиск всех префабов в проекте

```javascript
// Найти все префабы в папке Assets
{
  "search_path": "Assets"
}
```

**MCP команда:**
```bash
mcp unity list_prefabs --search_path "Assets"
```

### 4. Поиск префабов в конкретной папке

```javascript
// Найти все префабы в папке Prefabs
{
  "search_path": "Assets/Prefabs"
}
```

**MCP команда:**
```bash
mcp unity list_prefabs --search_path "Assets/Prefabs"
```

## 🎯 Полный workflow тестирования

### Шаг 1: Анализ сцены
```bash
mcp unity scene_hierarchy
```

### Шаг 2: Создание префаба
```bash
mcp unity create_prefab --object_name "JumpingBall" --prefab_path "Assets/Prefabs/JumpingBall.prefab"
```

### Шаг 3: Проверка создания префаба
```bash
mcp unity list_prefabs --search_path "Assets/Prefabs"
```

### Шаг 4: Создание экземпляра префаба
```bash
mcp unity instantiate_prefab --prefab_path "Assets/Prefabs/JumpingBall.prefab" --position "[3, 1, 0]"
```

### Шаг 5: Создание еще одного экземпляра
```bash
mcp unity instantiate_prefab --prefab_path "Assets/Prefabs/JumpingBall.prefab" --position "[-3, 1, 0]" --rotation "[0, 45, 0]"
```

### Шаг 6: Финальная проверка сцены
```bash
mcp unity scene_hierarchy
```

## 🔄 Тестирование различных параметров

### Создание префаба с поворотом и масштабом
```javascript
{
  "prefab_path": "Assets/Prefabs/JumpingBall.prefab",
  "position": [0, 2, 0],
  "rotation": [0, 90, 0],
  "scale": [1.5, 1.5, 1.5]
}
```

### Создание префаба в разных позициях
```javascript
// Позиция 1
{
  "prefab_path": "Assets/Prefabs/JumpingBall.prefab",
  "position": [1, 1, 1]
}

// Позиция 2
{
  "prefab_path": "Assets/Prefabs/JumpingBall.prefab",
  "position": [-1, 1, -1]
}

// Позиция 3
{
  "prefab_path": "Assets/Prefabs/JumpingBall.prefab",
  "position": [0, 3, 0]
}
```

## 🚨 Тестирование обработки ошибок

### Тест 1: Несуществующий объект
```bash
mcp unity create_prefab --object_name "NonExistentObject" --prefab_path "Assets/Prefabs/Test.prefab"
```

**Ожидаемый результат:** Ошибка "GameObject 'NonExistentObject' not found in scene"

### Тест 2: Неверный путь к префабу
```bash
mcp unity instantiate_prefab --prefab_path "Assets/NonExistent/Test.prefab"
```

**Ожидаемый результат:** Ошибка "Prefab not found at path: Assets/NonExistent/Test.prefab"

### Тест 3: Пустой путь к префабу
```bash
mcp unity create_prefab --object_name "JumpingBall" --prefab_path ""
```

**Ожидаемый результат:** Ошибка "prefab_path parameter is required"

## 📊 Проверка результатов

### После создания префаба:
1. Проверить, что файл .prefab создан в Unity
2. Проверить, что AssetDatabase обновлен
3. Проверить, что префаб доступен в Project window

### После создания экземпляра:
1. Проверить, что объект появился на сцене
2. Проверить, что позиция, поворот и масштаб установлены правильно
3. Проверить, что сцена отмечена как измененная

### После поиска префабов:
1. Проверить, что возвращен правильный список
2. Проверить, что количество найденных префабов корректно
3. Проверить, что пути к префабам правильные

## 🎉 Успешное тестирование

Если все команды выполняются без ошибок, значит:

✅ **Unity Prefab Manager работает корректно**  
✅ **MCP может создавать префабы**  
✅ **MCP может загружать и создавать экземпляры**  
✅ **MCP может искать префабы в проекте**  
✅ **Проблема с префабами решена!**  

---

**Теперь MCP полноценно работает с Unity префабами!** 🎮✨
