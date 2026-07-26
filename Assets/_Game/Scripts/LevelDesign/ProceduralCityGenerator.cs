using System.Collections.Generic;
using UnityEngine;
using MissionSystem.Adapters;

namespace AeroByte.LevelDesign
{
    public class ProceduralCityGenerator : MonoBehaviour
    {
        [Header("Referencias de Edificios")]
        [Tooltip("Arrastra aquí los prefabs de los edificios que formarán la ciudad.")]
        public GameObject[] buildingPrefabs;

        [Header("Dimensiones de la Ciudad")]
        [Tooltip("Longitud de la ciudad (Eje Z). Distancia hacia la meta.")]
        public float cityLength = 10000f;
        [Tooltip("Ancho de la ciudad (Eje X).")]
        public float cityWidth = 4000f;
        
        [Header("Configuración de Cuadrícula")]
        [Tooltip("Tamaño de cada bloque o manzana (espacio entre edificios).")]
        public float gridSize = 200f;
        [Tooltip("Probabilidad (0 a 1) de que aparezca un edificio en una celda. Un valor de 0.75 deja un 25% de huecos (plazas).")]
        [Range(0f, 1f)]
        public float buildingDensity = 0.85f;

        [Header("Variación de Altura")]
        public float minHeightScale = 1f;
        public float maxHeightScale = 5f;

        [Header("Ajuste de OOB Automático")]
        [Tooltip("Si es true, el script ajustará automáticamente el collider del OutOfBoundsZone para rodear la ciudad.")]
        public bool autoConfigureOutOfBounds = true;

        [Header("Corrección Blender")]
        [Tooltip("Activa esto si los techos apuntan hacia el eje Z. Rotará los modelos -90 en X y los escalará en Z.")]
        public bool applyBlenderFix = true;

        private void Start()
        {
            GenerateCity();
            
            if (autoConfigureOutOfBounds)
            {
                ConfigureOutOfBounds();
            }
        }

        private void GenerateCity()
        {
            if (buildingPrefabs == null || buildingPrefabs.Length == 0)
            {
                Debug.LogError("[ProceduralCityGenerator] No has asignado ningún Prefab de edificio.");
                return;
            }

            if (gridSize <= 0.1f)
            {
                Debug.LogError("[ProceduralCityGenerator] ¡Error Crítico! El 'Grid Size' es demasiado pequeño o cero. Se forzó a 50 para evitar que Unity se congele.");
                gridSize = 50f;
            }

            int buildingsSpawned = 0;
            int maxBuildingsAllowed = 15000; // Límite de seguridad
            
            Transform cityParent = new GameObject("City_Buildings").transform;
            cityParent.SetParent(this.transform);

            float halfWidth = cityWidth / 2f;

            // Iteramos sobre la cuadrícula (X y Z)
            for (float z = 0; z < cityLength; z += gridSize)
            {
                for (float x = -halfWidth; x <= halfWidth; x += gridSize)
                {
                    if (buildingsSpawned >= maxBuildingsAllowed)
                    {
                        Debug.LogError("[ProceduralCityGenerator] LÍMITE DE SEGURIDAD ALCANZADO. Se intentó generar demasiados edificios y se detuvo para que Unity no colapse. Aumenta el 'Grid Size' o reduce las dimensiones de la ciudad.");
                        return;
                    }

                    // Dejamos un "pasillo" central más despejado opcionalmente, 
                    // o simplemente usamos probabilidad pura.
                    if (Random.value <= buildingDensity)
                    {
                        SpawnBuilding(new Vector3(x, 0, z), cityParent);
                        buildingsSpawned++;
                    }
                }
            }

            Debug.Log($"[ProceduralCityGenerator] Ciudad generada con éxito. {buildingsSpawned} edificios construidos.");
        }

        private void SpawnBuilding(Vector3 position, Transform parent)
        {
            // Seleccionar edificio aleatorio
            GameObject prefab = buildingPrefabs[Random.Range(0, buildingPrefabs.Length)];
            
            // Instanciar
            GameObject building = Instantiate(prefab, position, Quaternion.identity, parent);
            
            // Rotación aleatoria en Y (espacio de mundo)
            float randomYRot = Random.Range(0, 4) * 90f;
            float randomScaleHeight = Random.Range(minHeightScale, maxHeightScale);

            if (applyBlenderFix)
            {
                // Endereza el edificio rotando -90 en X, y aplica la rotación Y aleatoria
                building.transform.rotation = Quaternion.Euler(-90f, randomYRot, 0f);
                
                // En modelos exportados de Blender, la altura local suele ser el eje Z
                building.transform.localScale = new Vector3(
                    building.transform.localScale.x, 
                    building.transform.localScale.y, 
                    building.transform.localScale.z * randomScaleHeight
                );
            }
            else
            {
                building.transform.rotation = Quaternion.Euler(0f, randomYRot, 0f);
                
                // En modelos nativos de Unity, la altura local es el eje Y
                building.transform.localScale = new Vector3(
                    building.transform.localScale.x, 
                    building.transform.localScale.y * randomScaleHeight, 
                    building.transform.localScale.z
                );
            }
        }

        private void ConfigureOutOfBounds()
        {
            var oobZone = FindFirstObjectByType<OutOfBoundsZone>();
            if (oobZone != null)
            {
                var col = oobZone.GetComponent<BoxCollider>();
                if (col != null)
                {
                    // Centrar el OOB en medio de la ciudad generada
                    oobZone.transform.position = new Vector3(0, 2000f, cityLength / 2f);
                    
                    // Escalar el BoxCollider para rodear la ciudad
                    // Le damos altura suficiente y un poco más de margen en ancho/largo
                    col.size = new Vector3(cityWidth + 500f, 6000f, cityLength + 1000f);
                    
                    Debug.Log("[ProceduralCityGenerator] Zona OOB configurada automáticamente.");
                }
            }
            else
            {
                Debug.LogWarning("[ProceduralCityGenerator] No se encontró un OutOfBoundsZone en la escena para configurar.");
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.5f); // Azul claro semi-transparente
            
            // Dibujar el contorno general de la ciudad
            Vector3 center = transform.position + new Vector3(0, 0, cityLength / 2f);
            Vector3 size = new Vector3(cityWidth, 50f, cityLength);
            Gizmos.DrawWireCube(center, size);

            // Dibujar una línea central hacia la meta
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + new Vector3(0, 0, cityLength));
            
            // Dibujar el punto final (La Meta)
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position + new Vector3(0, 0, cityLength), 20f);
        }
#endif
    }
}
