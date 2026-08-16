using IRIS.MetaQuest3.QRCodeDetection;
using IRIS.Node;
using Meta.XR.MRUtilityKit;
using Meta.XR.Samples;
using UnityEngine;

public class MQ3QRAlignmentManager : Singleton<MQ3QRAlignmentManager>
{
    [SerializeField] private QRCodeManager qrCodeManager;

    [SerializeField] private string qrText = "IRIS";

    void Update()
    {
        if (qrCodeManager == null)
        {
            return;
        }

        if (qrCodeManager.TrackingEnabled)
        {
            UseLivePose();
        }
    }

    public void StartQRAlignment()
    {
        if (qrCodeManager == null)
        {
            Debug.LogError("[MQ3QRAlignmentManager] QRCodeManager is null.");
            return;
        }

        if (!QRCodeManager.HasPermissions)
        {
            Debug.LogError("[MQ3QRAlignmentManager] Scene permission is missing.");
            return;
        }

        if (!QRCodeManager.IsSupported)
        {
            Debug.LogError("[MQ3QRAlignmentManager] QR tracking is not supported by this runtime.");
            return;
        }

        qrCodeManager.TrackingEnabled = true;

        if (qrCodeManager.TrackingEnabled)
        {
            Debug.Log("[MQ3QRAlignmentManager] QR alignment started.");
        }
        else
        {
            Debug.LogError("[MQ3QRAlignmentManager] Failed to enable QR tracking.");
        }
    }

    public void StopQRAlignment()
    {
        if (qrCodeManager != null && qrCodeManager.TrackingEnabled)
        {
            qrCodeManager.TrackingEnabled = false;
        }
        Debug.Log("[MQ3QRAlignmentManager] QR alignment stopped.");
    }

    private void UseLivePose()
    {
        if (qrCodeManager.TryGetTrackedQRCode(qrText, out MRUKTrackable trackable))
        {
            Pose stablePose = new(
                trackable.transform.position,
                trackable.transform.rotation * Quaternion.Euler(90f, 0f, 0f));
            ApplyQRPose(stablePose);
        }
    }

    private void ApplyQRPose(Pose qrPose)
    {
        transform.SetPositionAndRotation(qrPose.position, qrPose.rotation);
    }
}
