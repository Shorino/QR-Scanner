using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using ZXing;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

public class QRScanner : MonoBehaviour
{
    [Header("References")]
    public RawImage backgroundRawImage;
    public AspectRatioFitter aspectRatioFitter;
    public RectTransform scanZone;

    [Header("Runtime")]
    bool isCamAvailable;
    WebCamTexture cameraTexture;

    // [Header("Config")]
    string logPrefix;

    void Start()
    {
        logPrefix = "[" + Application.productName + "][" + this.name + "] ";

#if UNITY_IOS || UNITY_WEBGL
        StartCoroutine(AskForPermissionIfRequired(UserAuthorization.WebCam, () => { SetUpCamera(); }));
        return;
#elif UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            AskCameraPermission();
            return;
        }
#endif
        SetUpCamera();
    }
    void Update()
    {
        UpdateCameraRender();
        Scan();
    }

#if UNITY_IOS || UNITY_WEBGL
    private bool CheckPermissionAndRaiseCallbackIfGranted(UserAuthorization authenticationType, Action authenticationGrantedAction)
    {
        if (Application.HasUserAuthorization(authenticationType))
        {
            if (authenticationGrantedAction != null)
                authenticationGrantedAction();

            return true;
        }
        return false;
    }

    private IEnumerator AskForPermissionIfRequired(UserAuthorization authenticationType, Action authenticationGrantedAction)
    {
        if (!CheckPermissionAndRaiseCallbackIfGranted(authenticationType, authenticationGrantedAction))
        {
            yield return Application.RequestUserAuthorization(authenticationType);
            if (!CheckPermissionAndRaiseCallbackIfGranted(authenticationType, authenticationGrantedAction))
                Debug.LogWarning($"{logPrefix}Permission {authenticationType} Denied");
        }
    }
#elif UNITY_ANDROID
    private void PermissionCallbacksPermissionGranted(string permissionName)
    {
        StartCoroutine(DelayedCameraInitialization());
    }

    private IEnumerator DelayedCameraInitialization()
    {
        yield return null;
        SetUpCamera();
    }

    private void PermissionCallbacksPermissionDenied(string permissionName)
    {
        Debug.LogWarning($"{logPrefix}Permission {permissionName} Denied");
    }

    private void AskCameraPermission()
    {
        var callbacks = new PermissionCallbacks();
        callbacks.PermissionDenied += PermissionCallbacksPermissionDenied;
        callbacks.PermissionGranted += PermissionCallbacksPermissionGranted;
        Permission.RequestUserPermission(Permission.Camera, callbacks);
    }
#endif

    void SetUpCamera()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        Debug.Log(logPrefix + "WebCam Devices: " + devices.Length);

        if (devices.Length == 0)
        {
            isCamAvailable = false;
            return;
        }

        for (int i = 0; i < devices.Length; i++)
        {
            if (!devices[i].isFrontFacing)
            {
                cameraTexture = new WebCamTexture(devices[i].name, (int)scanZone.rect.width, (int)scanZone.rect.height);
            }
        }

        if (cameraTexture != null)
        {
            cameraTexture.Play();
            backgroundRawImage.texture = cameraTexture;
            isCamAvailable = true;
        }
        else
        {
            isCamAvailable = false;
        }
    }

    void UpdateCameraRender()
    {
        if (!isCamAvailable) return;

        float ratio = (float)cameraTexture.width / (float)cameraTexture.height;
        aspectRatioFitter.aspectRatio = ratio;

        int orientation = -cameraTexture.videoRotationAngle;
        backgroundRawImage.rectTransform.localEulerAngles = new Vector3(0, 0, orientation);
    }

    void Scan()
    {
        if (!isCamAvailable) return;

        try
        {
            IBarcodeReader barcodeReader = new BarcodeReader();
            Result result = barcodeReader.Decode(cameraTexture.GetPixels32(), cameraTexture.width, cameraTexture.height);

            if (result != null)
            {
                // TODO: after getting result
                Application.OpenURL(result.Text);
            }
        }
        catch
        {
        }
    }
}
