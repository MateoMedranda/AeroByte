using UnityEngine;
using WeatherSystem.Domain.Interfaces;

namespace AeroByte.WeatherSystem.Framework.Config
{
    [CreateAssetMenu(fileName = "WeatherStats", menuName = "WeatherSystem/Weather Stats Config", order = 0)]
    public class WeatherStatsConfig : ScriptableObject, IWeatherStats 
    {
        [Header("Viento Base")]
        [Tooltip("Dirección y fuerza del viento constante en Newtons (ej: Viento cruzado hacia el Este)")]
        [SerializeField] private Vector3 constantWindVelocity;

        [Header("Turbulencia y Ráfagas (Perlin Noise)")]
        [Tooltip("Fuerza multiplicadora de los 'baches de aire' o ráfagas repentinas")]
        [SerializeField] private float windGustIntensity = 5000f;
        
        [Tooltip("Fuerza rotacional que hace temblar/cabecear al avión de forma aleatoria")]
        [SerializeField] private float turbulenceTorqueIntensity = 2000f;

        [Tooltip("Escala del ruido. Menor = vientos largos y suaves. Mayor = temblor rápido y caótico")]
        [SerializeField] private float noiseScale = 0.1f;

        // Implementación de la Interfaz
        public Vector3 ConstantWindVelocity => constantWindVelocity;
        public float WindGustIntensity => windGustIntensity;
        public float TurbulenceTorqueIntensity => turbulenceTorqueIntensity;
        public float NoiseScale => noiseScale;
    }
}
