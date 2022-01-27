using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class BallHandler : MonoBehaviour
{
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private Rigidbody2D pivot;
    [SerializeField] private float detatchDelay = 1f;
    [SerializeField] private float respawnDelay = 1f;
    [SerializeField] private float touchSize = 1f;

    private Rigidbody2D currentBallRigidBody;
    private SpringJoint2D currentBallSprintJoint;

    private Camera mainCamera;
    private bool isDragging;
    private bool isGrabbing;
    private float currentMass = 1f;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        SpawnNewBall();
    }

    // Update is called once per frame
    void Update()
    {
        if (currentBallRigidBody == null) { return; }

        if (!Touchscreen.current.primaryTouch.press.isPressed)
        {
            if (isDragging)
            {
                LaunchBall();
            }
            return;
        }
        Vector2 touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(touchPosition);

        switch (Touchscreen.current.primaryTouch.phase.ReadValue())
        {
            case UnityEngine.InputSystem.TouchPhase.Began:
                if (Mathf.Abs(worldPosition.x - pivot.position.x) < touchSize && Mathf.Abs(worldPosition.y - pivot.position.y) < touchSize)
                {
                    isGrabbing = true;
                    isDragging = true;
                }
                break;
            case UnityEngine.InputSystem.TouchPhase.Moved:
                if (isGrabbing)
                {
                    if (worldPosition.x > 0f)
                    {
                        worldPosition.x = 0f;
                    }
                    currentBallRigidBody.position = worldPosition;
                    currentBallRigidBody.isKinematic = true;
                }
                break;
        }
    }

    private void LaunchBall()
    {
        currentBallRigidBody.isKinematic = false;
        currentBallRigidBody = null;

        Invoke(nameof(DetachBall), detatchDelay);
        Invoke(nameof(SpawnNewBall), respawnDelay);
    }

    private void DetachBall()
    {
        currentBallSprintJoint.enabled = false;
        currentBallSprintJoint = null;
    }

    private void SpawnNewBall()
    {
        GameObject ballInstance = Instantiate(ballPrefab, pivot.position, Quaternion.identity);
        currentBallRigidBody = ballInstance.GetComponent<Rigidbody2D>();
        currentBallRigidBody.mass = currentMass;
        currentMass *= 1.01f;
        currentBallSprintJoint = ballInstance.GetComponent<SpringJoint2D>();

        currentBallSprintJoint.connectedBody = pivot;
        isDragging = false;
        isGrabbing = false;
    }

    public void OnClickedRestart()
    {
        SceneManager.LoadScene(0);
    }
}
