using UnityEngine;
using FlightSystem.Domain.Entities;
using FlightSystem.Domain.Interfaces;
using FlightSystem.UseCases;
using AeroByte.FlightSystem.Framework.Config; 

namespace FlightSystem.Adapters
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlaneController : MonoBehaviour, IPhysicsGateway
    {
        [Header("Configuración (¡Usa el ScriptableObject!)")]
        public PlaneStatsConfig statsConfig; 
        
        [Header("Sensores del Avión")]
        public LayerMask groundMask;
        public float groundCheckDistance = 2f;

        private Rigidbody _rb;
        private PlaneState _planeState;
        private FlightPhysicsUseCase _physicsUseCase;
        private Vector3 _lastVelocity;
        private int _weatherZoneContacts;

        public Rigidbody Body => _rb;
        public PlaneState State => _planeState;
        public bool IsInWeatherZone => _weatherZoneContacts > 0;
        
        private CrashPlaneUseCase _crashUseCase;
        private IPlaneCrashPresenter _crashPresenter;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _planeState = new PlaneState(0f); 
            _physicsUseCase = new FlightPhysicsUseCase(this, statsConfig);
            
            _crashPresenter = GetComponent<IPlaneCrashPresenter>();
            if (_crashPresenter == null)
            {
                Debug.LogWarning("[PlaneController] No se encontró ningún componente que implemente IPlaneCrashPresenter en este GameObject.");
            }
            _crashUseCase = new CrashPlaneUseCase(_crashPresenter);
            
            _planeState.OnLandingGearStateChanged += HandleLandingGearStateChanged;
        }

        private void Start()
        {
            ApplyCustomPlaneColor();
        }

        public void ApplyCustomPlaneColor()
        {
            if (PlayerPrefs.GetInt("CustomPlaneColor_Enabled", 0) == 1)
            {
                float r = PlayerPrefs.GetFloat("CustomPlaneColor_R", 1f);
                float g = PlayerPrefs.GetFloat("CustomPlaneColor_G", 1f);
                float b = PlayerPrefs.GetFloat("CustomPlaneColor_B", 1f);
                Color customColor = new Color(r, g, b, 1f);

                Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
                foreach (var rend in renderers)
                {
                    if (rend == null) continue;
                    if (rend is ParticleSystemRenderer || rend is TrailRenderer || rend is LineRenderer) continue;

                    foreach (var mat in rend.materials)
                    {
                        if (mat == null) continue;
                        string matName = mat.name.ToLower();
                        if (matName.Contains("glass") || matName.Contains("vidrio") || matName.Contains("windshield") ||
                            matName.Contains("transparent") || matName.Contains("glow") || matName.Contains("trail") ||
                            matName.Contains("fire") || matName.Contains("light") || matName.Contains("foco") ||
                            matName.Contains("cristal") || matName.Contains("smoke"))
                        {
                            continue;
                        }

                        if (mat.HasProperty("_BaseColor"))
                            mat.SetColor("_BaseColor", customColor);
                        if (mat.HasProperty("_Color"))
                            mat.SetColor("_Color", customColor);
                    }
                }
                Debug.Log($"[PlaneController] Color personalizado de aeronave aplicado: RGB({r:F2}, {g:F2}, {b:F2})");
            }
        }

        private void OnDestroy() {
            if (_planeState != null) {
                _planeState.OnLandingGearStateChanged -= HandleLandingGearStateChanged;
            }
        }

        private void HandleLandingGearStateChanged(bool isDown) {
            Debug.Log($"[PlaneController] Tren de aterrizaje {(isDown ? "Desplegado" : "Retraído")}. Activando animación...");
            
            Animator animator = GetComponentInChildren<Animator>();
            if (animator != null) {
                animator.SetBool("GearDown", isDown);
            } else {
                Debug.LogWarning("[PlaneController] No se encontró componente Animator en el avión para el tren de aterrizaje.");
            }
        }

        public void OnControlInput(Vector3 input) => _planeState.SetControlInput(input);
        public void OnThrottleInput(float input) => _planeState.SetThrottleInput(input);
        public void OnToggleFlaps()
        {
            if (statsConfig == null) return;
            _planeState.ToggleFlaps(statsConfig.FlapsRetractSpeed);
        }
        public void OnToggleLights() => _planeState.ToggleLights();
        
        public void OnToggleLandingGear() {
            Debug.Log($"[DEBUG] OnToggleLandingGear llamado en PlaneController. Enviando a PlaneState... HasRetractableGear: {statsConfig.HasRetractableGear}, isGrounded actual: {_planeState.isGrounded}");
            _planeState.ToggleLandingGear(statsConfig.HasRetractableGear);
        }

        public void RegisterWeatherZoneEnter()
        {
            _weatherZoneContacts++;
        }

        public void RegisterWeatherZoneExit()
        {
            _weatherZoneContacts = Mathf.Max(0, _weatherZoneContacts - 1);
        }

        private void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;

            _planeState.SetGroundedState(Physics.Raycast(transform.position, -transform.up, groundCheckDistance, groundMask));
            CalculatePlaneState(dt);
            _physicsUseCase.Execute(_planeState, dt);
        }

        private void CalculatePlaneState(float dt) {
            var quaternion = Quaternion.Inverse(_rb.rotation);
            var localGForce = quaternion * ((_rb.linearVelocity - _lastVelocity) / dt);
            _planeState.SyncPhysicsState(
                _rb.linearVelocity, 
                quaternion * _rb.linearVelocity, 
                quaternion * _rb.angularVelocity,
                localGForce
            );
            _lastVelocity = _rb.linearVelocity;
        }

        public void ApplyRelativeForce(Vector3 force) => _rb.AddRelativeForce(force);
        public void ApplyRelativeTorque(Vector3 torque, ForceMode mode) => _rb.AddRelativeTorque(torque, mode);
        public void ApplyTransformDirection(Vector3 direction) => _rb.linearVelocity = transform.TransformDirection(direction);

        public void ForceCrash()
        {
            if (_crashUseCase != null && !_planeState.isCrashed)
            {
                _crashUseCase.Execute(_planeState);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (statsConfig == null) return;
            
            // Si el impacto físico excede el umbral de velocidad, el avión explota
            if (collision.relativeVelocity.magnitude >= statsConfig.CrashVelocityThreshold)
            {
                Debug.Log($"[PlaneController] Colisión detectada con {collision.gameObject.name}. Velocidad de impacto: {collision.relativeVelocity.magnitude:F2} m/s (Umbral: {statsConfig.CrashVelocityThreshold} m/s)");
                _crashUseCase.Execute(_planeState);
            }
        }

        public PlaneState GetState() => _planeState;
    }
}
