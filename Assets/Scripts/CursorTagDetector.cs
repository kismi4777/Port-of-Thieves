using UnityEngine;
using UnityEngine.InputSystem;

public class CursorTagDetector : MonoBehaviour
{
    [Header("Cursor Tag Detection")]
    public string currentTag = "None";
    
    [Header("Drag Settings")]
    public string draggableTag = "Draggable"; // Тег объектов, которые можно перетаскивать
    
    [Header("Drop Zones")]
    public bool useDropZone = true; // Включить зоны для отпускания
    public Vector2 zone1Center = Vector2.zero; // Центр первой зоны
    public Vector2 zone1Size = new Vector2(10f, 10f); // Размер первой зоны
    public Color zone1Color = Color.green; // Цвет первой зоны (для отладки)
    
    public Vector2 zone2Center = new Vector2(5f, 5f); // Центр второй зоны
    public Vector2 zone2Size = new Vector2(8f, 8f); // Размер второй зоны
    public Color zone2Color = Color.blue; // Цвет второй зоны (для отладки)
    
    public Vector2 zone3Center = new Vector2(-5f, 5f); // Центр третьей зоны
    public Vector2 zone3Size = new Vector2(6f, 6f); // Размер третьей зоны
    public Color zone3Color = Color.yellow; // Цвет третьей зоны (для отладки)
    
    [Header("Scale Effect")]
    public bool useScaleEffect = true; // Включить эффект масштаба
    public float scaleMultiplier = 1.2f; // Множитель масштаба (1.2 = +20%)
    
    [Header("Drop Zone 2 Scale Effect")]
    public bool useDropZone2ScaleEffect = true; // Включить эффект масштаба для drop zone 2
    public float dropZone2ScaleMultiplier = 1.5f; // Множитель масштаба для drop zone 2 (1.5 = +50%)
    
    [Header("Drop Zone 3 Scale Effect")]
    public bool useDropZone3ScaleEffect = true; // Включить эффект масштаба для drop zone 3
    public float dropZone3ScaleMultiplier = 1.8f; // Множитель масштаба для drop zone 3 (1.8 = +80%)
    
    [Header("Drop Zone 3 Destroy Effect")]
    public bool useDropZone3Destroy = true; // Включить удаление объектов в drop zone 3
    public float destroyDelay = 0.1f; // Задержка перед удалением (в секундах)
    
    [Header("Sound Effects")]
    public bool useSoundEffects = true; // Включить звуковые эффекты
    public AudioClip pickupSound; // Звук при поднятии объекта
    public AudioClip dropSound; // Звук при опускании объекта
    public AudioClip destroySound; // Звук при удалении объекта
    public float soundVolume = 1.0f; // Громкость звуков
    
    [Header("Particle Effects")]
    public bool useParticleEffects = true; // Включить эффекты частиц
    public GameObject dropParticlePrefab; // Префаб частиц при отпускании
    public GameObject destroyParticlePrefab; // Префаб частиц при удалении
    public float particleDuration = 2.0f; // Время до удаления частиц (в секундах)
    
    [Header("PrefabSpawner Integration")]
    public PrefabSpawner prefabSpawner; // Ссылка на PrefabSpawner для уведомлений
    
    
    private Camera mainCamera;
    private Mouse mouse;
    private bool isDragging = false;
    private Transform draggedObject = null;
    private Vector3 originalPosition; // Исходная позиция объекта
    private Vector3 originalScale; // Исходный масштаб объекта
    private Vector3 trueOriginalScale; // Истинно исходный масштаб объекта (без эффектов)
    private bool isInDropZone2 = false; // Флаг нахождения в drop zone 2
    private bool isInDropZone3 = false; // Флаг нахождения в drop zone 3
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
                
                // Проверяем, был ли объект в drop zone 2 или 3 перед взятием
                bool wasInDropZone2 = IsPositionInDropZone2(draggedObject.position);
                bool wasInDropZone3 = IsPositionInDropZone3(draggedObject.position);
                isInDropZone2 = false; // Сбрасываем флаг, так как объект взят
                isInDropZone3 = false; // Сбрасываем флаг, так как объект взят
                
                // Вычисляем истинно исходный масштаб
                if (wasInDropZone2 && useDropZone2ScaleEffect)
                {
                    // Если объект был в drop zone 2, восстанавливаем истинно исходный масштаб
                    trueOriginalScale = draggedObject.localScale / dropZone2ScaleMultiplier;
                    Debug.Log($"Объект был в drop zone 2, истинно исходный масштаб: {draggedObject.localScale} / {dropZone2ScaleMultiplier} = {trueOriginalScale}");
                }
                else if (wasInDropZone3 && useDropZone3ScaleEffect)
                {
                    // Если объект был в drop zone 3, восстанавливаем истинно исходный масштаб
                    trueOriginalScale = draggedObject.localScale / dropZone3ScaleMultiplier;
                    Debug.Log($"Объект был в drop zone 3, истинно исходный масштаб: {draggedObject.localScale} / {dropZone3ScaleMultiplier} = {trueOriginalScale}");
                }
                else
                {
                    // Если объект не был в специальных зонах, текущий масштаб и есть исходный
                    trueOriginalScale = draggedObject.localScale;
                    Debug.Log($"Объект не был в специальных зонах, истинно исходный масштаб: {trueOriginalScale}");
                }
                
                // Уведомляем PrefabSpawner о том, что объект был забран и начал перетаскивание
                if (prefabSpawner != null)
                {
                    prefabSpawner.MarkObjectAsDragging(draggedObject.gameObject);
                    Debug.Log($"Объект {draggedObject.name} взят для перетаскивания");
                }
                
                // Устанавливаем масштаб при взятии объекта
                if (useScaleEffect)
                {
                    // Всегда используем истинно исходный масштаб для эффекта перетаскивания
                    draggedObject.localScale = trueOriginalScale * scaleMultiplier;
                    
                    if (wasInDropZone2 && useDropZone2ScaleEffect)
                    {
                        Debug.Log($"Объект {draggedObject.name} взят из drop zone 2, масштаб восстановлен к обычному перетаскиванию: {trueOriginalScale} * {scaleMultiplier} = {draggedObject.localScale}");
                    }
                    else
                    {
                        Debug.Log($"Объект {draggedObject.name} взят, применен эффект перетаскивания: {trueOriginalScale} * {scaleMultiplier} = {draggedObject.localScale}");
                    }
                }
                
                Debug.Log($"Started dragging: {draggedObject.name} from {originalPosition}");
            }
        }
        
        // Если кнопка мыши отпущена
        if (mouse.leftButton.wasReleasedThisFrame)
        {
            if (isDragging && draggedObject != null)
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
                    
                    // Проверяем, находится ли объект в drop zone 2 или 3 и изменяем масштаб
                    if (useDropZone2ScaleEffect && IsPositionInDropZone2(worldPosition))
                    {
                        draggedObject.localScale = trueOriginalScale * dropZone2ScaleMultiplier;
                        isInDropZone2 = true;
                        isInDropZone3 = false;
                        Debug.Log($"Объект {draggedObject.name} помещен в drop zone 2, масштаб увеличен: {trueOriginalScale} * {dropZone2ScaleMultiplier} = {draggedObject.localScale}");
                    }
                    else if (IsPositionInDropZone3(worldPosition))
                    {
                        // Проверяем, нужно ли удалить объект в третьей зоне
                        if (useDropZone3Destroy)
                        {
                            // Уведомляем PrefabSpawner об удалении объекта
                            if (prefabSpawner != null)
                            {
                                prefabSpawner.MarkObjectAsDestroyed(draggedObject.gameObject);
                                Debug.Log($"Объект {draggedObject.name} помещен в drop zone 3 для удаления");
                            }
                            
                            // Запускаем корутину удаления с задержкой
                            StartCoroutine(DestroyObjectWithDelay(draggedObject.gameObject));
                            
                            // Сохраняем имя объекта для лога
                            string objectName = draggedObject.name;
                            
                            // Сбрасываем переменные перетаскивания
                            isDragging = false;
                            draggedObject = null;
                            isInDropZone2 = false;
                            isInDropZone3 = false;
                            
                            Debug.Log($"Объект {objectName} будет удален через {destroyDelay} секунд");
                        }
                        else if (useDropZone3ScaleEffect)
                        {
                            // Если удаление отключено, применяем только масштабирование
                            draggedObject.localScale = trueOriginalScale * dropZone3ScaleMultiplier;
                            isInDropZone2 = false;
                            isInDropZone3 = true;
                            Debug.Log($"Объект {draggedObject.name} помещен в drop zone 3, масштаб увеличен: {trueOriginalScale} * {dropZone3ScaleMultiplier} = {draggedObject.localScale}");
                        }
                    }
                    else
                    {
                        // Если не в специальных зонах, восстанавливаем исходный масштаб
                        draggedObject.localScale = trueOriginalScale;
                        isInDropZone2 = false;
                        isInDropZone3 = false;
                        Debug.Log($"Объект {draggedObject.name} помещен в drop zone 1, масштаб восстановлен к исходному: {trueOriginalScale}");
                    }
                    
                    // Воспроизводим частицы при успешном отпускании
                    PlayDropParticles(worldPosition);
                    
                    // Уведомляем PrefabSpawner о том, что объект помещен в drop zone
                    if (prefabSpawner != null && draggedObject != null)
                    {
                        prefabSpawner.MarkObjectAsInDropZone(draggedObject.gameObject);
                        Debug.Log($"Объект {draggedObject.name} помещен в drop zone");
                    }
                    
                    if (draggedObject != null)
                    {
                        Debug.Log($"Dropped: {draggedObject.name} at {worldPosition}");
                    }
                }
                else
                {
                    if (draggedObject != null)
                    {
                        // Возвращаем объект на исходную позицию
                        draggedObject.position = originalPosition;
                        
                        // Восстанавливаем исходный масштаб при возврате
                        if (useDropZone2ScaleEffect || useDropZone3ScaleEffect)
                        {
                            draggedObject.localScale = trueOriginalScale;
                            isInDropZone2 = false;
                            isInDropZone3 = false;
                            Debug.Log($"Объект {draggedObject.name} возвращен, масштаб восстановлен к исходному: {trueOriginalScale}");
                        }
                        
                        // Воспроизводим частицы при возврате
                        PlayDropParticles(draggedObject.position);
                        
                        // Уведомляем PrefabSpawner о том, что объект вне drop zone
                        if (prefabSpawner != null)
                        {
                            prefabSpawner.MarkObjectAsOutOfDropZone(draggedObject.gameObject);
                        }
                        
                        Debug.Log($"Returned: {draggedObject.name} to spawn point (outside drop zone)");
                    }
                }
                
                // Уведомляем PrefabSpawner о завершении перетаскивания
                if (prefabSpawner != null && draggedObject != null)
                {
                    prefabSpawner.MarkObjectAsDropped(draggedObject.gameObject);
                    Debug.Log($"Перетаскивание объекта {draggedObject.name} завершено");
                }
                
                // Сбрасываем флаги нахождения в специальных зонах
                isInDropZone2 = false;
                isInDropZone3 = false;
                isDragging = false;
                draggedObject = null;
            }
            else if (isDragging && draggedObject == null)
            {
                // Объект был уничтожен во время перетаскивания
                Debug.LogWarning("Перетаскиваемый объект был уничтожен во время перетаскивания");
                isDragging = false;
            }
        }
        
        // Если перетаскиваем объект - можно двигать везде
        if (isDragging && draggedObject != null)
        {
            // Обновляем позицию объекта - центр объекта следует за курсором
            draggedObject.position = worldPosition;
        }
        else if (isDragging && draggedObject == null)
        {
            // Объект был удален, прекращаем перетаскивание
            Debug.LogWarning("Перетаскиваемый объект был уничтожен во время перетаскивания");
            isDragging = false;
        }
    }
    
    // Проверяет, можно ли отпустить объект в данной позиции
    public bool CanDropAtPosition(Vector3 position)
    {
        if (!useDropZone)
            return true; // Если зона отключена, можно отпускать везде
        
        // Проверяем первую дроп зону
        if (IsPositionInZone(position, zone1Center, zone1Size))
        {
            Debug.Log("Объект помещен в первую дроп зону");
            return true;
        }
        
        // Проверяем вторую дроп зону
        if (IsPositionInZone(position, zone2Center, zone2Size))
        {
            Debug.Log("Объект помещен во вторую дроп зону");
            return true;
        }
        
        // Проверяем третью дроп зону
        if (IsPositionInZone(position, zone3Center, zone3Size))
        {
            Debug.Log("Объект помещен в третью дроп зону");
            return true;
        }
        
        return false; // Позиция не входит ни в одну из дроп зон
    }
    
    // Проверяет, находится ли позиция в указанной зоне
    private bool IsPositionInZone(Vector3 position, Vector2 zoneCenter, Vector2 zoneSize)
    {
        float minX = zoneCenter.x - zoneSize.x / 2f;
        float maxX = zoneCenter.x + zoneSize.x / 2f;
        float minY = zoneCenter.y - zoneSize.y / 2f;
        float maxY = zoneCenter.y + zoneSize.y / 2f;
        
        return position.x >= minX && position.x <= maxX && 
               position.y >= minY && position.y <= maxY;
    }
    
    // Проверяет, находится ли позиция в drop zone 2
    public bool IsPositionInDropZone2(Vector3 position)
    {
        return IsPositionInZone(position, zone2Center, zone2Size);
    }
    
    // Проверяет, находится ли позиция в drop zone 3
    public bool IsPositionInDropZone3(Vector3 position)
    {
        return IsPositionInZone(position, zone3Center, zone3Size);
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
    
    // Воспроизводит звук удаления объекта
    void PlayDestroySound()
    {
        if (useSoundEffects && destroySound != null && audioSource != null)
        {
            // Останавливаем текущий звук
            audioSource.Stop();
            // Устанавливаем звук и воспроизводим напрямую (быстрее чем PlayOneShot)
            audioSource.clip = destroySound;
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
    
    // Воспроизводит эффекты при удалении объекта
    void PlayDestroyEffects(Vector3 position)
    {
        // Воспроизводим звук удаления
        PlayDestroySound();
        
        // Воспроизводим частицы удаления
        if (useParticleEffects && destroyParticlePrefab != null)
        {
            // Создаем экземпляр префаба частиц в позиции удаления
            GameObject particleInstance = Instantiate(destroyParticlePrefab, position, Quaternion.identity);
            
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
    
    // Уничтожает объект с задержкой
    System.Collections.IEnumerator DestroyObjectWithDelay(GameObject objectToDestroy)
    {
        // Ждем заданное время
        yield return new WaitForSeconds(destroyDelay);
        
        if (objectToDestroy != null)
        {
            // Воспроизводим эффекты удаления
            PlayDestroyEffects(objectToDestroy.transform.position);
            
            // Уничтожаем объект
            Destroy(objectToDestroy);
            Debug.Log($"Объект {objectToDestroy.name} удален из сцены");
        }
    }
    
    
    // Визуализация зон в Scene View (только в редакторе)
    void OnDrawGizmos()
    {
        if (useDropZone)
        {
            // Рисуем первую дроп зону
            Gizmos.color = zone1Color;
            Gizmos.DrawWireCube(new Vector3(zone1Center.x, zone1Center.y, 0), new Vector3(zone1Size.x, zone1Size.y, 0));
            
            // Показываем центр первой зоны
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(new Vector3(zone1Center.x, zone1Center.y, 0), 0.2f);
            
            // Рисуем вторую дроп зону
            Gizmos.color = zone2Color;
            Gizmos.DrawWireCube(new Vector3(zone2Center.x, zone2Center.y, 0), new Vector3(zone2Size.x, zone2Size.y, 0));
            
            // Показываем центр второй зоны
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(new Vector3(zone2Center.x, zone2Center.y, 0), 0.2f);
            
            // Рисуем третью дроп зону
            Gizmos.color = zone3Color;
            Gizmos.DrawWireCube(new Vector3(zone3Center.x, zone3Center.y, 0), new Vector3(zone3Size.x, zone3Size.y, 0));
            
            // Показываем центр третьей зоны
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(new Vector3(zone3Center.x, zone3Center.y, 0), 0.2f);
        }
        
    }
}
