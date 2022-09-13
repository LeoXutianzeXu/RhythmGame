using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtonController : MonoBehaviour
{
    [SerializeField] string loadLevel;

    public void LoadSceneButton()
    {
        SceneManager.LoadScene(loadLevel);
    }
}
