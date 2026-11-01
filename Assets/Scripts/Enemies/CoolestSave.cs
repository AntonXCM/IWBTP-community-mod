using System;
using System.Collections;
using UnityEngine;

public class CoolestSave : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private Collider2D triggerCollider; // Сам триггер (Collider2D с enabled = true/false)
    [SerializeField] private Animator animator;         // Аниматор для запуска анимации Exchange
    [SerializeField] private AudioSource audioSource;   // Источник звука
    
    [SerializeField] private ParableNarrator dialogue;
    private bool isProcessing = false;

    private void Start()
    {
        for (int i = 0; i < 33; i++)
        {
            PlayerPrefs.SetInt("achieve" + i, 1);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isProcessing) return;
        if (!HasAnyAchieveOne())
        {
            dialogue.Subtitles.text = Localization.GetTextFormatted("p_greetings_aend", formatArgs: Environment.UserName);
            return;
        }

        StartCoroutine(ExchangeRoutine());
    }

    private IEnumerator ExchangeRoutine()
    {
        isProcessing = true;

        triggerCollider.enabled = false;
        animator?.Play("Exchange");

        yield return new WaitForSeconds(0.75f);

        if (audioSource != null)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
            else
            {
                audioSource.volume += 0.05f;
                audioSource.maxDistance += 16;
            }
        }
        for (int i = 0; i < 33; i++)
        {
            if (PlayerPrefs.GetInt("achieve" + i) != 1) continue;

            PlayerPrefs.SetInt("achieve" + i, 2);
            dialogue.Subtitles.text = Localization.GetText("p_greetings_a" + i);
            break; // Обрабатываем по одному за срабатывание триггера
        }

        // Сохраняем изменения в PlayerPrefs
        PlayerPrefs.Save();

        triggerCollider.enabled = true;

        isProcessing = false;
    }

    // Вспомогательный метод для проверки наличия хотя бы одного достижения со значением 1
    private bool HasAnyAchieveOne()
    {
        for (int i = 0; i < 33; i++)
        {
            if (PlayerPrefs.GetInt("achieve" + i) == 1)
            {
                return true;
            }
        }
        return false;
    }
}