using Unity.Collections;
using UnityEngine;

public class LocalProfileDataHolder : MonoBehaviour
{
    public FixedString32Bytes username;
    public void LoadData(ProfileData data)
    {
        this.username = data.username;
    }
}
