# Unity Component Manager - Инструкция по установке

## 🚀 Быстрая установка

### Шаг 1: Копирование файлов Unity Bridge

1. Скопируйте папку `advanced-mcp/tools/unity-bridge/unity-extension` в ваш Unity проект
2. Поместите её в папку `Assets/Editor/` (создайте папку Editor если её нет)

### Шаг 2: Открытие Unity Bridge Window

1. В Unity Editor перейдите в меню `Window` → `Unity Bridge`
2. Откроется окно Unity Bridge
3. Нажмите кнопку "Start Server" для запуска HTTP сервера

### Шаг 3: Проверка работы

1. В консоли Unity должно появиться сообщение: "Unity Bridge started on port 7777"
2. Откройте браузер и перейдите по адресу: `http://localhost:7777/api/status`
3. Должен вернуться JSON ответ

## 🔧 Детальная настройка

### Структура файлов

```
Assets/
├── Editor/
│   └── UnityBridge/
│       ├── UnityBridge.cs
│       ├── UnityBridgeWindow.cs
│       ├── UnityOperations.cs
│       ├── ResponseBuilder.cs
│       ├── Messages.cs
│       ├── JsonUtils.cs
│       ├── ErrorCollector.cs
│       └── HttpServer.cs
└── Scripts/
    └── (ваши скрипты будут создаваться здесь)
```

### Настройка MCP сервера

1. Убедитесь, что MCP сервер запущен
2. В конфигурации MCP добавьте Unity модуль:

```json
{
  "mcpServers": {
    "unity": {
      "command": "node",
      "args": ["path/to/advanced-mcp/index.js"],
      "env": {
        "UNITY_PORT": "7777"
      }
    }
  }
}
```

### Проверка портов

Убедитесь, что порт 7777 свободен:

```bash
# Windows
netstat -an | findstr :7777

# macOS/Linux
netstat -an | grep :7777
```

## 🚨 Устранение проблем

### Ошибка "Port already in use"

1. Найдите процесс, использующий порт 7777
2. Завершите его или измените порт в UnityBridge.cs

### Ошибка "Compilation failed"

1. Проверьте консоль Unity на наличие ошибок компиляции
2. Убедитесь, что все скрипты Unity Bridge скомпилированы
3. Перезапустите Unity Editor

### Ошибка "Connection refused"

1. Проверьте, что Unity Bridge Window открыт
2. Убедитесь, что сервер запущен (кнопка "Start Server")
3. Проверьте файрвол Windows

### Ошибка "Script not found"

1. Убедитесь, что скрипт скомпилирован
2. Проверьте, что скрипт наследуется от MonoBehaviour
3. Подождите завершения компиляции Unity

## 📋 Требования системы

### Unity
- ✅ Unity 2021.3 LTS или новее
- ✅ .NET 4.x Scripting Backend
- ✅ Windows, macOS, Linux

### Система
- ✅ .NET Framework 4.7.1+ (Windows)
- ✅ Mono 6.0+ (macOS/Linux)
- ✅ 4 GB RAM минимум
- ✅ Свободный порт 7777

### MCP
- ✅ Node.js 16+ 
- ✅ MCP сервер (Cursor, Claude Desktop, etc.)

## 🔄 Обновление

### Обновление Unity Bridge

1. Остановите Unity Bridge сервер
2. Замените старые файлы новыми
3. Перезапустите Unity Editor
4. Запустите Unity Bridge заново

### Обновление MCP модуля

1. Остановите MCP сервер
2. Замените файлы в `advanced-mcp/tools/unity.js`
3. Перезапустите MCP сервер

## 🧪 Тестирование

### Тест 1: Проверка соединения

```bash
curl http://localhost:7777/api/status
```

### Тест 2: Создание скрипта

Используйте MCP инструмент `create_and_add_script`:

```javascript
{
  "script_name": "TestScript",
  "script_content": "public float speed = 5f;",
  "object_name": "TestObject"
}
```

### Тест 3: Добавление компонента

Используйте MCP инструмент `add_component`:

```javascript
{
  "object_name": "TestObject",
  "script_name": "TestScript"
}
```

## 📞 Поддержка

### Логи Unity
- Консоль Unity Editor
- Logs/AssetImportWorker*.log

### Логи MCP
- Консоль MCP сервера
- advanced-mcp/logs/ (если включено логирование)

### Логи Unity Bridge
- Unity Bridge Window
- Консоль Unity Editor

## 🎯 Следующие шаги

После успешной установки:

1. **Изучите примеры** в `test_script_example.cs`
2. **Протестируйте** создание простых скриптов
3. **Изучите документацию** в `UNITY_COMPONENT_MANAGER_README.md`
4. **Создайте свои скрипты** через MCP интерфейс

---

**Unity Component Manager** готов к работе! 🎮✨

Если у вас возникли проблемы, проверьте логи и убедитесь, что все компоненты установлены правильно.
