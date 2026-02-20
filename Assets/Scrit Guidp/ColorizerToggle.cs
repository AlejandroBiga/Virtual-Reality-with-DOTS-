using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARPlane))]
[RequireComponent(typeof(MeshRenderer))]
public class ColorizerToggle : MonoBehaviour
{
    [Header("Highlight Settings")]
    [SerializeField] private bool onlyHighlightTables = true;
    [SerializeField] private Color tableColor = Color.yellow;
    [SerializeField] private float tableAlpha = 0.5f;
    [SerializeField] private float nonTableAlpha = 0.0f; // 0 = invisible, 0.1 = very faint
    
    ARPlane m_ARPlane;
    MeshRenderer m_PlaneMeshRenderer;
    
    void Awake()
    {
        m_ARPlane = GetComponent<ARPlane>();
        m_PlaneMeshRenderer = GetComponent<MeshRenderer>();
        UpdatePlaneColor();
    }
    
    void OnEnable()
    {
        // Subscribe to classification changes
        m_ARPlane.boundaryChanged += OnBoundaryChanged;
    }
    
    void OnDisable()
    {
        m_ARPlane.boundaryChanged -= OnBoundaryChanged;
    }
    
    void OnBoundaryChanged(ARPlaneBoundaryChangedEventArgs args)
    {
        // Update color when plane classification might have changed
        UpdatePlaneColor();
    }
    
    void UpdatePlaneColor()
    {
        if (onlyHighlightTables)
        {
            // Only show tables, hide everything else
            if (m_ARPlane.classifications.HasFlag(PlaneClassifications.Table))
            {
                // This is a table - highlight it
                Color planeColor = tableColor;
                planeColor.a = tableAlpha;
                m_PlaneMeshRenderer.material.color = planeColor;
                m_PlaneMeshRenderer.enabled = true;
            }
            else
            {
                // Not a table - make it invisible or very faint
                Color planeColor = Color.gray;
                planeColor.a = nonTableAlpha;
                m_PlaneMeshRenderer.material.color = planeColor;
                
                // Optionally disable renderer completely for better performance
                if (nonTableAlpha == 0f)
                {
                    m_PlaneMeshRenderer.enabled = false;
                }
            }
        }
        else
        {
            // Original behavior - color everything by classification
            Color planeMatColor = GetColorByClassification(m_ARPlane.classifications);
            planeMatColor.a = 0.35f;
            m_PlaneMeshRenderer.material.color = planeMatColor;
            m_PlaneMeshRenderer.enabled = true;
        }
    }
    
    private Color GetColorByClassification(PlaneClassifications classifications)
    {
        if (classifications.HasFlag(PlaneClassifications.Table)) return Color.yellow;
        if (classifications.HasFlag(PlaneClassifications.Floor)) return Color.green;
        if (classifications.HasFlag(PlaneClassifications.WallFace)) return Color.white;
        if (classifications.HasFlag(PlaneClassifications.Ceiling)) return Color.red;
        if (classifications.HasFlag(PlaneClassifications.Couch)) return Color.blue;
        if (classifications.HasFlag(PlaneClassifications.Seat)) return Color.blue;
        if (classifications.HasFlag(PlaneClassifications.SeatOfAnyType)) return Color.blue;
        if (classifications.HasFlag(PlaneClassifications.WallArt)) return new Color(1f, 0.4f, 0f);  //orange
        if (classifications.HasFlag(PlaneClassifications.DoorFrame)) return Color.magenta;
        if (classifications.HasFlag(PlaneClassifications.WindowFrame)) return Color.cyan;
        return Color.gray; //Other - Default color
    }
}