using UnityEngine;
using UnityEngine.InputSystem;

public class AttatchableCamera : MonoBehaviour
{
    public int MaxFollowDistance = 8;
    public const float LookSensitivity = 20f;

    [Tooltip("Whether or not to have this camera apply it's own rotation to the object it is attached to.")]
    public bool ForceRotationOnObject = true;
    public float objectScale = 1f;

    private GameObject attatchedObject;     //The object the camera is attatched to.
    private Vector2 newLookVector;
    private Vector2 oldLookVector;
    private Vector3 expectedCameraPosition;

    private float xRotation;
    private float yRotation;
    private float currentFollowDistance;
    private float newFollowDistance;

    public void Start()
    {
        attatchedObject = transform.parent.gameObject;
        newFollowDistance = currentFollowDistance = MaxFollowDistance;
        Cursor.lockState = CursorLockMode.Locked;
        newLookVector = attatchedObject.transform.eulerAngles;
    }

    public void FixedUpdate()
    {
        GetProperZoom();
        GetProperRotation();
        SetPosition();
    }

    public void LateUpdate()
    {
        transform.localPosition = expectedCameraPosition * objectScale;
    }

    /// <summary>
    /// Updates the position of the camera.
    /// </summary>
    public void SetPosition()
    {
        if (oldLookVector != newLookVector)
        {
            expectedCameraPosition = new Vector3(0f, 0f, -currentFollowDistance + 2f);
            Vector3 raycastPosition = attatchedObject.transform.position;
            Vector3 raycastDirection = transform.position - attatchedObject.transform.position;
            Ray ray = new Ray(raycastPosition + -raycastDirection.normalized, raycastDirection);
            float averageDistance = 0f;
            for (int i = 0; i < 2; i++)
            {
                if (Physics.Raycast(ray, out RaycastHit hitInfo, currentFollowDistance))
                {
                    if (hitInfo.distance <= 0f)
                        continue;

                    averageDistance += hitInfo.distance;
                }
            }
            if (averageDistance > 0f)
            {
                averageDistance /= 2f;
                expectedCameraPosition.z = -averageDistance + 2f;
            }
            oldLookVector = newLookVector;
        }
    }

    /// <summary>
    /// Lerps the zoom to whatever the current value is.
    /// </summary>
    public void GetProperZoom()
    {
        currentFollowDistance = Mathf.Lerp(currentFollowDistance, newFollowDistance, 0.96f);
    }

    /// <summary>
    /// Updates the rotation of the camrea with the new look vector obtained through inputs.
    /// </summary>
    public void GetProperRotation()
    {
        if (ForceRotationOnObject)
        {
            attatchedObject.transform.eulerAngles = newLookVector;
            transform.eulerAngles = attatchedObject.transform.eulerAngles;
        }
        else
        {
            transform.eulerAngles = newLookVector;
        }
    }

    /// <summary>
    /// Updates the rotation of the camera based on the new input coordinates.
    /// </summary>
    /// <param name="vector">The 2D mouse vector to use.</param>
    /// <returns>A new 3D rotation based on the input vector.</returns>
    public Vector3 GetNewRotation(Vector2 vector)
    {
        float mouseX = vector.x * LookSensitivity * Time.deltaTime;
        float mouseY = vector.y * LookSensitivity * Time.deltaTime;

        yRotation += mouseX;
        xRotation += -mouseY;
        xRotation = Mathf.Clamp(xRotation, -90, 90);

        return new Vector3(xRotation, yRotation, 0.0f);
    }

    //Called by the input thing
    public void OnMousePosX(InputValue value)
    {
        float deltaX = value.Get<float>();
        Vector3 newLookAngle = GetNewRotation(new Vector2(deltaX, 0f));
        newLookVector = newLookAngle;
    }

    //Called by the input thing
    public void OnMousePosY(InputValue value)
    {
        float deltaY = value.Get<float>();
        Vector3 newLookAngle = GetNewRotation(new Vector2(0f, deltaY));
        newLookVector = newLookAngle;
    }

    //Called by the input thing
    public void OnScroll(InputValue value)
    {
        Vector2 scrollDelta = value.Get<Vector2>();

        newFollowDistance += -scrollDelta.y / 200f;
        newFollowDistance = Mathf.Clamp(newFollowDistance, MaxFollowDistance / 2, MaxFollowDistance);
    }
}
