using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string levelSceneName = "Level_01";

    [Header("Controls Panel")]
    [SerializeField] private GameObject controlsPanel;

    public void StartGame()
    {
        GameManager gm = FindFirstObjectByType<GameManager>();

        if (gm != null)
        {
            gm.ResetGame();
        }
        if (string.IsNullOrEmpty(levelSceneName))
        {
            Debug.LogError("MainMenu: levelSceneName is not set!");
            return;
        }

        SceneManager.LoadScene(levelSceneName);
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }

    public void GoToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void OpenControls()
    {
        if (controlsPanel != null)
            controlsPanel.SetActive(true);
    }

    public void CloseControls()
    {
        if (controlsPanel != null)
            controlsPanel.SetActive(false);
    }
}
