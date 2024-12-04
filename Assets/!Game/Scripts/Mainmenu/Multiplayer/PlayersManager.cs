using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayersManager : NetworkBehaviour
{
    NetworkVariable<FixedString32Bytes> username = new NetworkVariable<FixedString32Bytes>();
    [SerializeField] TextMeshProUGUI usernameText;

    public override void OnNetworkSpawn()
    {
        username.OnValueChanged += OnPlayerNameChanged;

        if (IsOwner)
        {
            username.Value = ClientBackend.playerUsername;
        }

        if (IsServer)
        {
            GameObject temp = GameObject.FindGameObjectWithTag("PlayerScreen");
            transform.SetParent(temp.transform);
        }

        base.OnNetworkSpawn();
    }

    private void OnPlayerNameChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
    {
        username.Value = newValue;
        usernameText.text = newValue.Value.ToString();
    }
}
