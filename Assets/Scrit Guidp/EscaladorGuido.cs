using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit; // ← REQUIRED for SelectExitEventArgs
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
public class EscaladorGuido : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target; // What gets scaled (UI panel, window, etc.)

    [Header("Scaling")]
    [SerializeField] private bool enableTwoHandScaling = true;
    [SerializeField] private float minScale = 0.3f;
    [SerializeField] private float maxScale = 2.5f;
    [SerializeField] private float scaleMultiplier = 1f;

    private XRBaseInteractable interactable;
    private readonly List<XRBaseInteractor> interactors = new();

    private float startDistance;
    private Vector3 startScale;

    private void Awake()
    {
        if (target == null)
            target = transform;

        interactable = GetComponent<XRBaseInteractable>();
    }

    private void OnEnable()
    {
        interactable.selectEntered.AddListener(OnSelectEntered);
        interactable.selectExited.AddListener(OnSelectExited);
    }

    private void OnDisable()
    {
        interactable.selectEntered.RemoveListener(OnSelectEntered);
        interactable.selectExited.RemoveListener(OnSelectExited);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        var interactor = args.interactorObject as XRBaseInteractor;
        if (interactor == null)
            return;

        if (!interactors.Contains(interactor))
            interactors.Add(interactor);

        // Cache scale data when second hand grabs
        if (enableTwoHandScaling && interactors.Count == 2)
        {
            startDistance = Vector3.Distance(
                interactors[0].transform.position,
                interactors[1].transform.position
            );

            startScale = target.localScale;
        }
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        var interactor = args.interactorObject as XRBaseInteractor;
        if (interactor == null)
            return;

        interactors.Remove(interactor);
    }

    private void Update()
    {
        if (!enableTwoHandScaling)
            return;

        if (interactors.Count != 2)
            return;

        float currentDistance = Vector3.Distance(
            interactors[0].transform.position,
            interactors[1].transform.position
        );

        if (startDistance <= 0.001f)
            return;

        float scaleFactor = (currentDistance / startDistance) * scaleMultiplier;

        Vector3 targetScale = startScale * scaleFactor;

        float clamped = Mathf.Clamp(targetScale.x, minScale, maxScale);
        target.localScale = Vector3.one * clamped;
    }
}
