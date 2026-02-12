using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class KineticShiftController : MonoBehaviour
{
    [Header("Scale Settings")]
    public float smallScale = 0.2f;
    public float largeScale = 3.0f;
    public float baseScale = 1.0f;
    private float currentScale;

    [Header("Mass & Physics Settings")]
    public float baseMass = 10.0f;
    public float smallMassMultiplier = 0.1f;
    public float largeMassMultiplier = 5.0f;
    public const float GROUND_LEVEL_Y = 0.5f;

    public PhysicsMaterial highFrictionMaterial;
    public PhysicsMaterial lowFrictionMaterial;

    private Rigidbody rb;
    private Collider objectCollider;
    private NavMeshObstacle obstacle;

    private Coroutine reenableCoroutine = null;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        objectCollider = GetComponent<Collider>();

        obstacle = GetComponent<NavMeshObstacle>();
        if (obstacle == null)
        {
            Debug.LogWarning("KineticShiftController: NavMeshObstacle not found on this object. It's optional but recommended for AI avoidance.");
        }

        rb.mass = baseMass;
        transform.localScale = Vector3.one * baseScale;
        currentScale = baseScale;

        if (obstacle != null)
        {
            obstacle.enabled = true;
        }

        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
        rb.interpolation = RigidbodyInterpolation.None;
    }

    public void DecreaseScale()
    {
        if (currentScale == largeScale) PerformScaleShift(baseScale);
        else if (currentScale == baseScale) PerformScaleShift(smallScale);
    }

    public void IncreaseScale()
    {
        if (currentScale == smallScale) PerformScaleShift(baseScale);
        else if (currentScale == baseScale) PerformScaleShift(largeScale);
    }

    public void ShiftToNormal() => PerformScaleShift(baseScale);

    public void PerformScaleShift(float targetScale)
    {
        if (rb == null || objectCollider == null)
        {
            Debug.LogWarning("KineticShiftController: Missing Rigidbody or Collider.");
            return;
        }

        bool prevCarving = false;
        if (obstacle != null)
        {
            prevCarving = obstacle.carving;
            obstacle.enabled = false;
            if (reenableCoroutine != null) StopCoroutine(reenableCoroutine);
        }

        rb.isKinematic = true;
        rb.Sleep();

        transform.localScale = Vector3.one * targetScale;

        Physics.SyncTransforms();
        float newMass = baseMass;
        if (Mathf.Approximately(targetScale, smallScale)) newMass = baseMass * smallMassMultiplier;
        else if (Mathf.Approximately(targetScale, largeScale)) newMass = baseMass * largeMassMultiplier;
        rb.mass = newMass;

        SwapFrictionMaterial(targetScale);

        SnapToSurfaceBelow();

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        if (obstacle != null)
        {
            bool carve = Mathf.Approximately(targetScale, largeScale);
            reenableCoroutine = StartCoroutine(ReEnableObstacleCarvingAfterDelay(0.05f, carve));
        }

        rb.isKinematic = false;
        rb.WakeUp();

        currentScale = targetScale;
    }

    private void SwapFrictionMaterial(float currentScale)
    {
        if (objectCollider == null) return;

        if (Mathf.Approximately(currentScale, largeScale) && highFrictionMaterial != null)
            objectCollider.material = highFrictionMaterial;
        else if (Mathf.Approximately(currentScale, smallScale) && lowFrictionMaterial != null)
            objectCollider.material = lowFrictionMaterial;
        else
            objectCollider.material = null;
    }

    private IEnumerator ReEnableObstacleCarvingAfterDelay(float delay, bool carve)
    {
        yield return new WaitForSeconds(delay);
        if (obstacle != null)
        {
            obstacle.enabled = true;
            obstacle.carving = carve;
        }
        reenableCoroutine = null;
    }

    private void SnapToSurfaceBelow()
    {
        const int samplesPerAxis = 3; 
        const float rayStartAbove = 1.0f; 
        const float maxRayDistance = 10f; 

        Physics.SyncTransforms();

        Bounds b = objectCollider.bounds;

        Vector3[] samplePoints = GetBaseSamplePoints(b, samplesPerAxis);

        bool foundAny = false;
        float bestSurfaceY = float.MinValue;
        Vector3 bestNormal = Vector3.up;
        Vector3 bestPoint = Vector3.zero;

        for (int i = 0; i < samplePoints.Length; i++)
        {
            Vector3 start = samplePoints[i] + Vector3.up * rayStartAbove;
            Ray ray = new Ray(start, Vector3.down);

            if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, ~0, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider != objectCollider)
                {
                    float surfaceY = hit.point.y;
                    if (!foundAny || surfaceY > bestSurfaceY)
                    {
                        foundAny = true;
                        bestSurfaceY = surfaceY;
                        bestNormal = hit.normal;
                        bestPoint = hit.point;
                    }
                }
            }

            Debug.DrawRay(start, Vector3.down * maxRayDistance, Color.cyan, 1.0f);
        }

        if (foundAny)
        {
            float halfHeight = b.extents.y;

            Vector3 targetPos = bestPoint + Vector3.up * halfHeight;

            transform.position = targetPos;

            Physics.SyncTransforms();

            if (IsOverlappingEnvironment())
            {
                float nudgeStep = 0.02f;
                const int maxNudges = 15;
                bool resolved = false;
                for (int i = 0; i < maxNudges; i++)
                {
                    transform.position += Vector3.up * nudgeStep;
                    Physics.SyncTransforms();
                    if (!IsOverlappingEnvironment()) { resolved = true; break; }
                }

                if (!resolved)
                {
                    for (int i = 0; i < maxNudges; i++)
                    {
                        transform.position += Vector3.down * nudgeStep;
                        Physics.SyncTransforms();
                        if (!IsOverlappingEnvironment()) { resolved = true; break; }
                    }
                }

                if (!resolved)
                {
                    Debug.LogWarning($"SnapToSurfaceBelow: Could not resolve overlap for {gameObject.name}. Consider increasing sampling or adjusting colliders.");
                }
            }
        }
        else
        {
            float halfHeight = b.extents.y;
            Vector3 fallback = transform.position;
            fallback.y = halfHeight;
            transform.position = fallback;
            Physics.SyncTransforms();
        }
    }

    private Vector3[] GetBaseSamplePoints(Bounds b, int samplesPerAxis)
    {
        List<Vector3> pts = new List<Vector3>();

        float minX = b.min.x;
        float maxX = b.max.x;
        float minZ = b.min.z;
        float maxZ = b.max.z;

        for (int xi = 0; xi < samplesPerAxis; xi++)
        {
            float tX = samplesPerAxis == 1 ? 0.5f : (xi / (float)(samplesPerAxis - 1)); // 0..1
            float worldX = Mathf.Lerp(minX, maxX, tX);

            for (int zi = 0; zi < samplesPerAxis; zi++)
            {
                float tZ = samplesPerAxis == 1 ? 0.5f : (zi / (float)(samplesPerAxis - 1));
                float worldZ = Mathf.Lerp(minZ, maxZ, tZ);

                Vector3 sample = new Vector3(worldX, b.center.y, worldZ);
                pts.Add(sample);
            }
        }

        return pts.ToArray();
    }

    private bool IsOverlappingEnvironment()
    {
        Bounds b = objectCollider.bounds;
        Vector3 center = b.center;
        Vector3 halfExtents = b.extents * 0.98f; 

        Collider[] hits = Physics.OverlapBox(center, halfExtents, Quaternion.identity, ~0, QueryTriggerInteraction.Ignore);

        foreach (var c in hits)
        {
            if (c == objectCollider) continue;
            return true;
        }
        return false;
    }
}
