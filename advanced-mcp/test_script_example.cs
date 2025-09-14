// Пример скрипта для тестирования Unity Component Manager
// Этот скрипт можно создать и добавить на объект через MCP

using UnityEngine;

public class TestMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 100f;
    
    [Header("References")]
    public Transform target;
    
    void Start()
    {
        // Если цель не назначена, ищем объект с тегом "Player"
        if (target == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }
        
        Debug.Log($"TestMovement initialized on {gameObject.name}");
    }
    
    void Update()
    {
        HandleMovement();
        HandleRotation();
    }
    
    void HandleMovement()
    {
        // Простое движение вперед
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
        
        // Если есть цель, двигаемся к ней
        if (target != null)
        {
            var direction = (target.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
    }
    
    void HandleRotation()
    {
        // Вращение вокруг оси Y
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }
    
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"{gameObject.name} collided with {other.gameObject.name}");
        
        // Если столкнулись с игроком, можно добавить логику
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player detected! Adding points or effects here.");
        }
    }
    
    // Публичный метод для изменения скорости через Inspector или код
    public void SetSpeed(float newSpeed)
    {
        moveSpeed = Mathf.Max(0, newSpeed);
        Debug.Log($"Speed changed to: {moveSpeed}");
    }
    
    // Публичный метод для изменения цели
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        Debug.Log($"Target changed to: {(target != null ? target.name : "null")}");
    }
}
