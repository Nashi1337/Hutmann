using System.Collections;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// "oh no the flashlight model is too detailed" funny mode.
/// Attach to the Player (or any persistent GameObject in the scene).
/// </summary>
public class FlashlightCurseEffect : MonoBehaviour
{
    [Header("References (auto-found if empty)")]
    [SerializeField] private PlayerEquipment equipment;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Transform cameraPivot;   // the thing that pitches up/down
    [SerializeField] private LowResScaler lowResScaler;

    [Header("Glitch Canvas (created at runtime if null)")]
    [SerializeField] private Canvas glitchCanvas;

    // ---------- tuning knobs ----------
    [Header("Lag Spike Settings")]
    [SerializeField] private float lagSpikeMinInterval = 2f;
    [SerializeField] private float lagSpikeMaxInterval = 5f;
    [SerializeField] private float lagSpikeMinDuration = 0.05f;
    [SerializeField] private float lagSpikeMaxDuration = 0.35f;

    [Header("Camera Shake")]
    [SerializeField] private float shakeAmplitude = 0.4f;
    [SerializeField] private float shakeFrequency = 18f;

    [Header("Resolution Glitch")]
    [SerializeField] private int glitchResWidth  = 80;
    [SerializeField] private int glitchResDuration = 1;   // frames to stay low

    [Header("Visual Glitch Bars")]
    [SerializeField] private int maxGlitchBars = 6;

    // ---------- state ----------
    private bool cursed = false;
    private Coroutine lagRoutine;
    private Coroutine shakeRoutine;
    private Coroutine glitchBarRoutine;
    private Coroutine consoleSpamRoutine;

    private Vector3 shakePivotOrigin;

    // Glitch bar pool
    private RawImage[] glitchBars;

    private static readonly string[] FakeErrors = new[]
    {
        "[GPU] Vertex buffer overflow — 1,847,392 polys in flashlight model",
        "[Renderer] Draw call budget exceeded by 400%",
        "[Memory] VRAM usage: 16.2 GB / 4 GB",
        "[Physics] Collision mesh too complex, approximating as SPHERE",
        "[LOD] No LOD group found on flashlight. Rendering all 2M tris.",
        "[Shadow] Shadow map resolution capped at 32x32 due to budget",
        "[GI] Baking flashlight reflections... ETA: 4 hours",
        "[Audio] Flashlight hum frequency exceeding Nyquist limit",
        "[UnityEngine.Debug] NullReferenceException: why is the flashlight this detailed",
        "[Optimization] Have you considered a lower-poly flashlight?",
        "[GarbageCollector] Please. I'm begging you.",
    };

    // -------------------------------------------------------

    void Awake()
    {
        if (equipment == null)
            equipment = GetComponentInParent<PlayerEquipment>() ?? FindFirstObjectByType<PlayerEquipment>();

        if (playerController == null)
            playerController = GetComponentInParent<PlayerController>() ?? FindFirstObjectByType<PlayerController>();

        if (cameraPivot == null)
        {
            var go = GameObject.Find("CameraPivot") ?? GameObject.Find("Camera Pivot");
            if (go) cameraPivot = go.transform;
        }

        if (lowResScaler == null)
            lowResScaler = FindFirstObjectByType<LowResScaler>();
    }

    void OnEnable()
    {
        if (equipment != null)
            equipment.OnEquippedChanged += OnEquipped;
    }

    void OnDisable()
    {
        if (equipment != null)
            equipment.OnEquippedChanged -= OnEquipped;
        SetCursed(false);
    }

    // -------------------------------------------------------

    private void OnEquipped(ItemDefinition item, int _)
    {
        bool shouldBeCursed = item != null && item.itemType == ItemType.Flashlight;
        if (shouldBeCursed != cursed)
            SetCursed(shouldBeCursed);
    }

    private void SetCursed(bool on)
    {
        cursed = on;

        if (on)
        {
            Debug.LogWarning("[FlashlightCurse] Oh no. The model is too detailed. I'm so sorry.");
            EnsureGlitchCanvas();
            lagRoutine        = StartCoroutine(LagSpikeLoop());
            shakeRoutine      = StartCoroutine(CameraShakeLoop());
            glitchBarRoutine  = StartCoroutine(GlitchBarLoop());
            consoleSpamRoutine = StartCoroutine(ConsoleSpamLoop());
        }
        else
        {
            StopIfRunning(ref lagRoutine);
            StopIfRunning(ref shakeRoutine);
            StopIfRunning(ref glitchBarRoutine);
            StopIfRunning(ref consoleSpamRoutine);

            // Restore timescale just in case
            Time.timeScale = 1f;

            if (playerController != null)
                playerController.SetLookLocked(false);

            // Hide all glitch bars
            if (glitchBars != null)
                foreach (var b in glitchBars)
                    if (b) b.enabled = false;

            // Restore resolution
            if (lowResScaler != null)
                RestoreResolution();

            // Restore camera shake
            if (cameraPivot != null)
                cameraPivot.localPosition = Vector3.zero;

            Debug.Log("[FlashlightCurse] Phew. Crisis averted.");
        }
    }

    // -------------------------------------------------------  Coroutines

    private IEnumerator LagSpikeLoop()
    {
        while (true)
        {
            float wait = Random.Range(lagSpikeMinInterval, lagSpikeMaxInterval);
            yield return new WaitForSeconds(wait);

            // Freeze time to simulate a lag spike
            float spikeDuration = Random.Range(lagSpikeMinDuration, lagSpikeMaxDuration);

            if (playerController != null)
                playerController.SetLookLocked(true);

            Time.timeScale = 0f;

            // We can't use WaitForSeconds(spikeDuration) while timeScale=0, use unscaled time
            float end = Time.realtimeSinceStartup + spikeDuration;
            while (Time.realtimeSinceStartup < end)
                yield return null;

            Time.timeScale = 1f;

            if (playerController != null)
                playerController.SetLookLocked(false);

            // Optionally drop resolution for a couple frames right after the spike
            if (lowResScaler != null)
            {
                int savedW = lowResScaler.targetWidth;
                int savedH = lowResScaler.targetHeight;
                lowResScaler.targetWidth  = glitchResWidth;
                lowResScaler.targetHeight = Mathf.RoundToInt(glitchResWidth * 9f / 16f);
                yield return null;
                yield return null;
                lowResScaler.targetWidth  = savedW;
                lowResScaler.targetHeight = savedH;
            }
        }
    }

    private IEnumerator CameraShakeLoop()
    {
        if (cameraPivot == null) yield break;

        float t = 0f;
        while (true)
        {
            t += Time.unscaledDeltaTime * shakeFrequency;
            float x = (Mathf.PerlinNoise(t, 0f) - 0.5f) * 2f * shakeAmplitude;
            float y = (Mathf.PerlinNoise(0f, t) - 0.5f) * 2f * shakeAmplitude;
            cameraPivot.localPosition = new Vector3(x, y, 0f);
            yield return null;
        }
    }

    private IEnumerator GlitchBarLoop()
    {
        while (true)
        {
            // Random interval between glitches
            yield return new WaitForSecondsRealtime(Random.Range(0.3f, 1.5f));

            int count = Random.Range(1, maxGlitchBars + 1);
            for (int i = 0; i < count; i++)
            {
                if (i < glitchBars.Length)
                {
                    var bar = glitchBars[i];
                    bar.enabled = true;

                    // Random colored horizontal bar across the screen
                    float yPos = Random.Range(-0.5f, 0.5f); // normalized -0.5..0.5
                    float height = Random.Range(2f, 30f);
                    bar.rectTransform.anchorMin = new Vector2(0f, 0.5f + yPos);
                    bar.rectTransform.anchorMax = new Vector2(1f, 0.5f + yPos);
                    bar.rectTransform.offsetMin = new Vector2(0f, -height * 0.5f);
                    bar.rectTransform.offsetMax = new Vector2(0f,  height * 0.5f);

                    // Random glitchy colour: mostly blacks/whites with occasional bright
                    float roll = Random.value;
                    Color c;
                    if (roll < 0.4f)       c = new Color(0, 0, 0, Random.Range(0.5f, 0.9f));          // black
                    else if (roll < 0.7f)  c = new Color(1, 1, 1, Random.Range(0.3f, 0.7f));          // white
                    else if (roll < 0.85f) c = new Color(Random.value, 0f, 0f, 0.8f);                 // red channel
                    else                   c = new Color(0f, Random.value, Random.value, 0.6f);        // cyan-ish
                    bar.color = c;
                }
            }

            // Flash briefly then hide
            float flashDuration = Random.Range(0.02f, 0.12f);
            yield return new WaitForSecondsRealtime(flashDuration);

            foreach (var bar in glitchBars)
                bar.enabled = false;
        }
    }

    private IEnumerator ConsoleSpamLoop()
    {
        // Immediate first message
        yield return new WaitForSecondsRealtime(0.5f);
        int idx = 0;
        while (true)
        {
            // Rotate through fake errors
            string msg = FakeErrors[idx % FakeErrors.Length];
            // alternate between warning and error for variety
            if (idx % 3 == 0) Debug.LogError(msg);
            else               Debug.LogWarning(msg);
            idx++;
            yield return new WaitForSecondsRealtime(Random.Range(1.5f, 4f));
        }
    }

    // -------------------------------------------------------  Helpers

    private void EnsureGlitchCanvas()
    {
        if (glitchCanvas == null)
        {
            var go = new GameObject("FlashlightGlitchCanvas");
            DontDestroyOnLoad(go);
            glitchCanvas = go.AddComponent<Canvas>();
            glitchCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            glitchCanvas.sortingOrder = 999;
            go.AddComponent<CanvasScaler>();
            go.AddComponent<GraphicRaycaster>();
        }

        // Build bar pool
        if (glitchBars == null || glitchBars.Length != maxGlitchBars)
        {
            // Clean up old ones
            if (glitchBars != null)
                foreach (var b in glitchBars)
                    if (b) Destroy(b.gameObject);

            glitchBars = new RawImage[maxGlitchBars];
            for (int i = 0; i < maxGlitchBars; i++)
            {
                var barGo  = new GameObject($"GlitchBar_{i}");
                barGo.transform.SetParent(glitchCanvas.transform, false);
                var img = barGo.AddComponent<RawImage>();
                img.enabled = false;

                var rt = img.rectTransform;
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                glitchBars[i] = img;
            }
        }
    }

    private void RestoreResolution()
    {
        // We don't store original values here because LowResScaler owns them.
        // Just reset to its inspector-set defaults by calling Start equivalent.
        // The cleanest approach: just don't touch them — lag spike routine
        // already restores them after the spike.
    }

    private void StopIfRunning(ref Coroutine c)
    {
        if (c != null) { StopCoroutine(c); c = null; }
    }
}

