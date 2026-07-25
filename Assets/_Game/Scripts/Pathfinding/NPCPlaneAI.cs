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

    [Header("Landing Settings")]
    [Tooltip("Distancia mínima al punto de fin de pista para considerarse aterrizado")]
    public float landingStopDistance = 10f;
    [Tooltip("Altura adicional sobre el punto de fin de pista al aterrizar")]
    public float landingHeightOffset = 2f;

    [Header("Utility-Based Decision Making")]
    public bool useUtilityDecisionMaking = true;
    public float fuel = 100f;
    public float maxFuel = 100f;
    public float fuelConsumptionRate = 1.5f;
    public bool stormActive = false;
    public bool showDebugUI = true;
    
    [HideInInspector] public float patrolUtility;
    [HideInInspector] public float landingUtility;

    private PlaneState stateBeforeEmergency = PlaneState.FlightPlan;

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
        fuel = maxFuel; // Asegurar que inicien con tanque lleno al arrancar la simulación
        
        // --- AUTO-BUSCAR REFERENCIAS DE PISTA Y PATRULLA SI ESTÁN VACÍAS ---
        if (runwayStart == null)
        {
            GameObject obj = GameObject.Find("RunwayStart");
            if (obj == null) obj = GameObject.Find("Runway Start");
            if (obj != null) runwayStart = obj.transform;
        }
        if (runwayEnd == null)
        {
            GameObject obj = GameObject.Find("RunwayEnd");
            if (obj == null) obj = GameObject.Find("Runway End");
            if (obj != null) runwayEnd = obj.transform;
        }
        if (circlingCenter == null)
        {
            GameObject obj = GameObject.Find("CirclingCenter");
            if (obj == null) obj = GameObject.Find("Circling Center");
            if (obj != null) circlingCenter = obj.transform;
        }
        // -------------------------------------------------------------------

        // Guardar el estado inicial como estado por defecto para reanudar después
        if (currentState != PlaneState.Idle && currentState != PlaneState.Takeoff && currentState != PlaneState.Landing)
        {
            stateBeforeEmergency = currentState;
        }
        else
        {
            stateBeforeEmergency = PlaneState.FlightPlan;
        }

        if (pathfinder == null) {
            pathfinder = FindFirstObjectByType<AStar3D>();
        }
        previousState = currentState;
        InitializeState(currentState);
    }

    void InitializeState(PlaneState state)
    {
        currentPath = null;
        currentWaypointIndex = 0;

        switch (state)
        {
            case PlaneState.Takeoff: StartTakeoff(); break;
            case PlaneState.Circling: StartCircling(); break;
            case PlaneState.Landing: StartLanding(); break;
            case PlaneState.FlightPlan: StartFlightPlan(); break;
            case PlaneState.Idle: 
                finalDestination = transform.position;
                break;
        }
    }

    void Update()
    {
        // --- UTILITY DECISION MAKING UPDATE ---
        if (useUtilityDecisionMaking)
        {
            // Consumir combustible si no está quieto
            if (currentState != PlaneState.Idle)
            {
                fuel = Mathf.Max(0f, fuel - fuelConsumptionRate * Time.deltaTime);
            }
            else
            {
                // Si está en Idle, recargar combustible en tierra de forma progresiva
                fuel = Mathf.Min(maxFuel, fuel + fuelConsumptionRate * 5f * Time.deltaTime);

                // Si se carga al 100%, despegar automáticamente para reanudar su comportamiento anterior
                if (fuel >= maxFuel)
                {
                    Debug.LogWarning($"[Utility AI] Reabastecimiento completo. Despegando automáticamente para reanudar {stateBeforeEmergency}.");
                    StartTakeoff();
                }
            }

            // Calcular utilidades
            patrolUtility = 0.3f * (fuel / maxFuel);

            float fuelFactor = Mathf.Clamp01((maxFuel - fuel) / maxFuel);
            float fuelUtility = Mathf.Pow(fuelFactor, 3f) * 1.5f; // curva cúbica
            
            landingUtility = Mathf.Clamp01(fuelUtility + (stormActive ? 0.6f : 0f));

            // Tomar decisión
            if (landingUtility > patrolUtility && currentState != PlaneState.Landing && currentState != PlaneState.Idle)
            {
                stateBeforeEmergency = currentState; // Registrar qué comportamiento estaba haciendo
                Debug.LogWarning($"[Utility AI] Cambiando estado de {currentState} a Landing por emergencia. Fuel: {fuel:F1}%, Landing Utility: {landingUtility:F2}");
                currentState = PlaneState.Landing;
            }
        }
        // --------------------------------------

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

            float currentSpeed = flightSpeed;
            if (currentState == PlaneState.Landing)
            {
                Vector3 targetLandingPos = runwayEnd != null 
                    ? runwayEnd.position + Vector3.up * landingHeightOffset 
                    : transform.position;

                float distToTarget = Vector3.Distance(transform.position, targetLandingPos);
                
                // Desaceleración progresiva al acercarse al fin de pista
                if (distToTarget < 300f)
                {
                    currentSpeed = Mathf.Lerp(8f, flightSpeed, distToTarget / 300f);
                }
            }

            transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);
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
            // Reanudar el comportamiento que estaba realizando antes de la recarga
            if (stateBeforeEmergency == PlaneState.FlightPlan)
            {
                StartFlightPlan();
            }
            else
            {
                StartCircling();
            }
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
            Vector3 targetLandingPos = runwayEnd != null 
                ? runwayEnd.position + Vector3.up * landingHeightOffset 
                : transform.position;

            finalDestination = targetLandingPos;
            currentTargetPos = finalDestination;
            
            if (Vector3.Distance(transform.position, currentTargetPos) < landingStopDistance) {
                currentState = PlaneState.Idle; // Landed (comienza recarga progresiva en Idle)
                currentPitchAngle = 0;
                currentBankAngle = 0;
                transform.position = targetLandingPos; // Posicionar exactamente en las coordenadas y altura deseadas
                transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0); // Enderezar nave
            }
        }
    }

    void OnGUI()
    {
        if (!showDebugUI) return;

        // Convertir posición 3D a pantalla
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 screenPos = cam.WorldToScreenPoint(transform.position + Vector3.up * 15f); // 15m por encima del avión

        // Verificar si está en pantalla
        if (screenPos.z > 0 && screenPos.x > 0 && screenPos.x < Screen.width && screenPos.y > 0 && screenPos.y < Screen.height)
        {
            float yPos = Screen.height - screenPos.y;

            // Rectángulo del panel de depuración
            Rect rect = new Rect(screenPos.x - 100, yPos - 85, 200, 115);
            
            // Fondo semitransparente oscuro usando estilo de Box
            GUI.Box(rect, "");
            
            // Estilo de texto personalizado
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.UpperCenter;
            style.fontSize = 11;
            style.richText = true;

            // Dibujar información
            string text = $"<b><size=12>{gameObject.name}</size></b>\n" +
                          $"Estado: <color=yellow>{currentState}</color>\n" +
                          $"Combustible: <color=#00ffffff>{fuel:F1}%</color>\n" +
                          $"U_Patrullar: <color=#00ff00ff>{patrolUtility:F2}</color>\n" +
                          $"U_Aterrizar: <color=#ff3333ff>{landingUtility:F2}</color>";
            
            GUI.Label(rect, text, style);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!showDebugUI) return;

        // Texto limpio para la Scene View (sin tags HTML, ya que Handles.Label no los interpreta nativamente)
        string debugText = $"[ {gameObject.name} ]\n" +
                           $"Estado: {currentState}\n" +
                           $"Combustible: {fuel:F1}%\n" +
                           $"U_Patrullar: {patrolUtility:F2}\n" +
                           $"U_Aterrizar: {landingUtility:F2}";

        GUIStyle style = new GUIStyle();
        style.normal.textColor = currentState == PlaneState.Landing ? Color.red : Color.cyan;
        style.fontSize = 11;
        style.alignment = TextAnchor.UpperCenter;

        // Dibujar el texto flotante en la Scene View
        UnityEditor.Handles.Label(transform.position + Vector3.up * 18f, debugText, style);

        // Dibujar línea visual de guía hacia su destino de vuelo actual
        if (currentTargetPos != Vector3.zero)
        {
            Gizmos.color = currentState == PlaneState.Landing ? Color.red : Color.green;
            Gizmos.DrawLine(transform.position, currentTargetPos);
            Gizmos.DrawWireSphere(currentTargetPos, 5f);
        }
    }
#endif

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
