using UnityEngine;

public class PressurePlateController : MonoBehaviour
{
    [Header("Activation Settings")]
    public float requiredMassThreshold = 80f; 
    
    [Header("Puzzle Management")]
    public AI_Orchestrator orchestrator; 
    public bool IsActive => isActivated; 
    
    [Header("Level 3 Override")]
    public bool ignoreMassRequirement = false;
    
    private bool isActivated = false;

    [Header("Level 1 Door Control")]
    public GameObject targetDoor; 
    public float doorMoveDistance = 2.5f; 
    public float doorSpeed = 5f; 
    private Vector3 doorClosedPosition; 
    private Vector3 doorOpenPosition;


    void Start()
    {
        GetComponent<Collider>().isTrigger = true; 

        if (targetDoor != null)
        {
            doorClosedPosition = targetDoor.transform.position;
            doorOpenPosition = doorClosedPosition + Vector3.up * doorMoveDistance; 
        }
    }
    
    void Update()
    {
        if (targetDoor != null && targetDoor.GetComponent<DoorMovementController>() == null)
        {
            Vector3 targetPos = IsActive ? doorOpenPosition : doorClosedPosition;
            targetDoor.transform.position = Vector3.Lerp(targetDoor.transform.position, targetPos, Time.deltaTime * doorSpeed);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("WeightedObject"))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                bool meetsMass = rb.mass >= requiredMassThreshold;
                bool shouldActivate = meetsMass || ignoreMassRequirement;

                if (shouldActivate && !isActivated)
                {
                    ActivatePlate(true);
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("WeightedObject"))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            
            bool meetsMass = rb.mass >= requiredMassThreshold;
            bool shouldActivate = meetsMass || ignoreMassRequirement;

            if (isActivated && !shouldActivate)
            {
                ActivatePlate(false);
            }
            else if (!isActivated && shouldActivate)
            {
                ActivatePlate(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isActivated && other.gameObject.layer == LayerMask.NameToLayer("WeightedObject"))
        {
            ActivatePlate(false);
        }
    }

    void ActivatePlate(bool activate)
    {
        isActivated = activate;
        
        if (orchestrator != null)
        {
            orchestrator.CheckAllPlatesStatus();
        }

        Debug.Log("Plate Activation Status: " + isActivated);
    }
}