using UnityEngine;

// [RequireComponent] è una dichiarazione di dipendenza esplicita
// Garantisce che Transform esista sempre - evita null ref silenziosi
// e documenta l'intenzione direttamente nel codice
[RequireComponent(typeof(Transform))]
public class Spin : MonoBehaviour
{
    // [SerializeField] espone la variabile nell'inspector
    // SENZA renderla pubblica - rispetta l'incapsulamento
    // Preferibile a "public" perchè altri script non possono
    // modificare _speed direttamente
    [SerializeField] private float _speed = 3.0f;

    // Space.Self = ruota attorno agli assi LOCALI dell'oggetto
    // Space.World = ruota attorno agli assi GLOBALI della scena
    // Esposto in Inspector per testare la differenza senza ricompilare.
    [SerializeField] private Space _rotationSpace = Space.Self;

    // Vector3 cached - evita di ricreare new Vector3(0m _speed, 0)
    // ogni frame. Piccola ottimizzazione, ma buona abitudina
    private Vector3 _rotationAxis;

    private void Awake()
    {
        // Awake() è il posto corretto per inizializzare dati interni
        // che non dipendo da altri componenti
        _rotationAxis = Vector3.up; // (0, 1, 0) asse Y
    }

    private void Update()
    {
        // Time.deltaTime normalizza la rotazione rispetta al tempo reale
        // Senza di esso: a 30 fps ruoti la metà rispetto a 60fps
        // Con esso: la velocità è identica su qualsiasi framerate
        transform.Rotate(_rotationAxis, _speed * Time.deltaTime, _rotationSpace);
    }
}

/*
public → [SerializeField] private — stessa visibilità nell'Inspector, zero esposizione esterna.
speed → _speed * Time.deltaTime — il libro non lo menziona ancora, ma senza deltaTime la velocità dipende dal framerate. 
Su una macchina lenta il gioco gira più lento, su una veloce più veloce.
transform.Rotate(0, speed, 0) → overload con asse e angolo espliciti — più leggibile e meno ambiguo rispetto a tre float separati.
*/