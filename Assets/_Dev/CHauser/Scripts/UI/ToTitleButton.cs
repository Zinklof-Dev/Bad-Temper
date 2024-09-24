using UnityEngine;

public class ToTitleButton : MonoBehaviour
{
    [SerializeField] private GameObject toTitleButton;
    [SerializeField] private GameObject titleScreen;
    [SerializeField] private GameObject[] screens;

    void Update()
    {
        if(titleScreen.activeInHierarchy)
        { 
            toTitleButton.SetActive(false);
        }

        else
        {
            toTitleButton.SetActive(true);
        }
    }

    public void OnButtonClick()
    {
        foreach(var screen in screens) 
        {
            screen.SetActive(false);
        }

        titleScreen.SetActive(true);
    }
}
