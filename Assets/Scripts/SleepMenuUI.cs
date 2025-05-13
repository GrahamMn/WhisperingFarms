using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SleepMenuUI : MonoBehaviour
{
    public GameObject menuPanel;

    public void Show()
    {
        menuPanel.SetActive(true);
        Time.timeScale = 0f; // Pause game
    }

    public void Hide()
    {
        menuPanel.SetActive(false);
        Time.timeScale = 1f; // Resume game
    }

    public void OnSleepAndSave()
    {
        PlayerPrefs.SetInt("Day", PlayerPrefs.GetInt("Day", 1) + 1);
        PlayerPrefs.SetString("SpawnPoint", "HouseExit");
        PlayerPrefs.Save();

        Time.timeScale = 1f;
        SceneManager.LoadScene("FarmScene");
    }

    public void OnSaveAndQuit()
    {
        PlayerPrefs.SetString("SpawnPoint", "HouseExit");
        PlayerPrefs.Save();

        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void OnCancel()
    {
        Hide();
    }
}
