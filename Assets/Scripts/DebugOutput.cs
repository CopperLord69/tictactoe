using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
