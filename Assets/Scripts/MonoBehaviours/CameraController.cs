using System;
using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private float fieldOfViewMin;
    [SerializeField] private float fieldOfViewMax;

    private float targetFieldOfView;

    private void Awake()
    {
        targetFieldOfView = cinemachineCamera.Lens.FieldOfView;
    }

    private void Update()
    {
        Vector3 moveDir = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
        {
            moveDir.z = +1f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            moveDir.z = -1f;
        }
        if (Input.GetKey(KeyCode.A))
        {
            moveDir.x = -1f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveDir.x = +1f;
        }

        Transform cameraTransform = Camera.main.transform;
        moveDir = cameraTransform.forward * moveDir.z + cameraTransform.right * moveDir.x;
        moveDir.y = 0;
        moveDir.Normalize();

        float moveSpeed = 30f;
        transform.position += moveDir * Time.deltaTime * moveSpeed;

        float rotationAmount = 0f;
        if (Input.GetKey(KeyCode.Q))
        {
            rotationAmount = +1f;
        }

        if (Input.GetKey(KeyCode.E))
        {
            rotationAmount = -1f;
        }

        float rotationSpeed = 200f;
        transform.eulerAngles += new Vector3(0f, rotationAmount * Time.deltaTime * rotationSpeed, 0f);

        float zoomAmount = 4f;
        if (Input.mouseScrollDelta.y > 0)
        {
            targetFieldOfView -= zoomAmount;
        }
        if (Input.mouseScrollDelta.y < 0)
        {
            targetFieldOfView += zoomAmount;
        }

        targetFieldOfView = Mathf.Clamp(targetFieldOfView, fieldOfViewMin, fieldOfViewMax);

        float zoomSpeed = 10f;
        cinemachineCamera.Lens.FieldOfView = Mathf.Lerp(cinemachineCamera.Lens.FieldOfView, targetFieldOfView,
            Time.deltaTime * zoomSpeed);
    }
}