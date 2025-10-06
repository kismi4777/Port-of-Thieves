# Система случайных фраз для диалогов

## Описание
Система для отображения случайных фраз в диалогах с динамической подстановкой имени объекта.

## Компоненты

### 1. RandomPhraseManager
Менеджер для хранения и выбора случайных фраз.

**Основные методы:**
- `GetRandomPhrase(string objectName)` - получить случайную фразу с подстановкой имени
- `GetRandomPhraseTemplate()` - получить шаблон фразы с placeholder {0}
- `AddPhrase(string newPhrase)` - добавить новую фразу
- `GetPhrasesCount()` - получить количество фраз

**Пример использования:**
```csharp
RandomPhraseManager manager = GetComponent<RandomPhraseManager>();
string phrase = manager.GetRandomPhrase("Амулет Шторма");
// Результат: "Арр, я потерял свой Амулет Шторма, не видел ли ты его на горизонте?"
```

### 2. SearchDialogController
Контроллер для управления текстом диалога.

**Основные методы:**
- `UpdateDialogText()` - обновить текст случайной фразой
- `SetObjectName(string objectName)` - установить имя объекта и обновить текст
- `GetObjectName()` - получить текущее имя объекта

**Настройка в Inspector:**
- **Phrase Manager** - ссылка на RandomPhraseManager
- **Dialog Text** - ссылка на TextMeshProUGUI для отображения
- **Current Object Name** - текущее имя искомого объекта
- **Update On Start** - обновлять ли текст при запуске

## Быстрый старт

### Шаг 1: Настройка объекта Search
1. На объекте **Search** уже должен быть компонент `RandomPhraseManager`
2. Добавьте компонент `SearchDialogController` на объект **Search**

### Шаг 2: Настройка связей
1. В компоненте `SearchDialogController`:
   - **Phrase Manager** → перетащите объект Search (автоматически найдется)
   - **Object Data Extractor** → перетащите объект Main Camera с компонентом ObjectDataExtractor
   - **Dialog Text** → перетащите объект Search/Canvas/Info
   - **Update On Object Change** → включите для автоматического обновления при смене объекта

### Шаг 3: Настройка форматирования текста объектов
1. В разделе **"Настройки текста объектов"**:
   - **Object Text Color** → цвет для имени объекта (по умолчанию желтый)
   - **Object Text Alpha** → прозрачность для имени объекта (0-1, по умолчанию 1)
   - **Use Object Formatting** → включить/выключить форматирование

**Форматирование:**
- Имя объекта отображается заглавными буквами
- Применяется выбранный цвет с прозрачностью через Rich Text
- Остальной текст использует настройки TextMeshPro компонента Info

### Шаг 4: Использование в коде
```csharp
// Получить контроллер
SearchDialogController controller = FindObjectOfType<SearchDialogController>();

// Обновить текст (автоматически возьмет имя из ObjectDataExtractor)
controller.UpdateDialogText();

// Получить текущее имя объекта
string objectName = controller.GetObjectName();

// Принудительно обновить фразу
controller.ForceUpdatePhrase();

// Настроить цвет для имени объекта
controller.SetObjectTextColor(Color.yellow);

// Настроить прозрачность для имени объекта
controller.SetObjectTextAlpha(0.5f);

// Настроить цвет и прозрачность одновременно
controller.SetObjectTextColorAndAlpha(Color.red, 0.8f);

// Включить/выключить форматирование имени объекта
controller.SetObjectFormattingEnabled(true);
```

## Добавление своих фраз

### Через Inspector:
1. Выберите объект с компонентом `RandomPhraseManager`
2. В поле **Search Phrases** нажмите на треугольник
3. Измените **Size** на нужное количество
4. Введите свои фразы (обязательно используйте {0} для имени объекта)

### Через код:
```csharp
RandomPhraseManager manager = GetComponent<RandomPhraseManager>();
manager.AddPhrase("Эй, капитан! Ты не видел мой {0}?");
```

## Список фраз по умолчанию

1. "Арр, я потерял свой {0}, не видел ли ты его на горизонте?"
2. "Эй, приятель! Ты случайно не находил {0}?"
3. "Мой {0} пропал! Помоги мне найти его!"
4. "Куда же подевался мой {0}? Может, ты видел?"
5. "Проклятье! Где же мой {0}? Ты не замечал его?"
6. "Слушай, дружище, не встречал ли ты {0}?"
7. "Мне нужен {0}! Ты поможешь мне его найти?"
8. "Без {0} я пропал! Ты не видел его где-нибудь?"
9. "Эх, потерял я свой {0}... Может, ты на него наткнулся?"
10. "Морские дьяволы! Где мой {0}? Ты его не брал?"

## Тестирование

### В Inspector:
1. Выберите объект с `SearchDialogController`
2. ПКМ на компоненте → **Обновить фразу**
3. Текст обновится случайной фразой

### 🔧 Контекстные меню

**SearchDialogController:**
- "Обновить фразу" - тестирование случайного выбора фразы в Editor
- "Показать текущий объект" - отображение текущего объекта из ObjectDataExtractor
- "Тест форматирования объекта" - тестирование форматирования имени объекта

### В Runtime:
```csharp
// Периодически обновлять фразу
InvokeRepeating("UpdateDialogText", 0f, 5f);
```

