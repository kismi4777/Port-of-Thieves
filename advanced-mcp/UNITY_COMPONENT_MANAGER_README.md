# Unity Component Manager - Руководство пользователя

## 🎯 Обзор

Unity Component Manager - это расширение MCP (Model Context Protocol), которое позволяет AI ассистентам:

1. **Добавлять существующие скрипты** на объекты в Unity сцене
2. **Создавать новые скрипты** и автоматически добавлять их на объекты
3. **Управлять компонентами** объектов через AI интерфейс

## 🚀 Новые возможности

### 1. Добавление существующих скриптов (`add_component`)

Добавляет уже существующий скрипт на указанный GameObject.

**Параметры:**
- `object_name` - имя GameObject в сцене
- `script_name` - имя скрипта (MonoBehaviour)

**Пример использования:**
```javascript
// Добавить скрипт "PlayerController" на объект "Player"
{
  "object_name": "Player",
  "script_name": "PlayerController"
}
```

### 2. Создание и добавление скриптов (`create_and_add_script`)

Создает новый C# скрипт с заданным содержимым и автоматически добавляет его на объект.

**Параметры:**
- `script_name` - имя нового скрипта (без .cs)
- `script_content` - содержимое класса (без объявления класса)
- `object_name` - имя GameObject в сцене

**Пример использования:**
```javascript
// Создать скрипт "EnemyAI" и добавить на объект "Enemy"
{
  "script_name": "EnemyAI",
  "script_content": `
    public float moveSpeed = 5f;
    public Transform target;
    
    void Update() {
        if (target != null) {
            transform.position = Vector3.MoveTowards(
                transform.position, 
                target.position, 
                moveSpeed * Time.deltaTime
            );
        }
    }
  `,
  "object_name": "Enemy"
}
```

## 🔧 Технические детали

### Архитектура

1. **Unity Bridge** - HTTP сервер на порту 7777
2. **MCP Tools** - JavaScript интерфейс для AI
3. **Unity Operations** - C# логика выполнения операций
4. **Asset Management** - автоматическое создание и управление скриптами

### Безопасность

- ✅ Только MonoBehaviour скрипты
- ✅ Проверка существования объектов
- ✅ Предотвращение дублирования компонентов
- ✅ Автоматическое обновление сцены

### Ограничения

- ❌ Только Editor режим (не runtime)
- ❌ Только C# скрипты (не JavaScript)
- ❌ Только MonoBehaviour наследование

## 📋 Требования

1. **Unity 2021.3+** с открытым проектом
2. **Unity Bridge Window** открыт в Editor
3. **HTTP сервер** работает на порту 7777
4. **MCP сервер** запущен и подключен

## 🚨 Устранение неполадок

### Ошибка "GameObject not found"
- Проверьте, что объект существует в активной сцене
- Убедитесь, что имя объекта написано правильно

### Ошибка "Script not found"
- Убедитесь, что скрипт скомпилирован
- Проверьте, что скрипт наследуется от MonoBehaviour
- Подождите завершения компиляции Unity

### Ошибка "Component already exists"
- Компонент уже добавлен на объект
- Удалите существующий компонент вручную или переименуйте скрипт

## 💡 Примеры использования

### Создание простого скрипта движения
```javascript
{
  "script_name": "SimpleMovement",
  "script_content": `
    public float speed = 5f;
    
    void Update() {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        Vector3 movement = new Vector3(horizontal, 0, vertical);
        transform.Translate(movement * speed * Time.deltaTime);
    }
  `,
  "object_name": "Player"
}
```

### Создание скрипта сбора предметов
```javascript
{
  "script_name": "Collectible",
  "script_content": `
    public int points = 10;
    
    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            // Добавить очки игроку
            Destroy(gameObject);
        }
    }
  `,
  "object_name": "Coin"
}
```

## 🔄 Обновления

### Версия 1.0
- ✅ Добавление существующих компонентов
- ✅ Создание и добавление новых скриптов
- ✅ Автоматическое управление AssetDatabase
- ✅ Проверка безопасности и валидация

### Планы на будущее
- 🔄 Удаление компонентов
- 🔄 Редактирование существующих скриптов
- 🔄 Массовые операции с компонентами
- 🔄 Поддержка ScriptableObjects

## 📞 Поддержка

При возникновении проблем:
1. Проверьте консоль Unity на наличие ошибок
2. Убедитесь, что Unity Bridge работает
3. Проверьте логи MCP сервера
4. Перезапустите Unity Bridge Window

---

**Unity Component Manager** - делаем Unity разработку доступной для AI! 🎮✨
