using UnityEngine;
using TMPro;

public class LevelInfoDisplay : MonoBehaviour
{
    [Header("References (can be auto-linked by GameManager)")]
    public TextMeshProUGUI tipText;
    public GameObject infoPanel;

    [TextArea(3, 10)]
    public string levelTips;

    private bool isVisible = false;

    void Start()
    {
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }

        if (tipText != null && !string.IsNullOrEmpty(levelTips))
        {
            tipText.text = levelTips;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInfo();
        }
    }

    public void ToggleInfo()
    {
        if (infoPanel == null)
        {
            Debug.LogWarning("ToggleInfo: No infoPanel assigned for this scene. Ensure GameManager was able to link the Level_Info_Panel.");
            return;
        }


        infoPanel.SetActive(isVisible);

        Time.timeScale = isVisible ? 0f : 1f;
    }
}
