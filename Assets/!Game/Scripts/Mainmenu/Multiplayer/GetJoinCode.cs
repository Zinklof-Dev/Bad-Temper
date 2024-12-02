using System.Net;
using System.Linq;
using TMPro;
using UnityEngine;

public class GetJoinCode : MonoBehaviour
{
    [SerializeField] private int type;

    private TextMeshPro m_TextMeshPro;
    private TextMeshProUGUI m_TextMeshProUGUI;

    private void Start()
    {
        if (m_TextMeshPro == null)
        {
            m_TextMeshPro = gameObject.GetComponent<TextMeshPro>();
        }
        if (m_TextMeshProUGUI == null)
        {
            m_TextMeshProUGUI = gameObject.GetComponent<TextMeshProUGUI>();
        }
    }

    private void OnEnable()
    {
        if (type == 0 && m_TextMeshPro != null)
        {
            m_TextMeshPro.text = IPV4toHex.IPV4ToHexadecimal(Dns.GetHostEntry(Dns.GetHostName()).AddressList.First(f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToString());
        }
        else if (type == 0 && m_TextMeshProUGUI != null)
        {
            m_TextMeshProUGUI.text = IPV4toHex.IPV4ToHexadecimal(Dns.GetHostEntry(Dns.GetHostName()).AddressList.First(f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToString());

        }
    }
}
