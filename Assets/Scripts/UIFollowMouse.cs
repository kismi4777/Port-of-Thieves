using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class UIFollowMouse : MonoBehaviour
{
    [Header("UI Object Settings")]
    public RectTransform uiObject; // Публичное поле для UI объекта
    
    [Header("Follow Settings")]
    public bool followMouse = true;
    public float smoothSpeed = 10f;
    public Vector2 offset = Vector2.zero;
    
    private Camera uiCamera;
    private Canvas parentCanvas;
    
    void Start()
    {
        // Получаем Canvas и камеру
        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            uiCamera = parentCanvas.worldCamera;
            if (uiCamera == null)
            {
                uiCamera = Camera.main;
            }
        }
        
        // Если UI объект не назначен, используем текущий объект
        if (uiObject == null)
        {
            uiObject = GetComponent<RectTransform>();
        }
    }
    
    void Update()
    {
        if (followMouse && uiObject != null)
        {
            FollowMouse();
        }
    }
    
    private void FollowMouse()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector2 targetPosition;
        
        if (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            // Для Screen Space - Overlay Canvas нужно конвертировать координаты
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.transform as RectTransform,
                mousePosition,
                null, // Для Screen Space - Overlay камера не нужна
                out localPoint
            );
            targetPosition = localPoint + offset;
        }
        else
        {
            // Для Screen Space - Camera и World Space Canvas
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.transform as RectTransform,
                mousePosition,
                uiCamera,
                out localPoint
            );
            targetPosition = localPoint + offset;
        }
        
        // Плавное движение к позиции курсора
        uiObject.anchoredPosition = Vector2.Lerp(uiObject.anchoredPosition, targetPosition, smoothSpeed * Time.deltaTime);
    }
    
    // Метод для включения/выключения следования за курсором
    public void SetFollowMouse(bool follow)
    {
        followMouse = follow;
    }
    
    // Метод для установки смещения
    public void SetOffset(Vector2 newOffset)
    {
        offset = newOffset;
    }
}