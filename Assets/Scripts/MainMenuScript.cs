using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    public void OnClick_StartGame() { SceneManager.LoadScene(2); }
    public void OnClick_Logout() { SceneManager.LoadScene(1); }
}
