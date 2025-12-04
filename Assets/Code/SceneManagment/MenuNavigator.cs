using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuNavigator : MonoBehaviour
{
    [SerializeField] private RectTransform puntero;
    [SerializeField] private float offsetX = -40f;
    [SerializeField] private Selectable[] items;
    [SerializeField] private bool usarMouse = true;
    [SerializeField] private KeyCode teclaArriba = KeyCode.UpArrow;
    [SerializeField] private KeyCode teclaAbajo = KeyCode.DownArrow;
    [SerializeField] private KeyCode teclaIzquierda = KeyCode.LeftArrow;
    [SerializeField] private KeyCode teclaDerecha = KeyCode.RightArrow;
    [SerializeField] private KeyCode teclaAceptar = KeyCode.Return;
    [SerializeField] private KeyCode teclaAceptarAlt = KeyCode.Z;
    [SerializeField] private KeyCode teclaVolver = KeyCode.Escape;
    [SerializeField] private float sliderRepeatDelay = 0.3f;
    [SerializeField] private float sliderRepeatInterval = 0.08f;
    [SerializeField] private MenuOverlayManager overlayManager;

    private int indice = 0;
    private EventSystem es;
    private float nextLeftStepTime;
    private float nextRightStepTime;

    private void Awake()
    {
        es = EventSystem.current;
        if (items == null || items.Length == 0)
        {
            items = GetComponentsInChildren<Selectable>(true);
        }
        if (items.Length > 0)
        {
            SetSeleccion(items[indice]);
        }
    }

    private void OnEnable()
    {
        items = GetComponentsInChildren<Selectable>(true);
        if (items.Length > 0)
        {
            indice = Mathf.Clamp(indice, 0, items.Length - 1);
            SetSeleccion(items[indice]);
        }
    }

    private void Update()
    {
        if (items.Length == 0) return;

        // navegación arriba/abajo delegada al StandaloneInputModule (WASD/flechas)

        var actual = items[indice];
        // izquierda/derecha y sliders delegados al StandaloneInputModule

        if (Input.GetKeyDown(teclaAceptar) || Input.GetKeyDown(teclaAceptarAlt))
        {
            Activar(actual);
        }

        if (Input.GetKeyDown(teclaVolver))
        {
            if (overlayManager != null)
            {
                overlayManager.CloseOptions();
            }
        }

        var seleccionado = es != null ? es.currentSelectedGameObject : null;
        if (seleccionado == null && items.Length > 0)
        {
            SetSeleccion(items[indice]);
        }
        else if (seleccionado != null)
        {
            var sel = seleccionado.GetComponent<Selectable>();
            if (sel != null)
            {
                int i = System.Array.IndexOf(items, sel);
                if (i >= 0 && i != indice)
                {
                    indice = i;
                    SetSeleccion(items[indice]);
                }
            }
        }
    }

    private void Mover(int delta)
    {
        indice += delta;
        if (indice < 0) indice = items.Length - 1;
        if (indice >= items.Length) indice = 0;
        SetSeleccion(items[indice]);
    }

    

    private void SetSeleccion(Selectable s)
    {
        if (es != null) es.SetSelectedGameObject(s.gameObject);
        if (puntero != null)
        {
            var rt = s.transform as RectTransform;
            if (rt != null)
            {
            var p = puntero;
            p.SetParent(rt.parent, false);
            p.anchorMin = rt.anchorMin;
            p.anchorMax = rt.anchorMax;
            p.pivot = rt.pivot;
            p.anchoredPosition = new Vector2(rt.anchoredPosition.x + offsetX, rt.anchoredPosition.y);
        }
    }
    }

    private void Activar(Selectable s)
    {
        var btn = s as Button;
        if (btn != null)
        {
            btn.onClick.Invoke();
            return;
        }

        var tog = s as Toggle;
        if (tog != null)
        {
            tog.isOn = !tog.isOn;
            return;
        }

        var inp = s as InputField;
        if (inp != null)
        {
            inp.OnPointerClick(new PointerEventData(es));
            return;
        }
    }

    public void SetPointer(RectTransform p)
    {
        puntero = p;
        if (items != null && items.Length > 0)
        {
            SetSeleccion(items[indice]);
        }
    }

    public void RefreshItems()
    {
        items = GetComponentsInChildren<Selectable>(true);
    }

    public void FocusFirst()
    {
        if (items == null || items.Length == 0) return;
        indice = 0;
        SetSeleccion(items[indice]);
    }
}

