using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[System.Serializable]
public class CameraView
{
    public string name;
    public CinemachineCamera camera;
    [Tooltip("Transition time in seconds. Set to 0 for instant cut.")]
    public float transitionTime = 1.0f;
}

public class CameraSystemController : MonoBehaviour
{
    [SerializeField] private List<CameraView> cameraViews = new List<CameraView>();
    [SerializeField] private CinemachineBrain cinemachineBrain;
    [SerializeField] private int startingCameraIndex = 0;
    [SerializeField] private InputActionReference cycleCameraAction;
    
    private int currentCameraIndex = 0;
    private int basePriority = 10;
    
    void OnEnable()
    {
        if (cycleCameraAction != null)
        {
            cycleCameraAction.action.Enable();
            cycleCameraAction.action.performed += OnCycleCamera;
        }
    }
    
    void OnDisable()
    {
        if (cycleCameraAction != null)
        {
            cycleCameraAction.action.performed -= OnCycleCamera;
            cycleCameraAction.action.Disable();
        }
    }
    
    void Start()
    {
        if (cinemachineBrain == null)
        {
            cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
        }
        
        // Set starting camera
        currentCameraIndex = Mathf.Clamp(startingCameraIndex, 0, cameraViews.Count - 1);
        ActivateCamera(currentCameraIndex);
    }
    
    private void OnCycleCamera(InputAction.CallbackContext context)
    {
        CycleToNextCamera();
    }
    
    void CycleToNextCamera()
    {
        if (cameraViews.Count == 0) return;
        
        // Move to next camera, wrapping around to 0 if at the end
        currentCameraIndex = (currentCameraIndex + 1) % cameraViews.Count;
        ActivateCamera(currentCameraIndex);
    }
    
    void ActivateCamera(int index)
    {
        if (index < 0 || index >= cameraViews.Count) return;
        
        // Set all cameras to low priority
        for (int i = 0; i < cameraViews.Count; i++)
        {
            if (cameraViews[i].camera != null)
            {
                cameraViews[i].camera.Priority.Value = 0;
            }
        }
        
        // Activate the selected camera
        CameraView selectedView = cameraViews[index];
        if (selectedView.camera != null)
        {
            selectedView.camera.Priority.Value = basePriority;
            
            // Set blend time for this transition
            if (cinemachineBrain != null)
            {
                if (selectedView.transitionTime <= 0f)
                {
                    // Instant cut
                    cinemachineBrain.DefaultBlend.Style = CinemachineBlendDefinition.Styles.Cut;
                }
                else
                {
                    // Smooth blend with specified time
                    cinemachineBrain.DefaultBlend.Style = CinemachineBlendDefinition.Styles.EaseInOut;
                    cinemachineBrain.DefaultBlend.Time = selectedView.transitionTime;
                }
            }
            
            Debug.Log($"Switched to camera: {selectedView.name}");
        }
    }
}