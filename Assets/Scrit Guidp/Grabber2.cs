using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Grabber2 : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform windowTransform;
    [SerializeField] private Transform xrCamera;

    [Header("Movement")]
    [SerializeField] private float followSpeed = 20f;

    [Header("Rotation")]
    [SerializeField] private bool rotateToCamera = true;
    [SerializeField] private float rotateSpeed = 8f;

    private XRGrabInteractable grab;
    private Transform interactor;
    private bool isGrabbed;

    private void Awake()
    {
        Debug.Log($"[GrabberDebug] Awake on {name}");

        if (!windowTransform)
        {
            windowTransform = transform;
            Debug.Log("[GrabberDebug] windowTransform was NULL — defaulted to self");
        }
    }

    private void OnEnable()
    {
        grab = GetComponent<XRGrabInteractable>();

        if (grab == null)
        {
            Debug.LogError("[GrabberDebug] ❌ XRGrabInteractable NOT FOUND on object");
            return;
        }

        Debug.Log("[GrabberDebug] XRGrabInteractable FOUND");

        grab.selectEntered.AddListener(OnGrab);
        grab.selectExited.AddListener(OnRelease);
    }

    private void OnDisable()
    {
        if (grab == null)
            return;

        grab.selectEntered.RemoveListener(OnGrab);
        grab.selectExited.RemoveListener(OnRelease);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        Debug.Log($"[GrabberDebug] ✅ GRAB DETECTED on {name}");

        interactor = args.interactorObject.transform;
        isGrabbed = true;

        Debug.Log($"[GrabberDebug] Interactor = {interactor.name}");
        Debug.Log($"[GrabberDebug] Interactor Type = {args.interactorObject.GetType().Name}");
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        Debug.Log($"[GrabberDebug] ❌ RELEASE DETECTED on {name}");

        isGrabbed = false;
        interactor = null;
    }

    private void Update()
    {
        if (!isGrabbed)
            return;

        Debug.Log("[GrabberDebug] Update Running While Grabbed");

        if (interactor == null)
        {
            Debug.LogError("[GrabberDebug] ❌ Interactor LOST");
            return;
        }

        // Movement
        Vector3 oldPos = windowTransform.position;
        Vector3 targetPos = interactor.position;

        windowTransform.position = Vector3.Lerp(
            oldPos,
            targetPos,
            Time.deltaTime * followSpeed
        );

        Debug.Log($"[GrabberDebug] Moving from {oldPos} -> {windowTransform.position}");

        // Rotation
        if (rotateToCamera && xrCamera != null)
        {
            Vector3 dir = xrCamera.position - windowTransform.position;
            dir.y = 0f;

            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                windowTransform.rotation = Quaternion.Slerp(
                    windowTransform.rotation,
                    targetRot,
                    Time.deltaTime * rotateSpeed
                );

                Debug.Log("[GrabberDebug] Rotating toward camera");
            }
        }
        else
        {
            Debug.LogWarning("[GrabberDebug] Camera missing OR rotation disabled");
        }
    }

    [ContextMenu("PRINT XR DEBUG REPORT")]
    private void PrintReport()
    {
        Debug.Log("========== XR GRAB DEBUG REPORT ==========");

        Debug.Log($"Object: {name}");

        Debug.Log($"XRGrabInteractable Present: {GetComponent<XRGrabInteractable>() != null}");

        Debug.Log($"Collider Count: {GetComponentsInChildren<Collider>().Length}");

        Debug.Log($"Interaction Layer Mask: {grab?.interactionLayers}");

        Debug.Log($"Track Position: {grab?.trackPosition}");
        Debug.Log($"Track Rotation: {grab?.trackRotation}");
        Debug.Log($"Track Scale: {grab?.trackScale}");

        Debug.Log($"Default Grab Transformers Enabled: {grab?.addDefaultGrabTransformers}");

        Debug.Log($"Interactor Active: {interactor != null}");
        Debug.Log($"Is Grabbed: {isGrabbed}");

        Debug.Log("==========================================");
    }
}
