using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Golem.Vision
{
    /// <summary>
    /// Service for capturing camera frames for VLM analysis.
    /// Uses RenderTexture and AsyncGPUReadback for efficient capture.
    /// </summary>
    public class FrameCaptureService : MonoBehaviour
    {
        [Header("Configuration")]
        public VisionConfig config;

        [Header("Camera Settings")]
        [Tooltip("Camera to capture from. If null, uses main camera.")]
        public Camera targetCamera;

        [Tooltip("Optional secondary camera for third-person view.")]
        public Camera thirdPersonCamera;

        [Tooltip("Optional overhead camera.")]
        public Camera overheadCamera;

        private RenderTexture renderTexture;
        private Texture2D readbackTexture;
        private bool isCapturing;
        private int lastConfigWidth;
        private int lastConfigHeight;

        public bool IsCapturing => isCapturing;

        public event Action<CaptureResult> OnCaptureComplete;

        /// <summary>
        /// Compute a hash for image data.
        /// </summary>
        public static string ComputeImageHash(byte[] data)
        {
            if (data == null || data.Length == 0)
                return "";

            // Simple hash using first 8 hex chars
            int hash = 17;
            foreach (byte b in data)
            {
                hash = hash * 31 + b;
            }
            return hash.ToString("X8");
        }

        /// <summary>
        /// Capture a frame asynchronously.
        /// </summary>
        public void CaptureFrameAsync(Action<CaptureResult> callback, CaptureMode? overrideMode = null)
        {
            if (isCapturing)
            {
                callback?.Invoke(new CaptureResult
                {
                    success = false,
                    errorMessage = "Capture already in progress"
                });
                return;
            }

            StartCoroutine(CaptureCoroutine(callback, overrideMode ?? config.captureMode));
        }

        /// <summary>
        /// Capture a frame synchronously (blocking).
        /// </summary>
        public CaptureResult CaptureFrameSync(CaptureMode? overrideMode = null)
        {
            CaptureMode mode = overrideMode ?? config.captureMode;
            Camera cam = GetCameraForMode(mode);

            if (cam == null)
            {
                return new CaptureResult
                {
                    success = false,
                    errorMessage = "No camera available for capture"
                };
            }

            ReconfigureIfNeeded();

            // Render to texture
            RenderTexture previousRT = cam.targetTexture;
            cam.targetTexture = renderTexture;
            cam.Render();
            cam.targetTexture = previousRT;

            // Read pixels
            RenderTexture.active = renderTexture;
            readbackTexture.ReadPixels(new Rect(0, 0, config.captureWidth, config.captureHeight), 0, 0);
            readbackTexture.Apply();
            RenderTexture.active = null;

            // Encode to JPEG
            byte[] imageBytes = readbackTexture.EncodeToJPG(config.jpegQuality);

            return new CaptureResult
            {
                success = true,
                imageBytes = imageBytes,
                imageBase64 = Convert.ToBase64String(imageBytes),
                width = config.captureWidth,
                height = config.captureHeight,
                captureMode = mode,
                captureTime = Time.time
            };
        }

        /// <summary>
        /// Reconfigure render resources if settings changed.
        /// </summary>
        public void ReconfigureIfNeeded()
        {
            if (config == null)
                return;

            if (renderTexture == null ||
                lastConfigWidth != config.captureWidth ||
                lastConfigHeight != config.captureHeight)
            {
                CleanupResources();
                InitializeResources();
            }
        }

        private IEnumerator CaptureCoroutine(Action<CaptureResult> callback, CaptureMode mode)
        {
            isCapturing = true;

            Camera cam = GetCameraForMode(mode);

            if (cam == null)
            {
                isCapturing = false;
                var errorResult = new CaptureResult
                {
                    success = false,
                    errorMessage = "No camera available for capture"
                };
                callback?.Invoke(errorResult);
                OnCaptureComplete?.Invoke(errorResult);
                yield break;
            }

            ReconfigureIfNeeded();

            // Render to texture
            RenderTexture previousRT = cam.targetTexture;
            cam.targetTexture = renderTexture;
            cam.Render();
            cam.targetTexture = previousRT;

            // Use AsyncGPUReadback for non-blocking read
            var request = AsyncGPUReadback.Request(renderTexture, 0, TextureFormat.RGB24);

            while (!request.done)
            {
                yield return null;
            }

            CaptureResult result;

            if (request.hasError)
            {
                result = new CaptureResult
                {
                    success = false,
                    errorMessage = "AsyncGPUReadback failed"
                };
            }
            else
            {
                // Copy data to texture
                var data = request.GetData<byte>();
                readbackTexture.LoadRawTextureData(data);
                readbackTexture.Apply();

                // Encode to JPEG
                byte[] imageBytes = readbackTexture.EncodeToJPG(config.jpegQuality);

                result = new CaptureResult
                {
                    success = true,
                    imageBytes = imageBytes,
                    imageBase64 = Convert.ToBase64String(imageBytes),
                    width = config.captureWidth,
                    height = config.captureHeight,
                    captureMode = mode,
                    captureTime = Time.time
                };
            }

            isCapturing = false;
            callback?.Invoke(result);
            OnCaptureComplete?.Invoke(result);
        }

        private Camera GetCameraForMode(CaptureMode mode)
        {
            switch (mode)
            {
                case CaptureMode.AgentPOV:
                    return targetCamera ?? Camera.main;
                case CaptureMode.ThirdPerson:
                    return thirdPersonCamera ?? targetCamera ?? Camera.main;
                case CaptureMode.Overhead:
                    return overheadCamera ?? targetCamera ?? Camera.main;
                case CaptureMode.Multiple:
                    // For multiple mode, we'd capture from all cameras
                    // For now, return main camera
                    return targetCamera ?? Camera.main;
                default:
                    return targetCamera ?? Camera.main;
            }
        }

        private void InitializeResources()
        {
            if (config == null)
                return;

            lastConfigWidth = config.captureWidth;
            lastConfigHeight = config.captureHeight;

            renderTexture = new RenderTexture(
                config.captureWidth,
                config.captureHeight,
                24,
                RenderTextureFormat.ARGB32
            );
            renderTexture.Create();

            readbackTexture = new Texture2D(
                config.captureWidth,
                config.captureHeight,
                TextureFormat.RGB24,
                false
            );
        }

        private void CleanupResources()
        {
            if (renderTexture != null)
            {
                renderTexture.Release();
                Destroy(renderTexture);
                renderTexture = null;
            }

            if (readbackTexture != null)
            {
                Destroy(readbackTexture);
                readbackTexture = null;
            }
        }

        private void Awake()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }
        }

        private void Start()
        {
            if (config != null)
            {
                InitializeResources();
            }
        }

        private void OnDestroy()
        {
            CleanupResources();
        }
    }
}
