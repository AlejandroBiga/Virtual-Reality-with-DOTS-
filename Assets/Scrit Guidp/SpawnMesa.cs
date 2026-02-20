using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;
using System.Linq;

public class SpawnerMesa : MonoBehaviour
{
    [Header("AR Settings")]
    [SerializeField] private ARPlaneManager arPlaneManager;
    [SerializeField] private bool onlyHorizontalPlanes = true; // Only detect tables/floors
    [SerializeField] private float maxPlaneDistance = 5f; // Max distance to consider a plane
    
    [Header("Spawn Settings")]
    public GameObject prefabToSpawn;
    public float spawnHeightOffset = 0.01f;

    [Header("Preview Settings")]
    public Material previewMaterial;

    [Header("Audio")]
    public AudioClip spawnSound;
    public float soundVolume = 1f;

    [Header("Debug")]
    public bool enableDebug = true;

    // Internal
    private bool isHoveringUI = false;
    private bool hasValidPlane = false;

    private ARPlane currentClosestPlane;
    private Vector3 lockedSpawnPosition;
    private Quaternion lockedSpawnRotation;

    private GameObject previewInstance;

   void Start()
{
    // Auto-find ARPlaneManager if not assigned
    if (arPlaneManager == null)
    {
        arPlaneManager = FindFirstObjectByType<ARPlaneManager>();
        if (arPlaneManager == null)
        {
            Debug.LogError("[SpawnerMesa] No ARPlaneManager found in scene!");
        }
    }
}

    void Update()
    {
        UpdateClosestPlane();
        UpdatePreview();
        DrawDebug();
    }

    // ===========================
    // XR UI Hover Hooks
    // ===========================

    public void OnHoverEnter()
    {
        isHoveringUI = true;
        LockSpawnPosition();
        ShowPreview();

        Log("UI Hover ENTER — Locked spawn preview");
    }

    public void OnHoverExit()
    {
        isHoveringUI = false;
        HidePreview();

        Log("UI Hover EXIT — Preview hidden");
    }

    // ===========================
    // SPAWN FUNCTION
    // ===========================

    public void Spawn()
    {
        if (!prefabToSpawn)
        {
            Log("ERROR — No prefab assigned");
            return;
        }

        if (!hasValidPlane && previewInstance == null)
        {
            Log("ERROR — No valid AR plane detected");
            return;
        }

        Vector3 spawnPos = lockedSpawnPosition + Vector3.up * spawnHeightOffset;
        Quaternion spawnRot = lockedSpawnRotation;

        GameObject obj = Instantiate(prefabToSpawn, spawnPos, spawnRot);
        obj.transform.localScale = Vector3.one;

        PlaySpawnSound(spawnPos);

        Log("Spawned prefab at: " + spawnPos);
    }

    // ===========================
    // FIND CLOSEST AR PLANE
    // ===========================

    void UpdateClosestPlane()
    {
        if (arPlaneManager == null)
        {
            hasValidPlane = false;
            return;
        }

        ARPlane closestPlane = null;
        float closestDistance = float.MaxValue;
        Vector3 cameraPos = Camera.main.transform.position;

        // Iterate through all tracked planes
        foreach (ARPlane plane in arPlaneManager.trackables)
        {
            // Skip planes that aren't being tracked
            if (plane.trackingState != UnityEngine.XR.ARSubsystems.TrackingState.Tracking)
                continue;

            // Skip vertical planes if we only want horizontal (tables/floors)
            if (onlyHorizontalPlanes && !IsPlaneHorizontal(plane))
                continue;

            // Calculate distance from camera to plane center
            float distance = Vector3.Distance(cameraPos, plane.center);

            // Skip planes that are too far away
            if (distance > maxPlaneDistance)
                continue;

            // Track the closest plane
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlane = plane;
            }
        }

        // Update current state
        if (closestPlane != null)
        {
            hasValidPlane = true;
            currentClosestPlane = closestPlane;
        }
        else
        {
            hasValidPlane = false;
            currentClosestPlane = null;
        }
    }

    bool IsPlaneHorizontal(ARPlane plane)
    {
        // Get the plane's normal vector (pointing up from the surface)
        Vector3 normal = plane.transform.up;
        
        // Check if the normal is mostly pointing up (horizontal surface)
        // Dot product with Vector3.up: 1 = perfectly horizontal, 0 = vertical
        float horizontalness = Vector3.Dot(normal, Vector3.up);
        
        // Consider it horizontal if it's within 30 degrees of perfectly flat
        return horizontalness > 0.85f; // cos(30°) ≈ 0.866
    }

    // ===========================
    // LOCK POSITION
    // ===========================

    void LockSpawnPosition()
    {
        if (!hasValidPlane || currentClosestPlane == null)
        {
            Log("WARNING — No AR plane detected to lock spawn");
            return;
        }

        // Use the plane's center as spawn position
        lockedSpawnPosition = currentClosestPlane.center;
        
        // Align rotation to plane's orientation
        lockedSpawnRotation = currentClosestPlane.transform.rotation;

        Log($"Locked spawn at: {lockedSpawnPosition} on plane: {currentClosestPlane.trackableId}");
    }

    // ===========================
    // PREVIEW SYSTEM
    // ===========================

    void ShowPreview()
    {
        if (!prefabToSpawn) return;

        if (!previewInstance)
        {
            previewInstance = Instantiate(prefabToSpawn);
            ApplyPreviewMaterial(previewInstance);
        }

        previewInstance.SetActive(true);
    }

    void HidePreview()
    {
        if (previewInstance)
            previewInstance.SetActive(false);
    }

    void UpdatePreview()
    {
        if (!previewInstance || !isHoveringUI) return;

        Vector3 previewPos = lockedSpawnPosition + Vector3.up * spawnHeightOffset;
        previewInstance.transform.position = previewPos;
        previewInstance.transform.rotation = lockedSpawnRotation;
    }

    void ApplyPreviewMaterial(GameObject obj)
    {
        if (!previewMaterial) return;

        foreach (Renderer r in obj.GetComponentsInChildren<Renderer>())
        {
            r.material = previewMaterial;
        }
    }

    // ===========================
    // AUDIO
    // ===========================

    void PlaySpawnSound(Vector3 pos)
    {
        if (!spawnSound) return;
        AudioSource.PlayClipAtPoint(spawnSound, pos, soundVolume);
    }

    // ===========================
    // DEBUG DRAWING
    // ===========================

    void DrawDebug()
    {
        if (!enableDebug) return;

        // Draw line to closest plane
        if (hasValidPlane && currentClosestPlane != null)
        {
            Debug.DrawLine(Camera.main.transform.position, currentClosestPlane.center, Color.green);
            Debug.DrawRay(currentClosestPlane.center, Vector3.up * 0.15f, Color.yellow);
        }

        // Draw locked position when hovering
        if (isHoveringUI)
        {
            Debug.DrawRay(lockedSpawnPosition, Vector3.up * 0.2f, Color.magenta);
        }

        // Draw all detected planes
        if (arPlaneManager != null)
        {
            foreach (ARPlane plane in arPlaneManager.trackables)
            {
                if (plane.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Tracking)
                {
                    Color planeColor = (plane == currentClosestPlane) ? Color.cyan : Color.gray;
                    Debug.DrawRay(plane.center, plane.normal * 0.1f, planeColor);
                }
            }
        }
    }

    // ===========================
    // LOGGING
    // ===========================

    void Log(string msg)
    {
        if (enableDebug)
            Debug.Log("[SpawnerMesa] " + msg);
    }

    // ===========================
    // GIZMOS
    // ===========================

    void OnDrawGizmos()
    {
        // Draw current closest plane
        if (hasValidPlane && currentClosestPlane != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(currentClosestPlane.center, 0.03f);
            Gizmos.DrawRay(currentClosestPlane.center, currentClosestPlane.normal * 0.2f);
        }

        // Draw locked spawn position
        if (isHoveringUI)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(lockedSpawnPosition, 0.04f);
        }

        // Draw all AR planes (small dots)
        if (arPlaneManager != null)
        {
            foreach (ARPlane plane in arPlaneManager.trackables)
            {
                if (plane.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Tracking)
                {
                    Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
                    Gizmos.DrawWireCube(plane.center, new Vector3(0.1f, 0.01f, 0.1f));
                }
            }
        }
    }

    // ===========================
    // PUBLIC HELPERS
    // ===========================

    /// <summary>
    /// Get the currently detected closest plane (for debugging)
    /// </summary>
    public ARPlane GetCurrentPlane() => currentClosestPlane;

    /// <summary>
    /// Get count of all tracked planes
    /// </summary>
    public int GetTrackedPlaneCount()
    {
        if (arPlaneManager == null) return 0;
        return arPlaneManager.trackables.count;
    }
}