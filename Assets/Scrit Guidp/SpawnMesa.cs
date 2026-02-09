using UnityEngine;

public class SpawnerMesa : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject prefabToSpawn;
    public float spawnHeightOffset = 0.01f;

    [Header("Preview Settings")]
    public Material previewMaterial;

    [Header("Audio")]
    public AudioClip spawnSound;
    public float soundVolume = 1f;

    [Header("Raycast Settings")]
    public LayerMask tableLayer;
    public float rayDistance = 5f;

    [Header("Debug")]
    public bool enableDebug = true;

    // Internal
    private bool isHoveringUI = false;
    private bool hasValidHit = false;

    private RaycastHit lastHit;
    private Vector3 lockedSpawnPosition;
    private Quaternion lockedSpawnRotation;

    private GameObject previewInstance;

    void Update()
    {
        UpdateRaycast();
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

        if (!hasValidHit && previewInstance == null)
        {
            Log("ERROR — No valid spawn position");
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
    // RAYCAST TO FIND TABLE
    // ===========================

    void UpdateRaycast()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        hasValidHit = Physics.Raycast(ray, out lastHit, rayDistance, tableLayer);
    }

    // ===========================
    // LOCK POSITION
    // ===========================

    void LockSpawnPosition()
    {
        if (!hasValidHit)
        {
            Log("WARNING — No table detected to lock spawn");
            return;
        }

        lockedSpawnPosition = lastHit.point;
        lockedSpawnRotation = Quaternion.identity;

        Log("Locked spawn at: " + lockedSpawnPosition);
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

        if (hasValidHit)
        {
            Debug.DrawLine(Camera.main.transform.position, lastHit.point, Color.green);
            Debug.DrawRay(lastHit.point, Vector3.up * 0.15f, Color.yellow);
        }

        if (isHoveringUI)
        {
            Debug.DrawRay(lockedSpawnPosition, Vector3.up * 0.2f, Color.magenta);
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
        Gizmos.color = Color.green;
        if (hasValidHit)
            Gizmos.DrawSphere(lastHit.point, 0.015f);

        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(lockedSpawnPosition, 0.02f);
    }
}