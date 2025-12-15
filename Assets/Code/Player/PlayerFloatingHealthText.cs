using UnityEngine;
using TMPro;

public class PlayerFloatingHealthText : MonoBehaviour
{
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.5f, 0f);
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private int fontSize = 24;
    [SerializeField] private bool showMaxHealth = true;
    [SerializeField] private bool includeTemporaryShieldForMage = false;
    [SerializeField] private string prefix = "Vida: ";
    [SerializeField] private string sortingLayerName = "";
    [SerializeField] private int sortingOrder = 2000;
    [SerializeField] private TMP_FontAsset fontAsset;
    [SerializeField] private bool keepConstantWorldScale = true;

    private playerLife life;
    private TextMeshPro tmp;
    private Transform camT;

    private void Awake()
    {
        life = GetComponent<playerLife>();
        if (life == null)
        {
            life = FindFirstObjectByType<playerLife>(FindObjectsInactive.Exclude);
        }
        var go = new GameObject("FloatingHealthText");
        go.transform.SetParent(transform, false);
        tmp = go.AddComponent<TextMeshPro>();
        tmp.color = textColor;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        if (fontAsset != null) tmp.font = fontAsset;
        var mr = tmp.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            if (!string.IsNullOrEmpty(sortingLayerName)) mr.sortingLayerName = sortingLayerName;
            mr.sortingOrder = sortingOrder;
        }
        camT = Camera.main != null ? Camera.main.transform : null;
        UpdateTextImmediate();
    }

    private void LateUpdate()
    {
        if (tmp == null) return;
        tmp.transform.position = transform.position + worldOffset;
        UpdateTextImmediate();
        if (camT != null)
        {
            tmp.transform.rotation = Quaternion.identity;
        }
        if (keepConstantWorldScale)
        {
            float sx = transform.lossyScale.x;
            float sy = transform.lossyScale.y;
            if (Mathf.Abs(sx) < 1e-4f) sx = 1f;
            if (Mathf.Abs(sy) < 1e-4f) sy = 1f;
            float signX = sx < 0f ? -1f : 1f;
            tmp.transform.localScale = new Vector3(signX / Mathf.Abs(sx), 1f / Mathf.Abs(sy), 1f);
        }
    }

    private void UpdateTextImmediate()
    {
        if (life == null)
        {
            tmp.text = prefix + "?/?";
            return;
        }
        int current = life.Health;
        int max = life.MaxHealth;
        if (includeTemporaryShieldForMage && life.IsSecondCharacterMage)
        {
            current = Mathf.Clamp(life.Health + life.TempShield, 0, max + life.TempShieldMax);
            max = max + life.TempShieldMax;
        }
        tmp.text = showMaxHealth ? (prefix + current + "/" + max) : (prefix + current.ToString());
        tmp.enabled = current > 0;
    }
}
