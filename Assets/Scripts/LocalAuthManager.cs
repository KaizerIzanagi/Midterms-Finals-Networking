using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

[Serializable]
public class UserData
{
    public string username;
    public string password;
    public string email;
}

[Serializable]
public class UserDatabase
{
    public List<UserData> users = new List<UserData>();
}

[Serializable]
public class Snapshot
{
    public string username; 
    public string password;
}

public class LocalAuthManager : MonoBehaviour
{
    private static string dbPath;
    private static UserDatabase database;

    //public TMP_Text feedbackText;

    public bool isAuthenticated { get; private set; }
    public string User { get; private set; }

    private void Awake()
    {
        dbPath = Path.Combine(Application.dataPath, "../users.json");
        LoadDatabase();
    }

    // Internal I/O

    private void LoadDatabase()
    {
        if (File.Exists(dbPath))
        {
            string json = File.ReadAllText(dbPath);
            database = JsonUtility.FromJson<UserDatabase>(json);
        }
        else
        {
            database = new UserDatabase();
            SaveDatabase();
        }
    }

    private void SaveDatabase()
    {
        string json = JsonUtility.ToJson(database, true);
        File.WriteAllText(dbPath, json);
    }

    // SET API's

    public bool Register(string username, string password, string confirmPassword, string email)
    {
        if (username == "" || password == "")
        {
            PrintFeedback("Username or password must not be blank!", 1.2f, Color.red);
            return false;
        }

        // Check if the password matches
        if (password != confirmPassword)
        {
            PrintFeedback("Password must match!", 1.2f, Color.red);
            return false;
        }

        // Check if it already exists in the database
        if(database.users.Exists(u => u.username.Equals(username, StringComparison.OrdinalIgnoreCase)))
        {
            PrintFeedback("Username already exists!", 1.2f, Color.red);
            return false;
        }

        // If successful, we can proceed to create the user's data
        UserData newUser = new UserData
        {
            username = username,
            password = password,
            email = email
        };

        database.users.Add(newUser);
        SaveDatabase();
        PrintFeedback("Registered Successfully", 1.2f, Color.green);
        return true;
    }

    public bool Login(string username, string password)
    {
        if(username == "" || password == "")
        {
            PrintFeedback("Username or password must not be blank!", 1.2f, Color.red);
            return false;
        }

        Snapshot snap = new Snapshot { username = username, password = password };
        return ValidateSnapshot(snap);
    }

    public bool ValidateSnapshot(Snapshot snapshot)
    {
        foreach(var user in database.users)
        {
            if (user.username.Equals(snapshot.username, StringComparison.OrdinalIgnoreCase) &&
                user.password == snapshot.password)
            {
                PrintFeedback("Login Success!", 1.2f, Color.green);
                User = user.username;
                isAuthenticated = true;
                return true;
            }
        }

        PrintFeedback("Invalid username or password!", 1.2f, Color.red);
        return false;
    }

    // -- Helper Methods --

    private void PrintFeedback(string message, float time, Color color)
    {
        Debug.Log(message);
        //Invoke(nameof(RemoveFeedback), time);
    }

    public bool isUserAuthenticated()
    {
        return isAuthenticated;
    }

    public string GetUser()
    {
        return User;
    }

    void RemoveFeedback()
    {
        //feedbackText.text = "";
    }
}