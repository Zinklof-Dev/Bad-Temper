using NUnit.Framework;
using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    public GameObject[] Menus;

    private void Start()
    {
        closeAllMenus();
    }

    public void EnableMenu(int menuIndex)
    {
        Menus[menuIndex].SetActive(true);
    }

    public void DisableMenu(int menuIndex)
    {
        Menus[menuIndex].SetActive(false);
    }

    public void ExitApplication()
    {
        Application.Quit();
    }

    private void closeAllMenus()
    {
        foreach (var menu in Menus)
        {
            menu.SetActive(false);
        }
    }
}
