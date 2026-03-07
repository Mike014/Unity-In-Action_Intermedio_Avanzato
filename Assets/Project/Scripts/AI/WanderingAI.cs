using UnityEngine;

[RequireComponent(typeof(WanderingAI))]
[RequireComponent(typeof(ReactiveTarget))]
public class WanderingAI : MonoBehaviour
{
    [SerializeField, Range(0.1f, 10f)] private float _speed = 3.0f;
    [SerializeField, Range(0.1f, 10f)] private float _obstacleRange = 5.0f;
    [SerializeField, Range(.1f, 2f)] private float _sphereRadius = 0.75f;

    // Lo stato è privato - solo SetAlive() può modificarlo dall'esterno
    // Nessun altro script dovrebbe leggere o scrivere _alive direttamente
    private bool _alive = true;

    // Cache del transform - buona abitudine in Update()
    private Transform _transform;

    private void Awake()
    {
        _transform = transform;
    }

    void Update()
    {
        // Guardia in cima - se morto, nessun codice viene eseguito
        // Più leggibile di wrappare tutto in un if.
        if (!_alive) return;

        // Movimento in avanti — frame rate independent.
        transform.Translate(0, 0, _speed * Time.deltaTime);

        // SphereCast: rileva ostacoli con un raggio sferico.
        // Più robusto di Raycast per personaggi con larghezza fisica.
        Ray ray = new Ray(_transform.position, _transform.forward);

        if (_alive)
        {
            if (Physics.SphereCast(ray, _sphereRadius, out RaycastHit hit))
            {
                if (hit.distance < _obstacleRange)
                {
                    // Random.Range con int è esclusivo sull'estremo superiore.
                    // Con float è inclusivo su entrambi gli estremi.
                    float angle = Random.Range(-110, 110);
                    _transform.Rotate(0, angle, 0);
                }
            }
        }
    }

    // Interfaccia pubblica — unico punto di accesso allo stato.
    // ReactiveTarget chiama questo metodo, WanderingAI non sa nulla di ReactiveTarget.
    public void SetAliveBool(bool alive)
    {
        _alive = alive;
    }
}
