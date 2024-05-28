using UnityEngine;

public class CursorManager : MonoBehaviour
{
    private bool cursorLocked = true;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleCursorState();
        }
    }

    public void LockCursor()
    {
        cursorLocked = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockCursor()
    {
        cursorLocked = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ToggleCursorState()
    {
        if (cursorLocked)
        {
            UnlockCursor();
        }
        else
        {
            LockCursor();
        }
    }
}
