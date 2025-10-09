using UnityEngine;

public class ObjectStateManager : MonoBehaviour
{
    [Header("Object State Control")]
    public GameObject objectToDisable;
    public GameObject objectToEnable;
    public GameObject objectToKeepActive;
    
    void Start()
    {
        // Проверяем состояние объектов при старте
        CheckAndSetObjectStates();
    }
    
    void OnEnable()
    {
        // Проверяем состояние объектов каждый раз при активации этого объекта
        CheckAndSetObjectStates();
    }
    
    private void CheckAndSetObjectStates()
    {
        // Выключаем первый объект
        if (objectToDisable != null)
        {
            objectToDisable.SetActive(false);
            Debug.Log($"Object '{objectToDisable.name}' has been disabled");
        }
        else
        {
            Debug.LogWarning("Object to disable is not assigned!");
        }
        
        // Включаем второй объект
        if (objectToEnable != null)
        {
            objectToEnable.SetActive(true);
            Debug.Log($"Object '{objectToEnable.name}' has been enabled");
        }
        else
        {
            Debug.LogWarning("Object to enable is not assigned!");
        }
        
        // Убеждаемся что третий объект активен
        if (objectToKeepActive != null)
        {
            objectToKeepActive.SetActive(true);
            Debug.Log($"Object '{objectToKeepActive.name}' is kept active");
        }
        else
        {
            Debug.LogWarning("Object to keep active is not assigned!");
        }
    }
    
    // Публичные методы для ручного управления состоянием
    public void ToggleObjectState(GameObject obj)
    {
        if (obj != null)
        {
            obj.SetActive(!obj.activeInHierarchy);
            Debug.Log($"Object '{obj.name}' state toggled to: {obj.activeInHierarchy}");
        }
    }
    
    public void EnableObject(GameObject obj)
    {
        if (obj != null)
        {
            obj.SetActive(true);
            Debug.Log($"Object '{obj.name}' has been enabled");
        }
    }
    
    public void DisableObject(GameObject obj)
    {
        if (obj != null)
        {
            obj.SetActive(false);
            Debug.Log($"Object '{obj.name}' has been disabled");
        }
    }
}