using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace JetXR.VisionUI
{
    public class EscalaUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private GameObject objectToTransform;
        [SerializeField] private Animator animator;
        [SerializeField] private string hoveredBool = "RHovered";

        private bool isDrag;
        private bool isHover;
        private bool xrEnabled;

        private Vector3 defaultColliderSize;

        private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;
        private UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor interactor;
        private BoxCollider boxCollider;

        private Vector3 scaleOnSelectEntered;
        private float distanceOnSelectEntered;

        private void OnEnable()
        {
            xrEnabled = XRSettings.enabled;

            // SAFE Animator resolve
            if (animator == null)
            {
                animator = GetComponent<Animator>();
                if (animator == null)
                    animator = GetComponentInChildren<Animator>();
                if (animator == null)
                    animator = GetComponentInParent<Animator>();
            }

            if (animator == null)
                Debug.LogError($"[Resizer] Animator NOT FOUND on {gameObject.name}");
            else
                Debug.Log($"[Resizer] Animator found on {animator.gameObject.name}");

            if (!xrEnabled)
                return;

            boxCollider = GetComponent<BoxCollider>();
            defaultColliderSize = boxCollider.size;

            interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();

            if (interactable != null)
            {
                interactable.selectEntered.AddListener(OnSelectEntered);
                interactable.selectExited.AddListener(OnSelectExited);
            }
            else
            {
                Debug.LogError($"[Resizer] XRBaseInteractable NOT FOUND on {gameObject.name}");
            }
        }

        #region Non XR

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (isHover)
                return;

            isHover = true;

            if (!isDrag && animator != null)
                animator.SetBool(hoveredBool, isHover);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isHover)
                return;

            isHover = false;

            if (!isDrag && animator != null)
                animator.SetBool(hoveredBool, isHover);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Debug.Log("[Resizer] POINTER DOWN (Non XR)");

            if (xrEnabled)
                return;

            scaleOnSelectEntered = objectToTransform.transform.localScale;
            distanceOnSelectEntered = Vector3.Distance(objectToTransform.transform.position, eventData.position);

            isDrag = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Debug.Log("[Resizer] POINTER UP (Non XR)");

            if (xrEnabled)
                return;

            isDrag = false;

            if (!isHover && animator != null)
                animator.SetBool(hoveredBool, isHover);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (xrEnabled)
                return;

            objectToTransform.transform.localScale =
                scaleOnSelectEntered *
                Vector3.Distance(objectToTransform.transform.position, eventData.position) /
                distanceOnSelectEntered;
        }

        #endregion

        #region XR

        public void OnSelectEntered(SelectEnterEventArgs eventData)
        {
            Debug.Log("[Resizer] XR SELECT ENTER");

            if (!xrEnabled)
                return;

            interactor = eventData.interactorObject.transform.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor>();

            scaleOnSelectEntered = objectToTransform.transform.localScale;

            // fallback distance
            distanceOnSelectEntered = Vector3.Distance(objectToTransform.transform.position, interactor.transform.position);

            // Ray interactor optional
            if (interactor is UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor ray)
            {
                if (ray.TryGetCurrent3DRaycastHit(out var hit))
                    distanceOnSelectEntered = Vector3.Distance(objectToTransform.transform.position, hit.point);
            }

            distanceOnSelectEntered = Mathf.Max(distanceOnSelectEntered, 0.001f);

            boxCollider.size = new Vector3(1000, 1000, 1);

            isDrag = true;
        }

        public void OnSelectExited(SelectExitEventArgs eventData)
        {
            Debug.Log("[Resizer] XR SELECT EXIT");

            if (!xrEnabled)
                return;

            boxCollider.size = defaultColliderSize;

            if (!isHover && animator != null)
                animator.SetBool(hoveredBool, isHover);

            isDrag = false;
        }

        private void Update()
        {
            if (!xrEnabled || interactable == null || !interactable.isSelected)
                return;

            if (interactor == null)
                return;

            float currentDistance = Vector3.Distance(objectToTransform.transform.position, interactor.transform.position);

            if (interactor is UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor ray)
            {
                if (ray.TryGetCurrent3DRaycastHit(out var hit))
                    currentDistance = Vector3.Distance(objectToTransform.transform.position, hit.point);
            }

            currentDistance = Mathf.Max(currentDistance, 0.001f);

            objectToTransform.transform.localScale =
                scaleOnSelectEntered * (currentDistance / distanceOnSelectEntered);
        }

        #endregion

        public void SetReferences(GameObject objectToTransform, Animator animator, string hoveredBool)
        {
            this.objectToTransform = objectToTransform;
            this.animator = animator;
            this.hoveredBool = hoveredBool;
        }
    }
}
