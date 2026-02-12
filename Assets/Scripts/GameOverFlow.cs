using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverFlow : MonoBehaviour
{
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene("MainMenu");

        Time.timeScale = 1f; 
    }
}