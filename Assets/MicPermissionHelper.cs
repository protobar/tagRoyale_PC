using UnityEngine;
using UnityEngine.Android;

public class MicPermissionHelper : MonoBehaviour
{
    private const string PermissionMicrophone = Permission.Microphone;

    private void Start()
    {
        RequestMicrophonePermission();
    }

    private void RequestMicrophonePermission()
    {
        if (!Permission.HasUserAuthorizedPermission(PermissionMicrophone))
        {
            Permission.RequestUserPermission(PermissionMicrophone);
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && !Permission.HasUserAuthorizedPermission(PermissionMicrophone))
        {
            Permission.RequestUserPermission(PermissionMicrophone);
        }
    }

    private void OnGUI()
    {
        if (!Permission.HasUserAuthorizedPermission(PermissionMicrophone))
        {
            GUI.Label(new Rect(10, 10, 300, 30), "Microphone permission is required.");
            if (GUI.Button(new Rect(10, 40, 200, 30), "Request Microphone Permission"))
            {
                Permission.RequestUserPermission(PermissionMicrophone);
            }
        }
    }
}
