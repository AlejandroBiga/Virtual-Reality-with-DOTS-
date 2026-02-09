using UnityEngine;

public class ChangeMaterialOnClick : MonoBehaviour
{
    [SerializeField] private Material material1;
    [SerializeField] private Material material2;
    
    private Renderer meshRenderer;
    private bool isFirstMaterial = true;

    void Awake()
    {
        meshRenderer = GetComponent<Renderer>();
    }

    public void ToggleMaterial()
    {
        if (isFirstMaterial)
        {
            meshRenderer.material = material2;
        }
        else
        {
            meshRenderer.material = material1;
        }
        
        isFirstMaterial = !isFirstMaterial;
    }
}