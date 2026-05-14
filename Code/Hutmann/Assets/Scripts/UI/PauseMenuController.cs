using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PauseMenuController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private UIDocument pauseMenuDocument;
    [SerializeField] private GameObject pauseMenuPanel;

    [Header("UI Toolkit")]
    [SerializeField] private string tabViewName;
    [SerializeField] private string resumeButtonName = "ResumeButton";
    [SerializeField] private string quitButtonName = "QuitButton";

    [Header("Tabs")]
    [SerializeField] private int defaultTabIndex;

    [Header("Input")]
    [SerializeField] private string pauseActionName = "Pause";
    [SerializeField] private bool allowEscapeFallback = true;

    private InputAction pauseAction;
    private VisualElement root;
    private VisualElement menuRoot;
    private VisualElement tabView;
    private Button resumeButton;
    private Button quitButton;
    private bool isPaused;
    private Coroutine deferredOpenRoutine;

    private void Reset()
    {
        if (pauseMenuPanel != null)
            pauseMenuDocument = pauseMenuPanel.GetComponent<UIDocument>();

        if (pauseMenuDocument == null)
            pauseMenuDocument = GetComponent<UIDocument>();
    }

    private void OnValidate()
    {
        ResolvePauseMenuDocument();
    }

    private void Awake()
    {
        ResolvePauseMenuDocument();

        if (playerController == null)
            playerController = FindAnyObjectByType<PlayerController>();

        if (playerInput == null && playerController != null)
            playerInput = playerController.GetComponent<PlayerInput>();

        InitializeUiReferences();
        SetMenuVisible(false);
    }

    private void OnEnable()
    {
        if (playerInput != null && playerInput.actions != null)
        {
            pauseAction = playerInput.actions.FindAction(pauseActionName);
            if (pauseAction != null)
            {
                pauseAction.performed += OnPausePressed;
                pauseAction.Enable();
            }
        }
    }

    private void OnDisable()
    {
        if (deferredOpenRoutine != null)
        {
            StopCoroutine(deferredOpenRoutine);
            deferredOpenRoutine = null;
        }

        if (resumeButton != null)
            resumeButton.clicked -= ResumeGame;
        if (quitButton != null)
            quitButton.clicked -= QuitGame;

        if (pauseAction != null)
        {
            pauseAction.performed -= OnPausePressed;
            pauseAction = null;
        }

        if (isPaused)
            SetPaused(false);
    }

    private void Update()
    {
        if (pauseAction == null && allowEscapeFallback && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            TogglePause();
    }

    public void TogglePause()
    {
        SetPaused(!isPaused);
    }

    public void SetPaused(bool paused)
    {
        isPaused = paused;

        if (paused)
        {
            OpenMenu();
        }
        else
        {
            if (deferredOpenRoutine != null)
            {
                StopCoroutine(deferredOpenRoutine);
                deferredOpenRoutine = null;
            }

            SetMenuVisible(false);
        }

        Time.timeScale = paused ? 0f : 1f;

        UnityEngine.Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
        UnityEngine.Cursor.visible = paused;

        if (playerController != null)
            playerController.SetLookLocked(paused);

    }

    public void ResumeGame()
    {
        SetPaused(false);
    }

    public void ShowTab(int tabIndex)
    {
        if (tabView == null)
            return;

        int maxIndex = Mathf.Max(tabView.childCount - 1, 0);
        int safeIndex = Mathf.Clamp(tabIndex, 0, maxIndex);

        // Keep this reflection-based so it works across Unity versions where TabView API names differ.
        var tabViewType = tabView.GetType();
        var selectedTabIndexProperty = tabViewType.GetProperty("selectedTabIndex") ?? tabViewType.GetProperty("SelectedTabIndex");
        if (selectedTabIndexProperty != null && selectedTabIndexProperty.CanWrite)
            selectedTabIndexProperty.SetValue(tabView, safeIndex);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void InitializeUiReferences()
    {
        if (pauseMenuDocument == null)
        {
            Debug.LogWarning("PauseMenuController: No UIDocument found. Assign a UIDocument directly or on pauseMenuPanel.");
            return;
        }

        root = pauseMenuDocument.rootVisualElement;
        if (root == null)
            return;

        if (resumeButton != null)
            resumeButton.clicked -= ResumeGame;
        if (quitButton != null)
            quitButton.clicked -= QuitGame;

        menuRoot = root;
        tabView = string.IsNullOrWhiteSpace(tabViewName) ? GetFirstChildAsFallback(menuRoot ?? root) : root.Q<VisualElement>(tabViewName);

        resumeButton = root.Q<Button>(resumeButtonName);
        quitButton = root.Q<Button>(quitButtonName);

        if (resumeButton != null)
            resumeButton.clicked += ResumeGame;
        if (quitButton != null)
            quitButton.clicked += QuitGame;
    }

    private VisualElement GetFirstChildAsFallback(VisualElement element)
    {
        if (element == null || element.childCount == 0)
            return null;

        return element[0];
    }

    private void SetMenuVisible(bool visible)
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(visible);

        VisualElement target = menuRoot ?? root;
        if (target != null)
            target.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void OpenMenu()
    {
        SetMenuVisible(true);

        // UIDocument can need a frame after activation before rootVisualElement is ready.
        if (TryEnsureUiIsReady())
        {
            ShowTab(defaultTabIndex);
            return;
        }

        if (deferredOpenRoutine != null)
            StopCoroutine(deferredOpenRoutine);

        deferredOpenRoutine = StartCoroutine(DeferredOpenMenu());
    }

    private bool TryEnsureUiIsReady()
    {
        ResolvePauseMenuDocument();

        if (pauseMenuDocument == null)
            return false;

        if (root == null || tabView == null)
            InitializeUiReferences();

        if (root == null)
            return false;

        SetMenuVisible(true);
        return true;
    }

    private IEnumerator DeferredOpenMenu()
    {
        const int maxFrames = 6;
        for (int i = 0; i < maxFrames && isPaused; i++)
        {
            yield return null;

            if (!TryEnsureUiIsReady())
                continue;

            ShowTab(defaultTabIndex);
            deferredOpenRoutine = null;
            yield break;
        }

        deferredOpenRoutine = null;
    }

    private void ResolvePauseMenuDocument()
    {
        if (pauseMenuDocument != null)
            return;

        if (pauseMenuPanel != null)
            pauseMenuDocument = pauseMenuPanel.GetComponent<UIDocument>();

        if (pauseMenuDocument == null)
            pauseMenuDocument = GetComponent<UIDocument>();
    }

    private void OnPausePressed(InputAction.CallbackContext _)
    {
        TogglePause();
    }
}





