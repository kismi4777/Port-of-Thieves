using UnityEngine;

public class RandomRarityOnSpawn : MonoBehaviour
{
	private static readonly string[] AllCharacteristicsRu = new string[]
	{
		"Физический урон",
		"Магический урон",
		"Скорость атаки",
		"Критический шанс",
		"Критический урон",
		"Физическая защита",
		"Магическая защита",
		"Сопротивление огню",
		"Сопротивление яду",
		"Блокирование",
		"Поглощение урона",
		"Скорость передвижения",
		"Вампиризм Жизни",
		"Вампиризм Маны",
		"Сопротивление оглушению",
		"Сопротивление замедлению",
		"Отражение урона",
		"Регенерация здоровья",
		"Регенерация маны"
	};

	public enum Rarity
	{
		Обычный,
		Необычный,
		Редкий,
		Эпический,
		Легендарный,
		Мифический
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

	[Header("Характеристики предмета (макс 5, мин 1 по редкости)")]
	public string stat1 = "";
	public string stat2 = "";
	public string stat3 = "";
	public string stat4 = "";
	public string stat5 = "";

	[Header("Значения характеристик (1-100)")]
	public int stat1Value = 0;
	public int stat2Value = 0;
	public int stat3Value = 0;
	public int stat4Value = 0;
	public int stat5Value = 0;

	[Header("Цена предмета")]
	public int gold = 0;

	[SerializeField]
	private Color assignedColor;

	[Header("Вес выпадения (чем больше вес — тем чаще)")]
	[SerializeField] private float обычныйWeight = 50f;     // Обычный
	[SerializeField] private float необычныйWeight = 20f;   // Необычный
	[SerializeField] private float редкийWeight = 12f;       // Редкий
	[SerializeField] private float эпическийWeight = 8f;        // Эпический
	[SerializeField] private float легендарныйWeight = 6f;   // Легендарный
	[SerializeField] private float мифическийWeight = 4f;      // Мифический

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
			Mathf.Max(0f, обычныйWeight),
			Mathf.Max(0f, необычныйWeight),
			Mathf.Max(0f, редкийWeight),
			Mathf.Max(0f, эпическийWeight),
			Mathf.Max(0f, легендарныйWeight),
			Mathf.Max(0f, мифическийWeight)
		};

		float total = 0f;
		for (int i = 0; i < weights.Length; i++) total += weights[i];
		if (total <= 0f)
		{
			assignedRarity = Rarity.Обычный;
			assignedColor = GetColorForRarity(assignedRarity);
			colors.Clear();
			colors.Add(assignedColor);
			AssignCharacteristicsForRarity();
			CalculateGoldPrice();
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
				AssignCharacteristicsForRarity();
				CalculateGoldPrice();
				return;
			}
		}

		assignedRarity = Rarity.Мифический;
		assignedColor = GetColorForRarity(assignedRarity);
		colors.Clear();
		colors.Add(assignedColor);
		AssignCharacteristicsForRarity();
		CalculateGoldPrice();
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

	private void AssignCharacteristicsForRarity()
	{
		int count = GetCharacteristicCountForRarity(assignedRarity);
		string[] pool = new string[AllCharacteristicsRu.Length];
		for (int i = 0; i < AllCharacteristicsRu.Length; i++) pool[i] = AllCharacteristicsRu[i];
		Shuffle(pool);

		// Сбрасываем все слоты в пустые строки и нули значений
		stat1 = ""; stat2 = ""; stat3 = ""; stat4 = ""; stat5 = "";
		stat1Value = 0; stat2Value = 0; stat3Value = 0; stat4Value = 0; stat5Value = 0;

		// Назначаем уникальные характеристики согласно count
		if (count >= 1 && pool.Length > 0) { stat1 = pool[0]; stat1Value = GetRandomValueForRarity(assignedRarity); }
		if (count >= 2 && pool.Length > 1) { stat2 = pool[1]; stat2Value = GetRandomValueForRarity(assignedRarity); }
		if (count >= 3 && pool.Length > 2) { stat3 = pool[2]; stat3Value = GetRandomValueForRarity(assignedRarity); }
		if (count >= 4 && pool.Length > 3) { stat4 = pool[3]; stat4Value = GetRandomValueForRarity(assignedRarity); }
		if (count >= 5 && pool.Length > 4) { stat5 = pool[4]; stat5Value = GetRandomValueForRarity(assignedRarity); }
	}

	private int GetCharacteristicCountForRarity(Rarity rarity)
	{
		switch (rarity)
		{
			case Rarity.Обычный: return 1;
			case Rarity.Необычный: return 2;
			case Rarity.Редкий: return 3;
			case Rarity.Эпический: return 4;
			case Rarity.Легендарный: return 5;
			case Rarity.Мифический: return 5;
			default: return 1;
		}
	}

	private void Shuffle<T>(T[] array)
	{
		for (int i = array.Length - 1; i > 0; i--)
		{
			int j = Random.Range(0, i + 1);
			T tmp = array[i];
			array[i] = array[j];
			array[j] = tmp;
		}
	}

	private int GetRandomValueForRarity(Rarity rarity)
	{
		int maxValue = 15;
		switch (rarity)
		{
			case Rarity.Обычный: maxValue = 15; break;      // до 15
			case Rarity.Необычный: maxValue = 20; break;    // до 20
			case Rarity.Редкий: maxValue = 35; break;        // до 35
			case Rarity.Эпический: maxValue = 45; break;        // до 45
			case Rarity.Легендарный: maxValue = 60; break;   // до 60
			case Rarity.Мифический: maxValue = 99; break;      // до 99
		}
		
		float randomFloat = Random.value;
		
		// Для низких редкостей - уклон к низким значениям (квадрат)
		// Для высоких редкостей - уклон к высоким значениям (инвертированный квадрат)
		if ((int)rarity <= 1) // Обычный, Необычный - низкие значения чаще
		{
			randomFloat = randomFloat * randomFloat;
		}
		else if ((int)rarity >= 4) // Легендарный, Мифический - высокие значения чаще
		{
			randomFloat = 1f - (1f - randomFloat) * (1f - randomFloat);
		}
		// Редкий, Эпический - равномерное распределение
		
		int value = Mathf.RoundToInt(randomFloat * (maxValue - 1)) + 1; // 1..maxValue
		return value;
	}

	private void CalculateGoldPrice()
	{
		// Базовая цена по редкости
		int basePrice = 0;
		switch (assignedRarity)
		{
			case Rarity.Обычный: basePrice = 10; break;
			case Rarity.Необычный: basePrice = 25; break;
			case Rarity.Редкий: basePrice = 50; break;
			case Rarity.Эпический: basePrice = 100; break;
			case Rarity.Легендарный: basePrice = 250; break;
			case Rarity.Мифический: basePrice = 500; break;
		}

		// Бонус за количество характеристик
		int characteristicsCount = 0;
		if (!string.IsNullOrEmpty(stat1)) characteristicsCount++;
		if (!string.IsNullOrEmpty(stat2)) characteristicsCount++;
		if (!string.IsNullOrEmpty(stat3)) characteristicsCount++;
		if (!string.IsNullOrEmpty(stat4)) characteristicsCount++;
		if (!string.IsNullOrEmpty(stat5)) characteristicsCount++;

		int characteristicsBonus = characteristicsCount * 5;

		// Бонус за значения характеристик
		int valuesBonus = 0;
		valuesBonus += stat1Value;
		valuesBonus += stat2Value;
		valuesBonus += stat3Value;
		valuesBonus += stat4Value;
		valuesBonus += stat5Value;

		// Множитель редкости для значений характеристик
		float rarityMultiplier = 1f;
		switch (assignedRarity)
		{
			case Rarity.Обычный: rarityMultiplier = 0.5f; break;
			case Rarity.Необычный: rarityMultiplier = 0.7f; break;
			case Rarity.Редкий: rarityMultiplier = 1f; break;
			case Rarity.Эпический: rarityMultiplier = 1.3f; break;
			case Rarity.Легендарный: rarityMultiplier = 1.6f; break;
			case Rarity.Мифический: rarityMultiplier = 2f; break;
		}

		valuesBonus = Mathf.RoundToInt(valuesBonus * rarityMultiplier);

		// Итоговая цена
		gold = basePrice + characteristicsBonus + valuesBonus;
	}
}



