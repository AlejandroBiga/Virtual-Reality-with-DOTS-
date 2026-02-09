using UnityEngine;

public class ToggleParentDetach : MonoBehaviour
{
    [SerializeField] private Transform meshToAttach; // Reference to the Mesh object
    
    private Transform canvasOriginalParent;
    private Vector3 canvasLocalPosition;
    private Quaternion canvasLocalRotation;
    private Vector3 canvasLocalScale;
    
    private Transform meshOriginalParent;
    private Vector3 meshLocalPosition;
    private Quaternion meshLocalRotation;
    private Vector3 meshLocalScale;
    
    private Vector3 frozenWorldPosition;
    private Quaternion frozenWorldRotation;
    private Vector3 frozenWorldScale;
    
    private bool isDetached = false;

    void Awake()
    {
        // Save canvas original transform
        canvasOriginalParent = transform.parent;
        canvasLocalPosition = transform.localPosition;
        canvasLocalRotation = transform.localRotation;
        canvasLocalScale = transform.localScale;
        
        // Save mesh original transform
        if (meshToAttach != null)
        {
            meshOriginalParent = meshToAttach.parent;
            meshLocalPosition = meshToAttach.localPosition;
            meshLocalRotation = meshToAttach.localRotation;
            meshLocalScale = meshToAttach.localScale;
        }
    }

    public void ToggleDetach()
    {
        if (meshToAttach == null) return;

        if (isDetached)
        {
            // Reattach mesh to its original parent (CaderaDICOM)
            meshToAttach.SetParent(meshOriginalParent);
            meshToAttach.localPosition = meshLocalPosition;
            meshToAttach.localRotation = meshLocalRotation;
            meshToAttach.localScale = meshLocalScale;
            
            // Reattach canvas to its original parent (CaderaDICOM)
            transform.SetParent(canvasOriginalParent);
            transform.localPosition = canvasLocalPosition;
            transform.localRotation = canvasLocalRotation;
            transform.localScale = canvasLocalScale;
        }
        else
        {
            // Save current world transform of canvas before detaching
            frozenWorldPosition = transform.position;
            frozenWorldRotation = transform.rotation;
            frozenWorldScale = transform.lossyScale;
            
            // Detach canvas from parent
            transform.SetParent(null);
            
            // Set canvas to frozen world transform
            transform.position = frozenWorldPosition;
            transform.rotation = frozenWorldRotation;
            transform.localScale = frozenWorldScale;
        }
        
        isDetached = !isDetached;
    }

    void Update()
    {
        // Keep the canvas frozen in place while detached
        if (isDetached)
        {
            transform.position = frozenWorldPosition;
            transform.rotation = frozenWorldRotation;
            transform.localScale = frozenWorldScale;
        }
    }

    // Public method to check current state
    public bool IsDetached()
    {
        return isDetached;
    }
}