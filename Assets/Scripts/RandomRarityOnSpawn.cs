using UnityEngine;

public class RandomRarityOnSpawn : MonoBehaviour
{
	public enum Rarity
	{
		Common,      // Обычный
		Uncommon,    // Необычный
		Rare,        // Редкий
		Epic,        // Эпический
		Legendary,   // Легендарный
		Mythic       // Мифический
	}

	[SerializeField]
	private Rarity assignedRarity;

	[Header("Цвета редкостей (по порядку enum)")]
	[SerializeField]
	private Color[] rarityColors = new Color[6]
	{
		new Color(0.85f, 0.85f, 0.85f), // Common
		new Color(0.3f, 0.8f, 0.3f),     // Uncommon
		new Color(0.25f, 0.5f, 1f),      // Rare
		new Color(0.7f, 0.3f, 0.9f),     // Epic
		new Color(1f, 0.65f, 0.1f),      // Legendary
		new Color(1f, 0.2f, 0.2f)        // Mythic
	};

	[Header("Цвета объекта (оставляем только один — по редкости)")]
	[SerializeField]
	private System.Collections.Generic.List<Color> colors = new System.Collections.Generic.List<Color>();

	[SerializeField]
	private Color assignedColor;

	[Header("Вес выпадения (чем больше вес — тем чаще)")]
	[SerializeField] private float commonWeight = 50f;     // Обычный
	[SerializeField] private float uncommonWeight = 20f;   // Необычный
	[SerializeField] private float rareWeight = 12f;       // Редкий
	[SerializeField] private float epicWeight = 8f;        // Эпический
	[SerializeField] private float legendaryWeight = 6f;   // Легендарный
	[SerializeField] private float mythicWeight = 4f;      // Мифический

	public Rarity AssignedRarity => assignedRarity;
	public Color AssignedColor => assignedColor;

	void Start()
	{
		AssignWeightedRandomRarity();
	}

	private void AssignWeightedRandomRarity()
	{
		float[] weights = new float[]
		{
			Mathf.Max(0f, commonWeight),
			Mathf.Max(0f, uncommonWeight),
			Mathf.Max(0f, rareWeight),
			Mathf.Max(0f, epicWeight),
			Mathf.Max(0f, legendaryWeight),
			Mathf.Max(0f, mythicWeight)
		};

		float total = 0f;
		for (int i = 0; i < weights.Length; i++) total += weights[i];
		if (total <= 0f)
		{
			assignedRarity = Rarity.Common;
			assignedColor = GetColorForRarity(assignedRarity);
			colors.Clear();
			colors.Add(assignedColor);
			return;
		}

		float roll = Random.value * total;
		float cumulative = 0f;
		for (int i = 0; i < weights.Length; i++)
		{
			cumulative += weights[i];
			if (roll <= cumulative)
			{
				assignedRarity = (Rarity)i;
				assignedColor = GetColorForRarity(assignedRarity);
				colors.Clear();
				colors.Add(assignedColor);
				return;
			}
		}

		assignedRarity = Rarity.Mythic;
		assignedColor = GetColorForRarity(assignedRarity);
		colors.Clear();
		colors.Add(assignedColor);
	}


	private Color GetColorForRarity(Rarity rarity)
	{
		int index = (int)rarity;
		if (rarityColors != null && index >= 0 && index < rarityColors.Length)
		{
			return rarityColors[index];
		}
		return Color.white;
	}
}



