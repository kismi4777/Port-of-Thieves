using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace PortOfThieves.Audio
{
    /// <summary>
    /// Менеджер для воспроизведения случайной фоновой музыки из списка
    /// </summary>
    public class BackgroundMusicManager : MonoBehaviour
    {
        [Header("Audio Settings")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip[] backgroundMusicClips;
        [SerializeField] private bool playOnStart = true;
        [SerializeField] private bool loopMusic = false; // Изменено на false для автоматического переключения
        [SerializeField] private bool shufflePlaylist = true;
        [SerializeField] private bool autoPlayNext = true; // Автоматически играть следующий трек
        
        [Header("Volume Settings")]
        [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.5f;
        [SerializeField] private bool fadeInOnPlay = true;
        [SerializeField] private float fadeInDuration = 2f;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;
        
        // Приватные переменные
        private List<AudioClip> shuffledPlaylist;
        private int currentTrackIndex = 0;
        private bool isPlaying = false;
        private Coroutine fadeCoroutine;
        
        // События
        public static event System.Action<AudioClip> OnTrackStarted;
        public static event System.Action<AudioClip> OnTrackFinished;
        
        private void Awake()
        {
            // Автоматически найти AudioSource если не назначен
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }
            
            // Настройка AudioSource
            SetupAudioSource();
            
            // Инициализация плейлиста
            InitializePlaylist();
        }
        
        private void Start()
        {
            if (playOnStart && backgroundMusicClips.Length > 0)
            {
                PlayRandomTrack();
            }
        }
        
        private void SetupAudioSource()
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false; // Управляем циклом вручную
            audioSource.volume = musicVolume;
            audioSource.priority = 0; // Высокий приоритет для фоновой музыки
        }
        
        private void InitializePlaylist()
        {
            if (backgroundMusicClips.Length == 0)
            {
                Debug.LogWarning("BackgroundMusicManager: Нет аудиоклипов для воспроизведения!");
                return;
            }
            
            // Создаем перемешанный плейлист
            shuffledPlaylist = new List<AudioClip>(backgroundMusicClips);
            if (shufflePlaylist)
            {
                ShufflePlaylist();
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"BackgroundMusicManager: Инициализирован плейлист из {backgroundMusicClips.Length} треков");
            }
        }
        
        private void ShufflePlaylist()
        {
            for (int i = 0; i < shuffledPlaylist.Count; i++)
            {
                AudioClip temp = shuffledPlaylist[i];
                int randomIndex = Random.Range(i, shuffledPlaylist.Count);
                shuffledPlaylist[i] = shuffledPlaylist[randomIndex];
                shuffledPlaylist[randomIndex] = temp;
            }
        }
        
        /// <summary>
        /// Воспроизводит случайный трек из плейлиста
        /// </summary>
        public void PlayRandomTrack()
        {
            if (backgroundMusicClips.Length == 0)
            {
                Debug.LogWarning("BackgroundMusicManager: Нет доступных треков для воспроизведения!");
                return;
            }
            
            // Выбираем случайный трек
            int randomIndex = Random.Range(0, backgroundMusicClips.Length);
            AudioClip selectedTrack = backgroundMusicClips[randomIndex];
            
            PlayTrack(selectedTrack);
        }
        
        /// <summary>
        /// Воспроизводит следующий трек из перемешанного плейлиста
        /// </summary>
        public void PlayNextTrack()
        {
            if (shuffledPlaylist.Count == 0) return;
            
            AudioClip nextTrack = shuffledPlaylist[currentTrackIndex];
            PlayTrack(nextTrack);
            
            // Переходим к следующему треку
            currentTrackIndex = (currentTrackIndex + 1) % shuffledPlaylist.Count;
        }
        
        /// <summary>
        /// Воспроизводит указанный трек
        /// </summary>
        /// <param name="track">Аудиоклип для воспроизведения</param>
        public void PlayTrack(AudioClip track)
        {
            if (track == null)
            {
                Debug.LogWarning("BackgroundMusicManager: Попытка воспроизвести null трек!");
                return;
            }
            
            // Останавливаем текущее воспроизведение
            StopMusic();
            
            // Устанавливаем новый трек
            audioSource.clip = track;
            
            // Настраиваем цикл (всегда false для автоматического переключения)
            audioSource.loop = false;
            
            // Воспроизводим
            audioSource.Play();
            isPlaying = true;
            
            // Запускаем fade in если включен
            if (fadeInOnPlay)
            {
                StartFadeIn();
            }
            
            // Уведомляем о начале трека
            OnTrackStarted?.Invoke(track);
            
            if (showDebugInfo)
            {
                Debug.Log($"BackgroundMusicManager: Начинается воспроизведение трека '{track.name}'");
            }
            
            // Всегда запускаем корутину для автоматического переключения на следующий трек
            if (autoPlayNext)
            {
                StartCoroutine(WaitForTrackEnd());
            }
        }
        
        /// <summary>
        /// Останавливает воспроизведение музыки
        /// </summary>
        public void StopMusic()
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }
            
            audioSource.Stop();
            isPlaying = false;
            
            if (showDebugInfo)
            {
                Debug.Log("BackgroundMusicManager: Воспроизведение остановлено");
            }
        }
        
        /// <summary>
        /// Приостанавливает воспроизведение музыки
        /// </summary>
        public void PauseMusic()
        {
            if (isPlaying)
            {
                audioSource.Pause();
                isPlaying = false;
                
                if (showDebugInfo)
                {
                    Debug.Log("BackgroundMusicManager: Воспроизведение приостановлено");
                }
            }
        }
        
        /// <summary>
        /// Возобновляет воспроизведение музыки
        /// </summary>
        public void ResumeMusic()
        {
            if (!isPlaying && audioSource.clip != null)
            {
                audioSource.UnPause();
                isPlaying = true;
                
                if (showDebugInfo)
                {
                    Debug.Log("BackgroundMusicManager: Воспроизведение возобновлено");
                }
            }
        }
        
        /// <summary>
        /// Устанавливает громкость музыки
        /// </summary>
        /// <param name="volume">Громкость от 0 до 1</param>
        public void SetVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            audioSource.volume = musicVolume;
            
            if (showDebugInfo)
            {
                Debug.Log($"BackgroundMusicManager: Громкость установлена на {musicVolume:F2}");
            }
        }
        
        /// <summary>
        /// Перемешивает плейлист заново
        /// </summary>
        public void ReshufflePlaylist()
        {
            if (shuffledPlaylist != null)
            {
                ShufflePlaylist();
                currentTrackIndex = 0;
                
                if (showDebugInfo)
                {
                    Debug.Log("BackgroundMusicManager: Плейлист перемешан заново");
                }
            }
        }
        
        /// <summary>
        /// Включает/выключает автоматическое переключение на следующий трек
        /// </summary>
        /// <param name="enabled">Включить автоматическое переключение</param>
        public void SetAutoPlayNext(bool enabled)
        {
            autoPlayNext = enabled;
            
            if (showDebugInfo)
            {
                Debug.Log($"BackgroundMusicManager: Автоматическое переключение треков {(enabled ? "включено" : "выключено")}");
            }
        }
        
        private void StartFadeIn()
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            
            fadeCoroutine = StartCoroutine(FadeInCoroutine());
        }
        
        private IEnumerator FadeInCoroutine()
        {
            float startVolume = 0f;
            float targetVolume = musicVolume;
            float elapsedTime = 0f;
            
            audioSource.volume = startVolume;
            
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / fadeInDuration;
                audioSource.volume = Mathf.Lerp(startVolume, targetVolume, progress);
                yield return null;
            }
            
            audioSource.volume = targetVolume;
            fadeCoroutine = null;
        }
        
        private IEnumerator WaitForTrackEnd()
        {
            yield return new WaitUntil(() => !audioSource.isPlaying);
            
            // Уведомляем о завершении трека
            OnTrackFinished?.Invoke(audioSource.clip);
            
            if (showDebugInfo)
            {
                Debug.Log($"BackgroundMusicManager: Трек '{audioSource.clip.name}' завершился, переключаемся на следующий");
            }
            
            // Воспроизводим следующий трек (случайный или из плейлиста)
            if (shufflePlaylist)
            {
                PlayRandomTrack();
            }
            else
            {
                PlayNextTrack();
            }
        }
        
        #region Public Properties
        
        /// <summary>
        /// Текущий воспроизводимый трек
        /// </summary>
        public AudioClip CurrentTrack => audioSource.clip;
        
        /// <summary>
        /// Статус воспроизведения
        /// </summary>
        public bool IsPlaying => isPlaying;
        
        /// <summary>
        /// Текущая громкость
        /// </summary>
        public float Volume => musicVolume;
        
        /// <summary>
        /// Количество треков в плейлисте
        /// </summary>
        public int TrackCount => backgroundMusicClips.Length;
        
        #endregion
        
        #region Context Menu Methods
        
        [ContextMenu("Play Random Track")]
        private void ContextPlayRandomTrack()
        {
            PlayRandomTrack();
        }
        
        [ContextMenu("Play Next Track")]
        private void ContextPlayNextTrack()
        {
            PlayNextTrack();
        }
        
        [ContextMenu("Stop Music")]
        private void ContextStopMusic()
        {
            StopMusic();
        }
        
        [ContextMenu("Pause Music")]
        private void ContextPauseMusic()
        {
            PauseMusic();
        }
        
        [ContextMenu("Resume Music")]
        private void ContextResumeMusic()
        {
            ResumeMusic();
        }
        
        [ContextMenu("Reshuffle Playlist")]
        private void ContextReshufflePlaylist()
        {
            ReshufflePlaylist();
        }
        
        [ContextMenu("Show Track Info")]
        private void ContextShowTrackInfo()
        {
            if (CurrentTrack != null)
            {
                Debug.Log($"BackgroundMusicManager: Текущий трек: '{CurrentTrack.name}' | Длительность: {CurrentTrack.length:F2}с | Громкость: {Volume:F2}");
            }
            else
            {
                Debug.Log("BackgroundMusicManager: Нет активного трека");
            }
        }
        
        [ContextMenu("Enable Auto Play Next")]
        private void ContextEnableAutoPlayNext()
        {
            SetAutoPlayNext(true);
        }
        
        [ContextMenu("Disable Auto Play Next")]
        private void ContextDisableAutoPlayNext()
        {
            SetAutoPlayNext(false);
        }
        
        #endregion
    }
}
