using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TitleTextAnimator : MonoBehaviour
{
    private TextMeshProUGUI titleTextComponent;
    // Start is called before the first frame update
    void Start()
    {
        titleTextComponent = GetComponent<TextMeshProUGUI>();
        StartCoroutine(TitleTextMovementRoutine());
    }
    IEnumerator TitleTextMovementRoutine()
    {
        // The position to use for the extra position
        Vector3 extraPosition = Vector3.up;
        // The startPosition
        Vector3 startPosition = titleTextComponent.rectTransform.anchoredPosition;
        // How much to move
        float moveDistance = 10f;
        // How fast to move
        float speed = 0.5f;
        // The timer
        float timer = 0f;
        while (true)
        {
            // Increase the timer with deltaTime * speed
            timer += Time.deltaTime * speed;
            // USe sin to rock up and down
            extraPosition = Vector3.up * Mathf.Sin(timer * Mathf.PI) * moveDistance;
            // Set the position
            titleTextComponent.rectTransform.anchoredPosition = startPosition + extraPosition;
            // When we get to 1, reset
            if(timer>= 1f)
            {
                timer = -1f;
            }
            yield return null;
        }
    }
}
