using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [Header("References")]
    private PlayerHealth player;       // assign in Inspector
    [SerializeField] private Transform heartContainer;   // the GameObject with Horizontal Layout
    [SerializeField] private GameObject heartPrefab;    // your HeartPrefab

    [Header("Heart Sprites")]
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite halfHeart;
    [SerializeField] private Sprite emptyHeart;

    private List<Image> hearts = new List<Image>();

    IEnumerator Start()
    {
        if (player == null)
        {
            // wait a frame so spawners have run
            yield return null;
            player = GameObject.FindWithTag("Player")?.GetComponent<PlayerHealth>();
        }

        if (player == null)
        {
            Debug.LogError("HealthUI: PlayerHealth reference not set or found!");
            yield break;
        }
   
        // how many hearts to show = one sprite per 2 HP
        int heartCount = Mathf.CeilToInt(player.MaxHealth / 2f);

        // instantiate your heart images
        for (int i = 0; i < heartCount; i++)
        {
            var go = Instantiate(heartPrefab, heartContainer);
            hearts.Add(go.GetComponent<Image>());
        }

        // initial draw
        UpdateHearts();

        // hook into health changes
        player.OnHealthChanged += UpdateHearts;
    }

    void OnDestroy()
    {
        player.OnHealthChanged -= UpdateHearts;
    }

    private void UpdateHearts()
    {
        float hp = player.CurrentHealth;
        for (int i = 0; i < hearts.Count; i++)
        {
            float threshold = i * 2;
            if (hp >= threshold + 2)
                hearts[i].sprite = fullHeart;
            else if (hp >= threshold + 1)
                hearts[i].sprite = halfHeart;
            else
                hearts[i].sprite = emptyHeart;
        }
    }
}
