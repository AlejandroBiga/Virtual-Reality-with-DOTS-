using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIProtractor : MonoBehaviour
{
    [Header("Measurement Points")]
    [SerializeField] private Transform vertexPoint; // Center point of angle
    [SerializeField] private Transform point1; // First ray point
    [SerializeField] private Transform point2; // Second ray point
    
    [Header("UI Elements")]
    [SerializeField] private RectTransform protractorImage; // The protractor image
    [SerializeField] private RectTransform line1; // Visual line to point1
    [SerializeField] private RectTransform line2; // Visual line to point2
    [SerializeField] private TextMeshProUGUI angleText; // Display angle value
    [SerializeField] private RectTransform arcIndicator; // Optional arc to show angle
    
    [Header("Settings")]
    [SerializeField] private bool updateContinuously = true;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Camera mainCamera;
    
    private float currentAngle = 0f;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void Update()
    {
        if (updateContinuously)
        {
            CalculateAndDisplayAngle();
        }
    }

    public void CalculateAndDisplayAngle()
    {
        if (vertexPoint == null || point1 == null || point2 == null)
        {
            Debug.LogWarning("Please assign all three points!");
            return;
        }

        // Calculate angle between three points in 3D space
        Vector3 direction1 = (point1.position - vertexPoint.position).normalized;
        Vector3 direction2 = (point2.position - vertexPoint.position).normalized;
        
        currentAngle = Vector3.Angle(direction1, direction2);
        
        // Update angle text
        if (angleText != null)
        {
            angleText.text = $"{currentAngle:F1}°";
        }
        
        // Update visual elements
        UpdateProtractorVisuals();
    }

    private void UpdateProtractorVisuals()
    {
        if (mainCamera == null || canvas == null) return;

        // Convert 3D positions to screen space
        Vector2 screenVertex = WorldToCanvasPosition(vertexPoint.position);
        Vector2 screenPoint1 = WorldToCanvasPosition(point1.position);
        Vector2 screenPoint2 = WorldToCanvasPosition(point2.position);
        
        // Position protractor at vertex
        if (protractorImage != null)
        {
            protractorImage.anchoredPosition = screenVertex;
            
            // Rotate protractor to align with first line
            Vector2 dir = (screenPoint1 - screenVertex).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            protractorImage.rotation = Quaternion.Euler(0, 0, angle - 90); // Adjust based on your image orientation
        }
        
        // Draw lines
        if (line1 != null)
        {
            DrawLineBetweenPoints(line1, screenVertex, screenPoint1);
        }
        
        if (line2 != null)
        {
            DrawLineBetweenPoints(line2, screenVertex, screenPoint2);
        }
        
        // Position angle text
        if (angleText != null)
        {
            angleText.rectTransform.anchoredPosition = screenVertex + new Vector2(0, 50);
        }
    }

    private Vector2 WorldToCanvasPosition(Vector3 worldPosition)
    {
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(mainCamera, worldPosition);
        
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            screenPoint,
            canvas.worldCamera,
            out Vector2 canvasPosition
        );
        
        return canvasPosition;
    }

    private void DrawLineBetweenPoints(RectTransform line, Vector2 start, Vector2 end)
    {
        Vector2 direction = end - start;
        float distance = direction.magnitude;
        
        line.anchoredPosition = start;
        line.sizeDelta = new Vector2(distance, line.sizeDelta.y);
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        line.rotation = Quaternion.Euler(0, 0, angle);
    }

    // Public method to set measurement points dynamically
    public void SetMeasurementPoints(Transform vertex, Transform p1, Transform p2)
    {
        vertexPoint = vertex;
        point1 = p1;
        point2 = p2;
        CalculateAndDisplayAngle();
    }

    // Get the current measured angle
    public float GetCurrentAngle()
    {
        return currentAngle;
    }
}