using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerUsernameManager : NetworkBehaviour
{
    [SerializeField] private TextMeshPro username;

    NetworkVariable<FixedString32Bytes> networkUsername = new NetworkVariable<FixedString32Bytes>("Unkown");

    public override void OnNetworkSpawn()
    {
        networkUsername.OnValueChanged += OnNetworkUsernameValueChanged;
        username.text = networkUsername.Value.ToString();

        if (IsOwner)
        {
            ClientBackend.OnClientEndUsernameChanged += OncClientUsernameChange;

            networkUsername.Value = ClientBackend.playerUsername;
        }

        base.OnNetworkSpawn();
    }

    void OncClientUsernameChange()
    {
        networkUsername.Value = ClientBackend.playerUsername;
    }

    void OnNetworkUsernameValueChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
    {
        username.text = newValue.Value.ToString();
    }
}
