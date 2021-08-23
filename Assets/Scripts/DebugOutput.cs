using TMPro;
using UnityEngine;

public class DebugOutput : MonoBehaviour
{
    public static DebugOutput Instance;

    [SerializeField]
    private TextMeshProUGUI textField;

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

    }

    public void PutMessage(string message)
    {
        textField.text += message + "\n";
    }
}
