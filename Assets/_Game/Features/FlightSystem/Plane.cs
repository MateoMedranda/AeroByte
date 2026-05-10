using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plane : MonoBehaviour {

    // VALORES FÍSICOS DE UNITY
    // Gravedad predeterminada: Physics.gravity = (0, -9.81, 0) m/s²
    // Fricción: Por defecto, los materiales físicos tienen fricción dinámica y estática en 0.6, pero depende del Physic Material asignado al collider.
    // PARÁMETROS DE VUELO
    [SerializeField]
    float maxThrust; // Empuje máximo que puede generar el avión (en Newtons)
    [SerializeField]
    float throttleSpeed; // Velocidad a la que el acelerador se mueve de 0 a 1 (en unidades por segundo)

    // CONTROLES
    float throttleInput; // Valor de entrada para el acelerador, esperado entre -1 (freno de aire) y 1 (aceleración máxima)
    Vector3 controlInput; // Vector3 donde X = Pitch, Y = Yaw, Z = Roll, cada uno esperado entre -1 y 1

    // DATOS LOCALES
    public Rigidbody Rigidbody { get; private set; }
    Vector3 lastVelocity;
    public float Throttle { get; private set; }
    public Vector3 Velocity { get; private set; }
    public Vector3 LocalVelocity { get; private set; }
    public Vector3 LocalGForce { get; private set; }
    public Vector3 LocalAngularVelocity { get; private set; }
    public float AngleOfAttack { get; private set; }
    public float AngleOfAttackYaw { get; private set; }
    public bool AirbrakeDeployed { get; private set; }

    void Awake() {
        Rigidbody = GetComponent<Rigidbody>();
    }
    
    void FixedUpdate() {
        float dt = Time.fixedDeltaTime;

        //CalculateState(dt);
        //CalculateAngleOfAttack();
        //CalculateGForce(dt);

        UpdateThrottle(dt);
        UpdateThrust();
    }

    // Calcula la velocidad, velocidad local, fuerzas G y velocidades angulares locales del avión 
    // util para el control aerodinámico y para efectos visuales como el movimiento de la cabina o el sonido del motor.
    // También es importante para calcular el ángulo de ataque, que es crucial para la aerodinámica del avión.
    void CalculateState(float dt) {
        var invRotation = Quaternion.Inverse(Rigidbody.rotation);
        Velocity = Rigidbody.linearVelocity;
        LocalVelocity = invRotation * Velocity; 
        LocalAngularVelocity = invRotation * Rigidbody.angularVelocity; 
    }


    void CalculateAngleOfAttack() {
        if (LocalVelocity.sqrMagnitude < 0.1f) {
            AngleOfAttack = 0;
            AngleOfAttackYaw = 0;
            return;
        }

        AngleOfAttack = Mathf.Atan2(-LocalVelocity.y, LocalVelocity.z);
        AngleOfAttackYaw = Mathf.Atan2(LocalVelocity.x, LocalVelocity.z);
    }

    void CalculateGForce(float dt) {
        var invRotation = Quaternion.Inverse(Rigidbody.rotation);
        var acceleration = (Velocity - lastVelocity) / dt;
        LocalGForce = invRotation * acceleration;
        lastVelocity = Velocity;
    }

    void UpdateThrottle(float dt) {
        float target = 0;
        if (throttleInput > 0) target = 1;

        Throttle = Utilities.MoveTo(Throttle, target, throttleSpeed * Mathf.Abs(throttleInput), dt);
        Debug.Log("Acelerador (Throttle): " + Throttle + " | Empuje Máximo: " + maxThrust);
        
        AirbrakeDeployed = Throttle == 0 && throttleInput == -1;
        
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


    void UpdateThrust() {
        Rigidbody.AddRelativeForce(Throttle * maxThrust * Vector3.forward);
    }

    public void SetThrottleInput(float input) {
        throttleInput = input;
    }

    public void SetControlInput(Vector3 input) {
        controlInput = Vector3.ClampMagnitude(input, 1);
    }
}