using UnityEngine;
using System.Collections.Generic;

public class NPCPlaneAI : MonoBehaviour
{
    public enum PlaneState { Idle, Takeoff, Circling, Landing, FlightPlan }
    
    [Header("State Settings")]
    public PlaneState currentState = PlaneState.Idle;
    public float flightSpeed = 120f;
    public float turnSpeed = 2f;
    public float reachDistance = 50f; 

    [Header("Navigation Targets")]
    public Transform runwayStart;
    public Transform runwayEnd;
    public Transform circlingCenter;
    
    [Header("Flight Plan (Waypoints)")]
    [Tooltip("Arrastra aquí los puntos por los que el avión debe pasar secuencialmente")]
    public Transform[] customRoute;
    public bool loopRoute = true;
    
    [Header("Flight Parameters")]
    public float cruisingAltitude = 600f;
    public float circlingRadius = 1000f;
    public float pathUpdateInterval = 3f;

    [Header("Realistic Flight Physics")]
    public float maxBankAngle = 45f;
    public float maxPitchAngle = 20f;
    public float bankSmoothTime = 2f;
    public float pitchSmoothTime = 2f;

    [Header("Traffic Separation (Boids)")]
    public float separationDistance = 150f;
    public float separationForce = 3f;

    [Header("Pathfinding")]
    public AStar3D pathfinder;

    private List<AirNode> currentPath;
    private int currentWaypointIndex = 0;
    private Vector3 currentTargetPos;
    private Vector3 finalDestination;
    
    private float updateTimer = 0f;
    private bool isCalculating = false;

    // Físicas suaves
    private float currentBankAngle = 0f;
    private float currentPitchAngle = 0f;
    
    // Variables de Ruta
    // Variables de Ruta
    private int currentRouteIndex = 0;
    private PlaneState previousState;

    void Start()
    {
        if (pathfinder == null) {
            pathfinder = FindObjectOfType<AStar3D>();
        }
        previousState = currentState;
        InitializeState(currentState);
    }

    void InitializeState(PlaneState state)
    {
        switch (state)
        {
            case PlaneState.Takeoff: StartTakeoff(); break;
            case PlaneState.Circling: StartCircling(); break;
            case PlaneState.Landing: StartLanding(); break;
            case PlaneState.FlightPlan: StartFlightPlan(); break;
            case PlaneState.Idle: 
                currentPath = null;
                finalDestination = transform.position;
                break;
        }
    }

    void Update()
    {
        if (currentState != previousState)
        {
            InitializeState(currentState);
            previousState = currentState;
        }

        switch (currentState)
        {
            case PlaneState.Takeoff:
                HandleTakeoff();
                break;
            case PlaneState.Circling:
                HandleCircling();
                break;
            case PlaneState.Landing:
                HandleLanding();
                break;
            case PlaneState.FlightPlan:
                HandleFlightPlan();
                break;
        }

        if (currentState != PlaneState.Idle && !isCalculating && pathfinder != null)
        {
            updateTimer -= Time.deltaTime;
            if (updateTimer <= 0f)
            {
                updateTimer = pathUpdateInterval;
                isCalculating = true;
                pathfinder.RequestPath(transform.position, finalDestination, transform.forward, OnPathFound);
            }
        }

        if (currentState != PlaneState.Idle)
        {
            FlyTowardsTarget();
            transform.Translate(Vector3.forward * flightSpeed * Time.deltaTime);
        }
    }

    void FlyTowardsTarget()
    {
        if (currentPath != null && currentWaypointIndex < currentPath.Count)
        {
            currentTargetPos = currentPath[currentWaypointIndex].worldPosition;
            float dist = Vector3.Distance(transform.position, currentTargetPos);
            
            // Check overshoot (si nos pasamos el punto por ir muy rápido)
            Vector3 dirToTarget = (currentTargetPos - transform.position).normalized;
            float dot = Vector3.Dot(transform.forward, dirToTarget);
            
            // Avanzamos si estamos en el radio de alcance, O si el punto quedó atrás y estamos cerca
            if (dist < reachDistance || (dot < 0f && dist < reachDistance * 3f))
            {
                currentWaypointIndex++;
            }
        }
        
        if (currentTargetPos != Vector3.zero) 
        {
            SmoothSteer();
        }
    }

    void SmoothSteer() 
    {
        Vector3 targetDirection = currentTargetPos - transform.position;
        
        // --- SEPARATION LOGIC (Evasión dinámica de tráfico) ---
        Collider[] nearbyPlanes = Physics.OverlapSphere(transform.position, separationDistance);
        Vector3 separationVector = Vector3.zero;
        int planesToAvoid = 0;
        
        foreach (Collider col in nearbyPlanes) {
            if (col.gameObject != gameObject && col.GetComponent<NPCPlaneAI>() != null) {
                Vector3 away = transform.position - col.transform.position;
                separationVector += away.normalized / (away.magnitude + 0.1f);
                planesToAvoid++;
            }
        }

        if (planesToAvoid > 0) {
            targetDirection += (separationVector * separationForce);
        }
        // ------------------------------------------------------

        if (targetDirection != Vector3.zero) 
        {
            Quaternion desiredLook = Quaternion.LookRotation(targetDirection.normalized);
            Vector3 eulerLook = desiredLook.eulerAngles;
            
            // Banking (Alabeo)
            float yawDelta = Mathf.DeltaAngle(transform.eulerAngles.y, eulerLook.y);
            float targetBank = Mathf.Clamp(-yawDelta, -maxBankAngle, maxBankAngle);
            currentBankAngle = Mathf.Lerp(currentBankAngle, targetBank, Time.deltaTime * bankSmoothTime);

            // Pitch (Cabeceo)
            float pitchDelta = Mathf.DeltaAngle(transform.eulerAngles.x, eulerLook.x);
            float targetPitch = Mathf.Clamp(pitchDelta, -maxPitchAngle, maxPitchAngle);
            currentPitchAngle = Mathf.Lerp(currentPitchAngle, targetPitch, Time.deltaTime * pitchSmoothTime);

            // Rotación final
            Quaternion smoothYaw = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, eulerLook.y, 0), turnSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(currentPitchAngle, smoothYaw.eulerAngles.y, currentBankAngle);
        }
    }

    // --- STATES ---

    [ContextMenu("Start Flight Plan")]
    public void StartFlightPlan()
    {
        currentState = PlaneState.FlightPlan;
        currentRouteIndex = 0;
        SetNextRouteWaypoint();
    }

    void HandleFlightPlan()
    {
        if (HasReachedTarget())
        {
            currentRouteIndex++;
            SetNextRouteWaypoint();
        }
    }

    void SetNextRouteWaypoint()
    {
        if (customRoute == null || customRoute.Length == 0) 
        {
            currentState = PlaneState.Idle;
            return;
        }

        if (currentRouteIndex >= customRoute.Length)
        {
            if (loopRoute) {
                currentRouteIndex = 0;
            } else {
                currentState = PlaneState.Idle;
                currentBankAngle = 0;
                currentPitchAngle = 0;
                return;
            }
        }

        if (customRoute[currentRouteIndex] != null) 
        {
            finalDestination = customRoute[currentRouteIndex].position;
            RequestPathTo(finalDestination);
        } 
        else 
        {
            currentRouteIndex++;
            SetNextRouteWaypoint();
        }
    }

    [ContextMenu("Start Takeoff")]
    public void StartTakeoff()
    {
        currentState = PlaneState.Takeoff;
        if (runwayStart != null) transform.position = runwayStart.position;
        
        finalDestination = runwayEnd != null 
            ? runwayEnd.position + (runwayEnd.forward * 500f) + (Vector3.up * cruisingAltitude) 
            : transform.position + transform.forward * 800f + Vector3.up * cruisingAltitude;
        
        RequestPathTo(finalDestination);
    }

    void HandleTakeoff()
    {
        if (HasReachedTarget())
        {
            StartCircling();
        }
    }

    [ContextMenu("Start Circling")]
    public void StartCircling()
    {
        currentState = PlaneState.Circling;
        SetNextCircleWaypoint();
    }

    void HandleCircling()
    {
        if (HasReachedTarget())
        {
            SetNextCircleWaypoint();
        }
    }

    void SetNextCircleWaypoint()
    {
        Vector2 randomCircle = Random.insideUnitCircle.normalized * circlingRadius;
        Vector3 circleTarget = circlingCenter != null ? circlingCenter.position : transform.position;
        circleTarget += new Vector3(randomCircle.x, cruisingAltitude, randomCircle.y);

        finalDestination = circleTarget;
        RequestPathTo(finalDestination);
    }

    [ContextMenu("Start Landing")]
    public void StartLanding()
    {
        currentState = PlaneState.Landing;
        finalDestination = runwayStart != null ? runwayStart.position + (Vector3.up * 50f) - (runwayStart.forward * 400f) : transform.position;
        RequestPathTo(finalDestination);
    }

    void HandleLanding()
    {
        if (HasReachedTarget())
        {
            finalDestination = runwayEnd != null ? runwayEnd.position : transform.position;
            currentTargetPos = finalDestination;
            if (Vector3.Distance(transform.position, currentTargetPos) < reachDistance) {
                currentState = PlaneState.Idle; // Landed
                currentPitchAngle = 0;
                currentBankAngle = 0;
                transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0); // Enderezar nave
            }
        }
    }

    // --- UTILS ---

    void RequestPathTo(Vector3 target) 
    {
        if (pathfinder != null) {
            isCalculating = true;
            updateTimer = pathUpdateInterval;
            pathfinder.RequestPath(transform.position, target, transform.forward, OnPathFound);
        } else {
            currentTargetPos = target; 
        }
    }

    private void OnPathFound(List<AirNode> newPath)
    {
        isCalculating = false;
        if (newPath != null && newPath.Count > 0)
        {
            currentPath = newPath;
            currentWaypointIndex = 0;
        }
        else
        {
            currentTargetPos = finalDestination; // Fallback
        }
    }

    bool HasReachedTarget() 
    {
        return (currentPath != null && currentWaypointIndex >= currentPath.Count) || 
               (currentPath == null && Vector3.Distance(transform.position, finalDestination) < reachDistance);
    }
}
