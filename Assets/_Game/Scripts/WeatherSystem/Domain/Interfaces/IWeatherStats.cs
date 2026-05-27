using UnityEngine;

namespace WeatherSystem.Domain.Interfaces
{
    public interface IWeatherStats 
    {
        Vector3 ConstantWindVelocity { get; }
        float WindGustIntensity { get; }
        float TurbulenceTorqueIntensity { get; }
        float NoiseScale { get; }
    }
}
