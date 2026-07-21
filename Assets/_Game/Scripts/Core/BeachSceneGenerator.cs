using UnityEngine;

namespace AeroByte.Core
{
    public class BeachSceneGenerator : MonoBehaviour
    {
        [Header("Terrain Settings")]
        public int terrainSize = 500;
        public float terrainHeight = 30f;
        public Material sandMaterial;

        [Header("Water")]
        public GameObject waterPrefab;
        public float waterHeight = -0.3f;

        [Header("Decorations")]
        public GameObject[] palmPrefabs;
        public int palmCount = 30;
        public GameObject[] beachProps;
        public int propCount = 15;
        public GameObject boatPrefab;
        public int boatCount = 2;

        [Header("Generate on Start")]
        public bool generateOnStart = true;

        private void Start()
        {
            if (generateOnStart)
                Generate();
        }

        [ContextMenu("Generate Beach")]
        public void Generate()
        {
            ClearGenerated();

            GenerateTerrain();
            GenerateWater();
            GeneratePalms();
            GenerateProps();
            GenerateBoats();

            Debug.Log("Beach scene generated!");
        }

        void ClearGenerated()
        {
            var root = transform.Find("_Generated");
            if (root != null)
                DestroyImmediate(root.gameObject);
        }

        void GenerateTerrain()
        {
            var root = new GameObject("_Generated").transform;
            root.SetParent(transform);

            var terrainGO = GameObject.CreatePrimitive(PrimitiveType.Plane);
            terrainGO.name = "Terreno_Playa";
            terrainGO.transform.SetParent(root);
            terrainGO.transform.localScale = new Vector3(terrainSize / 10f, 1, terrainSize / 10f);
            terrainGO.transform.position = new Vector3(0, 0, 0);

            if (sandMaterial != null)
                terrainGO.GetComponent<Renderer>().material = sandMaterial;
        }

        void GenerateWater()
        {
            var root = transform.Find("_Generated");
            if (waterPrefab != null)
            {
                var water = Instantiate(waterPrefab, root);
                water.name = "Oceano";
                water.transform.position = new Vector3(0, waterHeight, 0);
            }
            else
            {
                var water = GameObject.CreatePrimitive(PrimitiveType.Plane);
                water.name = "Oceano_Simple";
                water.transform.SetParent(root);
                water.transform.position = new Vector3(0, waterHeight, 0);
                water.transform.localScale = new Vector3(terrainSize / 5f, 1, terrainSize / 5f);
                water.GetComponent<Renderer>().material.color = new Color(0.2f, 0.5f, 0.8f, 0.8f);
            }
        }

        void GeneratePalms()
        {
            if (palmPrefabs == null || palmPrefabs.Length == 0) return;

            var root = transform.Find("_Generated");
            var palmRoot = new GameObject("Palmeras").transform;
            palmRoot.SetParent(root);

            for (int i = 0; i < palmCount; i++)
            {
                var prefab = palmPrefabs[Random.Range(0, palmPrefabs.Length)];
                if (prefab == null) continue;

                var angle = Random.Range(0f, Mathf.PI * 2f);
                var dist = Random.Range(terrainSize * 0.05f, terrainSize * 0.35f);
                var x = Mathf.Cos(angle) * dist;
                var z = Mathf.Sin(angle) * dist;
                var y = Mathf.PerlinNoise(x * 0.01f + 50f, z * 0.01f + 50f) * 2f;

                var palm = Instantiate(prefab, palmRoot);
                palm.transform.position = new Vector3(x, y, z);
                palm.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                var scale = Random.Range(0.7f, 1.3f);
                palm.transform.localScale = new Vector3(scale, scale, scale);
            }
        }

        void GenerateProps()
        {
            if (beachProps == null || beachProps.Length == 0) return;

            var root = transform.Find("_Generated");
            var propRoot = new GameObject("Props_Playa").transform;
            propRoot.SetParent(root);

            for (int i = 0; i < propCount; i++)
            {
                var prefab = beachProps[Random.Range(0, beachProps.Length)];
                if (prefab == null) continue;

                var x = Random.Range(-terrainSize * 0.1f, terrainSize * 0.1f);
                var z = Random.Range(-terrainSize * 0.1f, terrainSize * 0.1f);
                var y = Mathf.PerlinNoise(x * 0.01f + 50f, z * 0.01f + 50f) * 2f + 0.1f;

                var prop = Instantiate(prefab, propRoot);
                prop.transform.position = new Vector3(x, y, z);
                prop.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            }
        }

        void GenerateBoats()
        {
            if (boatPrefab == null || boatCount == 0) return;

            var root = transform.Find("_Generated");
            var boatRoot = new GameObject("Barcos").transform;
            boatRoot.SetParent(root);

            for (int i = 0; i < boatCount; i++)
            {
                var angle = Random.Range(0f, Mathf.PI * 2f);
                var dist = Random.Range(terrainSize * 0.15f, terrainSize * 0.35f);
                var x = Mathf.Cos(angle) * dist;
                var z = Mathf.Sin(angle) * dist;

                var boat = Instantiate(boatPrefab, boatRoot);
                boat.transform.position = new Vector3(x, waterHeight + 0.2f, z);
                boat.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            }
        }
    }
}
