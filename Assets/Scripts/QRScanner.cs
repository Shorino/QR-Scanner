using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZXing;

public class QRScanner : MonoBehaviour
{
    [Header("References")]
    public RawImage backgroundRawImage;
    public AspectRatioFitter aspectRatioFitter;
    public RectTransform scanZone;

    [Header("Runtime")]
    public bool isCamAvailable;
    public WebCamTexture cameraTexture;
    public bool scanning;

    public void Start()
    {
        SetUpCamera();
    }
    public void Update()
    {
        UpdateCameraRender();
        if (!scanning)
        {
            scanning = true;
            Scan();
        }
    }

    void SetUpCamera()
    {
        WebCamDevice[] devices = WebCamTexture.devices;

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

        cameraTexture.Play();
        backgroundRawImage.texture = cameraTexture;
        isCamAvailable = true;
    }

    void UpdateCameraRender()
    {
        if (!isCamAvailable) return;

        float ratio = (float)cameraTexture.width / (float)cameraTexture.height;
        aspectRatioFitter.aspectRatio = ratio;

        int orientation = cameraTexture.videoRotationAngle;
        backgroundRawImage.rectTransform.localEulerAngles = new Vector3(0, 0, orientation);
    }

    void Scan()
    {
        try
        {
            IBarcodeReader barcodeReader = new BarcodeReader();
            Result result = barcodeReader.Decode(cameraTexture.GetPixels32(), cameraTexture.width, cameraTexture.height);

            if (result == null)
            {
                Scan();
            }
            else
            {
                // TODO: after getting result
                Application.OpenURL(result.Text);
            }
        }
        catch
        {
            Scan();
        }
    }
}
