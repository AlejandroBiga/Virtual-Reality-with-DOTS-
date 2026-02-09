using UnityEngine;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;

public class QuestRefreshRate : MonoBehaviour
{
    void Start()
    {
        Application.targetFrameRate = 120;
        Debug.Log("Target framerate set to 120");
    }
}