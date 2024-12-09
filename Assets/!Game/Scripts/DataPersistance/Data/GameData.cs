using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

[System.Serializable]
public class ProfileData
{
    public string profileVers;
    public FixedString32Bytes username;

    //defines what to start with when no save data is found or new save is made
    public ProfileData()
    {
        username = new FixedString32Bytes("New Player");
        profileVers = "0.1";
    }
}
