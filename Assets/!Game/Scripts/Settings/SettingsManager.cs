using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [SerializeField] DataPersistanceManager dataPersistanceManager;
    [SerializeField] TextMeshProUGUI charLimitText;
    [SerializeField] GameObject canSaveText; // misleading name, actually shows if the player cannot save
    [SerializeField] TMP_InputField usernameInputField;
    [SerializeField] Button saveButton;

    ProfileData profile;

    short charCount;

    public void RefreshUsername()
    {
        profile = dataPersistanceManager.GetData();
        usernameInputField.text = profile.username.ToString();
        EvalUsernameLength();
    }

    public void OnInputChange()
    {
        EvalUsernameLength();
    }

    public void SaveButton()
    {
        profile.username = usernameInputField.text;
        dataPersistanceManager.ForceSaveThisData(profile);
    }

    private void EvalUsernameLength()
    {
        string temp = usernameInputField.text;

        charCount = (short)temp.Length;

        if (charCount > 24)
        {
            charLimitText.text = "<color=#ff0000>" + charCount + "/" + 24 + "</color>";
            saveButton.interactable = false;
            canSaveText.SetActive(true);
        }
        else
        {
            charLimitText.text = charCount + "/" + 24;
            saveButton.interactable = true;
            canSaveText.SetActive(false);
        }   
    }
}
