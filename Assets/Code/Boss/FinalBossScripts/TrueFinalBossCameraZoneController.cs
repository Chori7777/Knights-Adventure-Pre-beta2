using UnityEngine;

public class TrueFinalBossCameraZoneController : MonoBehaviour
{
    [System.Serializable]
    public class ZoneCamera
    {
        public string zoneName;
        public int checkpointIndex;
        public float targetCameraSize = 5f;
    }

    [SerializeField] private ZoneCamera[] cameras;

    public void ApplyCameraZone(string name)
    {
        var c = GetCamera(name);
        if (c == null) return;
        var cm = CameraManager.instance;
        if (cm != null)
        {
            cm.SetCameraSize(c.targetCameraSize);
        }
    }

    private ZoneCamera GetCamera(string name)
    {
        if (cameras == null) return null;
        for (int i = 0; i < cameras.Length; i++)
        {
            var z = cameras[i];
            if (z != null && z.zoneName == name) return z;
        }
        return null;
    }
}

