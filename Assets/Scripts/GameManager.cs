using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections; 
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game State")]
    public int playerLives = 3;
    public string gameOverSceneName = "Scene_Game_Over";

    [Header("Consequence Timing")]
    public float invulnerabilityDuration = 3.0f;
    private bool isInvulnerable = false;

    [Header("UI Feedback")]
    public TextMeshProUGUI lifeTextDisplay;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void DeductLife()
    {
        if (playerLives <= 0 || isInvulnerable) return;

        StartCoroutine(InvulnerabilityTimer());

        playerLives--;
        Debug.Log("WARNING: Drone hit! Lives remaining: " + playerLives);

        UpdateUI();

        if (playerLives <= 0)
        {
            EndGame(false);
        }
    }

    public void EndGame(bool victory)
    {
        if (!victory)
        {
            SceneManager.LoadScene(gameOverSceneName);
        }
        else
        {
            Debug.Log("VICTORY!");
        }
    }

    private IEnumerator InvulnerabilityTimer()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(invulnerabilityDuration);
        isInvulnerable = false;
    }

    public void UpdateUI()
    {
        if (lifeTextDisplay == null)
        {
            GameObject hudText = FindInSceneByName("LifeDisplay");
            if (hudText != null)
            {
                lifeTextDisplay = hudText.GetComponent<TextMeshProUGUI>();
            }
        }

        if (lifeTextDisplay != null)
        {
            lifeTextDisplay.text = "LIVES: " + playerLives;
        }
    }

    public void ResetGame()
    {
        playerLives = 3;
        UpdateUI();
        Debug.Log("Game state reset. Lives restored to 3.");
    }

    private void LinkInfoPanel()
    {
        LevelInfoDisplay infoScript = GetComponent<LevelInfoDisplay>();
        if (infoScript == null)
        {
            Debug.LogWarning("LinkInfoPanel: LevelInfoDisplay script not found on GameManager. Skipping panel link.");
            return;
        }

        GameObject panelGO = FindInSceneByName("Level_Info_Panel");
        if (panelGO == null)
        {
            panelGO = FindInSceneByName("InfoPanel") ?? FindInSceneByName("LevelInfoPanel");
        }

        if (panelGO != null)
        {
            infoScript.infoPanel = panelGO;
            infoScript.infoPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning($"LinkInfoPanel: Info panel not found in scene {SceneManager.GetActiveScene().name}. Ensure the panel exists and has the expected name.");
            infoScript.infoPanel = null;
        }

        TextMeshProUGUI tipText = null;
        if (infoScript.infoPanel != null)
        {
            tipText = infoScript.infoPanel.GetComponentInChildren<TextMeshProUGUI>(true);
        }
        else
        {
            GameObject tipGO = FindInSceneByName("Tip_Text") ?? FindInSceneByName("Tip_Text (Text Mesh Pro UGUI)");
            if (tipGO != null) tipText = tipGO.GetComponent<TextMeshProUGUI>();
        }

        if (tipText != null)
        {
            infoScript.tipText = tipText;
            if (!string.IsNullOrEmpty(infoScript.levelTips))
                infoScript.tipText.text = infoScript.levelTips;
        }
        else
        {
            Debug.LogWarning("LinkInfoPanel: Tip text (TextMeshProUGUI) not found in scene. Tip panel may show empty text.");
            infoScript.tipText = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateUI();

        LinkInfoPanel();
    }

    private GameObject FindInSceneByName(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        Scene current = SceneManager.GetActiveScene();
        GameObject[] roots = current.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            Transform found = RecursiveFindByName(roots[i].transform, name);
            if (found != null) return found.gameObject;
        }
        return null;
    }

    private Transform RecursiveFindByName(Transform parent, string name)
    {
        if (parent == null) return null;
        if (parent.name == name) return parent;
        foreach (Transform child in parent)
        {
            Transform r = RecursiveFindByName(child, name);
            if (r != null) return r;
        }
        return null;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
