using UnityEngine;

public class RandomPhraseManager : MonoBehaviour
{
    [Header("Фразы для поиска объектов")]
    [SerializeField] private string[] searchPhrases = new string[]
    {
        "Арр, я потерял свой {0}, не видел ли ты его на горизонте?",
        "Эй, приятель! Ты случайно не находил {0}?",
        "Мой {0} пропал! Помоги мне найти его!",
        "Куда же подевался мой {0}? Может, ты видел?",
        "Проклятье! Где же мой {0}? Ты не замечал его?",
        "Слушай, дружище, не встречал ли ты {0}?",
        "Мне нужен {0}! Ты поможешь мне его найти?",
        "Без {0} я пропал! Ты не видел его где-нибудь?",
        "Эх, потерял я свой {0}... Может, ты на него наткнулся?",
        "Морские дьяволы! Где мой {0}? Ты его не брал?"
    };

    /// <summary>
    /// Получить случайную фразу с подстановкой имени объекта
    /// </summary>
    /// <param name="objectName">Имя объекта для подстановки</param>
    /// <returns>Фраза с подставленным именем объекта</returns>
    public string GetRandomPhrase(string objectName)
    {
        if (searchPhrases == null || searchPhrases.Length == 0)
        {
            Debug.LogWarning("Массив фраз пуст!");
            return $"Где мой {objectName}?";
        }

        int randomIndex = Random.Range(0, searchPhrases.Length);
        return string.Format(searchPhrases[randomIndex], objectName);
    }

    /// <summary>
    /// Получить случайную фразу без подстановки (с placeholder {0})
    /// </summary>
    /// <returns>Фраза с placeholder</returns>
    public string GetRandomPhraseTemplate()
    {
        if (searchPhrases == null || searchPhrases.Length == 0)
        {
            Debug.LogWarning("Массив фраз пуст!");
            return "Где мой {0}?";
        }

        int randomIndex = Random.Range(0, searchPhrases.Length);
        return searchPhrases[randomIndex];
    }

    /// <summary>
    /// Добавить новую фразу в массив
    /// </summary>
    /// <param name="newPhrase">Новая фраза (должна содержать {0} для подстановки)</param>
    public void AddPhrase(string newPhrase)
    {
        if (string.IsNullOrEmpty(newPhrase))
        {
            Debug.LogWarning("Попытка добавить пустую фразу!");
            return;
        }

        if (!newPhrase.Contains("{0}"))
        {
            Debug.LogWarning("Фраза должна содержать {0} для подстановки имени объекта!");
            return;
        }

        System.Array.Resize(ref searchPhrases, searchPhrases.Length + 1);
        searchPhrases[searchPhrases.Length - 1] = newPhrase;
    }

    /// <summary>
    /// Получить количество доступных фраз
    /// </summary>
    public int GetPhrasesCount()
    {
        return searchPhrases != null ? searchPhrases.Length : 0;
    }
}

