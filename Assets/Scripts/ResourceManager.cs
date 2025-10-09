using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace PortOfThieves.Resources
{
    public class ResourceManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI goldText;
        
        [Header("Resources")]
        [SerializeField] private Resource gold = new Resource("Gold", 0, 999999);
        
        [Header("Settings")]
        [SerializeField] private bool updateUIOnStart = true;
        [SerializeField] private string goldDisplayFormat = "Gold: {0}";

        // События для уведомления об изменениях ресурсов
        public static event Action<string, int, int> OnResourceChanged; // resourceName, currentAmount, maxAmount
        public static event Action<Resource> OnGoldChanged;

        // Свойства для доступа к ресурсам
        public Resource Gold => gold;
        public int GoldAmount => gold.CurrentAmount;
        public int GoldMaxAmount => gold.MaxAmount;

        private void Start()
        {
            if (updateUIOnStart)
            {
                UpdateGoldUI();
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

        #region Gold Operations
        
        /// <summary>
        /// Добавляет золото к текущему количеству
        /// </summary>
        /// <param name="amount">Количество золота для добавления</param>
        /// <returns>true если удалось добавить полное количество, false если достигнут максимум</returns>
        public bool AddGold(int amount)
        {
            bool success = gold.TryAdd(amount);
            NotifyGoldChanged();
            return success;
        }

        /// <summary>
        /// Удаляет золото из текущего количества
        /// </summary>
        /// <param name="amount">Количество золота для удаления</param>
        /// <returns>true если удалось удалить указанное количество</returns>
        public bool RemoveGold(int amount)
        {
            bool success = gold.TryRemove(amount);
            NotifyGoldChanged();
            return success;
        }

        /// <summary>
        /// Устанавливает точное количество золота
        /// </summary>
        /// <param name="amount">Новое количество золота</param>
        public void SetGold(int amount)
        {
            gold.SetAmount(amount);
            NotifyGoldChanged();
        }

        /// <summary>
        /// Устанавливает максимальное количество золота
        /// </summary>
        /// <param name="maxAmount">Новый максимум золота</param>
        public void SetGoldMaxAmount(int maxAmount)
        {
            gold.SetMaxAmount(maxAmount);
            NotifyGoldChanged();
        }

        /// <summary>
        /// Проверяет, достаточно ли золота для операции
        /// </summary>
        /// <param name="amount">Требуемое количество золота</param>
        /// <returns>true если золота достаточно</returns>
        public bool HasEnoughGold(int amount)
        {
            return gold.CurrentAmount >= amount;
        }

        #endregion

        #region Generic Resource Operations

        /// <summary>
        /// Добавляет ресурс по имени
        /// </summary>
        /// <param name="resourceName">Имя ресурса</param>
        /// <param name="amount">Количество для добавления</param>
        /// <returns>true если операция успешна</returns>
        public bool AddResource(string resourceName, int amount)
        {
            Resource resource = GetResourceByName(resourceName);
            if (resource == null) return false;

            bool success = resource.TryAdd(amount);
            NotifyResourceChanged(resource);
            return success;
        }

        /// <summary>
        /// Удаляет ресурс по имени
        /// </summary>
        /// <param name="resourceName">Имя ресурса</param>
        /// <param name="amount">Количество для удаления</param>
        /// <returns>true если операция успешна</returns>
        public bool RemoveResource(string resourceName, int amount)
        {
            Resource resource = GetResourceByName(resourceName);
            if (resource == null) return false;

            bool success = resource.TryRemove(amount);
            NotifyResourceChanged(resource);
            return success;
        }

        /// <summary>
        /// Получает ресурс по имени
        /// </summary>
        /// <param name="resourceName">Имя ресурса</param>
        /// <returns>Ресурс или null если не найден</returns>
        public Resource GetResourceByName(string resourceName)
        {
            switch (resourceName.ToLower())
            {
                case "gold":
                    return gold;
                default:
                    Debug.LogWarning($"Resource '{resourceName}' not found!");
                    return null;
            }
        }

        #endregion

        #region UI Updates

        /// <summary>
        /// Обновляет UI отображение золота
        /// </summary>
        public void UpdateGoldUI()
        {
            if (goldText != null)
            {
                goldText.text = string.Format(goldDisplayFormat, gold.CurrentAmount);
            }
        }

        /// <summary>
        /// Устанавливает ссылку на TextMeshPro для отображения золота
        /// </summary>
        /// <param name="textComponent">Компонент TextMeshPro</param>
        public void SetGoldText(TextMeshProUGUI textComponent)
        {
            goldText = textComponent;
            UpdateGoldUI();
        }

        /// <summary>
        /// Устанавливает формат отображения золота
        /// </summary>
        /// <param name="format">Формат строки (используйте {0} для количества)</param>
        public void SetGoldDisplayFormat(string format)
        {
            goldDisplayFormat = format;
            UpdateGoldUI();
        }

        #endregion

        #region Event Notifications

        private void NotifyGoldChanged()
        {
            OnGoldChanged?.Invoke(gold);
            OnResourceChanged?.Invoke(gold.ResourceName, gold.CurrentAmount, gold.MaxAmount);
            UpdateGoldUI();
        }

        private void NotifyResourceChanged(Resource resource)
        {
            OnResourceChanged?.Invoke(resource.ResourceName, resource.CurrentAmount, resource.MaxAmount);
        }

        private void HandleResourceChanged(string resourceName, int currentAmount, int maxAmount)
        {
            // Здесь можно добавить дополнительную логику обработки изменений ресурсов
            Debug.Log($"Resource '{resourceName}' changed: {currentAmount}/{maxAmount}");
        }

        #endregion

        #region Debug Methods

        [ContextMenu("Add 100 Gold")]
        private void DebugAddGold()
        {
            AddGold(100);
        }

        [ContextMenu("Remove 50 Gold")]
        private void DebugRemoveGold()
        {
            RemoveGold(50);
        }

        [ContextMenu("Reset Gold")]
        private void DebugResetGold()
        {
            SetGold(0);
        }

        #endregion
    }
}
