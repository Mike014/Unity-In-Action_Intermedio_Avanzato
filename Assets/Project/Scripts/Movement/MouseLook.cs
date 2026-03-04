using UnityEngine;

// Separare la responsabilità: questo script gestisce SOLO la rotazione della camera.
// Il movimento del player è competenza di un altro componente.
// Single Responsibility Principle applicato a Unity.
[DisallowMultipleComponent]
public class MouseLook : MonoBehaviour
{
    // Enum come tipo esplicito — evita magic numbers e appare
    // come dropdown nell'Inspector. Valore di default = entrambi gli assi,
    // che è il caso d'uso più comune in un FPS.
    public enum RotationAxes { MouseXAndY, MouseX, MouseY }

    [SerializeField] private RotationAxes _axes = RotationAxes.MouseXAndY;

    // Header raggruppa visivamente le variabili nell'Inspector.
    // Utile quando il componente ha molti campi serializzati.
    [Header("Sensitivity")]
    [SerializeField, Range(0.1f, 20f)] private float _sensitivityHor  = 9f;
    [SerializeField, Range(0.1f, 20f)] private float _sensitivityVert = 9f;

    [Header("Vertical Limits")]
    [SerializeField, Range(-90f, 0f)]  private float _minimumVert = -45f;
    [SerializeField, Range(0f,  90f)]  private float _maximumVert =  90f;

    // Stato interno — non esposto. Solo questo script deve conoscerlo.
    private float _rotationX = 0f;

    // Cache del transform: evitare di chiamare la property "transform"
    // ogni frame è una micro-ottimizzazione, ma è buona abitudine
    // in componenti che operano in Update().
    private Transform _transform;

    private void Awake()
    {
        _transform = transform;

        // FreezeRotation: se esiste un Rigidbody, la simulazione fisica
        // non deve sovrascrivere la rotazione controllata dal mouse.
        // TryGetComponent è preferibile a GetComponent + null check:
        // non alloca in caso di fallimento e comunica l'intento chiaramente.
        if (TryGetComponent(out Rigidbody rb))
            rb.freezeRotation = true;
    }

    private void Update()
    {
        switch (_axes)
        {
            case RotationAxes.MouseX:
                HandleHorizontal();
                break;

            case RotationAxes.MouseY:
                HandleVertical();
                break;

            default:
                HandleBoth();
                break;
        }
    }

    // Rotazione orizzontale — delega a Rotate() perché non ha limiti.
    // Space.Self: ruota attorno all'asse Y locale dell'oggetto,
    // non quello globale della scena. Corretto per un player FPS.
    private void HandleHorizontal()
    {
        float delta = Input.GetAxis("Mouse X") * _sensitivityHor;
        _transform.Rotate(0f, delta, 0f, Space.Self);
    }

    // Rotazione verticale — non usa Rotate() perché deve essere
    // clamped. Si manipola l'angolo direttamente come float,
    // poi si riassegna l'intero Vector3 agli EulerAngles.
    // ATTENZIONE: localEulerAngles è read-only per singolo asse —
    // devi sempre assegnare un nuovo Vector3 completo.
    private void HandleVertical()
    {
        // -= perché Mouse Y positivo = mouse su = pitch negativo (guarda in su).
        // Senza il segno invertito, il mouse sarebbe "invertito" verticalmente.
        _rotationX -= Input.GetAxis("Mouse Y") * _sensitivityVert;

        // Clamp impedisce di guardare oltre i limiti definiti.
        // Senza di esso potresti ruotare completamente a testa in giù.
        _rotationX = Mathf.Clamp(_rotationX, _minimumVert, _maximumVert);
        
        // Debug
        // Debug.DrawLine(this.transform.position, transform.forward * 10f, Color.blue);
        // Debug.Log($"_rotationX è: {_rotationX}");

        // Leggiamo la Y corrente per non azzerarla durante l'assegnazione.
        float rotationY = _transform.localEulerAngles.y;
        _transform.localEulerAngles = new Vector3(_rotationX, rotationY, 0f);
    }

    // Entrambi gli assi: combina la logica verticale (con clamp)
    // e calcola la nuova Y orizzontale manualmente invece di usare Rotate().
    // Delta = "quantità di cambiamento" — termine matematico standard.
    private void HandleBoth()
    {
        _rotationX -= Input.GetAxis("Mouse Y") * _sensitivityVert;
        _rotationX  = Mathf.Clamp(_rotationX, _minimumVert, _maximumVert);

        float delta     = Input.GetAxis("Mouse X") * _sensitivityHor;
        float rotationY = _transform.localEulerAngles.y + delta;

        _transform.localEulerAngles = new Vector3(_rotationX, rotationY, 0f);
    }
}

/*
Le differenze sostanziali rispetto all'originale
if/else if/else → switch — più leggibile quando i casi sono enumerati esplicitamente.
GetComponent<Rigidbody>() → TryGetComponent — non alloca memoria in caso di componente assente.
[Range] sui float — previene valori assurdi direttamente nell'Inspector, senza bisogno di validazione a runtime.
Logica estratta in metodi privati — HandleHorizontal, HandleVertical, HandleBoth — invece di un unico Update() monolitico. Più leggibile, più testabile.
*/