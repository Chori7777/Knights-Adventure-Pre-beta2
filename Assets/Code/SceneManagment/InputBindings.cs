using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputBindings : MonoBehaviour
{
    public enum GameAction { MoveLeft, MoveRight, Jump, UseItem, Action1Attack, Action2Shield }

    private static KeyCode[] primarios = new KeyCode[6];
    private static KeyCode[] secundarios = new KeyCode[6];
    private static bool inicializado;


    private bool escuchando;
    private GameAction accionEscuchar;
    private int slotEscuchar;

    private void Awake()
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.sendNavigationEvents = false;
        }
        InicializarSiEsNecesario();
    }

    private void Update()
    {
        // ✅ ARREGLO: Si está escuchando, capturar tecla PRIMERO
        if (escuchando)
        {
            CapturarNuevaTecla();
            return; // ✅ CRÍTICO: No procesar resto del input
        }

        // Interacción UI: solo mouse. Sin atajos de teclado.
        var es = EventSystem.current;
        if (es == null) return;
    }

    public void StartRebind(string accion, int slot)
    {
        InicializarSiEsNecesario();
        accionEscuchar = ParseAction(accion);
        slotEscuchar = Mathf.Clamp(slot, 0, 1);
        escuchando = true;

        Debug.Log($"[InputBindings] Escuchando para {accion} slot {slot}");
    }

    public void CancelRebind()
    {
        escuchando = false;
        Debug.Log("[InputBindings] Rebind cancelado");
    }

    private void CapturarNuevaTecla()
    {
        // ✅ Ignorar teclas de sistema
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelRebind();
            return;
        }

        // ✅ Capturar cualquier tecla presionada
        foreach (KeyCode k in System.Enum.GetValues(typeof(KeyCode)))
        {
            // ✅ Ignorar Mouse y teclas problemáticas
            if (k >= KeyCode.Mouse0 && k <= KeyCode.Mouse6) continue;
            if (k == KeyCode.None) continue;

            if (Input.GetKeyDown(k))
            {
                Debug.Log($"[InputBindings] Tecla capturada: {k}");

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
        primarios[(int)GameAction.UseItem] = KeyCode.R;
        secundarios[(int)GameAction.UseItem] = KeyCode.None;
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

            if (!string.IsNullOrEmpty(p))
            {
                try
                {
                    primarios[i] = (KeyCode)System.Enum.Parse(typeof(KeyCode), p);
                }
                catch
                {
                    Debug.LogWarning($"[InputBindings] Error parseando binding primario {i}: {p}");
                }
            }

            if (!string.IsNullOrEmpty(s))
            {
                try
                {
                    secundarios[i] = (KeyCode)System.Enum.Parse(typeof(KeyCode), s);
                }
                catch
                {
                    Debug.LogWarning($"[InputBindings] Error parseando binding secundario {i}: {s}");
                }
            }
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
        Debug.Log("[InputBindings] Bindings guardados");
    }

    private void AsignarTecla(GameAction accion, int slot, KeyCode tecla)
    {
        int idx = (int)accion;
        if (slot == 0)
            primarios[idx] = tecla;
        else
            secundarios[idx] = tecla;

        Debug.Log($"[InputBindings] Asignado {tecla} a {accion} slot {slot}");
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
        Debug.Log("[InputBindings] Inicializado");
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
        Debug.Log("[InputBindings] Bindings reseteados");
    }

    // ✅ Getter público para debugging
    public bool IsListening => escuchando;
}
