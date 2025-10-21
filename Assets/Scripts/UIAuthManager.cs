using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIAuthManager : MonoBehaviour
{
    public LocalAuthManager auth;

    private void Awake()
    {
        if(auth == null)
            auth = FindObjectOfType<LocalAuthManager>();
    }

    [Header("LOGIN UI")]
    public TMP_InputField usernameInputField;
    public TMP_InputField passwordInputField;
    public Button loginButton;
    public Button goToRegisterButton;

    [Header("REGISTER UI")]
    public TMP_InputField reg_UsernameInputField;
    public TMP_InputField reg_PasswordInputField;
    public TMP_InputField reg_ConfirmPasswordInputField;
    public TMP_InputField reg_EmailInputField;
    public Button registerButton;

    [Header("MISC")]
    public Button backToLoginButton;

    [Header("UI PANELS")]
    public GameObject loginUIPanel;
    public GameObject registerUIPanel;

    private void Start()
    {
        RegisterButtons();
    }

    void RegisterButtons()
    {
        loginButton.onClick.AddListener(TryLogin);
        registerButton.onClick.AddListener(TryRegister);

        backToLoginButton.onClick.AddListener(() => { OpenPanel("Login", true); OpenPanel("Register", false); });
        goToRegisterButton.onClick.AddListener(() => { OpenPanel("Login", false); OpenPanel("Register", true); });
    }

    // POST API's
    public void TryLogin()
    {
        bool success = auth.Login(usernameInputField.text, passwordInputField.text);

        if (success) LoadGame();
    }

    public void TryRegister()
    {
        bool success = auth.Register(reg_UsernameInputField.text, reg_PasswordInputField.text, reg_ConfirmPasswordInputField.text, reg_EmailInputField.text);

        if (success)
        {
            OpenPanel("Login", true);
            OpenPanel("Register", false);
        }
    }

    // -- Helper Methods --
    public void OpenPanel(string panel, bool toggle)
    {
        switch (panel)
        {
            case "Login":
                loginUIPanel.SetActive(toggle);
                break;
            case "Register":
                registerUIPanel.SetActive(toggle);
                break;
            default:
                loginUIPanel.SetActive(true);
                registerUIPanel.SetActive(false);
                Debug.LogError($"{panel} is not valid Panel!");
                break;
        }
    }


    // POST-CALLBACK API's
    public void LoadGame()
    {
        OpenPanel("Login", false);
        FusionManualBootstrapper.Instance.OnLoadStartServer();
    }
}