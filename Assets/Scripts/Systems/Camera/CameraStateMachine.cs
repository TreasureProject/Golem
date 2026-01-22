using UnityEngine;

public class CameraStateMachine : MonoBehaviour
{
    public CameraStateSO initialState;
    public Transform target;   // your actor / character root
    public CameraStateTransitionSO defaultTransition; // single global transition for all state changes

    private CameraStateSO currentState;

    private bool isTransitioning;
    private float transitionTimer;
    private float transitionDuration;
    private CameraStateTransitionSO currentTransition;
    private CameraStateSO transitionTarget; // the state we're transitioning to
    private Vector3 startPos, endPos;
    private Quaternion startRot, endRot;

    // For arc transitions
    private Vector3 arcControlPoint;

    void Start()
    {
        currentState = initialState;
        // Optionally immediately snap camera to initial state
        Vector3 initPos = ComputeStateTargetPosition(currentState);
        Quaternion initRot = ComputeStateRotation(currentState, initPos);
        transform.position = initPos;
        transform.rotation = initRot;
    }

    void LateUpdate()
    {
        if (target == null) return;

        if (isTransitioning)
        {
            transitionTimer += Time.deltaTime;
            float t = Mathf.Clamp01(transitionTimer / transitionDuration);
            float eval = currentTransition != null && currentTransition.interpolationCurve != null
                ? currentTransition.interpolationCurve.Evaluate(t)
                : t;

            Vector3 desiredPos;
            if (currentTransition != null && currentTransition.avoidInside)
            {
                // sample quadratic bezier (start, control, end)
                desiredPos = SampleArcPosition(startPos, arcControlPoint, endPos, eval);
                // ensure we don't clip into world
                if (transitionTarget != null && transitionTarget.enableCollision)
                {
                    desiredPos = ResolveCollision(transitionTarget, desiredPos);
                }
            }
            else
            {
                desiredPos = Vector3.Lerp(startPos, endPos, eval);
                if (transitionTarget != null && transitionTarget.enableCollision)
                {
                    desiredPos = ResolveCollision(transitionTarget, desiredPos);
                }
            }

            Quaternion desiredRot = Quaternion.Slerp(startRot, endRot, eval);

            transform.position = desiredPos;
            transform.rotation = desiredRot;

            if (t >= 1f)
            {
                isTransitioning = false;
                currentState = transitionTarget;
                currentTransition = null;
                transitionTarget = null;
            }
        }
        else
        {
            if (currentState == null) return;

            Vector3 desiredPos = ComputeStateTargetPosition(currentState);
            Quaternion desiredRot = ComputeStateRotation(currentState, desiredPos);

            // smoothing (exponential smoothing)
            float moveLerp = 1 - Mathf.Exp(-currentState.moveSpeed * Time.deltaTime);
            float rotateLerp = 1 - Mathf.Exp(-currentState.rotateSpeed * Time.deltaTime);

            Vector3 smoothedPos = Vector3.Lerp(transform.position, desiredPos, moveLerp);
            Quaternion smoothedRot = Quaternion.Slerp(transform.rotation, desiredRot, rotateLerp);

            if (currentState.enableCollision)
            {
                smoothedPos = ResolveCollision(currentState, smoothedPos);
            }

            transform.position = smoothedPos;
            transform.rotation = smoothedRot;
        }
    }

    public void ChangeState(CameraStateSO newState)
    {
        Debug.Log("CameraStateMachine: ChangeState to " + (newState != null ? newState.name : "null"));
        if (newState == currentState) return;

        // If we don't have a current state (e.g. at startup), just snap
        if (currentState == null)
        {
            Debug.Log(" No current state, snapping to new state.");
            SnapToState(newState);
            return;
        }

        // If a defaultTransition is assigned, use it for every state change
        if (defaultTransition != null)
        {
            var trans = defaultTransition;

            isTransitioning = true;
            currentTransition = trans;
            transitionTarget = newState;
            transitionTimer = 0f;
            // enforce a visible minimum duration so we don't instantly finish
            transitionDuration = Mathf.Max(0.15f, trans.transitionDuration);

            // capture raw start/end from transforms
            Vector3 rawStart = transform.position;
            Vector3 rawEnd = ComputeStateTargetPosition(newState);

            startRot = transform.rotation;
            endRot = ComputeStateRotation(newState, rawEnd);

            // If transition requests avoiding inside geometry, pre-adjust both endpoints so interpolation doesn't go through the target or inside colliders
            if (trans.avoidInside)
            {
                startPos = AdjustPositionForTransition(currentState, rawStart);
                endPos = AdjustPositionForTransition(newState, rawEnd);

                // if adjustments collapsed positions, add a stronger lateral offset so arc is visible
                float sep = Vector3.Distance(startPos, endPos);
                if (sep < 0.15f)
                {
                    // build lateral from the vector around the target
                    Vector3 fromTarget = (startPos - GetCollisionOrigin(newState));
                    if (fromTarget.sqrMagnitude < 0.0001f)
                        fromTarget = target.forward * 0.5f;
                    Vector3 lateral = Vector3.Cross(fromTarget.normalized, Vector3.up);
                    if (lateral.sqrMagnitude < 0.0001f)
                        lateral = target.right;
                    lateral = lateral.normalized * (0.6f + trans.arcHeight * 0.4f);
                    endPos += lateral;
                    sep = Vector3.Distance(startPos, endPos);
                }

                // compute arc control point above the target between start and end
                Vector3 mid = (startPos + endPos) * 0.5f;
                // raise along target up direction (or world up)
                Vector3 up = (target != null) ? target.up : Vector3.up;
                float arcH = Mathf.Max(0.05f, trans.arcHeight);
                arcControlPoint = mid + up * arcH;

                // If control point is inside geometry, push it outward along mid->control direction
                RaycastHit hit;
                Vector3 midDir = arcControlPoint - target.position;
                float midDist = midDir.magnitude;
                if (midDist > 0.001f)
                {
                    midDir /= midDist;
                    if (Physics.SphereCast(GetCollisionOrigin(newState), newState.collisionRadius, midDir, out hit, midDist - 0.01f, newState.collisionMask))
                    {
                        arcControlPoint = hit.point - midDir * newState.collisionRadius;
                    }
                }
            }
            else
            {
                startPos = rawStart;
                endPos = rawEnd;
            }

            // ensure transform starts exactly at startPos so interpolation is visible
            transform.position = startPos;
            transform.rotation = startRot;

            return;
        }

        // No default transition: fallback immediate switch
        currentState = newState;

        // Snap into new position/rotation (respect min/max distance and collision)
        Vector3 p = ComputeStateTargetPosition(newState);
        if (newState.enableCollision)
        {
            p = AdjustPositionForTransition(newState, p);
            p = ResolveCollision(newState, p);
        }
        Quaternion r = ComputeStateRotation(newState, p);
        transform.position = p;
        transform.rotation = r;
    }

    // Helper to snap instantly to a state (used on startup or when there's no current state)
    private void SnapToState(CameraStateSO newState)
    {
        currentState = newState;
        Vector3 p = ComputeStateTargetPosition(newState);
        if (newState.enableCollision)
        {
            p = AdjustPositionForTransition(newState, p);
            p = ResolveCollision(newState, p);
        }
        Quaternion r = ComputeStateRotation(newState, p);
        transform.position = p;
        transform.rotation = r;
    }

    Vector3 ComputeStateTargetPosition(CameraStateSO state)
    {
        Transform bone = target;
        if (!string.IsNullOrEmpty(state.targetBoneName))
        {
            Transform child = target.Find(state.targetBoneName);
            if (child != null)
                bone = child;
        }
        // localOffset is in local bone space, so transform direction
        return bone.position + bone.TransformDirection(state.localOffset);
    }

    Quaternion ComputeStateRotation(CameraStateSO state, Vector3 camPos)
    {
        if (state.lookAtTarget)
        {
            Vector3 lookAt = target.position;
            // optionally use bone position instead
            return Quaternion.LookRotation(lookAt - camPos) * Quaternion.Euler(state.additionalRotationOffset);
        }
        else
        {
            return Quaternion.Euler(state.eulerAngles);
        }
    }

    // Resolve collision using the bone (head/chest) as origin. If desiredPos is inside geometry, push it out using Collider.ClosestPoint.
    Vector3 ResolveCollision(CameraStateSO state, Vector3 desiredPos)
    {
        if (state == null || target == null) return desiredPos;

        Vector3 from = GetCollisionOrigin(state);
        Vector3 to = desiredPos;
        Vector3 dir = to - from;
        float dist = dir.magnitude;

        // If the desired position is extremely close, just return it
        if (dist < 0.001f)
            return desiredPos;

        dir /= dist;  // normalize

        RaycastHit hit;
        float safetyMargin = 0.01f;

        // If there's geometry between the bone and the desired position, move camera to just before hit
        if (Physics.SphereCast(from, state.collisionRadius, dir, out hit, dist - safetyMargin, state.collisionMask))
        {
            Vector3 hitPos = hit.point - dir * state.collisionRadius;
            return hitPos;
        }

        // If desiredPos is inside geometry (overlap), push it to nearest surface point + radius
        Collider[] overlaps = Physics.OverlapSphere(desiredPos, state.collisionRadius * 0.5f, state.collisionMask);
        if (overlaps != null && overlaps.Length > 0)
        {
            // find the closest collider point and push outward
            foreach (var col in overlaps)
            {
                if (col == null) continue;
                Vector3 closest = col.ClosestPoint(desiredPos);
                Vector3 pushDir = (desiredPos - closest);
                float pushLen = pushDir.magnitude;
                if (pushLen < 0.001f)
                {
                    // fallback push direction away from target
                    pushDir = (desiredPos - from);
                    pushLen = pushDir.magnitude;
                }

                if (pushLen > 0.001f)
                {
                    pushDir /= pushLen;
                    return closest + pushDir * state.collisionRadius;
                }
            }
        }

        return desiredPos;
    }

    // Get the best origin point for collision checks (prefer specified bone)
    Vector3 GetCollisionOrigin(CameraStateSO state)
    {
        if (target == null || state == null) return Vector3.zero;
        Transform bone = target;
        if (!string.IsNullOrEmpty(state.targetBoneName))
        {
            Transform child = target.Find(state.targetBoneName);
            if (child != null) bone = child;
        }
        return bone.position;
    }

    // Ensure the position lies on the ray away from the target, respects min/max distance and avoids starting inside colliders
    Vector3 AdjustPositionForTransition(CameraStateSO state, Vector3 originalPos)
    {
        if (state == null || target == null) return originalPos;

        Vector3 origin = GetCollisionOrigin(state);
        Vector3 dir = originalPos - origin;
        float dist = dir.magnitude;
        if (dist < 0.001f)
        {
            // fallback direction
            dir = (target.forward != Vector3.zero) ? target.forward : Vector3.up;
            dist = state.minDistance;
        }
        else
        {
            dir /= dist;
        }

        // Clamp distance to allowed range for this state
        float clamped = Mathf.Clamp(dist, state.minDistance, state.maxDistance);
        Vector3 adjusted = origin + dir * clamped;

        // If there is geometry between origin and adjusted position, move to just before the hit
        RaycastHit hit;
        float safetyMargin = 0.01f;
        if (Physics.SphereCast(origin, state.collisionRadius, dir, out hit, clamped - safetyMargin, state.collisionMask))
        {
            adjusted = hit.point - dir * state.collisionRadius;
        }

        return adjusted;
    }

    // Quadratic Bezier sample
    Vector3 SampleArcPosition(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        float u = 1 - t;
        return u * u * a + 2 * u * t * b + t * t * c;
    }
}
