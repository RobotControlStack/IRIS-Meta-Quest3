using System;
using IRIS.Node;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OffsetConfigMenuManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the MQ3QRAlignmentManager in the scene")]
    [SerializeField] private IRISOrigin irisOrigin;

    [Header("UI Components")]
    [SerializeField] Slider offsetX, offsetY, offsetZ;
    [SerializeField] Slider rotX, rotY, rotZ;

    [SerializeField] private TMP_Text offsetXText, offsetYText, offsetZText;
    [SerializeField] private TMP_Text rotXText, rotYText, rotZText;
    // [SerializeField] private TMP_Text SceneNameText;

    [Header("Settings")]
    [SerializeField] private float posStepSize = 0.001f; // Defined in Meters (e.g. 0.001 = 1mm)
    [SerializeField] private float rotStepSize = 1f;    // Defined in Degrees

    private SceneOffset offset = new();
    private bool listenersRegistered;

    void Start()
    {
        if (irisOrigin == null)
        {
            Debug.LogError("[OffsetConfigMenuManager] IrisOrigin reference is missing!");
            return;
        }
        irisOrigin.OnOffsetApplied += Initialize;
        AddListeners();
    }

    private void OnDestroy()
    {
        if (irisOrigin != null)
        {
            irisOrigin.OnOffsetApplied -= Initialize;
        }
        RemoveListeners();
    }

    public void Initialize(SceneOffset offset)
    {
        Debug.Log($"[OffsetConfigMenuManager] Init: {name}");

        // Prevent infinite loops if re-initializing with same object
        if (this.offset != null && offset == this.offset) return;

        this.offset = offset;

        // Initialize Sliders (Position: Meters -> mm, Rotation: Degrees -> Degrees)
        offsetX.value = offset.x * 1000f;
        offsetY.value = offset.y * 1000f;
        offsetZ.value = offset.z * 1000f;
        rotX.value = offset.rotX;
        rotY.value = offset.rotY;
        rotZ.value = offset.rotZ;

        // Initialize Text
        UpdatePositionText(offsetX.value, offsetXText);
        UpdatePositionText(offsetY.value, offsetYText);
        UpdatePositionText(offsetZ.value, offsetZText);
        UpdateRotationText(rotX.value, rotXText);
        UpdateRotationText(rotY.value, rotYText);
        UpdateRotationText(rotZ.value, rotZText);
    }

    private void HandlePositionChange(float sliderValueMM, Action<float> setOffsetAction, TMP_Text textComponent)
    {
        // Convert Slider (mm) to Data (meters)
        setOffsetAction(sliderValueMM / 1000f);

        // Update UI Text
        UpdatePositionText(sliderValueMM, textComponent);

        // Send Network Update
        irisOrigin.ApplyAlignmentOffset(offset);

    }

    private void HandleRotationChange(float sliderValueDeg, Action<float> setOffsetAction, TMP_Text textComponent)
    {
        // Direct assignment (Degrees to Degrees)
        setOffsetAction(sliderValueDeg);

        // Update UI Text
        UpdateRotationText(sliderValueDeg, textComponent);

        // Send Network Update
        irisOrigin.ApplyAlignmentOffset(offset);
    }

    // Helper to format text consistently
    private void UpdatePositionText(float mm, TMP_Text text) => text.text = (mm / 10f).ToString("F1") + " cm";
    private void UpdateRotationText(float deg, TMP_Text text) => text.text = deg.ToString("F0") + "°";


    private void AddListeners()
    {
        if (listenersRegistered) return;

        offsetX.onValueChanged.AddListener(val => HandlePositionChange(val, x => offset.x = x, offsetXText));
        offsetY.onValueChanged.AddListener(val => HandlePositionChange(val, y => offset.y = y, offsetYText));
        offsetZ.onValueChanged.AddListener(val => HandlePositionChange(val, z => offset.z = z, offsetZText));

        rotX.onValueChanged.AddListener(val => HandleRotationChange(val, x => offset.rotX = x, rotXText));
        rotY.onValueChanged.AddListener(val => HandleRotationChange(val, y => offset.rotY = y, rotYText));
        rotZ.onValueChanged.AddListener(val => HandleRotationChange(val, z => offset.rotZ = z, rotZText));

        listenersRegistered = true;
    }

    private void RemoveListeners()
    {
        if (!listenersRegistered) return;
        offsetX.onValueChanged.RemoveAllListeners();
        offsetY.onValueChanged.RemoveAllListeners();
        offsetZ.onValueChanged.RemoveAllListeners();
        rotX.onValueChanged.RemoveAllListeners();
        rotY.onValueChanged.RemoveAllListeners();
        rotZ.onValueChanged.RemoveAllListeners();
        listenersRegistered = false;
    }

    public void StepOffsetX(int step) => offsetX.value += step * (posStepSize * 1000f);
    public void StepOffsetY(int step) => offsetY.value += step * (posStepSize * 1000f);
    public void StepOffsetZ(int step) => offsetZ.value += step * (posStepSize * 1000f);

    public void StepRotX(int step) => rotX.value += step * rotStepSize;
    public void StepRotY(int step) => rotY.value += step * rotStepSize;
    public void StepRotZ(int step) => rotZ.value += step * rotStepSize;
}
