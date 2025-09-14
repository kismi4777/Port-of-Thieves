# Тестирование Unity Component Manager через MCP

## 🧪 Команды для тестирования

### 1. Проверка иерархии сцены

```javascript
// Получить базовую иерархию сцены
{
  "detailed": false
}

// Получить детальную иерархию сцены
{
  "detailed": true
}
```

### 2. Создание простого скрипта движения

```javascript
// Создать скрипт движения и добавить на объект "Player"
{
  "script_name": "PlayerMovement",
  "script_content": `
    public float speed = 5f;
    public float jumpForce = 10f;
    
    private Rigidbody rb;
    
    void Start() {
        rb = GetComponent<Rigidbody>();
    }
    
    void Update() {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        Vector3 movement = new Vector3(horizontal, 0, vertical);
        transform.Translate(movement * speed * Time.deltaTime);
        
        if (Input.GetKeyDown(KeyCode.Space)) {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
  `,
  "object_name": "Player"
}
```

### 3. Создание скрипта вращения

```javascript
// Создать скрипт вращения и добавить на объект "Enemy"
{
  "script_name": "EnemyRotation",
  "script_content": `
    public float rotationSpeed = 90f;
    public Transform target;
    
    void Update() {
        if (target != null) {
            Vector3 direction = target.position - transform.position;
            direction.y = 0;
            
            if (direction != Vector3.zero) {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation, 
                    targetRotation, 
                    rotationSpeed * Time.deltaTime
                );
            }
        }
    }
  `,
  "object_name": "Enemy"
}
```

### 4. Создание скрипта сбора предметов

```javascript
// Создать скрипт сбора и добавить на объект "Coin"
{
  "script_name": "CoinCollector",
  "script_content": `
    public int points = 10;
    public GameObject collectEffect;
    
    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            // Добавить очки игроку (здесь можно добавить GameManager)
            Debug.Log($"Collected coin worth {points} points!");
            
            // Создать эффект сбора
            if (collectEffect != null) {
                Instantiate(collectEffect, transform.position, transform.rotation);
            }
            
            // Уничтожить монету
            Destroy(gameObject);
        }
    }
  `,
  "object_name": "Coin"
}
```

### 5. Создание скрипта камеры

```javascript
// Создать скрипт следования камеры и добавить на объект "Main Camera"
{
  "script_name": "CameraFollow",
  "script_content": `
    public Transform target;
    public Vector3 offset = new Vector3(0, 5, -10);
    public float smoothSpeed = 5f;
    
    void Start() {
        if (target == null) {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) {
                target = player.transform;
            }
        }
    }
    
    void LateUpdate() {
        if (target != null) {
            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(
                transform.position, 
                desiredPosition, 
                smoothSpeed * Time.deltaTime
            );
            transform.position = smoothedPosition;
            
            transform.LookAt(target);
        }
    }
  `,
  "object_name": "Main Camera"
}
```

### 6. Добавление существующего скрипта

```javascript
// Добавить существующий скрипт "BouncingBall" на объект "Ball"
{
  "object_name": "Ball",
  "script_name": "BouncingBall"
}
```

## 🔍 Проверка результатов

### После выполнения команд проверьте:

1. **В Unity Editor:**
   - Скрипты появились в папке Assets/Scripts/
   - Компоненты добавлены на объекты
   - Нет ошибок компиляции

2. **В консоли Unity:**
   - Сообщения об успешном создании скриптов
   - Сообщения об успешном добавлении компонентов

3. **В Inspector:**
   - Объекты имеют новые компоненты
   - Параметры скриптов доступны для настройки

## 🚨 Возможные ошибки

### "GameObject not found"
- Проверьте имя объекта в сцене
- Убедитесь, что объект активен

### "Script not found"
- Подождите завершения компиляции Unity
- Проверьте, что скрипт создался в Assets/Scripts/

### "Component already exists"
- Компонент уже добавлен на объект
- Удалите существующий компонент вручную

## 💡 Советы по тестированию

1. **Начните с простых скриптов** без сложной логики
2. **Проверяйте консоль Unity** после каждой команды
3. **Используйте детальную иерархию** для проверки компонентов
4. **Тестируйте по одному скрипту** за раз
5. **Сохраняйте сцену** после успешного добавления компонентов

## 🎯 Следующие шаги

После успешного тестирования:

1. **Создайте более сложные скрипты** с игровой логикой
2. **Добавьте взаимодействие** между объектами
3. **Создайте систему управления** игроком
4. **Добавьте физику** и коллизии
5. **Создайте UI элементы** для игрового интерфейса

---

**Unity Component Manager** работает! Теперь вы можете создавать и управлять скриптами через AI! 🎮✨
