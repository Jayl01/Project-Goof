using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;

public class AttatchableCamera : MonoBehaviour
{
    public int MaxFollowDistance = 8;
    public const float LookSensitivity = 20f;

    private Camera camera;
    private GameObject attatchedObject;     //The object the camera is attatched to.
    private BoxCollider cameraBoxCollider;
    private Vector2 expectedLookVector;
    private Vector2 oldLookVector;
    private Vector3 expectedCameraPosition;

    private float xRotationView;
    private float yRotationView;
    private float currentFollowDistance;
    private float expectedZoom;
    private bool issuedRaycastFix = false;
    private int triggerDetectionTimer = 0;

    public void Start()
    {
        camera = GetComponent<Camera>();
        attatchedObject = transform.parent.gameObject;
        currentFollowDistance = MaxFollowDistance;
        expectedZoom = MaxFollowDistance;
        Cursor.lockState = CursorLockMode.Locked;
        expectedLookVector = attatchedObject.transform.eulerAngles;
        cameraBoxCollider = GetComponent<BoxCollider>();
        cameraBoxCollider.center = new Vector3(0f, 0f, (expectedZoom / 2f) - 1f);
        cameraBoxCollider.size = new Vector3(0f, 0f, expectedZoom);
    }

    public void FixedUpdate()
    {
        if (triggerDetectionTimer > 0)
            triggerDetectionTimer--;
        //LimitDistance();
        GetProperZoom();
        GetProperRotation();
        SetPosition();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.None)
                Cursor.lockState = CursorLockMode.Locked;
            else
                Cursor.lockState = CursorLockMode.None;
        }
    }

    public void LateUpdate()
    {
        transform.localPosition = expectedCameraPosition;
    }

    public void OnTriggerStay(Collider other)
    {
        Vector3 raycastPosition = attatchedObject.transform.position;
        Ray ray = new Ray(raycastPosition, transform.position - attatchedObject.transform.position);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, currentFollowDistance))
        {
            expectedCameraPosition.z = -hitInfo.distance + 1f;
            cameraBoxCollider.center = new Vector3(0f, 0f, (expectedZoom / 2f) - 1f);
            cameraBoxCollider.size = new Vector3(0f, 0f, expectedZoom);
            issuedRaycastFix = true;
        }
        triggerDetectionTimer = 10;
    }


    public void SetPosition()
    {
        if (triggerDetectionTimer > 0)
            return;

        if (oldLookVector != expectedLookVector)
        {
            oldLookVector = expectedLookVector;
            expectedCameraPosition = new Vector3(0f, 0f, -currentFollowDistance);
            issuedRaycastFix = false;
        }
    }

    public void LimitDistance()
    {
        Vector3 currentPosition = transform.localPosition;
        if (Vector3.Distance(currentPosition, attatchedObject.transform.position) < currentFollowDistance)
        {
            Vector3 newPosition = currentPosition - attatchedObject.transform.position;
            newPosition.Normalize();
            expectedCameraPosition = newPosition * currentFollowDistance;
        }
    }

    public void GetProperZoom()
    {
        currentFollowDistance = Mathf.Lerp(currentFollowDistance, expectedZoom, 0.96f);
    }

    public void GetProperRotation()
    {
        attatchedObject.transform.eulerAngles = expectedLookVector;
        transform.eulerAngles = attatchedObject.transform.eulerAngles;
    }

    public Vector3 GetNewRotation(Vector2 vector)
    {
        float mouseX = vector.x * LookSensitivity * Time.deltaTime;
        float mouseY = vector.y * LookSensitivity * Time.deltaTime;

        yRotationView += mouseX;
        xRotationView += -mouseY;
        xRotationView = Mathf.Clamp(xRotationView, -90, 90);

        return new Vector3(xRotationView, yRotationView, 0.0f);
    }

    public void OnMousePosX(InputValue value)
    {
        float deltaX = value.Get<float>();
        Vector3 newLookAngle = GetNewRotation(new Vector2(deltaX, 0f));
        expectedLookVector = newLookAngle;
    }

    public void OnMousePosY(InputValue value)
    {
        float deltaY = value.Get<float>();
        Vector3 newLookAngle = GetNewRotation(new Vector2(0f, deltaY));
        expectedLookVector = newLookAngle;
    }

    public void OnScroll(InputValue value)
    {
        Vector2 scrollDelta = value.Get<Vector2>();

        expectedZoom += -scrollDelta.y / 200f;
        expectedZoom = Mathf.Clamp(expectedZoom, MaxFollowDistance / 2, MaxFollowDistance);
    }
}
