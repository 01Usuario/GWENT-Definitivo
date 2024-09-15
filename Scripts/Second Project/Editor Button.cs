using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditorButton : MonoBehaviour
{
    public GameObject editorWindow;

    void Start()
    {
        Button btn = GetComponent<Button>();
        btn.onClick.AddListener(OpenEditor);
    }

    void OpenEditor()
    {
        editorWindow.SetActive(true);
    }
}
