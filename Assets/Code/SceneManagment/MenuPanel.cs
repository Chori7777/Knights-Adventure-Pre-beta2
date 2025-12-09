using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuPanel : MonoBehaviour
{
    [Header("Estilos por defecto")]
    [SerializeField] private Color colorNormal = Color.white;
    [SerializeField] private Color colorSeleccionado = new Color(1f, 0.9f, 0.2f);
    [SerializeField] private float escalaNormal = 1f;
    [SerializeField] private float escalaSeleccionado = 1.1f;
    [SerializeField] private float duracionTween = 0.15f;

    [Header("Navegación")]

    [SerializeField] private RectTransform puntero;

    private void Awake()
    {
        var selectables = GetComponentsInChildren<Selectable>(true);
        foreach (var s in selectables)
        {
            var style = s.gameObject.GetComponent<MenuSelectionStyling>();
            if (style == null)
                style = s.gameObject.AddComponent<MenuSelectionStyling>();

            Graphic graphic = null;
            var tmp = s.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp != null) graphic = tmp;
            if (graphic == null) graphic = s.GetComponentInChildren<Graphic>(true);

            style.Configure(
                graphic,
                colorNormal,
                colorSeleccionado,
                escalaNormal,
                escalaSeleccionado,
                duracionTween
            );
        }

    }
}
