using UnityEngine;

[CreateAssetMenu(menuName = "Camera/CameraTransition")]
public class CameraStateTransitionSO : ScriptableObject
{
    // Single global transition asset — no per-state from/to fields
    public AnimationCurve interpolationCurve;
    public float transitionDuration = 1f;
    public bool avoidInside = true;   // flag to control path so it doesn’t clip inside

    [Header("Arc fallback when avoiding inside")]
    // Height multiplier for arc during transition when avoidInside is true. Higher values move camera over the target.
    public float arcHeight = 1f;
}
