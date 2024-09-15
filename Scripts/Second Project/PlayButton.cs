using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class PlayButton : MonoBehaviour
{
    public void LoadScene(int num)
    {
        SceneManager.LoadScene(1);
    }
}
