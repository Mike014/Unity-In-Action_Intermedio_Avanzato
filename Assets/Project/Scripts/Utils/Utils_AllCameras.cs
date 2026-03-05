using UnityEngine;

public class Utils_AllCamersa : MonoBehaviour
{
    private void Start()
    {
        Camera[] cameras = Camera.allCameras;
        Debug.Log($"Telecamere attive: {cameras.Length}");

        foreach (Camera cam in cameras)
        {
            Debug.Log($"[{cam.name}] Position: {cam.transform.position} | Depth: {cam.depth}");
        }
    }

    private void Update()
    {
        foreach (Camera cam in Camera.allCameras)
        {
            // Visualizza direzione e posizione di ogni camera nella Scene view
            Debug.DrawRay(cam.transform.position, cam.transform.forward * 5f, Color.cyan);
        }
    }
}