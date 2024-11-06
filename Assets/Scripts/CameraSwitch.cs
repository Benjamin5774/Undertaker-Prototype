using UnityEngine;

public class CameraSwitch : MonoBehaviour
{
    [SerializeField] private GameObject overshoulderCamera;
    [SerializeField] private GameObject overheadCamera;
    [SerializeField] private GameObject narrativeCamera;

    // Call this function to disable FPS camera,
    // and enable overhead camera and narrative camera.
    private void Awake()
    {
        overshoulderCamera.SetActive(false);
        overheadCamera.SetActive(false);
        narrativeCamera.SetActive(true);
    }
    public void ShowOverheadView()
    {
        overshoulderCamera.SetActive(false);
        narrativeCamera.SetActive(false);
        overheadCamera.SetActive(true);
    }

    // Call this function to enable FPS camera,
    // and disable overhead camera and narrative camera.
    public void ShowOvershoulderView()
    {
        overshoulderCamera.SetActive(true);
        overheadCamera.SetActive(false);
        narrativeCamera.SetActive(false);
    }

    // Call this function to enable FPS camera,
    // and disable overhead camera and narrative camera.
    public void ShowNarrativeView()
    {
        narrativeCamera.SetActive(true);
        overheadCamera.SetActive(false);
        overshoulderCamera.SetActive(false);
    }
}