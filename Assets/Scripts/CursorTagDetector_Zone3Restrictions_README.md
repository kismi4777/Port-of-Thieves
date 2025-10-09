# CursorTagDetector - Zone 3 Object Restrictions

## Описание
Система ограничений для Zone 3, которая позволяет размещать и удалять только объекты, выбранные ObjectDataExtractor.

## Новые возможности

### 🔒 Ограничения Zone 3
- **Проверка соответствия** - только объекты, соответствующие extracted data, могут быть размещены в zone 3
- **Блокировка размещения** - несоответствующие объекты не могут быть размещены в zone 3
- **Блокировка удаления** - несоответствующие объекты не могут быть удалены в zone 3

### 🔧 Настройки в Inspector

#### Zone 3 Object Restrictions
- **Use Zone 3 Object Restrictions** - включить ограничения для объектов в zone 3
- **Object Data Extractor** - ссылка на ObjectDataExtractor для проверки соответствия
- **Auto Find Object Data Extractor** - автоматический поиск ObjectDataExtractor
- **Show Zone 3 Restriction Debug Info** - показывать отладочную информацию об ограничениях

#### Zone 3 Particle Effects
- **Zone 3 Success Particle Prefab** - префаб партиклов при успешном удалении правильного объекта
- **Zone 3 Blocked Particle Prefab** - префаб партиклов при блокировке неправильного объекта
- **Use Zone 3 Particle Effects** - включить партиклы для zone 3

### 🎯 Контекстные меню для тестирования
- **"Принудительно найти ObjectDataExtractor"** - поиск компонента на сцене
- **"Показать текущие extracted data"** - отображение данных из ObjectDataExtractor
- **"Тест соответствия объекта"** - проверка соответствия перетаскиваемого объекта
- **"Тест партиклов Zone 3 - Успех"** - тестирование партиклов успешного удаления
- **"Тест партиклов Zone 3 - Блокировка"** - тестирование партиклов блокировки

## Логика работы

### 1. Размещение объектов в Zone 3
```csharp
// Проверка соответствия перед размещением
if (obj != null && !IsObjectMatchingExtractedData(obj))
{
    // Объект не соответствует - доступ запрещен
    return false;
}
```

### 2. Удаление объектов в Zone 3
```csharp
// Проверка соответствия перед удалением
if (useZone3ObjectRestrictions && !IsObjectMatchingExtractedData(draggedObject.gameObject))
{
    // Объект не соответствует - воспроизводим партиклы блокировки и возвращаем
    PlayZone3BlockedParticles(worldPosition);
    draggedObject.position = originalPosition;
    return;
}

// Объект соответствует - воспроизводим партиклы успеха и удаляем
PlayZone3SuccessParticles(worldPosition);
StartCoroutine(DestroyObjectWithDelay(draggedObject.gameObject));
```

## Отладочные сообщения

### При успешном размещении:
```
CursorTagDetector: Проверка соответствия объекта 'Sword(Clone)' с extracted data 'Sword(Clone)' - СООТВЕТСТВУЕТ
Debug.Log: Объект помещен в третью дроп зону
Zone 3: Воспроизведены партиклы успешного удаления в позиции (x, y, z)
```

### При блокировке размещения:
```
CursorTagDetector: Проверка соответствия объекта 'Shield(Clone)' с extracted data 'Sword(Clone)' - НЕ СООТВЕТСТВУЕТ
CursorTagDetector: Zone 3: Объект 'Shield(Clone)' не соответствует extracted data - доступ запрещен
Zone 3: Воспроизведены партиклы блокировки в позиции (x, y, z)
```

### При блокировке удаления:
```
CursorTagDetector: Zone 3: Объект 'Shield(Clone)' не соответствует extracted data - удаление запрещено
Zone 3: Воспроизведены партиклы блокировки в позиции (x, y, z)
```

## Интеграция с ClientManager

Система ограничений интегрирована с ClientManager:
- Client отключается только при удалении соответствующих объектов
- Несоответствующие объекты не влияют на состояние Client
- Полная совместимость с существующей логикой проверки соответствия

## Рекомендуемые настройки

### CursorTagDetector:
- ✅ **Use Zone 3 Object Restrictions**: true
- ✅ **Auto Find Object Data Extractor**: true
- ✅ **Show Zone 3 Restriction Debug Info**: true
- ✅ **Use Drop Zone 3 Destroy**: true
- ✅ **Use Zone 3 Particle Effects**: true
- ✅ **Zone 3 Success Particle Prefab**: назначить префаб для успешного удаления
- ✅ **Zone 3 Blocked Particle Prefab**: назначить префаб для блокировки

### ClientManager:
- ✅ **Check Object Matching**: true
- ✅ **Only Deactivate On Matching Object**: true

## Тестирование

1. **Настройте ObjectDataExtractor** - выберите объект для извлечения данных
2. **Включите ограничения** - установите `Use Zone 3 Object Restrictions` в true
3. **Настройте партиклы** - назначьте префабы для успешного удаления и блокировки
4. **Протестируйте размещение** - попробуйте разместить разные объекты в zone 3
5. **Протестируйте удаление** - попробуйте удалить разные объекты в zone 3
6. **Проверьте партиклы** - убедитесь в корректном воспроизведении партиклов
7. **Проверьте логи** - убедитесь в корректной работе отладочных сообщений
8. **Используйте контекстные меню** - для тестирования партиклов и соответствия объектов
