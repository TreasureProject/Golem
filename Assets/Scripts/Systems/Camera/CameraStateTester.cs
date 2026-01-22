using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CameraStateTester : MonoBehaviour
{
    [SerializeField] private CameraStateMachine cameraStateMachine;
    [SerializeField] private CameraStateSO[] states;

    // index of the currently selected state in the array. starts at -1 so first press goes to states[0]
    private int currentIndex = -1;

    void Start()
    {
        if (cameraStateMachine == null)
            cameraStateMachine = FindObjectOfType<CameraStateMachine>();

        if (cameraStateMachine == null)
            Debug.LogWarning("CameraStateTester: No CameraStateMachine found. Assign one in inspector or place a CameraStateMachine in the scene.");

        if (states == null || states.Length == 0)
            Debug.LogWarning("CameraStateTester: No CameraStateSO entries assigned in the inspector.");

        // Ensure a transition asset is assigned for smooth transitions
        if (cameraStateMachine != null && cameraStateMachine.defaultTransition == null)
        {
            CameraStateTransitionSO transition = null;
            // Try to load from Resources folder
            transition = Resources.Load<CameraStateTransitionSO>("CameraTransition");
#if UNITY_EDITOR
            // If not found, try to find any asset in the project (Editor only)
            if (transition == null)
            {
                string[] guids = AssetDatabase.FindAssets("t:CameraStateTransitionSO");
                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    transition = AssetDatabase.LoadAssetAtPath<CameraStateTransitionSO>(path);
                }
            }
#endif
            if (transition != null)
            {
                cameraStateMachine.defaultTransition = transition;
                Debug.Log("CameraStateTester: Assigned CameraStateTransitionSO for smooth transitions.");
            }
            else
            {
                Debug.LogWarning("CameraStateTester: No CameraStateTransitionSO found. Transitions will be instant.");
            }
        }

        // If the machine has an initialState that lives in the array, align the index so the first K press moves to the next one
        if (cameraStateMachine != null && states != null && states.Length > 0)
        {
            int idx = Array.IndexOf(states, cameraStateMachine.initialState);
            currentIndex = idx >= 0 ? idx : -1;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (cameraStateMachine == null || states == null || states.Length == 0) return;

            currentIndex = (currentIndex + 1) % states.Length;
            CameraStateSO next = states[currentIndex];
            cameraStateMachine.ChangeState(next);

            Debug.Log($"CameraStateTester: Transitioning to state '{next.name}' (index {currentIndex}).");
        }
    }
}