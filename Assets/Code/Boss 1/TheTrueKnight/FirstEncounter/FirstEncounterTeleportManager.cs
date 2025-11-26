using UnityEngine;
using System.Collections;

public class FirstEncounterTeleportManager : MonoBehaviour
{
    [Header("Sonido de Teleport")]
    [SerializeField] private AudioClip sonidoTeleport;

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

        if (FadeController.Instance != null)
        {
            FadeController.Instance.ActivarFadeOut();
        }

        if (AudioManager.Instance != null && sonidoTeleport != null)
        {
            AudioManager.Instance.PlaySFX(sonidoTeleport, 0.8f);
        }

        yield return new WaitForSeconds(0.2f);

        if (playerSpawn != null)
        {
            player.transform.position = playerSpawn.position;
        }

        Camera cam = Camera.main;
        if (cam != null && cameraTarget != null)
        {
            cam.transform.position = cameraTarget.position;
        }

        yield return new WaitForSeconds(0.1f);

        if (FadeController.Instance != null)
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

        if (FadeController.Instance != null)
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

        yield return new WaitForSeconds(holdSeconds);

        if (FadeController.Instance != null)
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
