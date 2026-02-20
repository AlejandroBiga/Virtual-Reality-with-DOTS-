using UnityEngine;

public class MeshMirrorMagnifier : MonoBehaviour
{
    [Header("Objects to Mirror")]
    [SerializeField] private Transform object1;
    [SerializeField] private Transform object2;
    
    [Header("Display Location")]
    [SerializeField] private Transform displayLocationEmpty;
    
    [Header("Magnification Settings")]
    [SerializeField] private float magnificationScale1 = 5f;
    [SerializeField] private float magnificationScale2 = 5f;
    
    [Header("Materials")]
    [SerializeField] private Material overrideMaterial1;
    [SerializeField] private Material overrideMaterial2;
    
    [Header("Offsets")]
    [SerializeField] private Vector3 offset1 = new Vector3(-0.5f, 0, 0);
    [SerializeField] private Vector3 offset2 = new Vector3(0.5f, 0, 0);
    
    private GameObject duplicate1;
    private GameObject duplicate2;

    void Start()
    {
        CreateDuplicates();
    }

    void CreateDuplicates()
    {
        if (object1 != null)
        {
            duplicate1 = Instantiate(object1.gameObject);
            duplicate1.name = object1.name + "_Magnified";
            
            if (overrideMaterial1 != null)
            {
                ApplyMaterial(duplicate1, overrideMaterial1);
            }
        }
        
        if (object2 != null)
        {
            duplicate2 = Instantiate(object2.gameObject);
            duplicate2.name = object2.name + "_Magnified";
            
            if (overrideMaterial2 != null)
            {
                ApplyMaterial(duplicate2, overrideMaterial2);
            }
        }
    }

    void ApplyMaterial(GameObject obj, Material mat)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.material = mat;
        }
    }

    void LateUpdate()
    {
        if (displayLocationEmpty == null) return;

        if (duplicate1 != null && object1 != null)
        {
            duplicate1.transform.position = displayLocationEmpty.position + offset1;
            duplicate1.transform.rotation = object1.rotation;
            duplicate1.transform.localScale = object1.lossyScale * magnificationScale1;
        }

        if (duplicate2 != null && object2 != null)
        {
            duplicate2.transform.position = displayLocationEmpty.position + offset2;
            duplicate2.transform.rotation = object2.rotation;
            duplicate2.transform.localScale = object2.lossyScale * magnificationScale2;
        }
    }

    void OnDestroy()
    {
        if (duplicate1 != null) Destroy(duplicate1);
        if (duplicate2 != null) Destroy(duplicate2);
    }
}