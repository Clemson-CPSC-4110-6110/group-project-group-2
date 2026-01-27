using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

/// <summary>
/// Initializes XR-specific optimizations for Meta Quest 3.
/// Attach this to a GameObject in your startup scene.
///
/// KEY CONCEPTS FOR VR OPTIMIZATION:
///
/// 1. REFRESH RATE: How many times per second the display updates. Higher rates (90Hz, 120Hz)
///    feel smoother but require more GPU power. Quest 3 supports 72Hz, 90Hz, and 120Hz.
///
/// 2. RENDER SCALE: The resolution multiplier for rendering. A value of 1.0 renders at native
///    resolution. Lower values (0.8) reduce GPU load but decrease visual clarity.
///
/// 3. FOVEATED RENDERING: A technique that renders the center of your vision at full quality
///    while reducing quality in your peripheral vision (where you can't see detail anyway).
///    This significantly improves performance with minimal visual impact.
///
/// 4. GAZE-TRACKED FOVEATION: Uses eye tracking to move the high-quality region to wherever
///    you're actually looking, rather than just the center of the display.
/// </summary>
public class XROptimizationManager : MonoBehaviour
{
    [Header("Display Settings")]
    [Tooltip("REFRESH RATE: How many frames per second the Quest display shows.\n\n" +
             "- 72Hz: Best for thermal management and battery life. Good for most apps.\n" +
             "- 90Hz: Smoother experience, recommended for games with fast movement.\n" +
             "- 120Hz: Smoothest possible, but requires very optimized scenes.\n\n" +
             "Higher refresh rates require rendering more frames, which increases GPU load and heat.")]
    [SerializeField] private QuestRefreshRate targetRefreshRate = QuestRefreshRate.Hz72;

    /// <summary>
    /// Quest 3 supported refresh rates.
    /// Higher values = smoother visuals but more GPU/battery usage.
    /// </summary>
    public enum QuestRefreshRate
    {
        /// <summary>72 frames per second - Best battery life, lowest heat</summary>
        Hz72 = 72,
        /// <summary>90 frames per second - Balanced smoothness and performance</summary>
        Hz90 = 90,
        /// <summary>120 frames per second - Smoothest, highest GPU demand</summary>
        Hz120 = 120
    }

    [Header("Rendering")]
    [Tooltip("RENDER SCALE: Multiplier for the rendering resolution.\n\n" +
             "- 1.0 = Native resolution (clearest image)\n" +
             "- 0.8 = 80% resolution (good performance boost, slight blur)\n" +
             "- 0.5 = 50% resolution (significant blur, use only if needed)\n\n" +
             "Lower values reduce the number of pixels the GPU must render, " +
             "improving frame rate at the cost of visual sharpness. " +
             "VR is more forgiving of lower resolutions than flat screens.")]
    [Range(0.5f, 1.5f)]
    [SerializeField] private float renderScale = 1.0f;

    [Header("Foveated Rendering")]
    [Tooltip("FOVEATED RENDERING: Renders the center of your view at full quality " +
             "while reducing detail in your peripheral vision.\n\n" +
             "WHY IT WORKS: Your eyes can only see fine detail in a small central area " +
             "(the fovea). Peripheral vision detects motion but not detail. " +
             "By rendering peripheral areas at lower quality, we save significant GPU time " +
             "with almost no visible difference.\n\n" +
             "RECOMMENDATION: Always enable this for Quest development.")]
    [SerializeField] private bool enableFoveatedRendering = true;

    [Tooltip("FOVEATION STRENGTH: How aggressively to reduce peripheral quality.\n\n" +
             "- 0.0 = Disabled (full quality everywhere)\n" +
             "- 0.5 = Medium (subtle reduction, hard to notice)\n" +
             "- 0.75 = High (good balance of quality and performance)\n" +
             "- 1.0 = Maximum (best performance, may notice blur in periphery)\n\n" +
             "Start at 1.0 and only reduce if you notice visual artifacts.")]
    [Range(0f, 1f)]
    [SerializeField] private float foveationLevel = 1.0f;

    [Tooltip("GAZE-TRACKED FOVEATION: Uses Quest 3's eye tracking to move the " +
             "high-quality rendering region to follow where you're actually looking.\n\n" +
             "WITHOUT eye tracking: High quality is always at screen center.\n" +
             "WITH eye tracking: High quality follows your eye gaze.\n\n" +
             "REQUIREMENT: Your app must request eye tracking permission in the Android manifest. " +
             "If permission isn't granted, falls back to fixed foveation.")]
    [SerializeField] private bool enableGazeTracking = true;

    [Header("Debug")]
    [Tooltip("Log optimization settings to the console on startup. " +
             "Useful for verifying settings are applied correctly.")]
    [SerializeField] private bool logOptimizationInfo = true;

    private void Awake()
    {
        ApplyOptimizations();
    }

    private void ApplyOptimizations()
    {
        // Set application target frame rate to match display refresh rate
        // The XR runtime uses this as a hint for display timing
        Application.targetFrameRate = (int)targetRefreshRate;

        // Disable VSync - the XR runtime handles frame timing to minimize latency
        // In VR, the compositor manages synchronization with the display
        QualitySettings.vSyncCount = 0;

        // Set XR render scale (eye texture resolution multiplier)
        // This is one of the most impactful performance settings
        XRSettings.eyeTextureResolutionScale = renderScale;

        // Configure foveated rendering via the SRP Foveation API
        if (enableFoveatedRendering)
        {
            SetFoveatedRendering(foveationLevel, enableGazeTracking);
        }
        else
        {
            SetFoveatedRendering(0f, false);
        }

        if (logOptimizationInfo)
        {
            Debug.Log($"[XROptimization] === XR Optimization Settings Applied ===");
            Debug.Log($"[XROptimization] Refresh Rate: {(int)targetRefreshRate}Hz");
            Debug.Log($"[XROptimization] Render Scale: {renderScale:F2} ({renderScale * 100:F0}% resolution)");
            Debug.Log($"[XROptimization] Foveated Rendering: {(enableFoveatedRendering ? "Enabled" : "Disabled")}");
            if (enableFoveatedRendering)
            {
                Debug.Log($"[XROptimization] Foveation Level: {foveationLevel:F2} ({foveationLevel * 100:F0}% strength)");
                Debug.Log($"[XROptimization] Gaze Tracking: {(enableGazeTracking ? "Enabled" : "Disabled")}");
            }
            Debug.Log($"[XROptimization] XR Device: {XRSettings.loadedDeviceName}");
        }
    }

    private void SetFoveatedRendering(float level, bool gazeTracked)
    {
        // Get the active XR display subsystem
        List<XRDisplaySubsystem> xrDisplays = new List<XRDisplaySubsystem>();
        SubsystemManager.GetSubsystems(xrDisplays);

        if (xrDisplays.Count > 0)
        {
            var display = xrDisplays[0];

            // Set foveation level (0 = no foveation, 1 = maximum foveation)
            // Higher values = more aggressive quality reduction in periphery
            display.foveatedRenderingLevel = level;

            // Set whether eye tracking can be used for gaze-directed foveation
            // GazeAllowed: High-quality region follows eye gaze (requires eye tracking)
            // None: High-quality region stays at screen center (fixed foveation)
            display.foveatedRenderingFlags = gazeTracked
                ? XRDisplaySubsystem.FoveatedRenderingFlags.GazeAllowed
                : XRDisplaySubsystem.FoveatedRenderingFlags.None;
        }
        else
        {
            Debug.LogWarning("[XROptimization] No XRDisplaySubsystem found. " +
                "Foveated rendering requires an active XR session.");
        }
    }

    #region Performance Presets

    /// <summary>
    /// HIGH PERFORMANCE MODE: Best for complex scenes or when the device is getting warm.
    /// - 72Hz refresh rate (lowest GPU demand)
    /// - 80% render scale (reduced resolution)
    /// - Maximum foveation (aggressive peripheral quality reduction)
    /// </summary>
    public void SetHighPerformanceMode()
    {
        targetRefreshRate = QuestRefreshRate.Hz72;
        renderScale = 0.8f;
        foveationLevel = 1.0f;
        enableGazeTracking = true;
        ApplyOptimizations();
        Debug.Log("[XROptimization] Switched to HIGH PERFORMANCE mode (72Hz, 0.8x scale, max foveation)");
    }

    /// <summary>
    /// BALANCED MODE: Good default for most applications.
    /// - 72Hz refresh rate (good battery life)
    /// - 100% render scale (full resolution)
    /// - High foveation (good performance with minimal visual impact)
    /// </summary>
    public void SetBalancedMode()
    {
        targetRefreshRate = QuestRefreshRate.Hz72;
        renderScale = 1.0f;
        foveationLevel = 0.75f;
        enableGazeTracking = true;
        ApplyOptimizations();
        Debug.Log("[XROptimization] Switched to BALANCED mode (72Hz, 1.0x scale, high foveation)");
    }

    /// <summary>
    /// QUALITY MODE: Best visual quality for simple scenes.
    /// - 90Hz refresh rate (smoother motion)
    /// - 100% render scale (full resolution)
    /// - Medium foveation (better peripheral quality)
    /// Only use this if your scene runs well and the device stays cool.
    /// </summary>
    public void SetQualityMode()
    {
        targetRefreshRate = QuestRefreshRate.Hz90;
        renderScale = 1.0f;
        foveationLevel = 0.5f;
        enableGazeTracking = false;
        ApplyOptimizations();
        Debug.Log("[XROptimization] Switched to QUALITY mode (90Hz, 1.0x scale, medium foveation)");
    }

    #endregion

    #region Runtime API

    /// <summary>
    /// Get the name of the connected XR device (e.g., "Oculus Quest").
    /// </summary>
    public string GetXRDeviceName()
    {
        return XRSettings.loadedDeviceName;
    }

    /// <summary>
    /// Check if an XR device is currently active and rendering.
    /// </summary>
    public bool IsXRActive()
    {
        return XRSettings.isDeviceActive;
    }

    /// <summary>
    /// Change foveation strength at runtime.
    /// </summary>
    /// <param name="level">0 = disabled, 1 = maximum foveation</param>
    public void SetFoveationLevel(float level)
    {
        foveationLevel = Mathf.Clamp01(level);
        SetFoveatedRendering(foveationLevel, enableGazeTracking);
        Debug.Log($"[XROptimization] Foveation level changed to {foveationLevel:F2}");
    }

    /// <summary>
    /// Change render scale at runtime. Affects visual clarity vs performance.
    /// </summary>
    /// <param name="scale">0.5 to 1.5 (1.0 = native resolution)</param>
    public void SetRenderScale(float scale)
    {
        renderScale = Mathf.Clamp(scale, 0.5f, 1.5f);
        XRSettings.eyeTextureResolutionScale = renderScale;
        Debug.Log($"[XROptimization] Render scale changed to {renderScale:F2}");
    }

    /// <summary>
    /// Change target refresh rate at runtime.
    /// </summary>
    /// <param name="rate">Quest 3 supports 72Hz, 90Hz, or 120Hz</param>
    public void SetTargetRefreshRate(QuestRefreshRate rate)
    {
        targetRefreshRate = rate;
        Application.targetFrameRate = (int)rate;
        Debug.Log($"[XROptimization] Refresh rate changed to {(int)rate}Hz");
    }

    /// <summary>
    /// Enable or disable gaze-tracked foveation at runtime.
    /// </summary>
    /// <param name="enabled">True to use eye tracking for foveation center</param>
    public void SetGazeTracking(bool enabled)
    {
        enableGazeTracking = enabled;
        SetFoveatedRendering(foveationLevel, enableGazeTracking);
        Debug.Log($"[XROptimization] Gaze tracking {(enabled ? "enabled" : "disabled")}");
    }

    #endregion
}
