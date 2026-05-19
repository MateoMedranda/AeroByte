using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plane : MonoBehaviour {

    // VALORES FÍSICOS DE UNITY
    // Gravedad predeterminada: Physics.gravity = (0, -9.81, 0) m/s²
    // Fricción: Por defecto, los materiales físicos tienen fricción dinámica y estática en 0.6, pero depende del Physic Material asignado al collider.
    // PARÁMETROS DE VUELO
    [Header("Velocidades y Límites")]
    [SerializeField]
    protected float maxThrust; // Empuje máximo que puede generar el avión (en Newtons)
    [SerializeField]
    protected float throttleSpeed; // Velocidad a la que el acelerador se mueve de 0 a 1 (en unidades por segundo)
    [SerializeField]
    protected float gLimit; // Límite general de fuerzas G que el avión puede soportar antes de perder el control (en Gs)
    [SerializeField]
    protected float gLimitPitch; // Límite de fuerzas G para maniobras de pitch (en Gs)

    // steering significa la capacidad de maniobra del avión, cuanto más alto, más rápido puede cambiar su dirección a altas velocidades
    [Header("Maniobrabilidad")]
    [SerializeField]
    Vector3 turnSpeed; // Velocidad máxima de giro en grados por segundo para cada eje (Pitch, Yaw, Roll)
    [SerializeField]
    Vector3 turnAcceleration; // Aceleración angular en grados por segundo al cuadrado, controla qué tan rápido el avión puede alcanzar su velocidad de giro máxima
    [SerializeField]
    AnimationCurve steeringCurve; // Curva que modula la capacidad de maniobra del avión en función de su velocidad, generalmente va de 0 a 1, donde 0 significa sin capacidad de maniobra y 1 significa maniobra completa

    [Header("Flaps")]
    [SerializeField]
    bool flapsDeployed; // Variable para rastrear el estado de los flaps, se controla a través del método ToggleFlaps()
    [SerializeField]
    float initialSpeed; // Velocidad inicial del avión al despegar, se puede usar para configurar la velocidad mínima a la que el avión puede volar sin caer

    // lift significa la capacidad de generar sustentación, cuanto más alto, más sustentación genera a una velocidad dada
    [Header("Sustentacion")]
    [SerializeField]
    float liftPower; // Potencia de sustentación general del avión, controla cuánto lift genera a una velocidad dada
    [SerializeField]
    AnimationCurve liftAOACurve; // Curva que modula la sustentación en función del ángulo de ataque, generalmente tiene un pico en el ángulo de ataque óptimo y luego cae a medida que el ángulo de ataque aumenta (stall)
    [SerializeField]
    float inducedDrag; // Potencia de arrastre inducido, controla cuánto arrastre adicional se genera a medida que la sustentación aumenta, lo que simula el efecto de resistencia al avance que ocurre en la vida real
    [SerializeField]
    AnimationCurve inducedDragCurve; // Curva que modula el arrastre inducido en función de la velocidad, generalmente aumenta a medida que la velocidad aumenta, simulando cómo el arrastre inducido se vuelve más significativo a altas velocidades
    [SerializeField]
    float rudderPower; // Potencia del timón, controla cuánto lift genera el timón para maniobras de yaw
    [SerializeField]
    AnimationCurve rudderAOACurve; // Curva que modula la sustentación del timón en función del ángulo de ataque del timón, similar a liftAOACurve pero para el timón, lo que permite simular cómo el timón genera sustentación para maniobras de yaw
    [SerializeField]
    AnimationCurve rudderInducedDragCurve; // Curva que modula el arrastre inducido del timón en función de la velocidad, similar a inducedDragCurve pero para el timón
    [SerializeField]
    float flapsLiftPower; // Potencia de sustentación adicional que proporcionan los flaps cuando están desplegados, lo que permite al avión generar más sustentación a bajas velocidades, ideal para despegues y aterrizajes
    [SerializeField]
    float flapsAOABias; // Bias de ángulo de ataque que los flaps añaden al ángulo de ataque efectivo, lo que simula cómo los flaps cambian la forma del ala y permiten un mayor ángulo de ataque sin entrar en stall
    [SerializeField]
    float flapsDrag; // Potencia de arrastre que generan los flaps cuando están desplegados, lo que simula el efecto de resistencia al avance que ocurre en la vida real
    [SerializeField]
    float flapsRetractSpeed; // Velocidad a la que los flaps se retraen automáticamente, si la velocidad del avión supera este valor, los flaps se retraen para reducir el arrastre y permitir velocidades más altas

    [SerializeField]
    Transform Propeller;
    [SerializeField]
    Transform LeftElevator;
    [SerializeField]
    Transform RightElevator;
    [SerializeField]
    Transform LeftFlaps;
    [SerializeField]
    Transform RightFlaps;
    [SerializeField]
    Transform LeftSpoiler;
    [SerializeField]
    Transform RightSpoiler;
    [SerializeField]
    Transform Rudder;

    [Header("Superficies de Control Visuales")]
    [SerializeField, Tooltip("Ángulo máximo de deflexión de los elevadores en grados (positivo: arriba, negativo: abajo)")]
    float maxElevatorDeflection = 25f;
    [SerializeField, Tooltip("Ángulo máximo de deflexión de los flaps en grados (positivo: abajo, negativo: arriba)")]
    float maxFlapsDeflection = 35f;
    [SerializeField, Tooltip("Ángulo máximo de deflexión de los spoilers en grados (positivo: abajo, negativo: arriba)")]
    float maxSpoilersDeflection = 35f;
    [SerializeField, Tooltip("Ángulo máximo de deflexión del timón en grados (positivo: derecha, negativo: izquierda)")]
    float maxRudderDeflection = 35f;

    // CONTROLES
    float throttleInput; // Valor de entrada para el acelerador, esperado entre -1 (freno de aire) y 1 (aceleración máxima)
    Vector3 controlInput; // Vector3 donde X = Pitch, Y = Yaw, Z = Roll, cada uno esperado entre -1 y 1
    Vector3 lastVelocity;

    // DATOS LOCALES
    public Rigidbody Rb { get; private set; }
    public Vector3 EffectiveInput { get; private set; }
    public float Throttle { get; private set; }
    public Vector3 Velocity { get; private set; }
    public Vector3 LocalVelocity { get; private set; }
    public Vector3 LocalGForce { get; private set; }
    public Vector3 LocalAngularVelocity { get; private set; }
    public float AngleOfAttack { get; private set; }
    public float AngleOfAttackYaw { get; private set; }
    public bool AirbrakeDeployed { get; private set; }

    private Quaternion leftFlapStartRotation;
    private Quaternion rightFlapStartRotation;
    private Quaternion leftSpoilerStartRotation;
    private Quaternion rightSpoilerStartRotation;
    

    // FlapsDeployed es una propiedad que controla el estado de los flaps del avión. Se puede cambiar a través del método ToggleFlaps(), que alterna entre desplegado y retraído. Además, los flaps se retraen automáticamente si la velocidad del avión supera el valor definido en flapsRetractSpeed, lo que ayuda a reducir el arrastre a altas velocidades.
    public bool FlapsDeployed {
        get {
            return flapsDeployed;
        }
        private set {
            flapsDeployed = value;
        }
    }

    private void Awake() {
        Rb = GetComponent<Rigidbody>();
        if (LeftFlaps != null) leftFlapStartRotation = LeftFlaps.localRotation;
        if (RightFlaps != null) rightFlapStartRotation = RightFlaps.localRotation;
        if (LeftSpoiler != null) leftSpoilerStartRotation = LeftSpoiler.localRotation;
        if (RightSpoiler != null) rightSpoilerStartRotation = RightSpoiler.localRotation;
    }
    
    private void FixedUpdate() {
        float dt = Time.fixedDeltaTime;

        Debug.Log($"Delta Time: {dt:F4} s");
        
        Debug.Log($"[Plane Debug] Velocidad Local: {LocalVelocity}, Ángulo de Ataque: {AngleOfAttack * Mathf.Rad2Deg:F2}°, Flaps Desplegados: {FlapsDeployed}");

        CalculatePlaneState(dt);
        CalculateAngleOfAttack();
        CalculateGForce(dt);

        UpdateThrottle(dt);
        UpdateThrust();
        UpdateLift();

        UpdateSteering(dt);

        UpdateElevatorVisual();
        UpdateFlapsVisual();
        UpdateSpoilersVisual();
        UpdateFlaps();
    }
    // Rota el elevador visualmente según el input de pitch, incluso en tierra
    // Variables para interpolación suave de elevadores
    private float currentElevatorAngle = 0f;
    private float elevatorLerpSpeed = 5f; // Puedes ajustar este valor para mayor o menor suavidad
    private void UpdateElevatorVisual()
    {
        if (LeftElevator != null && RightElevator != null)
        {
            float targetAngle = controlInput.x * maxElevatorDeflection * 1.2f; // Ángulo mayor (20% más)
            currentElevatorAngle = Mathf.Lerp(currentElevatorAngle, targetAngle, Time.fixedDeltaTime * elevatorLerpSpeed);
            LeftElevator.localRotation = Quaternion.Euler(-currentElevatorAngle, 0f, 0f);
            RightElevator.localRotation = Quaternion.Euler(-currentElevatorAngle, 0f, 0f);
        }
    }

    // Variables para interpolación suave de flaps
    private float currentFlapAngle = 0f;
    private float flapLerpSpeed = 5f; // Puedes ajustar este valor para mayor o menor suavidad
    private void UpdateFlapsVisual()
    {
        if (LeftFlaps != null && RightFlaps != null)
        {
            float targetAngle = FlapsDeployed ? maxFlapsDeflection * 1.2f : 0f; // Ángulo mayor (20% más)
            currentFlapAngle = Mathf.Lerp(currentFlapAngle, targetAngle, Time.fixedDeltaTime * flapLerpSpeed);
            LeftFlaps.localRotation = leftFlapStartRotation * Quaternion.Euler(0f, currentFlapAngle, 0f);
            RightFlaps.localRotation = rightFlapStartRotation * Quaternion.Euler(0f, -currentFlapAngle, 0f);
        }
    }

    // Variables para interpolación suave de spoilers
    private float currentSpoilerAngle = 0f;
    private float spoilerLerpSpeed = 5f; // Puedes ajustar este valor para mayor o menor suavidad
    private void UpdateSpoilersVisual()
    {
        if (LeftSpoiler != null && RightSpoiler != null)
        {
            float targetAngle = controlInput.z * maxSpoilersDeflection * 1.2f; // Ángulo mayor (20% más)
            currentSpoilerAngle = Mathf.Lerp(currentSpoilerAngle, targetAngle, Time.fixedDeltaTime * spoilerLerpSpeed);
            LeftSpoiler.localRotation = leftSpoilerStartRotation * Quaternion.Euler(0f, currentSpoilerAngle, 0f);
            RightSpoiler.localRotation = rightSpoilerStartRotation * Quaternion.Euler(0f, currentSpoilerAngle, 0f);
        }
    }

    private void UpdateRudderVisual() {
        // Similar a los elevadores, pero para el timón de dirección (yaw)
        // El input de yaw es controlInput.y (positivo: girar a la derecha)
        // El ángulo máximo es maxRudderDeflection
        // Rota en el eje local Y (bisagra del timón)
        if(Rudder != null) {
            float angle = controlInput.y * maxRudderDeflection;
            Rudder.localRotation = Quaternion.Euler(0f, angle, 0f);
        }
    }

    // El método UpdateThrust() aplica una fuerza de empuje relativa al avión en la dirección hacia adelante (Vector3.forward) multiplicada por el valor actual del acelerador (Throttle) y la potencia máxima de empuje (maxThrust). Esto simula cómo el motor del avión genera empuje para propulsarlo hacia adelante, y el valor de Throttle controla cuánto empuje se aplica en cada momento, permitiendo al avión acelerar o desacelerar según la entrada del jugador.
    private void UpdateThrust() {
        Rb.AddRelativeForce(Throttle * maxThrust * Vector3.forward);
        Propeller.Rotate(Vector3.up * Velocity.magnitude * 40f); // Gira la hélice en función del acelerador y la velocidad de giro definida, multiplicado por 10 para hacerla más visible
    }

    // El método UpdateFlaps() verifica la velocidad local del avión en el eje Z (hacia adelante) y compara si es mayor que el valor definido en flapsRetractSpeed. Si la velocidad supera ese umbral, los flaps se retraen automáticamente estableciendo FlapsDeployed en false. Esto simula el comportamiento real de los aviones, donde los flaps se utilizan principalmente a bajas velocidades para aumentar la sustentación durante el despegue y aterrizaje, pero se retraen a altas velocidades para reducir el arrastre y permitir un vuelo más eficiente.
    private void UpdateFlaps() {
        if (LocalVelocity.z > flapsRetractSpeed) {
            FlapsDeployed = false;
        }
    }
    
    public void ToggleFlaps() {
        if (LocalVelocity.z < flapsRetractSpeed) {
            FlapsDeployed = !FlapsDeployed;
        }
    }

    public void SetThrottleInput(float input) {
        throttleInput = input;
    }

    public void SetControlInput(Vector3 input) {
        controlInput = Vector3.ClampMagnitude(input, 1);
    }

    // Calcula la velocidad, velocidad local, fuerzas G y velocidades angulares locales del avión 
    // util para el control aerodinámico y para efectos visuales como el movimiento de la cabina o el sonido del motor.
    // También es importante para calcular el ángulo de ataque, que es crucial para la aerodinámica del avión.
    private void CalculatePlaneState(float dt) {
        var quaternion = Quaternion.Inverse(Rb.rotation);
        Debug.Log($"[Plane State Debug] Rb.rotation: {Rb.rotation}, Quaternion Inverso: {quaternion}");
        Velocity = Rb.linearVelocity;
        LocalVelocity = quaternion * Velocity; 
        LocalAngularVelocity = quaternion * Rb.angularVelocity; 
    }

    private void CalculateAngleOfAttack() {
        Debug.Log($"[AoA Debug] LocalVelocity: {LocalVelocity.sqrMagnitude:F2} (x: {LocalVelocity.x:F2}, y: {LocalVelocity.y:F2}, z: {LocalVelocity.z:F2})");
        if (LocalVelocity.sqrMagnitude < 0.1f) {
            AngleOfAttack = 0;
            AngleOfAttackYaw = 0;
            return;
        }

        AngleOfAttack = Mathf.Atan2(-LocalVelocity.y, LocalVelocity.z);
        AngleOfAttackYaw = Mathf.Atan2(LocalVelocity.x, LocalVelocity.z);
    }

    private void CalculateGForce(float dt) {
        var quaternion = Quaternion.Inverse(Rb.rotation);
        var acceleration = (Velocity - lastVelocity) / dt;
        LocalGForce = quaternion * acceleration;
        lastVelocity = Velocity;
    }

    private void UpdateThrottle(float dt) {
        float target = 0;
        if (throttleInput > 0) target = 1;
        Debug.Log($"[Throttle Debug] Throttle Input: {throttleInput:F2}, Target: {target}, Current Throttle: {Throttle:F2}");
        Throttle = Utilities.MoveTo(Throttle, target, throttleSpeed * Mathf.Abs(throttleInput), dt);
        
        // AirbrakeDeployed = Throttle == 0 && throttleInput == -1;
        
        /*
        if (AirbrakeDeployed) {
            foreach (var lg in landingGear) {
                lg.sharedMaterial = landingGearBrakesMaterial;
            }
        } else {
            foreach (var lg in landingGear) {
                lg.sharedMaterial = landingGearDefaultMaterial;
            }
        }
        */
    }

    // El método CalculateLift() calcula la fuerza de sustentación y el arrastre inducido que actúan sobre el avión en función del ángulo de ataque, la velocidad local, y las propiedades aerodinámicas definidas por las curvas de sustentación y arrastre. Utiliza la proyección de la velocidad local en el plano perpendicular al eje de rotación para determinar la dirección y magnitud de la sustentación, y también calcula el arrastre inducido que se genera como resultado de la sustentación, lo que simula cómo el avión experimenta resistencia al avance a medida que genera sustentación.
    Vector3 CalculateLift(float angleOfAttack, Vector3 rightAxis, float liftPower, AnimationCurve aoaCurve, AnimationCurve inducedDragCurve) {
        var liftVelocity = Vector3.ProjectOnPlane(LocalVelocity, rightAxis);    
        var v2 = liftVelocity.sqrMagnitude;                                     

        var liftCoefficient = aoaCurve.Evaluate(angleOfAttack * Mathf.Rad2Deg);
        var liftForce = v2 * liftCoefficient * liftPower;

        var liftDirection = Vector3.Cross(liftVelocity.normalized, rightAxis);
        var lift = liftDirection * liftForce;

        var dragForce = liftCoefficient * liftCoefficient;
        var dragDirection = -liftVelocity.normalized;
        var inducedDrag = dragDirection * v2 * dragForce * this.inducedDrag * inducedDragCurve.Evaluate(Mathf.Max(0, LocalVelocity.z));

        return lift + inducedDrag;
    }


    private void UpdateLift() {
        if (LocalVelocity.sqrMagnitude < 1f) return;

        float flapsLiftPower = FlapsDeployed ? this.flapsLiftPower : 0;
        float flapsAOABias = FlapsDeployed ? this.flapsAOABias : 0;

        var liftForce = CalculateLift(
            AngleOfAttack + (flapsAOABias * Mathf.Deg2Rad), Vector3.right,
            liftPower + flapsLiftPower,
            liftAOACurve,
            inducedDragCurve
        );

        var yawForce = CalculateLift(AngleOfAttackYaw, Vector3.up, rudderPower, rudderAOACurve, rudderInducedDragCurve);

        Rb.AddRelativeForce(liftForce);
        Rb.AddRelativeForce(yawForce);
    }

    private Vector3 CalculateGForce(Vector3 angularVelocity, Vector3 velocity) {
        return Vector3.Cross(angularVelocity, velocity);
    }

    private Vector3 CalculateGForceLimit(Vector3 input) {
        return Utilities.Scale6(input,
            gLimit, gLimitPitch,    //pitch down, pitch up
            gLimit, gLimit,         //yaw
            gLimit, gLimit          //roll
        ) * 9.81f;
    }

    private float CalculateGLimiter(Vector3 controlInput, Vector3 maxAngularVelocity) {
        if (controlInput.magnitude < 0.01f) {
            return 1;
        }

        var maxInput = controlInput.normalized;

        var limit = CalculateGForceLimit(maxInput);
        var maxGForce = CalculateGForce(Vector3.Scale(maxInput, maxAngularVelocity), LocalVelocity);

        if (maxGForce.magnitude > limit.magnitude) {
            return limit.magnitude / maxGForce.magnitude;
        }

        return 1;
    }

    private float CalculateSteering(float dt, float angularVelocity, float targetVelocity, float acceleration) {
        var error = targetVelocity - angularVelocity;
        var accel = acceleration * dt;
        return Mathf.Clamp(error, -accel, accel);
    }

    private void UpdateSteering(float dt) {
        var speed = Mathf.Max(0, LocalVelocity.z);
        var steeringPower = steeringCurve.Evaluate(speed);
        // --- CÓDIGO DE DIAGNÓSTICO (COLÓCALO AQUÍ) ---
        Debug.Log($"[Steering Debug] controlInput (Pitch/Yaw/Roll): {controlInput}");
        Debug.Log($"[Steering Debug] Velocidad Adelante Actual: {speed:F2} m/s");
        Debug.Log($"[Steering Debug] Potencia de Curva (steeringPower): {steeringPower:F2}");

        // Si steeringPower es 0, el resto de la función se bloquea
        if (steeringPower < 0.01f) {
            // Este log solo aparece si la dirección está desactivada
            Debug.LogWarning("[Steering Debug] ¡La dirección está APAGADA! steeringPower es casi 0.");
        }
        // ------------------------------------------------

        var gForceScaling = CalculateGLimiter(controlInput, turnSpeed * Mathf.Deg2Rad * steeringPower);

        var targetAV = Vector3.Scale(controlInput, turnSpeed * steeringPower * gForceScaling);
        var av = LocalAngularVelocity * Mathf.Rad2Deg;

        var correction = new Vector3(
            CalculateSteering(dt, av.x, targetAV.x, turnAcceleration.x * steeringPower),
            CalculateSteering(dt, av.y, targetAV.y, turnAcceleration.y * steeringPower),
            CalculateSteering(dt, av.z, targetAV.z, turnAcceleration.z * steeringPower)
        );

        Rb.AddRelativeTorque(correction * Mathf.Deg2Rad, ForceMode.VelocityChange);    //ignore rigidbody mass

        var correctionInput = new Vector3(
            Mathf.Clamp((targetAV.x - av.x) / turnAcceleration.x, -1, 1),
            Mathf.Clamp((targetAV.y - av.y) / turnAcceleration.y, -1, 1),
            Mathf.Clamp((targetAV.z - av.z) / turnAcceleration.z, -1, 1)
        );

        var effectiveInput = (correctionInput + controlInput) * gForceScaling;

        EffectiveInput = new Vector3(
            Mathf.Clamp(effectiveInput.x, -1, 1),
            Mathf.Clamp(effectiveInput.y, -1, 1),
            Mathf.Clamp(effectiveInput.z, -1, 1)
        );
    }
    
}