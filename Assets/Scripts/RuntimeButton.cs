using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Storage;
using TMPro;

public class RuntimeButton : MonoBehaviour
{
    public void Onclick()
    {
        int id = int.Parse(transform.name);
        FirebaseManage.instance.Getdetail(id);
    }
}
