using System.Collections.Generic;
using UnityEngine;

namespace VillaDelChef.Core
{
    /// <summary>
    /// Administrador central de Object Pooling para optimización móvil (60 FPS estables).
    /// Elimina las llamadas continuas a Instantiate y Destroy para entidades frecuentes
    /// (comensales, textos flotantes, monedas, efectos de partículas) reduciendo a cero los picos de GC.
    /// </summary>
    public class ObjectPoolManager : MonoBehaviour
    {
        public static ObjectPoolManager Instance { get; private set; }

        private readonly Dictionary<string, Queue<GameObject>> pools = new Dictionary<string, Queue<GameObject>>();
        private readonly Dictionary<string, GameObject> prefabTemplates = new Dictionary<string, GameObject>();
        private readonly Dictionary<string, Transform> poolContainers = new Dictionary<string, Transform>();
        private readonly Dictionary<GameObject, string> instanceToPoolKey = new Dictionary<GameObject, string>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Precalienta el pool instanciando previamente objetos inactivos para evitar caídas de frames durante el juego.
        /// </summary>
        public void Prewarm(string poolKey, GameObject prefab, int initialCount)
        {
            if (prefab == null || string.IsNullOrEmpty(poolKey)) return;

            if (!prefabTemplates.ContainsKey(poolKey))
            {
                prefabTemplates[poolKey] = prefab;
            }

            Queue<GameObject> queue = GetOrCreateQueue(poolKey);
            Transform container = GetOrCreateContainer(poolKey);

            for (int i = 0; i < initialCount; i++)
            {
                GameObject obj = Instantiate(prefab, container);
                obj.name = $"{prefab.name}_Pooled_{i}";
                obj.SetActive(false);
                instanceToPoolKey[obj] = poolKey;
                queue.Enqueue(obj);
            }
        }

        /// <summary>
        /// Obtiene un objeto reciclado del pool o instancia uno nuevo si la cola está vacía.
        /// </summary>
        public GameObject Spawn(string poolKey, Vector3 position, Quaternion rotation, GameObject fallbackPrefab = null)
        {
            if (string.IsNullOrEmpty(poolKey)) return null;

            Queue<GameObject> queue = GetOrCreateQueue(poolKey);
            GameObject obj = null;

            while (queue.Count > 0)
            {
                obj = queue.Dequeue();
                if (obj != null) break;
            }

            if (obj == null)
            {
                GameObject template = null;
                if (prefabTemplates.TryGetValue(poolKey, out GameObject t))
                {
                    template = t;
                }
                else if (fallbackPrefab != null)
                {
                    template = fallbackPrefab;
                    prefabTemplates[poolKey] = fallbackPrefab;
                }

                if (template != null)
                {
                    Transform container = GetOrCreateContainer(poolKey);
                    obj = Instantiate(template, position, rotation, container);
                    obj.name = $"{template.name}_Pooled";
                }
                else
                {
                    Debug.LogWarning($"[ObjectPoolManager] No hay prefab registrado para la clave de pool '{poolKey}'.");
                    return null;
                }
            }
            else
            {
                obj.transform.position = position;
                obj.transform.rotation = rotation;
            }

            instanceToPoolKey[obj] = poolKey;
            obj.SetActive(true);

            // Notificar a componentes que implementen IPoolable
            IPoolable[] poolables = obj.GetComponentsInChildren<IPoolable>(true);
            for (int i = 0; i < poolables.Length; i++)
            {
                poolables[i].OnSpawnFromPool();
            }

            return obj;
        }

        /// <summary>
        /// Devuelve un objeto al pool correspondiente, desactivándolo y limpiando su estado.
        /// </summary>
        public void Despawn(string poolKey, GameObject instance)
        {
            if (instance == null) return;

            // Notificar retorno a componentes IPoolable
            IPoolable[] poolables = instance.GetComponentsInChildren<IPoolable>(true);
            for (int i = 0; i < poolables.Length; i++)
            {
                poolables[i].OnReturnToPool();
            }

            instance.SetActive(false);

            Transform container = GetOrCreateContainer(poolKey);
            instance.transform.SetParent(container);

            Queue<GameObject> queue = GetOrCreateQueue(poolKey);
            queue.Enqueue(instance);
        }

        /// <summary>
        /// Devuelve un objeto al pool deduciendo automáticamente su clave registrada.
        /// </summary>
        public void Despawn(GameObject instance)
        {
            if (instance == null) return;

            if (instanceToPoolKey.TryGetValue(instance, out string poolKey))
            {
                Despawn(poolKey, instance);
            }
            else
            {
                instance.SetActive(false);
            }
        }

        private Queue<GameObject> GetOrCreateQueue(string poolKey)
        {
            if (!pools.TryGetValue(poolKey, out Queue<GameObject> queue))
            {
                queue = new Queue<GameObject>();
                pools[poolKey] = queue;
            }
            return queue;
        }

        private Transform GetOrCreateContainer(string poolKey)
        {
            if (!poolContainers.TryGetValue(poolKey, out Transform container) || container == null)
            {
                GameObject containerObj = new GameObject($"-- Pool [{poolKey}] --");
                containerObj.transform.SetParent(transform);
                container = containerObj.transform;
                poolContainers[poolKey] = container;
            }
            return container;
        }
    }
}
