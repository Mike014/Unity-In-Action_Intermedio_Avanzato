using UnityEngine;

public class LifecycleDemo : MonoBehaviour
{
    // Awake → PRIMA di Start, anche se il componente è disabilitato.
    // Usalo per inizializzare riferimenti interni (dipendenze proprie).
    private void Awake()
    {
        // Inizializza ciò che NON dipende da altri componenti.
    }

    // OnEnable → ogni volta che il componente viene riattivato.
    // Utile per sottoscrivere eventi.
    private void OnEnable()
    {
        // GameEvents.OnPlayerDied += HandlePlayerDied;
    }

    private void Start()
    {
        // Inizializza ciò che DIPENDE da altri componenti.
        // A questo punto tutti gli Awake() sono già stati eseguiti.
    }

    private void Update()
    {
        // Logica frame-by-frame (input, movimento non fisico).
    }

    private void FixedUpdate()
    {
        // Fisica. Chiamato a intervalli FISSI (default 0.02s).
        // Usa sempre questo per Rigidbody, mai Update.
    }

    private void LateUpdate()
    {
        // Chiamato DOPO tutti gli Update.
        // Classico uso: follow camera, in modo che il personaggio
        // abbia già aggiornato la sua posizione.
    }

    // OnDisable → speculare a OnEnable.
    // SEMPRE disiscrivi gli eventi qui per evitare memory leak.
    private void OnDisable()
    {
        // GameEvents.OnPlayerDied -= HandlePlayerDied;
    }

    // OnDestroy → cleanup finale.
    private void OnDestroy()
    {
        // Rilascia risorse non gestite dal GC.
    }
}