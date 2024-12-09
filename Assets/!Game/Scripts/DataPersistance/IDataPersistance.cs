using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDataPersistance
{

    void LoadData(ProfileData data);

    void SaveData(ProfileData data);
}
