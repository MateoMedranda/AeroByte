using UnityEngine;
using WeatherSystem.UseCases;
using AeroByte.WeatherSystem.Framework.Config;
using FlightSystem.Adapters; // Para encontrar el PlaneController

namespace WeatherSystem.Adapters
{
    [RequireComponent(typeof(BoxCollider))]
    public class WeatherZoneController : MonoBehaviour
    {
        [Header("Configuración del Clima")]
        public WeatherStatsConfig weatherConfig;

        private WeatherPhysicsUseCase _weatherUseCase;

        private void Awake()
        {
            GetComponent<BoxCollider>().isTrigger = true;
            _weatherUseCase = new WeatherPhysicsUseCase(weatherConfig);
        }

        private void OnTriggerStay(Collider other)
        {
            PlaneController plane = other.GetComponentInParent<PlaneController>();
            
            if (plane != null)
            {
                _weatherUseCase.ApplyWeather(plane, plane.transform.position, Time.time);
            }
        }
    }
}
