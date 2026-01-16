using UnityEngine;

[CreateAssetMenu(menuName = "Camera/CameraState")]
public class CameraStateSO : ScriptableObject
{
    [Header("Targeting")]
    public string targetBoneName;         // e.g. “Head”, “Chest”, etc (or empty = root)
    public Vector3 localOffset;           // offset from the bone or from target transform

    [Header("Rotation / Look")]
    public Vector3 eulerAngles;           // desired rotation relative to target, or absolute angles
    public bool lookAtTarget = true;      // whether camera should lookAt the target
    public Vector3 additionalRotationOffset;

    [Header("Movement / Smoothing")]
    public float moveSpeed = 5f;
    public float rotateSpeed = 5f;
    public float damping = 0.1f;

    [Header("Collision / Occlusion")]
    public bool enableCollision = true;
    public float collisionRadius = 0.5f;
    public LayerMask collisionMask;

    [Header("Transition fallback / constraints")]
    public float minDistance = 0.5f;      // minimum allowed distance to target
    public float maxDistance = 20f;       // maybe clamp range
    // maybe field for “transition arc height” or curve, etc.

    // (Optional) reference to transitions from this state
    public CameraStateTransitionSO[] transitions;
}
