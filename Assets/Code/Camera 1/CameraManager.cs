using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;

    private Camera mainCamera;
    public float cameraSpeed = 5f;

    public Transform[] checkpoints;
    [SerializeField] private int startingCheckpoint = 1;
    private int currentCameraIndex = 0;
    private bool isMoving = false;
    private Vector3 targetPosition;
    private float cameraZ = -10f;
    public float cameraSize = 5f;

    // 🔹 Variables nuevas para zoom suave
    private float targetCameraSize;
    public float zoomSpeed = 2f; // Qué tan rápido hace el zoom

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        mainCamera.orthographicSize = cameraSize;
        targetCameraSize = cameraSize; // Iniciar sincronizado

        if (checkpoints == null || checkpoints.Length == 0)
        {
            GameObject[] checkpointObjects = GameObject.FindGameObjectsWithTag("CameraCheckpoint");
            checkpoints = new Transform[checkpointObjects.Length];
            for (int i = 0; i < checkpointObjects.Length; i++)
                checkpoints[i] = checkpointObjects[i].transform;
        }

        if (checkpoints.Length > 0)
        {
            currentCameraIndex = startingCheckpoint;
            Vector3 startPos = checkpoints[currentCameraIndex].position;
            startPos.z = cameraZ;
            transform.position = startPos;
            targetPosition = startPos;
            isMoving = false;

            Debug.Log("Cámara inicializada en checkpoint " + currentCameraIndex + " con " + checkpoints.Length + " checkpoints totales");
        }
    }

    void Update()
    {
        if (isMoving) MoveCamera();

        // 🔹 Suaviza el zoom en cada frame
        if (Mathf.Abs(mainCamera.orthographicSize - targetCameraSize) > 0.01f)
        {
            mainCamera.orthographicSize = Mathf.Lerp(
                mainCamera.orthographicSize,
                targetCameraSize,
                zoomSpeed * Time.deltaTime
            );
        }
    }

    private void MoveCamera()
    {
        transform.position = Vector3.Lerp(transform.position, targetPosition, cameraSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            transform.position = targetPosition;
            isMoving = false;
        }
    }

    public void AvanzarCamara()
    {
        if (currentCameraIndex < checkpoints.Length - 1)
        {
            currentCameraIndex++;
            Vector3 newPos = checkpoints[currentCameraIndex].position;
            newPos.z = cameraZ;
            targetPosition = newPos;
            isMoving = true;
            Debug.Log("Cámara avanzó al checkpoint " + currentCameraIndex);
        }
    }

    public void RetrocederCamara()
    {
        if (currentCameraIndex > 0)
        {
            currentCameraIndex--;
            Vector3 newPos = checkpoints[currentCameraIndex].position;
            newPos.z = cameraZ;
            targetPosition = newPos;
            isMoving = true;
            Debug.Log("Cámara retrocedió al checkpoint " + currentCameraIndex);
        }
    }

    public void IrAlCheckpoint(int index)
    {
        if (index >= 0 && index < checkpoints.Length)
        {
            currentCameraIndex = index;
            Vector3 newPos = checkpoints[index].position;
            newPos.z = cameraZ;
            targetPosition = newPos;
            isMoving = true;
            Debug.Log("Cámara se movió al checkpoint " + index);
        }
    }

    public void AgregarCheckpoint(Vector3 posicion)
    {
        System.Array.Resize(ref checkpoints, checkpoints.Length + 1);
        GameObject newCheckpoint = new GameObject("Checkpoint_" + checkpoints.Length);
        newCheckpoint.transform.position = posicion;
        checkpoints[checkpoints.Length - 1] = newCheckpoint.transform;
    }

    public void ResetearCamara()
    {
        currentCameraIndex = startingCheckpoint;
        if (checkpoints.Length > 0)
        {
            Vector3 startPos = checkpoints[startingCheckpoint].position;
            startPos.z = cameraZ;
            transform.position = startPos;
            targetPosition = startPos;
            isMoving = false;
        }
    }

    // 🔹 Nuevo sistema: en lugar de cambiar el tamaño directamente,
    // cambiamos el “objetivo” y el zoom se ajusta suavemente en Update()
    public void SetCameraSize(float newSize)
    {
        targetCameraSize = Mathf.Max(1f, newSize);
    }

    public float GetCameraSize()
    {
        return mainCamera.orthographicSize;
    }

    public void AlejarCamara(float cantidad = 2f)
    {
        SetCameraSize(targetCameraSize + cantidad);
    }

    public void AcercarCamara(float cantidad = 2f)
    {
        SetCameraSize(targetCameraSize - cantidad);
    }

    public int GetCurrentCheckpoint()
    {
        return currentCameraIndex;
    }

}
