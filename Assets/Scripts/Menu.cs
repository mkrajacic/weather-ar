using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{

public GameObject mainMenu;
public GameObject infoPanel;

    public void StartApp(){
        SceneManager.LoadScene("Weather");
    }

    public void AppInfo() {
        mainMenu.SetActive(!mainMenu.activeSelf);
        infoPanel.SetActive(!infoPanel.activeSelf);
    }

     public void BackButton() {
        infoPanel.SetActive(!infoPanel.activeSelf);
        mainMenu.SetActive(!mainMenu.activeSelf);
    }

    public void QuitApp(){
        Application.Quit();
    }
}
