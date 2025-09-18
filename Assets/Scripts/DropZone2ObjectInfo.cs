using UnityEngine;

/// <summary>
/// Компонент для отслеживания времени нахождения объекта в drop zone 2
/// </summary>
public class DropZone2ObjectInfo : MonoBehaviour
{
    [Header("Информация о времени в drop zone 2")]
    public float entryTime; // Время входа в drop zone 2
    public float lifetime; // Время жизни в drop zone 2
    
    void Start()
    {
        // Если время входа не установлено, устанавливаем текущее время
        if (entryTime == 0)
        {
            entryTime = Time.time;
        }
    }
    
    /// <summary>
    /// Сбрасывает время входа в drop zone 2
    /// </summary>
    public void ResetEntryTime()
    {
        entryTime = Time.time;
    }
    
    /// <summary>
    /// Проверяет, истекло ли время жизни объекта в drop zone 2
    /// </summary>
    public bool IsLifetimeExpired()
    {
        return Time.time - entryTime >= lifetime;
    }
    
    /// <summary>
    /// Возвращает оставшееся время жизни в drop zone 2
    /// </summary>
    public float GetRemainingTime()
    {
        float remaining = lifetime - (Time.time - entryTime);
        return Mathf.Max(0, remaining);
    }
}
