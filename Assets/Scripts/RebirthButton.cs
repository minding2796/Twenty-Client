using UnityEngine;
using UnityEngine.SceneManagement;

public class RebirthButton : MonoBehaviour
{
    public void OnClickRebirth()
    {
        SceneManager.LoadScene("MainGame");
    }
}
