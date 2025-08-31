using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScorePopup : MonoBehaviour
{
    [Header("Popup Settings")]
    public float lifetime = 2f;
    public float floatSpeed = 2f;
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 0.2f, 1.2f);
    public AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Header("UI Components")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI actionText;
    public CanvasGroup canvasGroup;

    private Camera playerCamera;
    private Vector3 startPosition;
    private float elapsedTime = 0f;

    private void Start()
    {
        playerCamera = Camera.main;
        if (playerCamera == null)
            playerCamera = FindFirstObjectByType<Camera>();

        startPosition = transform.position;

        // Make sure canvas faces camera
        if (playerCamera != null)
        {
            transform.LookAt(transform.position + playerCamera.transform.rotation * Vector3.forward,
                           playerCamera.transform.rotation * Vector3.up);
        }
    }

    public void Initialize(string points, string action)
    {
        if (scoreText != null)
            scoreText.text = points;

        if (actionText != null)
            actionText.text = action;

        StartCoroutine(AnimatePopup());
    }

    private IEnumerator AnimatePopup()
    {
        while (elapsedTime < lifetime)
        {
            float normalizedTime = elapsedTime / lifetime;

            // Animate position (float upward)
            Vector3 currentPos = startPosition + Vector3.up * (floatSpeed * elapsedTime);
            transform.position = currentPos;

            // Animate scale
            float scale = scaleCurve.Evaluate(normalizedTime);
            transform.localScale = Vector3.one * scale;

            // Animate alpha
            if (canvasGroup != null)
            {
                canvasGroup.alpha = alphaCurve.Evaluate(normalizedTime);
            }

            // Keep facing camera
            if (playerCamera != null)
            {
                transform.LookAt(transform.position + playerCamera.transform.rotation * Vector3.forward,
                               playerCamera.transform.rotation * Vector3.up);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
