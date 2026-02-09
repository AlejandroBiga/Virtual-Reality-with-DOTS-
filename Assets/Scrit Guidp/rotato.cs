using UnityEngine;

public class Rotato : MonoBehaviour
{
    [SerializeField] private Vector3 rotationAmount = new Vector3(0, 90, 0);
    [SerializeField] private float rotationDuration = 0.5f;
    
    private bool isRotating = false;

    public void RotateMesh()
    {
        if (!isRotating)
        {
            StartCoroutine(RotateCoroutine());
        }
    }

    private System.Collections.IEnumerator RotateCoroutine()
    {
        isRotating = true;
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(rotationAmount);
        
        float elapsed = 0f;
        
        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / rotationDuration;
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, t);
            yield return null;
        }
        
        transform.rotation = endRotation;
        isRotating = false;
    }
}