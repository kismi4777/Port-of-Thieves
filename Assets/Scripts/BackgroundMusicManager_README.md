# BackgroundMusicManager - Система управления фоновой музыкой

## 🎯 Архитектура системы

**Основное назначение:**
Система для воспроизведения случайной фоновой музыки из списка аудиоклипов с поддержкой перемешивания плейлиста, fade-in эффектов и автоматического переключения треков.

## 🏗️ Компоненты системы

### 1. Основные настройки
```csharp
[Header("Audio Settings")]
[SerializeField] private AudioSource audioSource;           // Компонент для воспроизведения
[SerializeField] private AudioClip[] backgroundMusicClips;  // Массив аудиоклипов
[SerializeField] private bool playOnStart = true;          // Воспроизводить при старте
[SerializeField] private bool loopMusic = false;           // Зацикливать треки (по умолчанию false)
[SerializeField] private bool shufflePlaylist = true;       // Перемешивать плейлист
[SerializeField] private bool autoPlayNext = true;         // Автоматически играть следующий трек
```

### 2. Настройки громкости
```csharp
[Header("Volume Settings")]
[SerializeField] [Range(0f, 1f)] private float musicVolume = 0.5f;  // Громкость музыки
[SerializeField] private bool fadeInOnPlay = true;                 // Fade-in при воспроизведении
[SerializeField] private float fadeInDuration = 2f;                 // Длительность fade-in
```

### 3. Система событий
```csharp
// События для интеграции с другими системами
public static event System.Action<AudioClip> OnTrackStarted;   // Трек начался
public static event System.Action<AudioClip> OnTrackFinished;  // Трек завершился
```

## 📊 API методы

### Основные методы воспроизведения
```csharp
// Воспроизвести случайный трек
public void PlayRandomTrack()

// Воспроизвести следующий трек из плейлиста
public void PlayNextTrack()

// Воспроизвести конкретный трек
public void PlayTrack(AudioClip track)

// Остановить воспроизведение
public void StopMusic()

// Приостановить воспроизведение
public void PauseMusic()

// Возобновить воспроизведение
public void ResumeMusic()
```

### Управление громкостью и плейлистом
```csharp
// Установить громкость (0-1)
public void SetVolume(float volume)

// Перемешать плейлист заново
public void ReshufflePlaylist()

// Включить/выключить автоматическое переключение треков
public void SetAutoPlayNext(bool enabled)
```

### Публичные свойства
```csharp
public AudioClip CurrentTrack { get; }  // Текущий трек
public bool IsPlaying { get; }          // Статус воспроизведения
public float Volume { get; }            // Текущая громкость
public int TrackCount { get; }           // Количество треков
```

## 🔄 Логика работы

### 1. Инициализация
- Автоматический поиск или создание AudioSource
- Настройка параметров воспроизведения
- Создание перемешанного плейлиста

### 2. Воспроизведение треков
```csharp
// Алгоритм случайного выбора
int randomIndex = Random.Range(0, backgroundMusicClips.Length);
AudioClip selectedTrack = backgroundMusicClips[randomIndex];
PlayTrack(selectedTrack);
```

### 3. Система плейлиста
- Создание перемешанного списка треков
- Последовательное воспроизведение при отключенном цикле
- Автоматическое переключение на следующий трек

### 4. Fade-in эффекты
```csharp
// Плавное увеличение громкости
private IEnumerator FadeInCoroutine()
{
    float startVolume = 0f;
    float targetVolume = musicVolume;
    
    while (elapsedTime < fadeInDuration)
    {
        audioSource.volume = Mathf.Lerp(startVolume, targetVolume, progress);
        yield return null;
    }
}
```

## 🎮 Настройка в Unity

### 1. Добавление компонента
1. Создайте пустой GameObject в сцене
2. Добавьте компонент `BackgroundMusicManager`
3. Настройте параметры в Inspector

### 2. Настройка аудиоклипов
1. Перетащите аудиофайлы из папки `Assets/Sound/Tem/` в поле `Background Music Clips`
2. Рекомендуемые форматы: MP3, WAV, OGG
3. Оптимальная длительность: 2-5 минут для фоновой музыки

### 3. Настройки воспроизведения
- **Play On Start**: ✅ true (автоматический запуск)
- **Loop Music**: ❌ false (не зацикливать, автоматически переключать)
- **Shuffle Playlist**: ✅ true (перемешивание)
- **Auto Play Next**: ✅ true (автоматическое переключение на следующий трек)
- **Music Volume**: 0.5 (50% громкости)
- **Fade In On Play**: ✅ true (плавное появление)
- **Fade In Duration**: 2.0 (2 секунды fade-in)

## 📈 Примеры использования

### Базовое использование
```csharp
BackgroundMusicManager musicManager = FindObjectOfType<BackgroundMusicManager>();

// Воспроизвести случайный трек
musicManager.PlayRandomTrack();

// Установить громкость
musicManager.SetVolume(0.7f);

// Остановить музыку
musicManager.StopMusic();
```

### Интеграция с событиями
```csharp
void Start()
{
    BackgroundMusicManager.OnTrackStarted += OnTrackStarted;
    BackgroundMusicManager.OnTrackFinished += OnTrackFinished;
}

void OnTrackStarted(AudioClip track)
{
    Debug.Log($"Начался трек: {track.name}");
}

void OnTrackFinished(AudioClip track)
{
    Debug.Log($"Завершился трек: {track.name}");
}
```

### Управление через UI
```csharp
public class MusicUI : MonoBehaviour
{
    [SerializeField] private BackgroundMusicManager musicManager;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Button playButton;
    [SerializeField] private Button stopButton;
    
    void Start()
    {
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        playButton.onClick.AddListener(() => musicManager.PlayRandomTrack());
        stopButton.onClick.AddListener(() => musicManager.StopMusic());
    }
    
    void OnVolumeChanged(float volume)
    {
        musicManager.SetVolume(volume);
    }
}
```

## 🎯 Контекстные меню для тестирования

Доступны через правый клик на компоненте в Inspector:
- **"Play Random Track"** - воспроизвести случайный трек
- **"Play Next Track"** - воспроизвести следующий трек
- **"Stop Music"** - остановить воспроизведение
- **"Pause Music"** - приостановить воспроизведение
- **"Resume Music"** - возобновить воспроизведение
- **"Reshuffle Playlist"** - перемешать плейлист
- **"Show Track Info"** - показать информацию о текущем треке
- **"Enable Auto Play Next"** - включить автоматическое переключение
- **"Disable Auto Play Next"** - выключить автоматическое переключение

## ⚡ Особенности системы

### Автоматическая настройка
- Автоматический поиск или создание AudioSource
- Оптимальные настройки для фоновой музыки
- Высокий приоритет воспроизведения

### Гибкое управление
- Поддержка как случайного, так и последовательного воспроизведения
- Настраиваемые fade-in эффекты
- Возможность зацикливания отдельных треков

### Производительность
- Эффективное управление памятью
- Оптимизированные корутины для fade-эффектов
- Минимальное влияние на производительность

### Расширяемость
- Система событий для интеграции с другими компонентами
- Легкое добавление новых треков через Inspector
- Поддержка различных форматов аудио

## 📋 Интеграция с проектом

### Рекомендуемая структура сцены
```
Main Camera
├── BackgroundMusicManager (компонент)
└── AudioSource (автоматически создается)
```

### Настройка аудиоклипов
1. Поместите аудиофайлы в `Assets/Sound/Tem/`
2. Добавьте их в массив `Background Music Clips`
3. Настройте параметры воспроизведения

### Интеграция с другими системами
- **ResourceManager**: события при изменении ресурсов
- **ClientManager**: управление музыкой при смене состояний
- **UI системы**: кнопки управления воспроизведением

## 🔧 Отладка

### Отладочные сообщения
```
BackgroundMusicManager: Инициализирован плейлист из 6 треков
BackgroundMusicManager: Начинается воспроизведение трека 'temv1'
BackgroundMusicManager: Громкость установлена на 0.50
BackgroundMusicManager: Плейлист перемешан заново
```

### Диагностика проблем
1. Проверьте наличие аудиоклипов в массиве
2. Убедитесь что AudioSource настроен корректно
3. Проверьте настройки громкости и fade-эффектов
4. Используйте контекстные меню для тестирования

## 📊 Статус

**Статус:** ✅ **СИСТЕМА ПОЛНОСТЬЮ РЕАЛИЗОВАНА И ГОТОВА К ИСПОЛЬЗОВАНИЮ**

### Реализованные функции:
- ✅ Случайное воспроизведение треков
- ✅ Перемешивание плейлиста
- ✅ Fade-in эффекты
- ✅ Автоматическое переключение треков
- ✅ Система событий
- ✅ Контекстные меню для тестирования
- ✅ Полная документация

### Готовые аудиоклипы:
- ✅ temv1.MP3
- ✅ temv2.MP3  
- ✅ temv3.MP3
- ✅ temv4.MP3
- ✅ temv5.MP3
- ✅ temv6.MP3
