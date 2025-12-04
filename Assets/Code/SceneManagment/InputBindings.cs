using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputBindings : MonoBehaviour
{
    public enum GameAction { MoveLeft, MoveRight, Jump, UseItem, Action1Attack, Action2Shield }

    private static KeyCode[] primarios = new KeyCode[6];
    private static KeyCode[] secundarios = new KeyCode[6];
    private static bool inicializado;

    [SerializeField] private KeyCode submitKey = KeyCode.Return;
    [SerializeField] private KeyCode submitAltKey = KeyCode.Z;
    [SerializeField] private KeyCode cancelKey = KeyCode.Escape;

    private bool escuchando;
    private GameAction accionEscuchar;
    private int slotEscuchar;

    private void Awake()
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.sendNavigationEvents = true;
        }
        InicializarSiEsNecesario();
    }

    private void Update()
    {
        if (escuchando)
        {
            CapturarNuevaTecla();
            return;
        }

        var es = EventSystem.current;
        if (es == null) return;

        var seleccionado = es.currentSelectedGameObject;

        if (Input.GetKeyDown(submitKey) || Input.GetKeyDown(submitAltKey))
        {
            if (seleccionado != null)
            {
                var selectable = seleccionado.GetComponent<Selectable>();
                if (selectable != null)
                {
                    var btn = selectable as Button;
                    if (btn != null)
                    {
                        btn.onClick.Invoke();
                        return;
                    }

                    var tog = selectable as Toggle;
                    if (tog != null)
                    {
                        tog.isOn = !tog.isOn;
                        return;
                    }

                    var inp = selectable as InputField;
                    if (inp != null)
                    {
                        inp.OnPointerClick(new PointerEventData(es));
                        return;
                    }
                }
            }
        }

        if (Input.GetKeyDown(cancelKey))
        {
            var overlay = Object.FindFirstObjectByType<MenuOverlayManager>();
            if (overlay != null)
            {
                overlay.CloseOptions();
            }
        }
    }

    public void StartRebind(string accion, int slot)
    {
        InicializarSiEsNecesario();
        accionEscuchar = ParseAction(accion);
        slotEscuchar = Mathf.Clamp(slot, 0, 1);
        escuchando = true;
    }

    public void CancelRebind()
    {
        escuchando = false;
    }

    private void CapturarNuevaTecla()
    {
        foreach (KeyCode k in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(k))
            {
                AsignarTecla(accionEscuchar, slotEscuchar, k);
                GuardarBindingsPersistentes();
                escuchando = false;
                break;
            }
        }
    }

    private static void CargarBindingsPorDefecto()
    {
        primarios[(int)GameAction.MoveLeft] = KeyCode.A;
        secundarios[(int)GameAction.MoveLeft] = KeyCode.LeftArrow;
        primarios[(int)GameAction.MoveRight] = KeyCode.D;
        secundarios[(int)GameAction.MoveRight] = KeyCode.RightArrow;
        primarios[(int)GameAction.Jump] = KeyCode.Space;
        secundarios[(int)GameAction.Jump] = KeyCode.UpArrow;
        primarios[(int)GameAction.UseItem] = KeyCode.E;
        secundarios[(int)GameAction.UseItem] = KeyCode.R;
        primarios[(int)GameAction.Action1Attack] = KeyCode.Z;
        secundarios[(int)GameAction.Action1Attack] = KeyCode.None;
        primarios[(int)GameAction.Action2Shield] = KeyCode.X;
        secundarios[(int)GameAction.Action2Shield] = KeyCode.None;
    }

    private static void CargarBindingsPersistentes()
    {
        for (int i = 0; i < primarios.Length; i++)
        {
            string p = PlayerPrefs.GetString("bind_p_" + i, "");
            string s = PlayerPrefs.GetString("bind_s_" + i, "");
            if (!string.IsNullOrEmpty(p)) primarios[i] = (KeyCode)System.Enum.Parse(typeof(KeyCode), p);
            if (!string.IsNullOrEmpty(s)) secundarios[i] = (KeyCode)System.Enum.Parse(typeof(KeyCode), s);
        }
    }

    private static void GuardarBindingsPersistentes()
    {
        for (int i = 0; i < primarios.Length; i++)
        {
            PlayerPrefs.SetString("bind_p_" + i, primarios[i].ToString());
            PlayerPrefs.SetString("bind_s_" + i, secundarios[i].ToString());
        }
        PlayerPrefs.Save();
    }

    private void AsignarTecla(GameAction accion, int slot, KeyCode tecla)
    {
        int idx = (int)accion;
        if (slot == 0) primarios[idx] = tecla; else secundarios[idx] = tecla;
    }

    public static KeyCode GetPrimary(GameAction accion)
    {
        InicializarSiEsNecesario();
        return primarios[(int)accion];
    }

    public static KeyCode GetSecondary(GameAction accion)
    {
        InicializarSiEsNecesario();
        return secundarios[(int)accion];
    }

    private GameAction ParseAction(string accion)
    {
        switch (accion)
        {
            case "MoveLeft": return GameAction.MoveLeft;
            case "MoveRight": return GameAction.MoveRight;
            case "Jump": return GameAction.Jump;
            case "UseItem": return GameAction.UseItem;
            case "Action1Attack": return GameAction.Action1Attack;
            case "Action2Shield": return GameAction.Action2Shield;
            default: return GameAction.Jump;
        }
    }

    private static void InicializarSiEsNecesario()
    {
        if (inicializado) return;
        CargarBindingsPorDefecto();
        CargarBindingsPersistentes();
        inicializado = true;
    }

    public static bool Get(GameAction accion)
    {
        InicializarSiEsNecesario();
        int i = (int)accion;
        KeyCode p = primarios[i];
        KeyCode s = secundarios[i];
        bool a = p != KeyCode.None && Input.GetKey(p);
        bool b = s != KeyCode.None && Input.GetKey(s);
        return a || b;
    }

    public static bool GetDown(GameAction accion)
    {
        InicializarSiEsNecesario();
        int i = (int)accion;
        KeyCode p = primarios[i];
        KeyCode s = secundarios[i];
        bool a = p != KeyCode.None && Input.GetKeyDown(p);
        bool b = s != KeyCode.None && Input.GetKeyDown(s);
        return a || b;
    }

    public static void SaveAll()
    {
        InicializarSiEsNecesario();
        GuardarBindingsPersistentes();
    }

    public static void ResetToDefaults()
    {
        CargarBindingsPorDefecto();
        GuardarBindingsPersistentes();
    }
}
