using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreSystem : MonoBehaviour
{
    [Header("Score Settings")]
    public int airTimePointsPerSecond = 10;
    public int driftPointsPerSecond = 5;
    public int barrelRollPoints = 100;
    public float minimumAirTime = 0.5f; // Minimum air time before points start
    public float minimumDriftAngle = 15f; // Minimum drift angle before points start

    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI actionFeedbackText;
    public GameObject scorePopupPrefab; // Optional: for floating score popups

    [Header("Vehicle References")]
    public Rigidbody vehicleRigidbody;
    public Transform vehicleTransform;

    [Header("Ground Detection")]
    public LayerMask groundLayerMask = 1;
    public float groundCheckDistance = 1.5f;
    public Transform[] groundCheckPoints; // Multiple points for better detection

    [Header("Drift Detection")]
    public float driftThreshold = 0.3f; // How sideways the vehicle needs to be moving
    public AnimationCurve driftMultiplierCurve = AnimationCurve.EaseInOut(0, 1, 1, 3);

    [Header("Barrel Roll Detection")]
    public float barrelRollThreshold = 270f; // Degrees needed for a barrel roll
    public float barrelRollTimeLimit = 3f; // Max time to complete a barrel roll

    private int totalScore = 0;
    private float currentComboMultiplier = 1f;
    private float comboTimer = 0f;
    private float comboDuration = 3f;

    // Air Time Tracking
    private bool isGrounded = false;
    private float currentAirTime = 0f;
    private bool isInAir = false;

    // Drift Tracking
    private bool isDrifting = false;
    private float currentDriftTime = 0f;
    private float driftAngle = 0f;

    // Barrel Roll Tracking
    private bool isAttemptingBarrelRoll = false;
    private float barrelRollStartTime = 0f;
    private float totalRollRotation = 0f;
    private float lastZRotation = 0f;
    private bool barrelRollCompleted = false;

    private void Start()
    {
        InitializeUI();

        // If no ground check points assigned, use the vehicle transform
        if (groundCheckPoints == null || groundCheckPoints.Length == 0)
        {
            groundCheckPoints = new Transform[] { vehicleTransform };
        }
    }

    private void InitializeUI()
    {
        UpdateScoreUI();

        if (comboText != null)
            comboText.gameObject.SetActive(false);

        if (actionFeedbackText != null)
            actionFeedbackText.gameObject.SetActive(false);
    }

    private void Update()
    {
        CheckGroundStatus();
        UpdateAirTime();
        CheckDrift();
        CheckBarrelRoll();
        UpdateComboTimer();
    }

    private void CheckGroundStatus()
    {
        bool wasGrounded = isGrounded;
        isGrounded = IsVehicleGrounded();

        // Just became airborne
        if (wasGrounded && !isGrounded)
        {
            isInAir = true;
            currentAirTime = 0f;
            ShowActionFeedback("AIRBORNE!", Color.cyan);
        }
        // Just landed
        else if (!wasGrounded && isGrounded)
        {
            if (isInAir && currentAirTime >= minimumAirTime)
            {
                int airTimePoints = Mathf.RoundToInt(currentAirTime * airTimePointsPerSecond);
                AddScore(airTimePoints, "Air Time");
                ShowActionFeedback($"AIR TIME! +{airTimePoints}", Color.cyan);
            }
            isInAir = false;
            currentAirTime = 0f;
        }
    }

    private bool IsVehicleGrounded()
    {
        foreach (Transform checkPoint in groundCheckPoints)
        {
            if (Physics.Raycast(checkPoint.position, Vector3.down, groundCheckDistance, groundLayerMask))
            {
                return true;
            }
        }
        return false;
    }

    private void UpdateAirTime()
    {
        if (isInAir)
        {
            currentAirTime += Time.deltaTime;
        }
    }

    private void CheckDrift()
    {
        if (!isGrounded || vehicleRigidbody.linearVelocity.magnitude < 5f) // Only drift when moving and on ground
        {
            if (isDrifting)
            {
                EndDrift();
            }
            return;
        }

        // Calculate drift angle (angle between forward direction and velocity direction)
        Vector3 forwardDirection = vehicleTransform.forward;
        Vector3 velocityDirection = vehicleRigidbody.linearVelocity.normalized;

        driftAngle = Vector3.Angle(forwardDirection, velocityDirection);

        // Check if we're moving sideways enough to be drifting
        Vector3 velocityLocal = vehicleTransform.InverseTransformDirection(velocityDirection);
        float sidewaysComponent = Mathf.Abs(velocityLocal.x);

        bool shouldBeDrifting = sidewaysComponent > driftThreshold && driftAngle > minimumDriftAngle;

        if (shouldBeDrifting && !isDrifting)
        {
            StartDrift();
        }
        else if (!shouldBeDrifting && isDrifting)
        {
            EndDrift();
        }

        if (isDrifting)
        {
            currentDriftTime += Time.deltaTime;

            // Award points continuously while drifting
            if (currentDriftTime > 1f) // Award every second
            {
                float driftMultiplier = driftMultiplierCurve.Evaluate(driftAngle / 90f);
                int driftPoints = Mathf.RoundToInt(driftPointsPerSecond * driftMultiplier);
                AddScore(driftPoints, "Drift");
                currentDriftTime = 0f; // Reset timer
            }
        }
    }

    private void StartDrift()
    {
        isDrifting = true;
        currentDriftTime = 0f;
        ShowActionFeedback("DRIFTING!", Color.yellow);
    }

    private void EndDrift()
    {
        if (isDrifting)
        {
            isDrifting = false;
            ShowActionFeedback("DRIFT END", Color.yellow);
        }
    }

    private void CheckBarrelRoll()
    {
        if (!isInAir)
        {
            ResetBarrelRoll();
            return;
        }

        float currentZRotation = vehicleTransform.eulerAngles.z;

        // Normalize rotation to -180 to 180 range for easier calculation
        if (currentZRotation > 180f) currentZRotation -= 360f;

        if (!isAttemptingBarrelRoll)
        {
            // Start tracking if we begin rotating significantly
            if (Mathf.Abs(currentZRotation) > 45f)
            {
                isAttemptingBarrelRoll = true;
                barrelRollStartTime = Time.time;
                totalRollRotation = 0f;
                lastZRotation = currentZRotation;
                barrelRollCompleted = false;
            }
        }
        else
        {
            // Calculate rotation delta
            float rotationDelta = Mathf.DeltaAngle(lastZRotation, currentZRotation);
            totalRollRotation += Mathf.Abs(rotationDelta);
            lastZRotation = currentZRotation;

            // Check if we completed a barrel roll
            if (totalRollRotation >= barrelRollThreshold && !barrelRollCompleted)
            {
                CompleteBarrelRoll();
            }

            // Check for timeout
            if (Time.time - barrelRollStartTime > barrelRollTimeLimit)
            {
                ResetBarrelRoll();
            }
        }
    }

    private void CompleteBarrelRoll()
    {
        barrelRollCompleted = true;
        AddScore(barrelRollPoints, "Barrel Roll");
        ShowActionFeedback($"BARREL ROLL! +{barrelRollPoints}", Color.magenta);

        // Increase combo multiplier
        IncreaseCombo();

        ResetBarrelRoll();
    }

    private void ResetBarrelRoll()
    {
        isAttemptingBarrelRoll = false;
        totalRollRotation = 0f;
        barrelRollCompleted = false;
    }

    private void AddScore(int points, string action)
    {
        int finalPoints = Mathf.RoundToInt(points * currentComboMultiplier);
        totalScore += finalPoints;

        UpdateScoreUI();

        // Create floating score popup if prefab exists
        if (scorePopupPrefab != null)
        {
            CreateScorePopup(finalPoints, action);
        }

        Debug.Log($"{action}: +{finalPoints} points (x{currentComboMultiplier:F1} combo)");
    }

    private void IncreaseCombo()
    {
        currentComboMultiplier += 0.5f;
        currentComboMultiplier = Mathf.Min(currentComboMultiplier, 5f); // Cap at 5x
        comboTimer = comboDuration;

        UpdateComboUI();
    }

    private void UpdateComboTimer()
    {
        if (comboTimer > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0)
            {
                ResetCombo();
            }
        }
    }

    private void ResetCombo()
    {
        currentComboMultiplier = 1f;
        UpdateComboUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {totalScore:N0}";
        }
    }

    private void UpdateComboUI()
    {
        if (comboText != null)
        {
            if (currentComboMultiplier > 1f)
            {
                comboText.gameObject.SetActive(true);
                comboText.text = $"COMBO x{currentComboMultiplier:F1}";
                comboText.color = Color.Lerp(Color.white, Color.red, (currentComboMultiplier - 1f) / 4f);
            }
            else
            {
                comboText.gameObject.SetActive(false);
            }
        }
    }

    private void ShowActionFeedback(string message, Color color)
    {
        if (actionFeedbackText != null)
        {
            StopCoroutine("ActionFeedbackCoroutine");
            StartCoroutine(ActionFeedbackCoroutine(message, color));
        }
    }

    private IEnumerator ActionFeedbackCoroutine(string message, Color color)
    {
        actionFeedbackText.gameObject.SetActive(true);
        actionFeedbackText.text = message;
        actionFeedbackText.color = color;

        yield return new WaitForSeconds(2f);

        actionFeedbackText.gameObject.SetActive(false);
    }

    private void CreateScorePopup(int points, string action)
    {
        // This would instantiate a floating score popup prefab
        // You would need to create a separate ScorePopup script for this
        GameObject popup = Instantiate(scorePopupPrefab, vehicleTransform.position + Vector3.up * 3f, Quaternion.identity);
        ScorePopup popupScript = popup.GetComponent<ScorePopup>();
        if (popupScript != null)
        {
            popupScript.Initialize($"+{points}", action);
        }
    }

    // Public methods for external access
    public int GetTotalScore() { return totalScore; }
    public float GetComboMultiplier() { return currentComboMultiplier; }
    public bool GetIsInAir() { return isInAir; }
    public bool GetIsDrifting() { return isDrifting; }
    public float GetCurrentAirTime() { return currentAirTime; }

    // Method to manually add bonus points
    public void AddBonusPoints(int points, string reason)
    {
        AddScore(points, reason);
    }

    private void OnDrawGizmosSelected()
    {
        // Draw ground check rays in scene view
        if (groundCheckPoints != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            foreach (Transform checkPoint in groundCheckPoints)
            {
                if (checkPoint != null)
                {
                    Gizmos.DrawRay(checkPoint.position, Vector3.down * groundCheckDistance);
                }
            }
        }
    }
}
