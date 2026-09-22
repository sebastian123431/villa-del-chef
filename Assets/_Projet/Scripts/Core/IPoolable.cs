namespace VillaDelChef.Core
{
    /// <summary>
    /// Interfaz para objetos reciclados a través del ObjectPoolManager.
    /// Permite limpiar y rearmar estados sin generar basura de memoria (GC allocations).
    /// </summary>
    public interface IPoolable
    {
        void OnSpawnFromPool();
        void OnReturnToPool();
    }
}
