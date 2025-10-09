using System;
using UnityEngine;

namespace PortOfThieves.Resources
{
    [Serializable]
    public class Resource
    {
        [SerializeField] private string resourceName;
        [SerializeField] private int currentAmount;
        [SerializeField] private int maxAmount;

        public string ResourceName => resourceName;
        public int CurrentAmount => currentAmount;
        public int MaxAmount => maxAmount;
        public bool IsFull => currentAmount >= maxAmount;

        public Resource(string name, int initialAmount = 0, int maxAmount = 999999)
        {
            this.resourceName = name;
            this.currentAmount = Mathf.Clamp(initialAmount, 0, maxAmount);
            this.maxAmount = maxAmount;
        }

        public bool TryAdd(int amount)
        {
            if (amount <= 0) return false;
            
            int newAmount = currentAmount + amount;
            if (newAmount > maxAmount)
            {
                currentAmount = maxAmount;
                return false; // Не удалось добавить полное количество
            }
            
            currentAmount = newAmount;
            return true;
        }

        public bool TryRemove(int amount)
        {
            if (amount <= 0 || currentAmount < amount) return false;
            
            currentAmount -= amount;
            return true;
        }

        public void SetAmount(int amount)
        {
            currentAmount = Mathf.Clamp(amount, 0, maxAmount);
        }

        public void SetMaxAmount(int newMaxAmount)
        {
            maxAmount = newMaxAmount;
            currentAmount = Mathf.Clamp(currentAmount, 0, maxAmount);
        }
    }
}
