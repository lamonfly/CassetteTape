using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    // Load main scene
    public void ToMain()
    {
        SceneManager.LoadScene(1);
    }
}
