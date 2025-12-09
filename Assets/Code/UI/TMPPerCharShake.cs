using UnityEngine;
using TMPro;

public class TMPPerCharShake : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tmp;
    [SerializeField] private float amplitude = 2f;
    [SerializeField] private float frequency = 12f;
    [SerializeField] private bool useUnscaledTime = true;

    private void Awake()
    {
        if (tmp == null) tmp = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        if (tmp == null) return;
        tmp.ForceMeshUpdate();
        var textInfo = tmp.textInfo;
        float t = useUnscaledTime ? Time.unscaledTime : Time.time;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            var ch = textInfo.characterInfo[i];
            if (!ch.isVisible) continue;
            int m = ch.materialReferenceIndex;
            int v = ch.vertexIndex;
            var verts = textInfo.meshInfo[m].vertices;

            float phaseX = (t + i * 0.13f) * frequency;
            float phaseY = (t + i * 0.37f) * frequency;
            Vector3 offset = new Vector3(Mathf.Sin(phaseX), Mathf.Cos(phaseY), 0f) * amplitude;

            verts[v + 0] += offset;
            verts[v + 1] += offset;
            verts[v + 2] += offset;
            verts[v + 3] += offset;
        }

        for (int m = 0; m < textInfo.meshInfo.Length; m++)
        {
            var mi = textInfo.meshInfo[m];
            mi.mesh.vertices = mi.vertices;
            tmp.UpdateGeometry(mi.mesh, m);
        }
    }
}
