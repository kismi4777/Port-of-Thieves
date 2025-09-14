/**
 * Unity Bridge MCP Module - Прозрачный мост
 * 
 * Новая упрощенная архитектура:
 * • Unity возвращает готовый массив messages
 * • JS просто передает данные без обработки
 * • Максимальная прозрачность связи
 */

import axios from 'axios';

const UNITY_BASE_URL = 'http://localhost:7777';

/**
 * Конвертер Unity messages в MCP формат
 */
function convertToMCPResponse(unityResponse) {
  // Новая Unity архитектура возвращает { messages: [...] }
  if (unityResponse.messages && Array.isArray(unityResponse.messages)) {
    const content = [];
    
    for (const msg of unityResponse.messages) {
      if (msg.type === 'text') {
        content.push({
          type: 'text',
          text: msg.content
        });
      } else if (msg.type === 'image') {
        // Добавляем описание изображения если есть
        if (msg.text) {
          content.push({
            type: 'text', 
            text: msg.text
              });
            }
        // Затем само изображение
        content.push({
          type: 'image',
          data: msg.content,
          mimeType: 'image/png'
        });
      }
    }
    
    return { content };
  }
  
  // Fallback для старого формата Unity API
  return convertLegacyResponse(unityResponse);
    }

/**
 * Fallback для старого формата Unity (временно)
 */
function convertLegacyResponse(unityData) {
  const content = [];
        
  // Основное сообщение
  if (unityData.message) {
    content.push({
      type: 'text',
      text: unityData.message
    });
  }
  
  // Данные результата
  if (unityData.data && unityData.data !== unityData.message) {
    content.push({
      type: 'text', 
      text: unityData.data
    });
  }
  
  // Изображение для скриншотов
  if (unityData.image) {
    content.push({
      type: 'text',
      text: 'Unity Screenshot'
    });
    content.push({
      type: 'image',
      data: unityData.image,
      mimeType: 'image/png'
    });
  }
  
  // Ошибки Unity
  if (unityData.errors && unityData.errors.length > 0) {
    const errorText = unityData.errors.map(err => {
      if (typeof err === 'object') {
        const level = err.Level || err.level || 'Info';
        const message = err.Message || err.message || 'Unknown error';
        return `${level}: ${message}`;
      }
      return err.toString();
    }).join('\n');
    
    content.push({
      type: 'text',
      text: `Unity Logs:\n${errorText}`
    });
  }
  
  // Если нет контента, добавляем статус
  if (content.length === 0) {
    content.push({
      type: 'text',
      text: `Unity Status: ${unityData.status || 'Unknown'}`
    });
  }
  
  return { content };
  }
  
/**
 * Универсальный обработчик Unity запросов
 */
async function handleUnityRequest(endpoint, data = {}, timeout = 10000) {
  try {
    // 🚀 Убеждаемся что данные корректно сериализуются в UTF-8
    const jsonData = JSON.stringify(data);
    
    const response = await axios.post(`${UNITY_BASE_URL}${endpoint}`, jsonData, {
      timeout,
      responseType: 'json',
      headers: { 
        'Content-Type': 'application/json; charset=utf-8',
        'Accept': 'application/json; charset=utf-8'
      }
    });
    
    return convertToMCPResponse(response.data);
  } catch (error) {
    const errorContent = [{
      type: 'text',
      text: `Unity Connection Error: ${error.message}\n\nПроверьте:\n• Unity запущен\n• Unity Bridge Window открыт\n• HTTP сервер работает на порту 7777`
    }];
    
    // Добавляем детали ошибки если есть
    if (error.response?.data) {
      try {
        const unityError = convertToMCPResponse(error.response.data);
        errorContent.push(...unityError.content);
      } catch {
        errorContent.push({
          type: 'text',
          text: `Unity Error Details: ${JSON.stringify(error.response.data)}`
        });
      }
    }
    
    return { content: errorContent };
        }
}

// Unity инструменты
const unityTools = [
  {
    name: "screenshot",
    description: 'Unity Game View скриншот',
    inputSchema: {
      type: 'object',
      properties: {
        systemScreenshot: {
          type: 'boolean',
          default: false,
          description: '🖥️ Включить скриншот рабочего стола. ИСПОЛЬЗОВАТЬ ТОЛЬКО ПРИ СТРОГОЙ НЕОБХОДИМОСТИ УВИДЕТЬ ЭКРАН ПОЛЬЗОВАТЕЛЯ И НЕ ИСПОЛЬЗОВАТЬ ПРОСТО ТАК!'
        }
      },
      required: []
    },
    handler: async (params) => {
      return await handleUnityRequest('/api/screenshot');
    }
  },
  
  {
    name: "camera_screenshot", 
    description: 'Unity скриншот с произвольной позиции камеры',
    inputSchema: {
      type: 'object',
      properties: {
        position: {
          type: 'array',
          items: { type: 'number' },
          minItems: 3,
          maxItems: 3,
          description: 'Позиция камеры [x, y, z]'
        },
        target: {
          type: 'array', 
          items: { type: 'number' },
          minItems: 3,
          maxItems: 3,
          description: 'Точка направления камеры [x, y, z]'
        },
        width: {
          type: 'number',
          default: 1920,
          minimum: 256,
          maximum: 4096,
          description: 'Ширина скриншота в пикселях'
        },
        height: {
          type: 'number',
          default: 1080,
          minimum: 256,
          maximum: 4096,
          description: 'Высота скриншота в пикселях'
        },
        fov: {
          type: 'number',
          default: 60,
          minimum: 10,
          maximum: 179,
          description: 'Поле зрения камеры в градусах'
        },
        systemScreenshot: {
          type: 'boolean',
          default: false,
          description: '🖥️ Включить скриншот рабочего стола. ИСПОЛЬЗОВАТЬ ТОЛЬКО ПРИ СТРОГОЙ НЕОБХОДИМОСТИ УВИДЕТЬ ЭКРАН ПОЛЬЗОВАТЕЛЯ И НЕ ИСПОЛЬЗОВАТЬ ПРОСТО ТАК!'
        }
      },
      required: ['position', 'target']
    },
    handler: async (params) => {
      const requestBody = {
        position: params.position,
        target: params.target,
        fov: params.fov || 60,
        width: params.width || 1920,
        height: params.height || 1080
      };
      
      return await handleUnityRequest('/api/camera_screenshot', requestBody, 20000);
    }
  },

  {
    name: "scene_hierarchy",
    description: 'Unity сцена: анализ объектов и иерархии',
    inputSchema: {
      type: 'object',
      properties: {
        detailed: {
          type: 'boolean',
          default: false,
          description: 'Детальный режим: false - только имена и структура, true - + позиция, компоненты, свойства'
        },
        systemScreenshot: {
          type: 'boolean',
          default: false,
          description: '🖥️ Включить скриншот рабочего стола. ИСПОЛЬЗОВАТЬ ТОЛЬКО ПРИ СТРОГОЙ НЕОБХОДИМОСТИ УВИДЕТЬ ЭКРАН ПОЛЬЗОВАТЕЛЯ И НЕ ИСПОЛЬЗОВАТЬ ПРОСТО ТАК!'
        }
      },
      required: []
    },
    handler: async (params) => {
      const requestBody = {
        detailed: params.detailed || false
      };
      
      return await handleUnityRequest('/api/scene_hierarchy', requestBody, 15000);
    }
  },

  {
    name: "execute",
    description: 'Unity C# Code Executor - выполнение C# кода в Unity Editor.\n\n✅ ПОДДЕРЖИВАЕТСЯ:\n• Простые классы с методами и конструкторами\n• Локальные функции (автоматически static)\n• Полный Unity API (GameObject, Transform, Material, Rigidbody, etc.)\n• LINQ операции (Where, Select, GroupBy, Sum, etc.)\n• Циклы, коллекции, математические вычисления\n• Using statements, многострочный код\n\n❌ НЕ ПОДДЕРЖИВАЕТСЯ:\n• Интерфейсы, абстрактные классы, наследование\n• Внешние библиотеки (JSON.NET, System.IO)\n• Атрибуты [Serializable], [System.Flags]\n• Сложная инициализация массивов в классах\n\n🎯 ПРИМЕРЫ:\n• Создание объектов: GameObject.CreatePrimitive(PrimitiveType.Cube)\n• Классы: public class Builder { public GameObject Create() {...} }\n• Функции: GameObject CreateCube(Vector3 pos) {...}\n• LINQ: objects.Where(o => o.name.Contains("Test")).ToList()',
    inputSchema: {
      type: 'object',
      properties: {
        code: {
          type: 'string',
          description: 'C# код для выполнения в Unity Editor'
        },
        systemScreenshot: {
          type: 'boolean',
          default: false,
          description: '🖥️ Включить скриншот рабочего стола. ИСПОЛЬЗОВАТЬ ТОЛЬКО ПРИ СТРОГОЙ НЕОБХОДИМОСТИ УВИДЕТЬ ЭКРАН ПОЛЬЗОВАТЕЛЯ И НЕ ИСПОЛЬЗОВАТЬ ТОЛЬКО ПРОСТО ТАК!'
        }
      },
      required: ['code']
    },
    handler: async (params) => {
        const requestBody = {
        code: params.code
      };
      
      return await handleUnityRequest('/api/execute', requestBody, 30000);
    }
  },

  {
    name: "add_component",
    description: 'Unity Component Manager - добавление скриптов на объекты в сцене.\n\n✅ ПОДДЕРЖИВАЕТСЯ:\n• Добавление любых MonoBehaviour скриптов на GameObject\n• Автоматический поиск скриптов по имени\n• Проверка существования компонентов\n• Автоматическое обновление сцены\n\n🎯 ПРИМЕРЫ:\n• Добавить скрипт "PlayerController" на объект "Player"\n• Добавить скрипт "EnemyAI" на объект "Enemy"\n• Добавить скрипт "Collectible" на объект "Coin"',
    inputSchema: {
      type: 'object',
      properties: {
        object_name: {
          type: 'string',
          description: 'Имя GameObject в сцене, на который нужно добавить компонент'
        },
        script_name: {
          type: 'string',
          description: 'Имя скрипта (MonoBehaviour), который нужно добавить'
        },
        systemScreenshot: {
          type: 'boolean',
          default: false,
          description: '🖥️ Включить скриншот рабочего стола. ИСПОЛЬЗОВАТЬ ТОЛЬКО ПРИ СТРОГОЙ НЕОБХОДИМОСТИ УВИДЕТЬ ЭКРАН ПОЛЬЗОВАТЕЛЯ И НЕ ИСПОЛЬЗОВАТЬ ПРОСТО ТАК!'
        }
      },
      required: ['object_name', 'script_name']
    },
    handler: async (params) => {
      const requestBody = {
        object_name: params.object_name,
        script_name: params.script_name
      };
      
      return await handleUnityRequest('/api/add_component', requestBody, 15000);
    }
  },

  {
    name: "create_and_add_script",
    description: 'Unity Script Creator & Component Manager - создание скрипта и добавление его на объект.\n\n✅ ПОДДЕРЖИВАЕТСЯ:\n• Создание нового C# скрипта с заданным содержимым\n• Автоматическое добавление скрипта на указанный GameObject\n• Компиляция и проверка синтаксиса\n• Автоматическое обновление проекта Unity\n\n🎯 ПРИМЕРЫ:\n• Создать скрипт "PlayerMovement" и добавить на объект "Player"\n• Создать скрипт "EnemyBehavior" и добавить на объект "Enemy"\n• Создать скрипт "CollectibleItem" и добавить на объект "Coin"',
    inputSchema: {
      type: 'object',
      properties: {
        script_name: {
          type: 'string',
          description: 'Имя нового скрипта (без расширения .cs)'
        },
        script_content: {
          type: 'string',
          description: 'Содержимое C# скрипта (код класса)'
        },
        object_name: {
          type: 'string',
          description: 'Имя GameObject в сцене, на который нужно добавить скрипт'
        },
        systemScreenshot: {
          type: 'boolean',
          default: false,
          description: '🖥️ Включить скриншот рабочего стола. ИСПОЛЬЗОВАТЬ ТОЛЬКО ПРИ СТРОГОЙ НЕОБХОДИМОСТИ УВИДЕТЬ ЭКРАН ПОЛЬЗОВАТЕЛЯ И НЕ ИСПОЛЬЗОВАТЬ ПРОСТО ТАК!'
        }
      },
      required: ['script_name', 'script_content', 'object_name']
    },
    handler: async (params) => {
      const requestBody = {
        script_name: params.script_name,
        script_content: params.script_content,
        object_name: params.object_name
      };
      
      return await handleUnityRequest('/api/create_and_add_script', requestBody, 20000);
    }
  },

  {
    name: "create_prefab",
    description: 'Unity Prefab Creator - создание префаба из объекта на сцене.\n\n✅ ПОДДЕРЖИВАЕТСЯ:\n• Создание префаба из любого GameObject\n• Автоматическое создание папок\n• Обновление AssetDatabase\n• Валидация входных данных\n\n🎯 ПРИМЕРЫ:\n• Создать префаб из объекта "Player" в "Assets/Prefabs/Player.prefab"\n• Создать префаб из объекта "Enemy" в "Assets/Enemies/Enemy.prefab"',
    inputSchema: {
      type: 'object',
      properties: {
        object_name: {
          type: 'string',
          description: 'Имя GameObject на сцене для создания префаба'
        },
        prefab_path: {
          type: 'string',
          description: 'Путь для сохранения префаба (например: Assets/Prefabs/MyPrefab.prefab)'
        },
        systemScreenshot: {
          type: 'boolean',
          default: false,
          description: '🖥️ Включить скриншот рабочего стола. ИСПОЛЬЗОВАТЬ ТОЛЬКО ПРИ СТРОГОЙ НЕОБХОДИМОСТИ УВИДЕТЬ ЭКРАН ПОЛЬЗОВАТЕЛЯ И НЕ ИСПОЛЬЗОВАТЬ ПРОСТО ТАК!'
        }
      },
      required: ['object_name', 'prefab_path']
    },
    handler: async (params) => {
      const requestBody = {
        object_name: params.object_name,
        prefab_path: params.prefab_path
      };
      
      return await handleUnityRequest('/api/create_prefab', requestBody, 15000);
    }
  },

  {
    name: "instantiate_prefab",
    description: 'Unity Prefab Instance Manager - создание экземпляров префабов на сцене.\n\n✅ ПОДДЕРЖИВАЕТСЯ:\n• Загрузка префабов по пути\n• Создание экземпляров с настраиваемой позицией\n• Установка позиции, поворота и масштаба\n• Автоматическое обновление сцены\n\n🎯 ПРИМЕРЫ:\n• Создать экземпляр "Assets/Prefabs/Player.prefab" в позиции (0, 1, 0)\n• Создать экземпляр "Assets/Enemies/Enemy.prefab" с поворотом (0, 90, 0)',
    inputSchema: {
      type: 'object',
      properties: {
        prefab_path: {
          type: 'string',
          description: 'Путь к префабу (например: Assets/Prefabs/MyPrefab.prefab)'
        },
        position: {
          type: 'array',
          items: { type: 'number' },
          minItems: 3,
          maxItems: 3,
          description: 'Позиция экземпляра [x, y, z]',
          default: [0, 0, 0]
        },
        rotation: {
          type: 'array',
          items: { type: 'number' },
          minItems: 3,
          maxItems: 3,
          description: 'Поворот экземпляра в градусах [x, y, z]',
          default: [0, 0, 0]
        },
        scale: {
          type: 'array',
          items: { type: 'number' },
          minItems: 3,
          maxItems: 3,
          description: 'Масштаб экземпляра [x, y, z]',
          default: [1, 1, 1]
        },
        systemScreenshot: {
          type: 'boolean',
          default: false,
          description: '🖥️ Включить скриншот рабочего стола. ИСПОЛЬЗОВАТЬ ТОЛЬКО ПРИ СТРОГОЙ НЕОБХОДИМОСТИ УВИДЕТЬ ЭКРАН ПОЛЬЗОВАТЕЛЯ И НЕ ИСПОЛЬЗОВАТЬ ПРОСТО ТАК!'
        }
      },
      required: ['prefab_path']
    },
    handler: async (params) => {
      const requestBody = {
        prefab_path: params.prefab_path,
        position: params.position || [0, 0, 0],
        rotation: params.rotation || [0, 0, 0],
        scale: params.scale || [1, 1, 1]
      };
      
      return await handleUnityRequest('/api/instantiate_prefab', requestBody, 15000);
    }
  },

  {
    name: "list_prefabs",
    description: 'Unity Prefab Explorer - поиск и список всех префабов в проекте.\n\n✅ ПОДДЕРЖИВАЕТСЯ:\n• Поиск префабов в указанной папке\n• Список всех .prefab файлов\n• Фильтрация по пути поиска\n• Информация о количестве найденных префабов\n\n🎯 ПРИМЕРЫ:\n• Найти все префабы в "Assets/Prefabs"\n• Найти все префабы в "Assets"\n• Найти все префабы в "Assets/Enemies"',
    inputSchema: {
      type: 'object',
      properties: {
        search_path: {
          type: 'string',
          description: 'Путь для поиска префабов (например: Assets/Prefabs)',
          default: 'Assets'
        },
        systemScreenshot: {
          type: 'boolean',
          default: false,
          description: '🖥️ Включить скриншот рабочего стола. ИСПОЛЬЗОВАТЬ ТОЛЬКО ПРИ СТРОГОЙ НЕОБХОДИМОСТИ УВИДЕТЬ ЭКРАН ПОЛЬЗОВАТЕЛЯ И НЕ ИСПОЛЬЗОВАТЬ ПРОСТО ТАК!'
        }
      },
      required: []
    },
    handler: async (params) => {
      const requestBody = {
        search_path: params.search_path || 'Assets'
      };
      
      return await handleUnityRequest('/api/list_prefabs', requestBody, 10000);
    }
  },

  // ===== CANVAS MANAGEMENT =====
  
  {
    name: "create_canvas",
    description: 'Unity Canvas Creator - создание Canvas для UI элементов.\n\n✅ ПОДДЕРЖИВАЕТСЯ:\n• Создание Canvas с настройками\n• Различные режимы рендеринга\n• Автоматическая настройка CanvasScaler\n• Добавление GraphicRaycaster\n\n🎯 ПРИМЕРЫ:\n• Создать Canvas "MainUI" в режиме ScreenSpaceOverlay\n• Создать Canvas "WorldUI" в режиме WorldSpace',
    inputSchema: {
      type: 'object',
      properties: {
        canvas_name: {
          type: 'string',
          description: 'Имя Canvas',
          default: 'New Canvas'
        },
        render_mode: {
          type: 'string',
          enum: ['ScreenSpaceOverlay', 'ScreenSpaceCamera', 'WorldSpace'],
          description: 'Режим рендеринга Canvas',
          default: 'ScreenSpaceOverlay'
        },
        sorting_order: {
          type: 'number',
          description: 'Порядок сортировки Canvas',
          default: 0
        },
        systemScreenshot: {
          type: 'boolean',
          default: false,
          description: '🖥️ Включить скриншот рабочего стола. ИСПОЛЬЗОВАТЬ ТОЛЬКО ПРИ СТРОГОЙ НЕОБХОДИМОСТИ УВИДЕТЬ ЭКРАН ПОЛЬЗОВАТЕЛЯ И НЕ ИСПОЛЬЗОВАТЬ ПРОСТО ТАК!'
        }
      },
      required: []
    },
    handler: async (params) => {
      const requestBody = {
        canvas_name: params.canvas_name || 'New Canvas',
        render_mode: params.render_mode || 'ScreenSpaceOverlay',
        sorting_order: params.sorting_order || 0
      };
      
      return await handleUnityRequest('/api/create_canvas', requestBody, 15000);
    }
  },

  // ===== UI ELEMENTS =====
  
  {
    name: "create_ui_element",
    description: 'Unity UI Element Creator - создание UI элементов.\n\n✅ ПОДДЕРЖИВАЕТСЯ:\n• Button, Text, Image, InputField\n• Panel, Slider, Toggle, Dropdown\n• Настройка позиции и размера\n• Автоматическое создание дочерних элементов\n\n🎯 ПРИМЕРЫ:\n• Создать кнопку "StartButton" на Canvas\n• Создать текстовое поле "ScoreText" на Panel',
    inputSchema: {
      type: 'object',
      properties: {
        element_type: {
          type: 'string',
          enum: ['button', 'text', 'textmeshpro', 'image', 'inputfield', 'input', 'panel', 'slider', 'toggle', 'dropdown'],
          description: 'Тип UI элемента'
        },
        element_name: {
          type: 'string',
          description: 'Имя UI элемента'
        },
        parent_name: {
          type: 'string',
          description: 'Имя родительского объекта (Canvas или другой UI элемент)'
        },
        position: {
          type: 'array',
          items: { type: 'number' },
          minItems: 2,
          maxItems: 2,
          description: 'Позиция элемента [x, y]',
          default: [0, 0]
        },
        size: {
          type: 'array',
          items: { type: 'number' },
          minItems: 2,
          maxItems: 2,
          description: 'Размер элемента [width, height]',
          default: [100, 100]
        },
        systemScreenshot: {
          type: 'boolean',
          default: false,
          description: '🖥️ Включить скриншот рабочего стола. ИСПОЛЬЗОВАТЬ ТОЛЬКО ПРИ СТРОГОЙ НЕОБХОДИМОСТИ УВИДЕТЬ ЭКРАН ПОЛЬЗОВАТЕЛЯ И НЕ ИСПОЛЬЗОВАТЬ ПРОСТО ТАК!'
        }
      },
      required: ['element_type']
    },
    handler: async (params) => {
      const requestBody = {
        element_type: params.element_type,
        element_name: params.element_name || `New ${params.element_type}`,
        parent_name: params.parent_name,
        position: params.position || [0, 0],
        size: params.size || [100, 100]
      };
      
      return await handleUnityRequest('/api/create_ui_element', requestBody, 15000);
    }
  },

  {
    name: "set_ui_properties",
    description: 'Unity UI Properties Setter - настройка свойств UI элементов.\n\n✅ ПОДДЕРЖИВАЕТСЯ:\n• Позиция, размер, якоря\n• Pivot, rotation, scale\n• Цвет, текст, изображения\n• Анимации и переходы\n\n🎯 ПРИМЕРЫ:\n• Изменить позицию кнопки на (100, 200)\n• Установить размер панели 300x400',
    inputSchema: {
      type: 'object',
      properties: {
        object_name: {
          type: 'string',
          description: 'Имя UI объекта для настройки'
        },
        properties: {
          type: 'object',
          description: 'Словарь свойств для установки',
          properties: {
            position: {
              type: 'array',
              items: { type: 'number' },
              minItems: 2,
              maxItems: 2,
              description: 'Позиция [x, y]'
            },
            size: {
              type: 'array',
              items: { type: 'number' },
              minItems: 2,
              maxItems: 2,
              description: 'Размер [width, height]'
            },
            anchor_min: {
              type: 'array',
              items: { type: 'number' },
              minItems: 2,
              maxItems: 2,
              description: 'Минимальный якорь [x, y]'
            },
            anchor_max: {
              type: 'array',
              items: { type: 'number' },
              minItems: 2,
              maxItems: 2,
              description: 'Максимальный якорь [x, y]'
            },
            pivot: {
              type: 'array',
              items: { type: 'number' },
              minItems: 2,
              maxItems: 2,
              description: 'Pivot [x, y]'
            }
          }
        },
        systemScreenshot: {
          type: 'boolean',
          default: false,
          description: '🖥️ Включить скриншот рабочего стола. ИСПОЛЬЗОВАТЬ ТОЛЬКО ПРИ СТРОГОЙ НЕОБХОДИМОСТИ УВИДЕТЬ ЭКРАН ПОЛЬЗОВАТЕЛЯ И НЕ ИСПОЛЬЗОВАТЬ ПРОСТО ТАК!'
        }
      },
      required: ['object_name', 'properties']
    },
    handler: async (params) => {
      const requestBody = {
        object_name: params.object_name,
        properties: params.properties
      };
      
      return await handleUnityRequest('/api/set_ui_properties', requestBody, 15000);
    }
  },

  {
    name: "list_ui_elements",
    description: 'Unity UI Elements Explorer - поиск и список UI элементов.\n\n✅ ПОДДЕРЖИВАЕТСЯ:\n• Поиск всех UI элементов в сцене\n• Фильтрация по Canvas\n• Информация о типе, позиции, размере\n• Статус активности элементов\n\n🎯 ПРИМЕРЫ:\n• Найти все UI элементы\n• Найти элементы конкретного Canvas',
    inputSchema: {
      type: 'object',
      properties: {
        canvas_name: {
          type: 'string',
          description: 'Имя Canvas для фильтрации (опционально)'
        },
        systemScreenshot: {
          type: 'boolean',
          default: false,
          description: '🖥️ Включить скриншот рабочего стола. ИСПОЛЬЗОВАТЬ ТОЛЬКО ПРИ СТРОГОЙ НЕОБХОДИМОСТИ УВИДЕТЬ ЭКРАН ПОЛЬЗОВАТЕЛЯ И НЕ ИСПОЛЬЗОВАТЬ ПРОСТО ТАК!'
        }
      },
      required: []
    },
    handler: async (params) => {
      const requestBody = {
        canvas_name: params.canvas_name
      };
      
      return await handleUnityRequest('/api/list_ui_elements', requestBody, 10000);
    }
  },

  // ===== ADVANCED PREFAB MANAGEMENT =====
  
  {
    name: "create_prefab_from_selection",
    description: 'Unity Prefab Creator from Selection - создание префаба из выбранных объектов.\n\n✅ ПОДДЕРЖИВАЕТСЯ:\n• Создание префаба из одного объекта\n• Создание префаба из нескольких объектов\n• Автоматическое создание папок\n• Обновление AssetDatabase\n\n🎯 ПРИМЕРЫ:\n• Создать префаб из выбранного Player\n• Создать префаб из группы объектов',
    inputSchema: {
      type: 'object',
      properties: {
        prefab_path: {
          type: 'string',
          description: 'Путь для сохранения префаба (например: Assets/Prefabs/MyPrefab.prefab)'
        },
        prefab_name: {
          type: 'string',
          description: 'Имя префаба (для множественного выбора)',
          default: 'New Prefab'
        },
        systemScreenshot: {
          type: 'boolean',
          default: false,
          description: '🖥️ Включить скриншот рабочего стола. ИСПОЛЬЗОВАТЬ ТОЛЬКО ПРИ СТРОГОЙ НЕОБХОДИМОСТИ УВИДЕТЬ ЭКРАН ПОЛЬЗОВАТЕЛЯ И НЕ ИСПОЛЬЗОВАТЬ ПРОСТО ТАК!'
        }
      },
      required: ['prefab_path']
    },
    handler: async (params) => {
      const requestBody = {
        prefab_path: params.prefab_path,
        prefab_name: params.prefab_name || 'New Prefab'
      };
      
      return await handleUnityRequest('/api/create_prefab_from_selection', requestBody, 15000);
    }
  },

  {
    name: "update_prefab",
    description: 'Unity Prefab Updater - обновление существующего префаба.\n\n✅ ПОДДЕРЖИВАЕТСЯ:\n• Обновление префаба из объекта на сцене\n• Сохранение изменений в AssetDatabase\n• Автоматическое обновление всех экземпляров\n\n🎯 ПРИМЕРЫ:\n• Обновить префаб Player из объекта Player на сцене\n• Сохранить изменения в существующий префаб',
    inputSchema: {
      type: 'object',
      properties: {
        prefab_path: {
          type: 'string',
          description: 'Путь к префабу для обновления'
        },
        object_name: {
          type: 'string',
          description: 'Имя объекта на сцене для обновления префаба'
        },
        systemScreenshot: {
          type: 'boolean',
          default: false,
          description: '🖥️ Включить скриншот рабочего стола. ИСПОЛЬЗОВАТЬ ТОЛЬКО ПРИ СТРОГОЙ НЕОБХОДИМОСТИ УВИДЕТЬ ЭКРАН ПОЛЬЗОВАТЕЛЯ И НЕ ИСПОЛЬЗОВАТЬ ПРОСТО ТАК!'
        }
      },
      required: ['prefab_path', 'object_name']
    },
    handler: async (params) => {
      const requestBody = {
        prefab_path: params.prefab_path,
        object_name: params.object_name
      };
      
      return await handleUnityRequest('/api/update_prefab', requestBody, 15000);
    }
  },

  // ===== ADVANCED SCRIPT MANAGEMENT =====
  
  {
    name: "create_script_template",
    description: 'Unity Script Template Creator - создание скриптов по шаблонам.\n\n✅ ПОДДЕРЖИВАЕТСЯ:\n• MonoBehaviour, Singleton, UI Controller\n• Автоматическая генерация кода\n• Поддержка namespace\n• Готовые шаблоны для разных типов\n\n🎯 ПРИМЕРЫ:\n• Создать MonoBehaviour скрипт "PlayerController"\n• Создать Singleton скрипт "GameManager"',
    inputSchema: {
      type: 'object',
      properties: {
        script_name: {
          type: 'string',
          description: 'Имя скрипта (без расширения .cs)'
        },
        template_type: {
          type: 'string',
          enum: ['monobehaviour', 'singleton', 'ui_controller'],
          description: 'Тип шаблона скрипта',
          default: 'monobehaviour'
        },
        namespace_name: {
          type: 'string',
          description: 'Имя namespace (опционально)'
        },
        systemScreenshot: {
          type: 'boolean',
          default: false,
          description: '🖥️ Включить скриншот рабочего стола. ИСПОЛЬЗОВАТЬ ТОЛЬКО ПРИ СТРОГОЙ НЕОБХОДИМОСТИ УВИДЕТЬ ЭКРАН ПОЛЬЗОВАТЕЛЯ И НЕ ИСПОЛЬЗОВАТЬ ПРОСТО ТАК!'
        }
      },
      required: ['script_name']
    },
    handler: async (params) => {
      const requestBody = {
        script_name: params.script_name,
        template_type: params.template_type || 'monobehaviour',
        namespace_name: params.namespace_name
      };
      
      return await handleUnityRequest('/api/create_script_template', requestBody, 15000);
    }
  },

  {
    name: "add_component_to_all",
    description: 'Unity Mass Component Adder - добавление компонента на множество объектов.\n\n✅ ПОДДЕРЖИВАЕТСЯ:\n• Добавление компонента на все объекты\n• Фильтрация по тегам и именам\n• Проверка существования компонентов\n• Массовое обновление сцены\n\n🎯 ПРИМЕРЫ:\n• Добавить "Rigidbody" на все объекты с тегом "Physics"\n• Добавить "AudioSource" на все объекты с именем "Enemy"',
    inputSchema: {
      type: 'object',
      properties: {
        script_name: {
          type: 'string',
          description: 'Имя скрипта (MonoBehaviour) для добавления'
        },
        tag_filter: {
          type: 'string',
          description: 'Фильтр по тегу (опционально)'
        },
        name_filter: {
          type: 'string',
          description: 'Фильтр по имени (опционально)'
        },
        systemScreenshot: {
          type: 'boolean',
          default: false,
          description: '🖥️ Включить скриншот рабочего стола. ИСПОЛЬЗОВАТЬ ТОЛЬКО ПРИ СТРОГОЙ НЕОБХОДИМОСТИ УВИДЕТЬ ЭКРАН ПОЛЬЗОВАТЕЛЯ И НЕ ИСПОЛЬЗОВАТЬ ПРОСТО ТАК!'
        }
      },
      required: ['script_name']
    },
    handler: async (params) => {
      const requestBody = {
        script_name: params.script_name,
        tag_filter: params.tag_filter,
        name_filter: params.name_filter
      };
      
      return await handleUnityRequest('/api/add_component_to_all', requestBody, 20000);
    }
  },

  // ===== SCENE MANAGEMENT =====
  
  {
    name: "create_empty_scene",
    description: 'Unity Empty Scene Creator - создание новой пустой сцены.\n\n✅ ПОДДЕРЖИВАЕТСЯ:\n• Создание новой сцены\n• Автоматическое сохранение\n• Создание папки Scenes\n• Настройка по умолчанию\n\n🎯 ПРИМЕРЫ:\n• Создать сцену "MainMenu"\n• Создать сцену "Level1"',
    inputSchema: {
      type: 'object',
      properties: {
        scene_name: {
          type: 'string',
          description: 'Имя новой сцены',
          default: 'New Scene'
        },
        systemScreenshot: {
          type: 'boolean',
          default: false,
          description: '🖥️ Включить скриншот рабочего стола. ИСПОЛЬЗОВАТЬ ТОЛЬКО ПРИ СТРОГОЙ НЕОБХОДИМОСТИ УВИДЕТЬ ЭКРАН ПОЛЬЗОВАТЕЛЯ И НЕ ИСПОЛЬЗОВАТЬ ПРОСТО ТАК!'
        }
      },
      required: []
    },
    handler: async (params) => {
      const requestBody = {
        scene_name: params.scene_name || 'New Scene'
      };
      
      return await handleUnityRequest('/api/create_empty_scene', requestBody, 15000);
    }
  },

  {
    name: "load_scene",
    description: 'Unity Scene Loader - загрузка существующей сцены.\n\n✅ ПОДДЕРЖИВАЕТСЯ:\n• Загрузка сцены по пути\n• Переключение между сценами\n• Сохранение текущей сцены\n\n🎯 ПРИМЕРЫ:\n• Загрузить сцену "Assets/Scenes/MainMenu.unity"\n• Переключиться на сцену "Level1"',
    inputSchema: {
      type: 'object',
      properties: {
        scene_path: {
          type: 'string',
          description: 'Путь к файлу сцены'
        },
        systemScreenshot: {
          type: 'boolean',
          default: false,
          description: '🖥️ Включить скриншот рабочего стола. ИСПОЛЬЗОВАТЬ ТОЛЬКО ПРИ СТРОГОЙ НЕОБХОДИМОСТИ УВИДЕТЬ ЭКРАН ПОЛЬЗОВАТЕЛЯ И НЕ ИСПОЛЬЗОВАТЬ ПРОСТО ТАК!'
        }
      },
      required: ['scene_path']
    },
    handler: async (params) => {
      const requestBody = {
        scene_path: params.scene_path
      };
      
      return await handleUnityRequest('/api/load_scene', requestBody, 15000);
    }
  },

  {
    name: "save_scene",
    description: 'Unity Scene Saver - сохранение текущей сцены.\n\n✅ ПОДДЕРЖИВАЕТСЯ:\n• Сохранение текущей сцены\n• Обновление AssetDatabase\n• Проверка успешности сохранения\n\n🎯 ПРИМЕРЫ:\n• Сохранить текущую сцену\n• Сохранить изменения в сцене',
    inputSchema: {
      type: 'object',
      properties: {
        systemScreenshot: {
          type: 'boolean',
          default: false,
          description: '🖥️ Включить скриншот рабочего стола. ИСПОЛЬЗОВАТЬ ТОЛЬКО ПРИ СТРОГОЙ НЕОБХОДИМОСТИ УВИДЕТЬ ЭКРАН ПОЛЬЗОВАТЕЛЯ И НЕ ИСПОЛЬЗОВАТЬ ПРОСТО ТАК!'
        }
      },
      required: []
    },
    handler: async (params) => {
      return await handleUnityRequest('/api/save_scene', {}, 10000);
    }
  }
];

export const unityModule = {
  name: 'unity',
  description: 'Unity Bridge: прозрачный мост AI ↔ Unity3D. Выполнение любого C# кода, скриншоты, анализ сцены.',
  tools: unityTools,
  
  decorators: {
    disableSystemInfo: true,
    disableDebugLogs: true
  }
}; 