using UnityEngine;

public class PanelManager : MonoBehaviour
{
    public GameObject panel;

    public void TogglePanel()
    {
        panel.SetActive(!panel.activeSelf);
    }
}
