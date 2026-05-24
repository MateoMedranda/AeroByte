using UnityEngine;
using WeatherSystem.Domain.Interfaces;
using FlightSystem.Domain.Interfaces; // Aprovechamos el Gateway que ya hicimos

namespace WeatherSystem.UseCases
{
    public class WeatherPhysicsUseCase
    {
        private readonly IWeatherStats _stats;

        public WeatherPhysicsUseCase(IWeatherStats stats)
        {
            _stats = stats;
        }

        // El clima inyecta fuerzas usando el IPhysicsGateway del avión
        public void ApplyWeather(IPhysicsGateway planePhysics, Vector3 planePosition, float time)
        {
            // 1. Viento Constante (Desplazamiento)
            planePhysics.ApplyRelativeForce(_stats.ConstantWindVelocity);

            // 2. Ráfagas Aleatorias (Ruido de Perlin basado en el tiempo y la posición para que sea "orgánico")
            float noiseX = Mathf.PerlinNoise(planePosition.x * _stats.NoiseScale, time) - 0.5f;
            float noiseY = Mathf.PerlinNoise(planePosition.y * _stats.NoiseScale, time) - 0.5f;
            float noiseZ = Mathf.PerlinNoise(planePosition.z * _stats.NoiseScale, time) - 0.5f;
            
            // Multiplicamos por 2f porque (Noise - 0.5f) va de -0.5 a 0.5. Así lo hacemos de -1 a 1.
            Vector3 gustForce = new Vector3(noiseX, noiseY, noiseZ) * 2f * _stats.WindGustIntensity;
            planePhysics.ApplyRelativeForce(gustForce);

            // 3. Turbulencia (Torque aleatorio que hace "temblar" la nariz del avión)
            float tNoiseX = Mathf.PerlinNoise(time, planePosition.x * _stats.NoiseScale) - 0.5f;
            float tNoiseY = Mathf.PerlinNoise(time, planePosition.y * _stats.NoiseScale) - 0.5f;
            float tNoiseZ = Mathf.PerlinNoise(time, planePosition.z * _stats.NoiseScale) - 0.5f;

            Vector3 turbulenceTorque = new Vector3(tNoiseX, tNoiseY, tNoiseZ) * 2f * _stats.TurbulenceTorqueIntensity;
            planePhysics.ApplyRelativeTorque(turbulenceTorque, ForceMode.Force);
        }
    }
}
