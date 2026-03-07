using UnityEngine;

public class SceneController : Singleton<SceneController>
{
    [SerializeField] private GameObject _enemyPrefab;
    private GameObject _enemy;

    public override void Awake()
    {
        base.Awake();
    }

    private void Update()
    {
        SpawnEnemyIfNeeded();
    }

    // Metodo con nome esplicito — comunica l'intenzione meglio di InstantiatePrefab().
    // "Spawn if needed" descrive QUANDO e PERCHÉ, non solo COSA fa.
    public void SpawnEnemyIfNeeded()
    {
        if (_enemy != null) return;

        // Null check sul prefab — se non è assegnato nell'Inspector,
        // LogError immediato invece di NullReferenceException silenziosa.
        if (_enemyPrefab == null)
        {
            Debug.LogError("[SceneController] _enemyPrefab non assegnato nell'inspector!");
            return;
        }

        // Instantiate<T> generico — type-safe, nessun cast necessario.
        // Preferibile a Instantiate(...) as GameObject.
        _enemy = Instantiate(_enemyPrefab);

        _enemy.transform.position = new Vector3(0f, 1f, 0f);
        _enemy.transform.Rotate(0f, Random.Range(0f, 360f), 0f);
    }
}


