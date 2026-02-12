using UnityEngine;

public class DoorMovementController : MonoBehaviour
{
    private bool shouldBeOpen = false;
    
    [Header("Movement Settings")]
    public float doorMoveDistance = 2.5f; 
    public float doorSpeed = 5f;
    
    private Vector3 doorClosedPosition;
    private Vector3 doorOpenPosition;
    
    void Start()
    {
        doorClosedPosition = transform.position;
        doorOpenPosition = doorClosedPosition + Vector3.up * doorMoveDistance; 
    }

    void Update()
    {
        Vector3 targetPos = shouldBeOpen ? doorOpenPosition : doorClosedPosition;
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * doorSpeed);
    }
    public void SetDoorState(bool open)
    {
        shouldBeOpen = open;
    }
}