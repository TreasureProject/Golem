using System.Collections.Generic;
using UnityEngine;

namespace Golem.Vision
{
    /// <summary>
    /// Verifies action outcomes using VIGA-style before/after visual comparison.
    /// </summary>
    public class ActionVerifier : MonoBehaviour
    {
        [Header("Configuration")]
        public VisionConfig config;

        [Header("Verification Settings")]
        [Tooltip("Actions that require visual verification.")]
        public string[] actionsRequiringVerification = { "sit", "stand", "open", "close", "pickup", "drop", "use" };

        [Tooltip("Actions that don't require visual verification.")]
        public string[] actionsNotRequiringVerification = { "move", "look", "wait", "think" };

        private Dictionary<string, CaptureResult> beforeCaptures = new Dictionary<string, CaptureResult>();
        private Dictionary<string, CaptureResult> afterCaptures = new Dictionary<string, CaptureResult>();
        private HashSet<string> pendingVerifications = new HashSet<string>();
        private ActionVerificationStats stats = new ActionVerificationStats();

        public bool IsVerifying => pendingVerifications.Count > 0;
        public int PendingVerificationCount => pendingVerifications.Count;

        /// <summary>
        /// Check if a result has high enough confidence.
        /// </summary>
        public bool IsConfidentResult(ActionVerificationResult result)
        {
            if (config == null)
                return result.confidence >= 0.7f;

            return result.confidence >= config.minVerificationConfidence;
        }

        /// <summary>
        /// Check if an action type requires visual verification.
        /// </summary>
        public bool RequiresVerification(string actionName)
        {
            if (string.IsNullOrEmpty(actionName))
                return false;

            string actionLower = actionName.ToLower();

            // Check if explicitly not requiring verification
            foreach (var noVerify in actionsNotRequiringVerification)
            {
                if (actionLower == noVerify.ToLower())
                    return false;
            }

            // Check if explicitly requiring verification
            foreach (var verify in actionsRequiringVerification)
            {
                if (actionLower == verify.ToLower())
                    return true;
            }

            // Unknown actions default to requiring verification for safety
            return true;
        }

        /// <summary>
        /// Store a before-action capture.
        /// </summary>
        public void SetBeforeCapture(string actionId, CaptureResult capture)
        {
            beforeCaptures[actionId] = capture;
            pendingVerifications.Add(actionId);
        }

        /// <summary>
        /// Store an after-action capture.
        /// </summary>
        public void SetAfterCapture(string actionId, CaptureResult capture)
        {
            afterCaptures[actionId] = capture;
        }

        /// <summary>
        /// Check if there's a pending verification for an action.
        /// </summary>
        public bool HasPendingVerification(string actionId)
        {
            return pendingVerifications.Contains(actionId);
        }

        /// <summary>
        /// Clear pending verification data for an action.
        /// </summary>
        public void ClearPendingVerification(string actionId)
        {
            beforeCaptures.Remove(actionId);
            afterCaptures.Remove(actionId);
            pendingVerifications.Remove(actionId);
        }

        /// <summary>
        /// Record a verification result in statistics.
        /// </summary>
        public void RecordVerificationResult(bool success)
        {
            stats.totalVerifications++;
            if (success)
                stats.successfulVerifications++;
            else
                stats.failedVerifications++;
        }

        /// <summary>
        /// Get verification statistics.
        /// </summary>
        public ActionVerificationStats GetStats()
        {
            return stats;
        }

        /// <summary>
        /// Reset statistics.
        /// </summary>
        public void ResetStats()
        {
            stats = new ActionVerificationStats();
        }
    }
}
