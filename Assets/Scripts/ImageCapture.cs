using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class ImageCapture : MonoBehaviour
{
	/*public void Capture()
	 {
		 // Don't attempt to use the camera if it is already open
		 if (NativeCamera.IsCameraBusy())
		 {
			 return;
		 }
		 else
		 {
			 TakePicture(512);
		 }

	 }

	 private void TakePicture(int maxSize)
	 {
		 NativeCamera.Permission permission = NativeCamera.TakePicture((path) =>
		 {
			 Debug.Log("Image path: " + path);
			 if (path != null)
			 {
				 // Create a Texture2D from the captured image
				 Texture2D texture = NativeCamera.LoadImageAtPath(path, maxSize);
				 if (texture == null)
				 {
					 Debug.Log("Couldn't load texture from " + path);
					 return;
				 }

				 // Assign texture to a temporary quad and destroy it after 5 seconds
				 GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
				 quad.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 2.5f;
				 quad.transform.forward = Camera.main.transform.forward;
				 quad.transform.localScale = new Vector3(1f, texture.height / (float)texture.width, 1f);

				 Material material = quad.GetComponent<Renderer>().material;
				 if (!material.shader.isSupported) // happens when Standard shader is not included in the build
					 material.shader = Shader.Find("Legacy Shaders/Diffuse");

				 material.mainTexture = texture;

				 Destroy(quad, 5f);

				 // If a procedural texture is not destroyed manually, 
				 // it will only be freed after a scene change
				 Destroy(texture, 5f);
			 }
		 }, maxSize);

		 Debug.Log("Permission result: " + permission);
	 }

	WebCamTexture webCamTexture;

	void Start()
	{
		webCamTexture = new WebCamTexture();
		webCamTexture.Play();
	}

	void Update()
	{
		gameObject.GetComponentInChildren<RawImage>().texture = webCamTexture;
	}

	public void capture()
    {
		StartCoroutine(TakePhoto());
    }

	IEnumerator TakePhoto()  // Start this Coroutine on some button click
	{

		// NOTE - you almost certainly have to do this here:

		yield return new WaitForEndOfFrame();

		Texture2D photo = new Texture2D(webCamTexture.width, webCamTexture.height);
		photo.SetPixels(webCamTexture.GetPixels());
		photo.Apply();

		//Encode to a PNG
		byte[] bytes = photo.EncodeToJPG();
		//Write out the PNG. Of course you have to substitute your_path for something sensible
		Debug.Log(Application.persistentDataPath + "/" + "photo" + Time.time + ".jpg");
		File.WriteAllBytes(Application.persistentDataPath + "/" + "photo" +Time.time+".jpg", bytes);
	}*/
	public RawImage webcamRAW;
	public RawImage img;
	Texture2D tex;
	WebCamTexture webCamTexture;
	WebCamDevice[] devices;

    IEnumerator Start()
	{

		devices = WebCamTexture.devices;
		webCamTexture = new WebCamTexture(1280, 720, 25);
		// bool itHasFrontCam = false;
		/*for (int i = 0; i < devices.Length; i++)
		{
			if (devices[i].isFrontFacing)
			{
				//  itHasFrontCam = true;
				webCamTexture.deviceName = devices[i].name;
			}
			Debug.Log(devices[i].name);
		}*/
		webCamTexture.deviceName = devices[0].name;
		webcamRAW.texture = webCamTexture;
		Debug.Log(webCamTexture.deviceName);
		//webcamRAW.material.mainTexture = webCamTexture;
		webCamTexture.Play();

		tex = new Texture2D(1280, 720, TextureFormat.RGB24, false);
		yield return null;
	}

	byte[] bytes;

	public void clickImage()
	{
		webCamTexture.Pause();
		StartCoroutine(getbytes());
	}

	public void reloadcam()
    {
		webCamTexture.Play();
		img.texture = null;
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
}
