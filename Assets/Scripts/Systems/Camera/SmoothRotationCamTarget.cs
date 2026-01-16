using UnityEngine;

public class SmoothRotationTarget : MonoBehaviour
{
    [SerializeField] private Transform targetToFollow;
    [SerializeField] private float rotationSpeed = 5f; // Higher = faster catch-up
    [SerializeField] private float yOffset = 0f; // Y-axis offset from target
    [SerializeField] private float rotationYOffset = 0f; // Y-axis rotation offset in degrees
    
    void LateUpdate()
    {
        if (targetToFollow == null) return;
        
        // Match position with Y offset
        Vector3 targetPosition = targetToFollow.position;
        targetPosition.y += yOffset;
        transform.position = targetPosition;
        
        // Smoothly rotate to match the target's rotation with Y offset
        Quaternion targetRotation = targetToFollow.rotation * Quaternion.Euler(0, rotationYOffset, 0);
        transform.rotation = Quaternion.Slerp(
            transform.rotation, 
            targetRotation, 
            rotationSpeed * Time.deltaTime
        );
    }
}