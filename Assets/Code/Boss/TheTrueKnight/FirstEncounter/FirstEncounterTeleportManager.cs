using UnityEngine;
using System.Collections;

public class FirstEncounterTeleportManager : MonoBehaviour
{
    [Header("Sonido de Teleport")]
    [SerializeField] private AudioClip sonidoTeleport;
    [Header("Fade")]
    [SerializeField] private bool useInstantFade = false;

    public void TeleportTo(Transform playerSpawn, Transform cameraTarget)
    {
        StartCoroutine(TeleportRoutine(playerSpawn, cameraTarget));
    }

    private IEnumerator TeleportRoutine(Transform playerSpawn, Transform cameraTarget)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) yield break;
        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        if (pm != null)
        {
            pm.canMove = false;
            pm.canJump = false;
            pm.canAttack = false;
            pm.canDash = false;
            pm.canWallCling = false;
            pm.canBlock = false;
        }

        if (!useInstantFade && FadeController.Instance != null)
        {
            FadeController.Instance.ActivarFadeOut();
        }

        if (AudioManager.Instance != null && sonidoTeleport != null)
        {
            AudioManager.Instance.PlaySFX(sonidoTeleport, 0.8f);
        }

        float outWait = useInstantFade ? 0f : 0.2f;
        yield return new WaitForSeconds(outWait);

        if (playerSpawn != null)
        {
            player.transform.position = playerSpawn.position;
        }

        Camera cam = Camera.main;
        if (cam != null && cameraTarget != null)
        {
            cam.transform.position = cameraTarget.position;
        }

        float inWait = useInstantFade ? 0f : 0.1f;
        yield return new WaitForSeconds(inWait);

        if (!useInstantFade && FadeController.Instance != null)
        {
            FadeController.Instance.ActivarFadeIn();
        }

        if (pm != null)
        {
            pm.canMove = true;
            pm.canJump = true;
            pm.canAttack = true;
            pm.canDash = true;
            pm.canWallCling = true;
            pm.canBlock = true;
        }
    }

    public void TeleportInstant(Transform playerSpawn, Transform cameraTarget, float holdSeconds)
    {
        StartCoroutine(TeleportInstantRoutine(playerSpawn, cameraTarget, holdSeconds));
    }

    public void TeleportRaw(Transform playerSpawn, Transform cameraTarget)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        if (pm != null)
        {
            pm.canMove = false;
            pm.canJump = false;
            pm.canAttack = false;
            pm.canDash = false;
            pm.canWallCling = false;
            pm.canBlock = false;
        }
        if (playerSpawn != null)
        {
            player.transform.position = playerSpawn.position;
        }
        Camera cam = Camera.main;
        if (cam != null && cameraTarget != null)
        {
            cam.transform.position = cameraTarget.position;
        }
        if (pm != null)
        {
            pm.canMove = true;
            pm.canJump = true;
            pm.canAttack = true;
            pm.canDash = true;
            pm.canWallCling = true;
            pm.canBlock = true;
        }
    }

    private IEnumerator TeleportInstantRoutine(Transform playerSpawn, Transform cameraTarget, float holdSeconds)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) yield break;

        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        if (pm != null)
        {
            pm.canMove = false;
            pm.canJump = false;
            pm.canAttack = false;
            pm.canDash = false;
            pm.canWallCling = false;
            pm.canBlock = false;
        }

        if (!useInstantFade && FadeController.Instance != null)
        {
            FadeController.Instance.ActivarFadeOut();
        }

        if (playerSpawn != null)
        {
            player.transform.position = playerSpawn.position;
        }

        Camera cam = Camera.main;
        if (cam != null && cameraTarget != null)
        {
            cam.transform.position = cameraTarget.position;
        }

        float hold = useInstantFade ? 0f : holdSeconds;
        yield return new WaitForSeconds(hold);

        if (!useInstantFade && FadeController.Instance != null)
        {
            FadeController.Instance.ActivarFadeIn();
        }

        if (pm != null)
        {
            pm.canMove = true;
            pm.canJump = true;
            pm.canAttack = true;
            pm.canDash = true;
            pm.canWallCling = true;
            pm.canBlock = true;
        }
    }
}
