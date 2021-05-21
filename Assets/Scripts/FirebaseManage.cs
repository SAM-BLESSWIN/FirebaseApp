using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Firebase;
using Firebase.Extensions;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Storage;
using TMPro;
using UnityEngine.UI;

public class FirebaseManage : MonoBehaviour
{
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser User;
    public DatabaseReference DBreference;
    private StorageReference storageRef;

    [Header("Login")]
    public TMP_InputField L_mailid;
    public TMP_InputField L_password;
    public TMP_Text L_error;

    [Header("Register")]
    public TMP_InputField R_mailid;
    public TMP_InputField R_password;
    public TMP_Text R_error;

    [Header("UserData")]
    public TMP_InputField Name;
    public TMP_InputField Description;
    public TMP_Text U_error;

    [Header("FirebaseData")]
    public TMP_InputField F_Name;
    public TMP_InputField F_Description;
    public TMP_Text F_error;

    [Header("UI screen")]
    public GameObject loginscreen;
    public GameObject registerscreen;
    public GameObject Imagesetterscreen;
    public GameObject Imagegetterscreen;
    public GameObject capturescreen;
    public GameObject galleryscreen;

    [Header("Gallery")]
    public GameObject ImageParent;
    public GameObject F_image;
    public TMP_Text G_error;

    private string F_Imagename;
    private string F_Imagedesc;
    private int count = 0;
    private int totalcount = 0;
   // private int currentcount = 0;
    private GameObject Imagespawned;
    private bool Nameexists = false;

    [Header("Capture")]
    public RawImage webcamRAW;
    public RawImage img;
    public RawImage G_img;
    Texture2D tex;
    WebCamTexture webCamTexture;
    WebCamDevice[] devices;
    byte[] bytes;
    byte[] F_bytes;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        //Check that all of the necessary dependencies for Firebase are present on the system
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    private void InitializeFirebase()
    {
        Debug.Log("Setting up Firebase Auth");
        auth = FirebaseAuth.DefaultInstance;
        DBreference = FirebaseDatabase.DefaultInstance.RootReference;
        storageRef = FirebaseStorage.DefaultInstance.GetReferenceFromUrl("gs://fir-unity-app-5c4e9.appspot.com");
    }

    public void LoginButton()
    {
        StartCoroutine(Login(L_mailid.text, L_password.text));
    }

    public void RegisterButton()
    {
        StartCoroutine(Register(R_mailid.text, R_password.text));
    }

    public void Onsubmit()
    {
        Nameexists = false;
        if (Name.text != "" && Description.text != "")
        {
            Debug.Log(Name.text + " " + Description.text);
            StartCoroutine(Imagedetails(Name.text, Description.text));
        }
    }

    public void Getdetail(int index)
    {
        StartCoroutine(Loaddetails(index));
    }

    public void Getgallery()
    {
        StartCoroutine(Countimages());
    }

    private IEnumerator Login(string email, string password)
    {
        //Call the Firebase auth signin function passing the email and password
        var LoginTask = auth.SignInWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

        if (LoginTask.Exception != null)
        {
            //If there are errors handle them
            Debug.LogWarning(message: $"Failed to register task with {LoginTask.Exception}");
            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "Login Failed!";
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "Missing Email";
                    break;
                case AuthError.MissingPassword:
                    message = "Missing Password";
                    break;
                case AuthError.WrongPassword:
                    message = "Wrong Password";
                    break;
                case AuthError.InvalidEmail:
                    message = "Invalid Email";
                    break;
                case AuthError.UserNotFound:
                    message = "Account does not exist";
                    break;
            }
            Debug.Log(message);
            L_error.text = message;
        }
        else
        {
            L_error.text = " ";
            User = LoginTask.Result;
            Debug.LogFormat("User signed in successfully: ", User.Email);
            loginscreen.SetActive(false);
            capturescreen.SetActive(true);
        }
    }

    private IEnumerator Register(string email, string password)
    {
        //Call the Firebase auth signin function passing the email and password
        var RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

        if (RegisterTask.Exception != null)
        {
            //If there are errors handle them
            Debug.LogWarning(message: $"Failed to register task with {RegisterTask.Exception}");
            FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "Register Failed!";
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "Missing Email";
                    break;
                case AuthError.MissingPassword:
                    message = "Missing Password";
                    break;
                case AuthError.WeakPassword:
                    message = "Weak Password";
                    break;
                case AuthError.EmailAlreadyInUse:
                    message = "Email Already In Use";
                    break;
            }
            Debug.Log(message);
            R_error.text = message;
        }
        else
        {
            R_error.text = " ";
            User = RegisterTask.Result;
            Debug.LogFormat("User signed in successfully: ", User.Email);
            registerscreen.SetActive(false);
            loginscreen.SetActive(true);
        }
    }

    private IEnumerator Imagedetails(string name, string desc)
    {
        var DBnfTask = DBreference.Child("users").Child(User.UserId).Child("Name").GetValueAsync();

        yield return new WaitUntil(predicate: () => DBnfTask.IsCompleted);

        if (DBnfTask.Exception != null)
        {
            F_error.text = DBnfTask.Exception.ToString();
            Debug.LogWarning(message: $"Failed to register task with {DBnfTask.Exception}");
        }
        else if (DBnfTask.Result.Value == null)
        {
            Debug.Log("no data");
        }
        else
        {
            U_error.text = " ";
            //Data has been retrieved
            DataSnapshot snapshot = DBnfTask.Result;
            foreach (DataSnapshot Name in snapshot.Children)
            {
                if (name == Name.Value.ToString())
                {
                    U_error.text = "Name already exists";
                    Nameexists = true;
                    break;
                }
            }
        }

        if (!Nameexists)
        {
            var DBnTask = DBreference.Child("users").Child(User.UserId).Child("Name").Push().SetValueAsync(name);
            var DBdTask = DBreference.Child("users").Child(User.UserId).Child("Description").Push().SetValueAsync(desc);

            var customBytes = bytes;
            StorageReference captureRef = storageRef.Child("users").Child(User.UserId).Child("Images/" + name + ".jpg");

            captureRef.PutBytesAsync(customBytes).ContinueWith((Task<StorageMetadata> task) => {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.Log(task.Exception.ToString());
                }
                else
                {
                    // Metadata contains file metadata such as size, content-type, and md5hash.
                    StorageMetadata metadata = task.Result;
                    string md5Hash = metadata.Md5Hash;
                    Debug.Log("Finished uploading...");
                    Debug.Log("md5 hash = " + md5Hash);
                }
            });

            yield return new WaitUntil(predicate: () => DBdTask.IsCompleted);

            if (DBdTask.Exception != null)
            {
                U_error.text = DBdTask.Exception.ToString();
                Debug.LogWarning(message: $"Failed to register task with {DBdTask.Exception}");
            }
            else
            {
                U_error.text = " ";
                Imagesetterscreen.SetActive(false);
                Getgallery();
                galleryscreen.SetActive(true);
                img.texture = null;
                Name.text = "";
                Description.text = "";
            }
        }
    }

    private IEnumerator Loaddetails(int i)
    {
        //Get the currently logged in user data
        var DBnTask = DBreference.Child("users").Child(User.UserId).Child("Name").GetValueAsync();
        var DBdTask = DBreference.Child("users").Child(User.UserId).Child("Description").GetValueAsync();

        yield return new WaitUntil(predicate: () => DBdTask.IsCompleted);

        if (DBnTask.Exception != null)
        {
            F_error.text = DBdTask.Exception.ToString();
            Debug.LogWarning(message: $"Failed to register task with {DBnTask.Exception}");
        }
        else if (DBnTask.Result.Value == null)
        {
            Debug.Log("no data");
        }
        else
        {
            F_error.text = " ";
            //Data has been retrieved
            DataSnapshot snapshot = DBnTask.Result;
            foreach (DataSnapshot Name in snapshot.Children)
            {
                Debug.Log(count + " - " + i);
                if (count < i)
                {
                    count++;
                    F_Imagename = "";
                }
                else
                {
                    F_Imagename = Name.Value.ToString();
                    count = 0;
                    break;
                }
            }
            Debug.Log(F_Imagename);
            F_Name.text = F_Imagename;
        }

        if (DBdTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBnTask.Exception}");
        }
        else if (DBdTask.Result.Value == null)
        {
            Debug.Log("no data");
        }
        else
        {
            //Data has been retrieved
            DataSnapshot snapshot = DBdTask.Result;
            foreach (DataSnapshot Desc in snapshot.Children)
            {
                Debug.Log(count + " - " + i);
                if (count < i)
                {
                    count++;
                    F_Imagedesc = "";
                }
                else
                {
                    F_Imagedesc = Desc.Value.ToString();
                    count = 0;
                    break;
                }
            }
            Debug.Log(F_Imagedesc);
            F_Description.text = F_Imagedesc;
        }

        StorageReference captureRef = storageRef.Child("users").Child(User.UserId).Child("Images/" + F_Imagename + ".jpg");

        const long maxAllowedSize = 1 * 1024 * 1024;
        captureRef.GetBytesAsync(maxAllowedSize).ContinueWithOnMainThread(task => {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogException(task.Exception);

            }
            else
            {
                F_bytes = task.Result;
                tex.LoadImage(F_bytes);
                tex.Apply();
                G_img.texture = tex;
                Invoke("Floadimg",0.1f);
                Debug.Log("Finished downloading!");
            }
        });       
    }

    private void Floadimg()
    {
        galleryscreen.SetActive(false);
        Imagegetterscreen.SetActive(true);
    }

    public void clearimg()
    {
        F_bytes = null;
        G_img.texture = null;
        galleryscreen.SetActive(true);
        Imagegetterscreen.SetActive(false); 
    }

    private IEnumerator Countimages()
    {
        var DBnTask = DBreference.Child("users").Child(User.UserId).Child("Name").GetValueAsync();

        yield return new WaitUntil(predicate: () => DBnTask.IsCompleted);

        if (DBnTask.Exception != null)
        {
            G_error.text = DBnTask.Exception.ToString();
            Debug.LogWarning(message: $"Failed to register task with {DBnTask.Exception}");
        }
        else if (DBnTask.Result.Value == null)
        {
            Debug.Log("no data");
            G_error.text = "No images";
        }
        else
        {
            G_error.text = " ";
            //Data has been retrieved
            DataSnapshot snapshot = DBnTask.Result;
            foreach (DataSnapshot Name in snapshot.Children)
            {
                Imagespawned = Instantiate(F_image, transform.position, Quaternion.identity, ImageParent.transform);
                Imagespawned.name = totalcount.ToString();

                string G_Imagename = Name.Value.ToString();
             
                Imagespawned.GetComponentInChildren<TMP_Text>().text = G_Imagename;
                totalcount++;
            }
        }
        /*for (; currentcount < totalcount; currentcount++)
        {
            Imagespawned = Instantiate(F_image, transform.position, Quaternion.identity, ImageParent.transform);
            Imagespawned.name = currentcount.ToString();
        }*/
        totalcount = 0;
        capturescreen.SetActive(false);
        galleryscreen.SetActive(true);
    }
    
    public void cleargallery()
    {
        foreach (Transform child in ImageParent.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    #region camera
    IEnumerator Start()
    { 
        devices = WebCamTexture.devices;
        webCamTexture = new WebCamTexture(1280, 720, 25);
        webCamTexture.deviceName = devices[0].name;
        webcamRAW.texture = webCamTexture;
        Debug.Log(webCamTexture.deviceName);
        webCamTexture.Play();

        tex = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        yield return null;
    }


    public void clickImage()
    {
        webCamTexture.Pause();
        StartCoroutine(getbytes());
    }

    public void reloadcam()
    {
        webCamTexture.Play();
        img.texture = null;
        Name.text = "";
        Description.text = "";
    }

    IEnumerator getbytes()
    {
        Texture2D snap = new Texture2D(1280, 720);
        snap.SetPixels(webCamTexture.GetPixels());
        snap.Apply();
        yield return new WaitForEndOfFrame();
        bytes = snap.EncodeToJPG();
        StartCoroutine(readImage());
    }

    IEnumerator readImage()
    {
        yield return new WaitForSeconds(0.1f);
        print(bytes.Length);
        tex.LoadImage(bytes);
        tex.Apply();
        img.texture = tex;
        yield return new WaitForSeconds(0.1f);
    }

    #endregion

    public static FirebaseManage instance;
}
