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
    
    private Camera mainCamera;
    private Mouse mouse;
    private bool isDragging = false;
    private Transform draggedObject = null;
    private Vector3 originalPosition; // Исходная позиция объекта
    
    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
        
        mouse = Mouse.current;
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
            // Если курсор над объектом с нужным тегом
            if (currentTag == draggableTag)
            {
                Collider2D[] colliders = Physics2D.OverlapCircleAll(new Vector2(worldPosition.x, worldPosition.y), 0.1f);
                if (colliders.Length > 0)
                {
                    isDragging = true;
                    draggedObject = colliders[0].transform;
                    // Запоминаем исходную позицию объекта
                    originalPosition = draggedObject.position;
                    Debug.Log($"Started dragging: {draggedObject.name} from {originalPosition}");
                }
            }
        }
        
        // Если кнопка мыши отпущена
        if (mouse.leftButton.wasReleasedThisFrame)
        {
            if (isDragging)
            {
                // Проверяем, можно ли отпустить объект в этой позиции
                if (CanDropAtPosition(worldPosition))
                {
                    // Объект остается в новой позиции
                    draggedObject.position = worldPosition;
                    Debug.Log($"Dropped: {draggedObject.name} at {worldPosition}");
                }
                else
                {
                    // Возвращаем объект на исходную позицию
                    draggedObject.position = originalPosition;
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
