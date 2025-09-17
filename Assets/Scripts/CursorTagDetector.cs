using UnityEngine;
using UnityEngine.InputSystem;

public class CursorTagDetector : MonoBehaviour
{
    [Header("Cursor Tag Detection")]
    public string currentTag = "None";
    
    [Header("Drag Settings")]
    public string draggableTag = "Draggable"; // Тег объектов, которые можно перетаскивать
    
    [Header("Drop Zone")]
    public bool useDropZone = true; // Включить зону для отпускания
    public Vector2 zoneCenter = Vector2.zero; // Центр зоны
    public Vector2 zoneSize = new Vector2(10f, 10f); // Размер зоны
    public Color zoneColor = Color.green; // Цвет зоны (для отладки)
    
    [Header("Scale Effect")]
    public bool useScaleEffect = true; // Включить эффект масштаба
    public float scaleMultiplier = 1.2f; // Множитель масштаба (1.2 = +20%)
    
    [Header("Sound Effects")]
    public bool useSoundEffects = true; // Включить звуковые эффекты
    public AudioClip pickupSound; // Звук при поднятии объекта
    public AudioClip dropSound; // Звук при опускании объекта
    public float soundVolume = 1.0f; // Громкость звуков
    
    [Header("Particle Effects")]
    public bool useParticleEffects = true; // Включить эффекты частиц
    public GameObject dropParticlePrefab; // Префаб частиц при отпускании
    public float particleDuration = 2.0f; // Время до удаления частиц (в секундах)
    
    private Camera mainCamera;
    private Mouse mouse;
    private bool isDragging = false;
    private Transform draggedObject = null;
    private Vector3 originalPosition; // Исходная позиция объекта
    private Vector3 originalScale; // Исходный масштаб объекта
    private AudioSource audioSource; // Компонент для воспроизведения звуков
    
    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
        
        mouse = Mouse.current;
        
        // Настраиваем AudioSource для воспроизведения звуков
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.volume = soundVolume;
        audioSource.loop = false;
        audioSource.priority = 0; // Высокий приоритет для быстрого воспроизведения
        audioSource.bypassEffects = true; // Отключаем эффекты для быстрого воспроизведения
        audioSource.bypassListenerEffects = true;
        audioSource.bypassReverbZones = true;
        
        Debug.Log($"Camera found: {mainCamera.name}, Position: {mainCamera.transform.position}");
    }
    
    void Update()
    {
        if (mainCamera != null && mouse != null)
        {
            Vector2 mousePosition = mouse.position.ReadValue();
            
            // Для ортогональной камеры используем другой подход
            Vector3 worldPosition;
            if (mainCamera.orthographic)
            {
                // Для ортогональной камеры
                float distance = Mathf.Abs(mainCamera.transform.position.z);
                worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, distance));
            }
            else
            {
                // Для перспективной камеры
                worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 10f));
            }
            
            // Проверяем все коллайдеры в небольшом радиусе
            Collider2D[] colliders = Physics2D.OverlapCircleAll(new Vector2(worldPosition.x, worldPosition.y), 0.1f);
            
            if (colliders.Length > 0)
            {
                currentTag = colliders[0].tag;
                Debug.Log($"Found collider: {colliders[0].name}, Tag: {colliders[0].tag}");
            }
            else
            {
                currentTag = "No Hit";
                // Показываем отладку только раз в секунду
                if (Time.time % 1f < 0.1f)
                {
                    Debug.Log($"No hit. Mouse: {mousePosition}, World: {worldPosition}, Camera ortho: {mainCamera.orthographic}");
                }
            }
            
            // Обработка перетаскивания
            HandleDragging(worldPosition);
        }
        else
        {
            currentTag = "No Camera/Mouse";
        }
    }
    
    void HandleDragging(Vector3 worldPosition)
    {
        // Проверяем нажатие левой кнопки мыши
        if (mouse.leftButton.wasPressedThisFrame)
        {
            // Проверяем коллайдеры СРАЗУ при нажатии (не полагаемся на currentTag)
            Collider2D[] colliders = Physics2D.OverlapCircleAll(new Vector2(worldPosition.x, worldPosition.y), 0.1f);
            if (colliders.Length > 0 && colliders[0].tag == draggableTag)
            {
                // Воспроизводим звук поднятия СРАЗУ при подборе объекта
                PlayPickupSound();
                
                isDragging = true;
                draggedObject = colliders[0].transform;
                // Запоминаем исходную позицию и масштаб объекта
                originalPosition = draggedObject.position;
                originalScale = draggedObject.localScale;
                
                // Увеличиваем масштаб при взятии объекта
                if (useScaleEffect)
                {
                    draggedObject.localScale = originalScale * scaleMultiplier;
                }
                
                Debug.Log($"Started dragging: {draggedObject.name} from {originalPosition}");
            }
        }
        
        // Если кнопка мыши отпущена
        if (mouse.leftButton.wasReleasedThisFrame)
        {
            if (isDragging)
            {
                // Воспроизводим звук опускания СРАЗУ при отпускании перетаскиваемого объекта
                PlayDropSound();
                
                // Восстанавливаем исходный масштаб
                if (useScaleEffect)
                {
                    draggedObject.localScale = originalScale;
                }
                
                // Проверяем, можно ли отпустить объект в этой позиции
                if (CanDropAtPosition(worldPosition))
                {
                    // Объект остается в новой позиции
                    draggedObject.position = worldPosition;
                    // Воспроизводим частицы при успешном отпускании
                    PlayDropParticles(worldPosition);
                    Debug.Log($"Dropped: {draggedObject.name} at {worldPosition}");
                }
                else
                {
                    // Возвращаем объект на исходную позицию
                    draggedObject.position = originalPosition;
                    // Воспроизводим частицы при возврате
                    PlayDropParticles(originalPosition);
                    Debug.Log($"Returned: {draggedObject.name} to {originalPosition} (outside drop zone)");
                }
                
                isDragging = false;
                draggedObject = null;
            }
        }
        
        // Если перетаскиваем объект - можно двигать везде
        if (isDragging && draggedObject != null)
        {
            // Обновляем позицию объекта - центр объекта следует за курсором
            draggedObject.position = worldPosition;
        }
    }
    
    // Проверяет, можно ли отпустить объект в данной позиции
    bool CanDropAtPosition(Vector3 position)
    {
        if (!useDropZone)
            return true; // Если зона отключена, можно отпускать везде
        
        float minX = zoneCenter.x - zoneSize.x / 2f;
        float maxX = zoneCenter.x + zoneSize.x / 2f;
        float minY = zoneCenter.y - zoneSize.y / 2f;
        float maxY = zoneCenter.y + zoneSize.y / 2f;
        
        return position.x >= minX && position.x <= maxX && 
               position.y >= minY && position.y <= maxY;
    }
    
    // Воспроизводит звук поднятия объекта
    void PlayPickupSound()
    {
        if (useSoundEffects && pickupSound != null && audioSource != null)
        {
            // Останавливаем текущий звук
            audioSource.Stop();
            // Устанавливаем звук и воспроизводим напрямую (быстрее чем PlayOneShot)
            audioSource.clip = pickupSound;
            audioSource.volume = soundVolume;
            audioSource.Play();
        }
    }
    
    // Воспроизводит звук опускания объекта
    void PlayDropSound()
    {
        if (useSoundEffects && dropSound != null && audioSource != null)
        {
            // Останавливаем текущий звук
            audioSource.Stop();
            // Устанавливаем звук и воспроизводим напрямую (быстрее чем PlayOneShot)
            audioSource.clip = dropSound;
            audioSource.volume = soundVolume;
            audioSource.Play();
        }
    }
    
    // Воспроизводит частицы при отпускании объекта
    void PlayDropParticles(Vector3 position)
    {
        if (useParticleEffects && dropParticlePrefab != null)
        {
            // Создаем экземпляр префаба частиц в позиции отпускания
            GameObject particleInstance = Instantiate(dropParticlePrefab, position, Quaternion.identity);
            
            // Принудительно запускаем частицы
            ParticleSystem[] particleSystems = particleInstance.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in particleSystems)
            {
                ps.Play();
            }
            
            // Уничтожаем частицы через заданное время
            StartCoroutine(DestroyParticlesAfterDelay(particleInstance));
        }
    }
    
    // Уничтожает частицы через заданное время
    System.Collections.IEnumerator DestroyParticlesAfterDelay(GameObject particleInstance)
    {
        // Ждем заданное время
        yield return new WaitForSeconds(particleDuration);
        
        if (particleInstance != null)
        {
            // Останавливаем все частицы перед удалением
            ParticleSystem[] particleSystems = particleInstance.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in particleSystems)
            {
                ps.Stop();
            }
            
            // Уничтожаем объект
            Destroy(particleInstance);
        }
    }
    
    // Визуализация зоны в Scene View (только в редакторе)
    void OnDrawGizmos()
    {
        if (useDropZone)
        {
            Gizmos.color = zoneColor;
            Gizmos.DrawWireCube(new Vector3(zoneCenter.x, zoneCenter.y, 0), new Vector3(zoneSize.x, zoneSize.y, 0));
            
            // Показываем центр зоны
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(new Vector3(zoneCenter.x, zoneCenter.y, 0), 0.2f);
        }
    }
}
