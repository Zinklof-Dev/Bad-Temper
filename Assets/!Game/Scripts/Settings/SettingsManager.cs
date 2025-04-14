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
        dataPersistanceManager.ForceSaveThisData(profile);
    }

    private void EvalUsernameLength()
    {
        string temp = usernameInputField.text;

        charCount = (short)temp.Length;

        if (charCount > 12)
        {
            charLimitText.text = "<color=#ff0000>" + charCount + "/" + 12 + "</color>";
            saveButton.interactable = false;
            canSaveText.SetActive(true);
        }
        else
        {
            charLimitText.text = charCount + "/" + 12;
            saveButton.interactable = true;
            canSaveText.SetActive(false);
        }   
    }
}
