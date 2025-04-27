using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIBlinker : MonoBehaviour
{
    [Header("Target UI Component")]
    public Graphic targetGraphic;

    [Header("Blink Settings")]
    public Color blinkColor = Color.red;
    public float blinkInterval = 0.5f;

    [Tooltip("Set to 0 to blink forever.")]
    public float blinkDuration = 0f;

    private Color originalColor;
    private Coroutine blinkCoroutine;

    void OnEnable()
    {
        if (targetGraphic != null)
        {
            originalColor = targetGraphic.color;
            blinkCoroutine = StartCoroutine(BlinkRoutine());
        }
    }

    void OnDisable()
    {
        StopBlinking();
    }

    IEnumerator BlinkRoutine()
    {
        float elapsed = 0f;

        while (blinkDuration == 0f || elapsed < blinkDuration)
        {
            targetGraphic.color = blinkColor;
            yield return new WaitForSeconds(blinkInterval);

            targetGraphic.color = originalColor;
            yield return new WaitForSeconds(blinkInterval);

            if (blinkDuration > 0f)
                elapsed += blinkInterval * 2;
        }

        targetGraphic.color = originalColor;
    }

    void StopBlinking()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        if (targetGraphic != null)
            targetGraphic.color = originalColor;
    }
}
