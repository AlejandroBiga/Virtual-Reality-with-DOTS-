using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScrewAngleMeasurer : MonoBehaviour
{
    [Header("Objects to Measure")]
    [SerializeField] private Transform screw; // The screw object
    [SerializeField] private Transform plate; // The plate/surface object
    
    [Header("Measurement Settings")]
    [SerializeField] private MeasurementMode mode = MeasurementMode.ScrewToPlateNormal;
    [SerializeField] private bool updateContinuously = true;
    
    [Header("UI Elements")]
    [SerializeField] private RectTransform protractorImage;
    [SerializeField] private RectTransform line1; // Plate reference line
    [SerializeField] private RectTransform line2; // Screw angle line
    [SerializeField] private TextMeshProUGUI angleText;
    [SerializeField] private Canvas canvas;
    
    [Header("Visualization")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private float gizmoSize = 0.05f;
    
    private Camera mainCamera;
    private float currentAngle = 0f;
    
    // Calculated points
    private Vector3 intersectionPoint; // Where screw meets plate
    private Vector3 plateReferencePoint; // Point along plate surface
    private Vector3 screwDirectionPoint; // Point along screw axis
    
    public enum MeasurementMode
    {
        ScrewToPlateNormal,    // Angle between screw and plate's up vector (perpendicular)
        ScrewToPlateForward,   // Angle between screw and plate's forward
        ScrewToWorldUp,        // Angle between screw and world up (Y axis)
        Custom                 // Manually set reference direction
    }

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (updateContinuously)
        {
            CalculateAngle();
            UpdateVisuals();
        }
    }

    void CalculateAngle()
    {
        if (screw == null || plate == null)
        {
            Debug.LogWarning("Screw or Plate not assigned!");
            return;
        }

        // Step 1: Find intersection point (where screw touches plate)
        intersectionPoint = FindIntersectionPoint();
        
        // Step 2: Get screw direction (its forward axis or custom)
        Vector3 screwDirection = GetScrewDirection();
        
        // Step 3: Get plate reference direction (normal/perpendicular to surface)
        Vector3 plateReference = GetPlateReference();
        
        // Step 4: Calculate angle between screw and plate reference
        currentAngle = Vector3.Angle(screwDirection, plateReference);
        
        // Step 5: Calculate visualization points
        screwDirectionPoint = intersectionPoint + screwDirection * 0.2f;
        plateReferencePoint = intersectionPoint + plateReference * 0.2f;
        
        // Update text
        if (angleText != null)
        {
            angleText.text = $"{currentAngle:F1}°";
        }
    }

    Vector3 FindIntersectionPoint()
    {
        // Try to find where screw intersects with plate
        
        // Method 1: Raycast from screw along its axis to hit the plate
        Collider plateCollider = plate.GetComponent<Collider>();
        if (plateCollider != null)
        {
            Vector3 screwDirection = -screw.forward; // Assuming screw points down
            Ray ray = new Ray(screw.position, screwDirection);
            
            if (plateCollider.Raycast(ray, out RaycastHit hit, 10f))
            {
                return hit.point;
            }
        }
        
        // Method 2: Project screw position onto plate surface
        Vector3 plateNormal = plate.up;
        Vector3 platePoint = plate.position;
        Vector3 screwPos = screw.position;
        
        // Find closest point on plate plane
        float distance = Vector3.Dot(plateNormal, screwPos - platePoint);
        Vector3 projectedPoint = screwPos - plateNormal * distance;
        
        return projectedPoint;
    }

    Vector3 GetScrewDirection()
    {
        // The screw's main axis (usually forward or up depending on how it's oriented)
        // Adjust this based on your screw's orientation
        return -screw.forward; // Negative if screw points downward
    }

    Vector3 GetPlateReference()
    {
        switch (mode)
        {
            case MeasurementMode.ScrewToPlateNormal:
                // Perpendicular to plate surface (most common for insertion angle)
                return plate.up;
                
            case MeasurementMode.ScrewToPlateForward:
                return plate.forward;
                
            case MeasurementMode.ScrewToWorldUp:
                return Vector3.up;
                
            default:
                return plate.up;
        }
    }

    void UpdateVisuals()
    {
        if (mainCamera == null || canvas == null) return;

        // Convert 3D positions to screen/canvas positions
        Vector2 screenIntersection = WorldToCanvasPosition(intersectionPoint);
        Vector2 screenPlateRef = WorldToCanvasPosition(plateReferencePoint);
        Vector2 screenScrewDir = WorldToCanvasPosition(screwDirectionPoint);
        
        // Position protractor at intersection
        if (protractorImage != null)
        {
            protractorImage.anchoredPosition = screenIntersection;
            
            // Rotate protractor to align with plate reference
            Vector2 dir = (screenPlateRef - screenIntersection).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            protractorImage.rotation = Quaternion.Euler(0, 0, angle - 90);
        }
        
        // Draw lines
        if (line1 != null)
        {
            DrawLineBetweenPoints(line1, screenIntersection, screenPlateRef);
        }
        
        if (line2 != null)
        {
            DrawLineBetweenPoints(line2, screenIntersection, screenScrewDir);
        }
        
        // Position angle text
        if (angleText != null)
        {
            angleText.rectTransform.anchoredPosition = screenIntersection + new Vector2(0, 50);
        }
    }

    Vector2 WorldToCanvasPosition(Vector3 worldPosition)
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

    void DrawLineBetweenPoints(RectTransform line, Vector2 start, Vector2 end)
    {
        Vector2 direction = end - start;
        float distance = direction.magnitude;
        
        line.anchoredPosition = start;
        line.sizeDelta = new Vector2(distance, line.sizeDelta.y);
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        line.rotation = Quaternion.Euler(0, 0, angle);
    }

    void OnDrawGizmos()
    {
        if (!showGizmos || screw == null || plate == null) return;

        // Draw intersection point (RED)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(intersectionPoint, gizmoSize);
        
        // Draw plate reference direction (GREEN)
        Gizmos.color = Color.green;
        Gizmos.DrawLine(intersectionPoint, plateReferencePoint);
        Gizmos.DrawWireSphere(plateReferencePoint, gizmoSize * 0.7f);
        
        // Draw screw direction (CYAN)
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(intersectionPoint, screwDirectionPoint);
        Gizmos.DrawWireSphere(screwDirectionPoint, gizmoSize * 0.7f);
        
        // Draw angle arc
        Gizmos.color = Color.yellow;
        Vector3 plateRef = GetPlateReference();
        Vector3 screwDir = GetScrewDirection();
        
        // Draw small arc to visualize angle
        int steps = 10;
        for (int i = 0; i < steps; i++)
        {
            float t1 = (float)i / steps;
            float t2 = (float)(i + 1) / steps;
            
            Vector3 dir1 = Vector3.Slerp(plateRef, screwDir, t1);
            Vector3 dir2 = Vector3.Slerp(plateRef, screwDir, t2);
            
            Gizmos.DrawLine(
                intersectionPoint + dir1 * gizmoSize * 2f,
                intersectionPoint + dir2 * gizmoSize * 2f
            );
        }
    }

    // Public methods
    public float GetCurrentAngle() => currentAngle;
    
    public void SetObjects(Transform screwTransform, Transform plateTransform)
    {
        screw = screwTransform;
        plate = plateTransform;
        CalculateAngle();
    }
}