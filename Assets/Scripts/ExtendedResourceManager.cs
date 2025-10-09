using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace PortOfThieves.Resources
{
    /// <summary>
    /// Расширяемый менеджер ресурсов с поддержкой динамического добавления новых типов ресурсов
    /// </summary>
    public class ExtendedResourceManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI goldText;
        
        [Header("Resource Settings")]
        [SerializeField] private List<ResourceData> resourceDataList = new List<ResourceData>();
        [SerializeField] private bool updateUIOnStart = true;
        [SerializeField] private string goldDisplayFormat = "Gold: {0}";

        // События для уведомления об изменениях ресурсов
        public static event Action<string, int, int> OnResourceChanged; // resourceName, currentAmount, maxAmount
        public static event Action<Resource> OnGoldChanged;

        // Словарь для быстрого доступа к ресурсам
        private Dictionary<string, Resource> resources = new Dictionary<string, Resource>();

        // Свойства для доступа к золоту (для обратной совместимости)
        public Resource Gold => GetResource("Gold");
        public int GoldAmount => GetResourceAmount("Gold");
        public int GoldMaxAmount => GetResourceMaxAmount("Gold");

        [Serializable]
        public class ResourceData
        {
            public string resourceName;
            public int initialAmount;
            public int maxAmount;
            public TextMeshProUGUI displayText;
            public string displayFormat = "{0}";

            public ResourceData(string name, int initial = 0, int max = 999999)
            {
                resourceName = name;
                initialAmount = initial;
                maxAmount = max;
            }
        }

        private void Awake()
        {
            InitializeResources();
        }

        private void Start()
        {
            if (updateUIOnStart)
            {
                UpdateAllUI();
            }
        }

        private void OnEnable()
        {
            OnResourceChanged += HandleResourceChanged;
        }

        private void OnDisable()
        {
            OnResourceChanged -= HandleResourceChanged;
        }

        #region Initialization

        /// <summary>
        /// Инициализирует все ресурсы из списка
        /// </summary>
        private void InitializeResources()
        {
            resources.Clear();
            
            foreach (var data in resourceDataList)
            {
                if (!string.IsNullOrEmpty(data.resourceName))
                {
                    var resource = new Resource(data.resourceName, data.initialAmount, data.maxAmount);
                    resources[data.resourceName] = resource;
                }
            }

            // Обеспечиваем наличие золота
            if (!resources.ContainsKey("Gold"))
            {
                var goldData = new ResourceData("Gold", 0, 999999);
                goldData.displayText = goldText;
                goldData.displayFormat = goldDisplayFormat;
                resourceDataList.Add(goldData);
                
                var goldResource = new Resource("Gold", 0, 999999);
                resources["Gold"] = goldResource;
            }
        }

        #endregion

        #region Resource Management

        /// <summary>
        /// Добавляет новый тип ресурса
        /// </summary>
        /// <param name="resourceName">Имя ресурса</param>
        /// <param name="initialAmount">Начальное количество</param>
        /// <param name="maxAmount">Максимальное количество</param>
        /// <param name="displayText">UI компонент для отображения</param>
        /// <param name="displayFormat">Формат отображения</param>
        public void AddResourceType(string resourceName, int initialAmount = 0, int maxAmount = 999999, 
            TextMeshProUGUI displayText = null, string displayFormat = "{0}")
        {
            if (resources.ContainsKey(resourceName))
            {
                Debug.LogWarning($"Resource '{resourceName}' already exists!");
                return;
            }

            var resource = new Resource(resourceName, initialAmount, maxAmount);
            resources[resourceName] = resource;

            var resourceData = new ResourceData(resourceName, initialAmount, maxAmount)
            {
                displayText = displayText,
                displayFormat = displayFormat
            };
            resourceDataList.Add(resourceData);

            NotifyResourceChanged(resource);
        }

        /// <summary>
        /// Удаляет тип ресурса
        /// </summary>
        /// <param name="resourceName">Имя ресурса</param>
        public void RemoveResourceType(string resourceName)
        {
            if (resourceName == "Gold")
            {
                Debug.LogWarning("Cannot remove Gold resource!");
                return;
            }

            resources.Remove(resourceName);
            resourceDataList.RemoveAll(data => data.resourceName == resourceName);
        }

        /// <summary>
        /// Получает ресурс по имени
        /// </summary>
        /// <param name="resourceName">Имя ресурса</param>
        /// <returns>Ресурс или null если не найден</returns>
        public Resource GetResource(string resourceName)
        {
            return resources.TryGetValue(resourceName, out Resource resource) ? resource : null;
        }

        /// <summary>
        /// Получает количество ресурса
        /// </summary>
        /// <param name="resourceName">Имя ресурса</param>
        /// <returns>Текущее количество ресурса</returns>
        public int GetResourceAmount(string resourceName)
        {
            var resource = GetResource(resourceName);
            return resource?.CurrentAmount ?? 0;
        }

        /// <summary>
        /// Получает максимальное количество ресурса
        /// </summary>
        /// <param name="resourceName">Имя ресурса</param>
        /// <returns>Максимальное количество ресурса</returns>
        public int GetResourceMaxAmount(string resourceName)
        {
            var resource = GetResource(resourceName);
            return resource?.MaxAmount ?? 0;
        }

        #endregion

        #region Resource Operations

        /// <summary>
        /// Добавляет ресурс
        /// </summary>
        /// <param name="resourceName">Имя ресурса</param>
        /// <param name="amount">Количество для добавления</param>
        /// <returns>true если операция успешна</returns>
        public bool AddResource(string resourceName, int amount)
        {
            var resource = GetResource(resourceName);
            if (resource == null) return false;

            bool success = resource.TryAdd(amount);
            NotifyResourceChanged(resource);
            return success;
        }

        /// <summary>
        /// Удаляет ресурс
        /// </summary>
        /// <param name="resourceName">Имя ресурса</param>
        /// <param name="amount">Количество для удаления</param>
        /// <returns>true если операция успешна</returns>
        public bool RemoveResource(string resourceName, int amount)
        {
            var resource = GetResource(resourceName);
            if (resource == null) return false;

            bool success = resource.TryRemove(amount);
            NotifyResourceChanged(resource);
            return success;
        }

        /// <summary>
        /// Устанавливает точное количество ресурса
        /// </summary>
        /// <param name="resourceName">Имя ресурса</param>
        /// <param name="amount">Новое количество</param>
        public void SetResourceAmount(string resourceName, int amount)
        {
            var resource = GetResource(resourceName);
            if (resource == null) return;

            resource.SetAmount(amount);
            NotifyResourceChanged(resource);
        }

        /// <summary>
        /// Проверяет, достаточно ли ресурса
        /// </summary>
        /// <param name="resourceName">Имя ресурса</param>
        /// <param name="amount">Требуемое количество</param>
        /// <returns>true если ресурса достаточно</returns>
        public bool HasEnoughResource(string resourceName, int amount)
        {
            var resource = GetResource(resourceName);
            return resource?.CurrentAmount >= amount;
        }

        #endregion

        #region Gold Operations (для обратной совместимости)

        public bool AddGold(int amount) => AddResource("Gold", amount);
        public bool RemoveGold(int amount) => RemoveResource("Gold", amount);
        public void SetGold(int amount) => SetResourceAmount("Gold", amount);
        public bool HasEnoughGold(int amount) => HasEnoughResource("Gold", amount);

        #endregion

        #region UI Management

        /// <summary>
        /// Обновляет UI для всех ресурсов
        /// </summary>
        public void UpdateAllUI()
        {
            foreach (var data in resourceDataList)
            {
                UpdateResourceUI(data.resourceName);
            }
        }

        /// <summary>
        /// Обновляет UI для конкретного ресурса
        /// </summary>
        /// <param name="resourceName">Имя ресурса</param>
        public void UpdateResourceUI(string resourceName)
        {
            var resource = GetResource(resourceName);
            if (resource == null) return;

            var data = resourceDataList.Find(d => d.resourceName == resourceName);
            if (data?.displayText != null)
            {
                data.displayText.text = string.Format(data.displayFormat, resource.CurrentAmount);
            }
        }

        /// <summary>
        /// Устанавливает ссылку на TextMeshPro для отображения золота
        /// </summary>
        /// <param name="textComponent">Компонент TextMeshPro</param>
        public void SetGoldText(TextMeshProUGUI textComponent)
        {
            goldText = textComponent;
            var goldData = resourceDataList.Find(d => d.resourceName == "Gold");
            if (goldData != null)
            {
                goldData.displayText = textComponent;
            }
            UpdateResourceUI("Gold");
        }

        /// <summary>
        /// Устанавливает ссылку на TextMeshPro для отображения ресурса
        /// </summary>
        /// <param name="resourceName">Имя ресурса</param>
        /// <param name="textComponent">Компонент TextMeshPro</param>
        public void SetResourceText(string resourceName, TextMeshProUGUI textComponent)
        {
            var data = resourceDataList.Find(d => d.resourceName == resourceName);
            if (data != null)
            {
                data.displayText = textComponent;
                UpdateResourceUI(resourceName);
            }
        }

        /// <summary>
        /// Устанавливает формат отображения ресурса
        /// </summary>
        /// <param name="resourceName">Имя ресурса</param>
        /// <param name="format">Формат строки</param>
        public void SetResourceDisplayFormat(string resourceName, string format)
        {
            var data = resourceDataList.Find(d => d.resourceName == resourceName);
            if (data != null)
            {
                data.displayFormat = format;
                UpdateResourceUI(resourceName);
            }
        }

        #endregion

        #region Event Notifications

        private void NotifyResourceChanged(Resource resource)
        {
            OnResourceChanged?.Invoke(resource.ResourceName, resource.CurrentAmount, resource.MaxAmount);
            
            if (resource.ResourceName == "Gold")
            {
                OnGoldChanged?.Invoke(resource);
            }
            
            UpdateResourceUI(resource.ResourceName);
        }

        private void HandleResourceChanged(string resourceName, int currentAmount, int maxAmount)
        {
            Debug.Log($"Resource '{resourceName}' changed: {currentAmount}/{maxAmount}");
        }

        #endregion

        #region Debug Methods

        [ContextMenu("Add 100 Gold")]
        private void DebugAddGold() => AddGold(100);

        [ContextMenu("Remove 50 Gold")]
        private void DebugRemoveGold() => RemoveGold(50);

        [ContextMenu("Reset Gold")]
        private void DebugResetGold() => SetGold(0);

        [ContextMenu("Add Wood Resource")]
        private void DebugAddWood()
        {
            AddResourceType("Wood", 0, 1000, null, "Wood: {0}");
        }

        [ContextMenu("Add 50 Wood")]
        private void DebugAddWoodAmount()
        {
            AddResource("Wood", 50);
        }

        #endregion
    }
}
