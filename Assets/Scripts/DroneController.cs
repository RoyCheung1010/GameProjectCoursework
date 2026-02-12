using UnityEngine;

public class DroneController : MonoBehaviour
{
    [Header("Drone Controls")]
    public float moveSpeed = 5f;

    [Header("Kinetic Shift Controls")]
    public float impulseRange = 3f;
    public float impulseForce = 500f;

    [Header("Movement Constraints")]
    public float maxAltitude = 1.9f; 

    [Header("Targeting Visuals")]
    public GameObject targetReticle;

    [Header("Camera Control")]
    public float lookSpeed = 2f;
    public float lookXLimit = 80f;
    private float rotationX = 0f;

    [Header("Visual Feedback")]
    public GameObject impulseParticlePrefab;

    private Rigidbody rb;
    private Vector3 moveInput;
    private bool targetFound;
    private RaycastHit lastHit;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("DroneController requires a Rigidbody on the same GameObject.");
            return;
        }

        rb.isKinematic = false;
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationY |
                         RigidbodyConstraints.FreezeRotationZ;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (rb == null) return;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");  
        float y = 0f;
        if (Input.GetKey(KeyCode.Space)) y = 1f;
        if (Input.GetKey(KeyCode.LeftControl)) y = -1f;

        moveInput = transform.right * x + transform.up * y + transform.forward * z;

        if (moveInput.sqrMagnitude > 1f)
            moveInput = moveInput.normalized;

        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);

        if (Camera.main != null)
            Camera.main.transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);

        float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
        transform.rotation *= Quaternion.Euler(0f, mouseX, 0f);

        Debug.DrawRay(transform.position, transform.forward * impulseRange, Color.red);

        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        targetFound = false;

        if (Physics.Raycast(ray, out hit, impulseRange))
        {
            lastHit = hit;
            KineticShiftController targetObject = hit.collider.GetComponent<KineticShiftController>();

            if (targetObject != null)
            {
                targetFound = true;

                if (Input.GetKeyDown(KeyCode.T)) targetObject.DecreaseScale();
                if (Input.GetKeyDown(KeyCode.Y)) targetObject.IncreaseScale();

                if (Input.GetKeyDown(KeyCode.F))
                {
                    Rigidbody targetRb = targetObject.GetComponent<Rigidbody>();
                    if (targetRb != null)
                        ApplyKineticImpulse(targetRb, hit.point);
                }
            }
        }

        if (targetReticle != null)
        {
            if (targetFound)
            {
                targetReticle.SetActive(true);
                targetReticle.transform.position = lastHit.point;
                targetReticle.transform.rotation = Quaternion.LookRotation(lastHit.normal);
            }
            else
            {
                targetReticle.SetActive(false);
            }
        }

        if (transform.position.y > maxAltitude)
        {
            Vector3 clampedPosition = transform.position;
            clampedPosition.y = maxAltitude;
            transform.position = clampedPosition;

            if (rb.linearVelocity.y > 0)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            }
        }
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        rb.linearVelocity = moveInput * moveSpeed;
    }

    void ApplyKineticImpulse(Rigidbody targetRb, Vector3 hitPoint)
    {
        if (impulseParticlePrefab != null)
        {
            GameObject effect = Instantiate(impulseParticlePrefab, hitPoint, Quaternion.identity);
            Destroy(effect, 1f);
        }

        Vector3 direction = (hitPoint - transform.position).normalized;
        targetRb.AddForce(direction * impulseForce, ForceMode.Impulse);
    }
}