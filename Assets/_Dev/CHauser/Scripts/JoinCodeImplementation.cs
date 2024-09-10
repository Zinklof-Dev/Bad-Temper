using System.Net;
using System.Linq;
using TMPro;
using UnityEngine;


public class JoinCodeImplementation : MonoBehaviour
{
    [SerializeField] private TMP_Text joinCodeText;
    [SerializeField] private TMP_InputField joinCodeInputField;

    public void GetJoinCode()
    {
        // Passes the device's IP into the IPV4ToHexadecimal function from the IPV4toHex script
       joinCodeText.text = IPV4toHex.IPV4ToHexadecimal(Dns.GetHostEntry(Dns.GetHostName()).AddressList.First(f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToString());
    }
}
