using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryFlow : MonoBehaviour
{
    public void GoToMainMenu()
    {
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 1f; 
   
        SceneManager.LoadScene("MainMenu");
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }
}