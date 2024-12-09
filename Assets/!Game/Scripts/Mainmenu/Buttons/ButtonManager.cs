using NUnit.Framework;
using TMPro;
using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    public GameObject[] menus;
    public GameObject[] screens;

    private void Start()
    {
        closeAllMenus();
    }

    public void EnableMenu(int menuIndex)
    {
        closeAllMenus();
        menus[menuIndex].SetActive(true);
    }

    public void DisableMenu(int menuIndex)
    {
        menus[menuIndex].SetActive(false);
    }

    public void EnableScreen(int index)
    {
        screens[index].SetActive(true);
    }

    public void DisableScreen(int index)
    {
        screens[index].SetActive(false);
    }

    public void ExitApplication()
    {
        Application.Quit();
    }

    private void closeAllMenus()
    {
        foreach (var menu in menus)
        {
            menu.SetActive(false);
        }
    }

    public void StartHostingLAN()
    {
        NetworkCommands.host();
    }

    public void ConnectViaLan(TextMeshProUGUI joinCode)
    {
        try
        {
            NetworkCommands.Connect(joinCode.text);
        }
        catch
        {

        }
    }
}
