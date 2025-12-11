using UnityEngine;

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

    private ZoneEntry current;

    public void ActivateZone(string name)
    {
        var entry = GetZone(name);
        if (entry == null) return;
        DeactivateCurrent();
        current = entry;
        if (current.tilemap != null) current.tilemap.SetActive(true);
        if (current.background != null) current.background.SetActive(true);
        if (current.particles != null) current.particles.Play();
        if (vfx != null) vfx.PlayPreset(current.vignettePresetIndex);
        if (cameraController != null) cameraController.ApplyCameraZone(name);
        if (current.zoneSfx != null && AudioManager.Instance != null) AudioManager.Instance.PlaySFX(current.zoneSfx, 1f, 1f);
    }

    public void DeactivateCurrent()
    {
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
}

