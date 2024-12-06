using System;
using UnityEngine;
using TMPro;

public class ProfileMenu : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI username;

    private Profile profile;

    public void UpdateUI()
    {
        profile = ProfileSystem.FetchProfile();

        username.text = profile.username.ToString();
    }

    public void SubmitChanges()
    {
        profile = ProfileSystem.FetchProfile();
    
        profile.username = username.text;

        ProfileSystem.SaveProfileChanges(profile);
    }
}
