using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlightSystem.Framework.Visuals
{
    public class TimeframeExplosion : MonoBehaviour
    {
        [Header("Configuración de Crecimiento (Explosión)")]
        [Tooltip("Multiplicador del tamaño final de la explosión respecto a su tamaño original en el editor")]
        [SerializeField] private float sizeMultiplier = 4f;
        
        [Tooltip("Duración de la fase de crecimiento (segundos)")]
        [SerializeField] private float growthDuration = 0.4f;

        [Tooltip("Curva para el crecimiento (facilita configurar una expansión rápida al principio)")]
        [SerializeField] private AnimationCurve growthCurve;

        [Header("Fase de Permanencia y Encogimiento")]
        [Tooltip("Tiempo que permanece en la escala máxima")]
        [SerializeField] private float holdDuration = 0.8f;

        [Tooltip("Duración de la fase de encogimiento para desaparecer (segundos)")]
        [SerializeField] private float shrinkDuration = 0.4f;

        [Tooltip("Curva para el encogimiento (por defecto va de 1 a 0)")]
        [SerializeField] private AnimationCurve shrinkCurve;

        [Tooltip("Destruir el objeto cuando termine todo el ciclo")]
        [SerializeField] private bool destroyOnComplete = true;

        [Header("Efectos Adicionales (Humo/Partículas)")]
        [Tooltip("Prefab adicional de humo o partículas que aparecerá al mismo tiempo")]
        [SerializeField] private GameObject additionalSmokePrefab;

        [Tooltip("Si está activo, el humo adicional escalará junto con la explosión. Si no, tendrá su tamaño original.")]
        [SerializeField] private bool scaleAdditionalSmokeWithExplosion = false;

        private enum ExplosionState
        {
            Growing,
            Holding,
            Shrinking,
            Completed
        }

        private ExplosionState _currentState = ExplosionState.Growing;
        private float _stateTimer = 0f;
        private GameObject _spawnedSmoke;

        private Vector3 _startScale;
        private Vector3 _targetScale;

        private void Awake()
        {
            // 1. Apagar y destruir animadores en el prefab principal para evitar conflictos con la escala
            DisableAnimators();

            // 2. Determinar las escalas basadas en la escala actual del objeto en el editor
            Vector3 originalScale = transform.localScale;
            _startScale = Vector3.zero;
            _targetScale = originalScale * sizeMultiplier;

            // Iniciar en escala cero
            transform.localScale = _startScale;

            // 3. Inicializar curvas si están vacías en el Inspector
            InitializeCurves();

            // 4. Instanciar el humo adicional (sus animadores no serán destruidos porque se crean después)
            if (additionalSmokePrefab != null)
            {
                if (scaleAdditionalSmokeWithExplosion)
                {
                    _spawnedSmoke = Instantiate(additionalSmokePrefab, transform.position, transform.rotation, transform);
                }
                else
                {
                    _spawnedSmoke = Instantiate(additionalSmokePrefab, transform.position, transform.rotation);
                }
            }
        }

        private void InitializeCurves()
        {
            // Curva de crecimiento: Inicio rápido y explosivo, se asienta al final
            if (growthCurve == null || growthCurve.keys.Length == 0)
            {
                growthCurve = new AnimationCurve();
                growthCurve.AddKey(new Keyframe(0f, 0f, 3f, 3f)); // Pendiente de entrada empinada
                growthCurve.AddKey(new Keyframe(1f, 1f, 0f, 0f)); // Desaceleración al final
            }

            // Curva de encogimiento: Va de 1 a 0 suavemente
            if (shrinkCurve == null || shrinkCurve.keys.Length == 0)
            {
                shrinkCurve = new AnimationCurve();
                shrinkCurve.AddKey(new Keyframe(0f, 1f, 0f, 0f));
                shrinkCurve.AddKey(new Keyframe(1f, 0f, -3f, -3f));
            }
        }

        private void Update()
        {
            _stateTimer += Time.deltaTime;

            switch (_currentState)
            {
                case ExplosionState.Growing:
                    if (growthDuration > 0f)
                    {
                        float t = Mathf.Clamp01(_stateTimer / growthDuration);
                        float curveValue = growthCurve.Evaluate(t);
                        transform.localScale = Vector3.LerpUnclamped(_startScale, _targetScale, curveValue);

                        if (_stateTimer >= growthDuration)
                        {
                            transform.localScale = _targetScale;
                            _currentState = ExplosionState.Holding;
                            _stateTimer = 0f;
                        }
                    }
                    else
                    {
                        transform.localScale = _targetScale;
                        _currentState = ExplosionState.Holding;
                        _stateTimer = 0f;
                    }
                    break;

                case ExplosionState.Holding:
                    transform.localScale = _targetScale;
                    if (_stateTimer >= holdDuration)
                    {
                        _currentState = ExplosionState.Shrinking;
                        _stateTimer = 0f;
                    }
                    break;

                case ExplosionState.Shrinking:
                    if (shrinkDuration > 0f)
                    {
                        float t = Mathf.Clamp01(_stateTimer / shrinkDuration);
                        float curveValue = shrinkCurve.Evaluate(t);
                        transform.localScale = Vector3.LerpUnclamped(Vector3.zero, _targetScale, curveValue);

                        if (_stateTimer >= shrinkDuration)
                        {
                            transform.localScale = Vector3.zero;
                            _currentState = ExplosionState.Completed;
                        }
                    }
                    else
                    {
                        _currentState = ExplosionState.Completed;
                    }
                    break;

                case ExplosionState.Completed:
                    if (destroyOnComplete)
                    {
                        Destroy(gameObject);
                    }
                    else
                    {
                        enabled = false;
                    }
                    break;
            }
        }

        private void DisableAnimators()
        {
            Animator[] animators = GetComponentsInChildren<Animator>(true);
            foreach (var anim in animators)
            {
                anim.enabled = false;
                anim.runtimeAnimatorController = null;
                Destroy(anim);
                Debug.Log($"[TimeframeExplosion] Desactivado y destruido Animator en {anim.gameObject.name} para evitar conflictos de escala.");
            }
        }

        private void OnDestroy()
        {
            // Si el humo adicional se instanció como independiente, nos aseguramos de limpiarlo aquí
            if (!scaleAdditionalSmokeWithExplosion && _spawnedSmoke != null)
            {
                Destroy(_spawnedSmoke);
            }
        }
    }
}
