using UnityEngine;
using TMPro;
using UnityEngine.Profiling;

public class DebugSystem : MonoBehaviour
{
    [Header("Primary Operation Values")]
    [SerializeField] private GameObject DebugMenu;

    [Header("Debug Values")]
    [SerializeField] private bool menuOpen = false;
    
    void OpenMenu() // misleading name it actually opens and closes the menu
    {
        if(menuOpen) // if menu already open then close
        {
            menuOpen = false;
            DebugMenu.SetActive(false);
        }
        else if(!menuOpen) // if menu closed then open
        {
            menuOpen = true;
            DebugMenu.SetActive(true);
        }
    }

    void Update()
    {
        if(Input.GetKeyUp(KeyCode.BackQuote) || Input.GetKeyUp(KeyCode.Tilde))
        {
            OpenMenu();
        }
    }
}
