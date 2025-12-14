using UnityEngine;
using System.Collections;

public class TrueFinalBossZoneManager : MonoBehaviour
{
    [System.Serializable]
    public class ZoneEntry
    {
        public string zoneName;
        public GameObject tilemap;
        public GameObject background;
        public ParticleSystem particles;
        public int vignettePresetIndex;
        public AudioClip zoneSfx;
    }

    [SerializeField] private ZoneEntry[] zones;
    [SerializeField] private TrueFinalBossVisualEffects vfx;
    [SerializeField] private TrueFinalBossCameraZoneController cameraController;
    [SerializeField] private bool affectCameraOnZoneChange = false;
    [SerializeField] private bool useFirstEncounterTransition = true;
    [SerializeField] private float transitionFadeOutWait = 0.2f;
    [SerializeField] private bool lockPlayerDuringTransition = true;
    [SerializeField] private bool disableZoneChanges = true;
    public void SetZoneChangesDisabled(bool value) { disableZoneChanges = value; }

    private ZoneEntry current;

    public void ActivateZone(string name)
    {
        if (disableZoneChanges) return;
        var entry = GetZone(name);
        if (entry == null) return;
        if (useFirstEncounterTransition)
        {
            StartCoroutine(ZoneTransitionRoutine(entry));
            return;
        }
        ApplyZone(entry);
    }

    private void ApplyZone(ZoneEntry entry)
    {
        if (disableZoneChanges) return;
        if (zones != null)
        {
            for (int i = 0; i < zones.Length; i++)
            {
                var z = zones[i];
                if (z == null) continue;
                bool isTarget = z == entry;
                if (z.tilemap != null) z.tilemap.SetActive(isTarget);
                if (z.background != null) z.background.SetActive(isTarget);
                if (z.particles != null)
                {
                    if (isTarget) z.particles.Play();
                    else z.particles.Stop(true, UnityEngine.ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }
        }
        current = entry;
        if (vfx != null) vfx.PlayPreset(current.vignettePresetIndex);
        if (affectCameraOnZoneChange && cameraController != null) cameraController.ApplyCameraZone(entry.zoneName);
        if (current.zoneSfx != null && AudioManager.Instance != null) AudioManager.Instance.PlaySFX(current.zoneSfx, 1f, 1f);
    }

    public void DeactivateCurrent()
    {
        if (disableZoneChanges) return;
        if (current == null) return;
        if (current.tilemap != null) current.tilemap.SetActive(false);
        if (current.background != null) current.background.SetActive(false);
        if (current.particles != null) current.particles.Stop();
        current = null;
    }

    public ZoneEntry GetZone(string name)
    {
        if (zones == null) return null;
        for (int i = 0; i < zones.Length; i++)
        {
            var z = zones[i];
            if (z != null && z.zoneName == name) return z;
        }
        return null;
    }

    private IEnumerator ZoneTransitionRoutine(ZoneEntry entry)
    {
        PlayerMovement pm = null;
        if (lockPlayerDuringTransition)
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            pm = playerObj != null ? playerObj.GetComponent<PlayerMovement>() : null;
            if (pm != null) pm.SetControlsEnabled(false);
        }
        if (FadeController.Instance != null)
        {
            FadeController.Instance.ActivarFadeOut();
            if (transitionFadeOutWait > 0f)
                yield return new WaitForSecondsRealtime(transitionFadeOutWait);
        }
        ApplyZone(entry);
        if (FadeController.Instance != null)
        {
            FadeController.Instance.ActivarFadeIn();
        }
        if (pm != null) pm.SetControlsEnabled(true);
    }
}
