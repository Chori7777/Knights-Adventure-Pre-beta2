using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class MenuNavigator : MonoBehaviour
{
    [SerializeField] private RectTransform puntero;
    [SerializeField] private float offsetX = -40f;
    [SerializeField] private Selectable[] items;
    [SerializeField] private bool pointerAdaptToItem = true;
    [SerializeField] private bool pointerAdaptToText = false;
    [SerializeField] private Vector2 pointerPadding = new Vector2(12f, 8f);
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
            items = GetComponentsInChildren<Selectable>(false);
        }
        if (items.Length > 0)
        {
            SetSeleccion(items[indice]);
        }
    }

    private void OnEnable()
    {
        items = GetComponentsInChildren<Selectable>(false);
        if (items.Length > 0)
        {
            indice = Mathf.Clamp(indice, 0, items.Length - 1);
            SetSeleccion(items[indice]);
        }
    }

    private void Update()
    {
        if (items.Length == 0) return;

        // navegación arriba/abajo con teclas configuradas
        if (Input.GetKeyDown(teclaArriba)) Mover(-1);
        if (Input.GetKeyDown(teclaAbajo)) Mover(+1);

        var actual = items[indice];
        // izquierda/derecha y sliders con repetición
        HandleHorizontalAndSliders(actual);

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

        if (usarMouse)
        {
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
            Vector2 size = rt.rect.size;
            if (pointerAdaptToItem)
            {
                size = new Vector2(Mathf.Max(0.01f, rt.rect.width + pointerPadding.x * 2f), Mathf.Max(0.01f, rt.rect.height + pointerPadding.y * 2f));
                p.sizeDelta = size;
            }
            else if (pointerAdaptToText)
            {
                var tmp = s.GetComponentInChildren<TextMeshProUGUI>(true);
                if (tmp != null)
                {
                    float w = tmp.preferredWidth + pointerPadding.x * 2f;
                    float h = tmp.preferredHeight + pointerPadding.y * 2f;
                    p.sizeDelta = new Vector2(w, h);
                }
                else
                {
                    var txt = s.GetComponentInChildren<Text>(true);
                    if (txt != null)
                    {
                        float w = txt.preferredWidth + pointerPadding.x * 2f;
                        float h = txt.preferredHeight + pointerPadding.y * 2f;
                        p.sizeDelta = new Vector2(w, h);
                    }
                }
            }
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
            if (es != null) es.SetSelectedGameObject(s.gameObject);
            return;
        }

        var tog = s as Toggle;
        if (tog != null)
        {
            tog.isOn = !tog.isOn;
            if (es != null) es.SetSelectedGameObject(s.gameObject);
            return;
        }

        var inp = s as InputField;
        if (inp != null)
        {
            inp.OnPointerClick(new PointerEventData(es));
            if (es != null) es.SetSelectedGameObject(s.gameObject);
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
        items = GetComponentsInChildren<Selectable>(false);
    }

    public void FocusFirst()
    {
        if (items == null || items.Length == 0) return;
        indice = 0;
        SetSeleccion(items[indice]);
    }
    private void HandleHorizontalAndSliders(Selectable actual)
    {
        bool leftPress = Input.GetKeyDown(teclaIzquierda);
        bool rightPress = Input.GetKeyDown(teclaDerecha);
        bool leftHeld = Input.GetKey(teclaIzquierda);
        bool rightHeld = Input.GetKey(teclaDerecha);

        if (leftPress)
        {
            StepHorizontal(actual, -1);
            nextLeftStepTime = Time.time + sliderRepeatDelay;
        }
        if (rightPress)
        {
            StepHorizontal(actual, +1);
            nextRightStepTime = Time.time + sliderRepeatDelay;
        }

        if (leftHeld && Time.time >= nextLeftStepTime)
        {
            StepHorizontal(actual, -1);
            nextLeftStepTime = Time.time + sliderRepeatInterval;
        }
        if (rightHeld && Time.time >= nextRightStepTime)
        {
            StepHorizontal(actual, +1);
            nextRightStepTime = Time.time + sliderRepeatInterval;
        }
    }

    private void StepHorizontal(Selectable s, int dir)
    {
        // Toggle
        var tog = s as Toggle;
        if (tog != null)
        {
            if (dir != 0) tog.isOn = !tog.isOn;
            return;
        }

        // Slider
        var sl = s.GetComponent<Slider>();
        if (sl != null)
        {
            float step = sl.wholeNumbers ? 1f : (sl.maxValue - sl.minValue) * 0.05f;
            sl.value = Mathf.Clamp(sl.value + step * dir, sl.minValue, sl.maxValue);
            return;
        }

        // TMP_Dropdown
        var dd = s.GetComponent<TMPro.TMP_Dropdown>();
        if (dd != null)
        {
            int v = dd.value + dir;
            if (v < 0) v = dd.options.Count - 1;
            if (v >= dd.options.Count) v = 0;
            dd.value = v;
            dd.RefreshShownValue();
            return;
        }
    }
}
