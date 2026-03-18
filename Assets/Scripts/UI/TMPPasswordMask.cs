using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_InputField))]
public class TMPPasswordMask : MonoBehaviour
{
    [SerializeField] private char maskCharacter = '●';

    private TMP_InputField inputField;

    private void Awake()
    {
        ApplyMaskSettings();
    }

    private void OnValidate()
    {
        ApplyMaskSettings();
    }

    [ContextMenu("Apply Password Mask")]
    public void ApplyMaskSettings()
    {
        if (inputField == null)
        {
            inputField = GetComponent<TMP_InputField>();
        }

        if (inputField == null)
        {
            return;
        }

        inputField.contentType = TMP_InputField.ContentType.Password;
        inputField.inputType = TMP_InputField.InputType.Password;
        inputField.asteriskChar = maskCharacter;
        inputField.ForceLabelUpdate();
    }
}