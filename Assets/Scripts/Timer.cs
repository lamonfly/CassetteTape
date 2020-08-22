using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    private TextMeshProUGUI text;
    private bool hasFinished = false;

    private void Start()
    {
        text = GetComponentInChildren<TextMeshProUGUI>();
    }

    void Update()
    {
        // Player has not finished
        if (!hasFinished)
        {
            // Timer text counts up using time scince load
            string minutes = "0" + ((int)Time.timeSinceLevelLoad / 60);
            string seconds = "0" + ((int)Time.timeSinceLevelLoad % 60);

            text.SetText(minutes.Substring(minutes.Length - 2) + ":" + seconds.Substring(seconds.Length - 2));
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // When player first reaches finish line
        if (collision.tag == "Player" && !hasFinished)
        {
            // Stop timer text and add green glow
            hasFinished = true;
            string minutes = "0" + ((int)Time.timeSinceLevelLoad / 60);
            string seconds = "0" + ((int)Time.timeSinceLevelLoad % 60);
            text.fontMaterial.SetColor(ShaderUtilities.ID_GlowColor, Color.green);
            text.SetText(minutes.Substring(minutes.Length - 2) + ":" + seconds.Substring(seconds.Length - 2));
        }
    }
}
