using UnityEngine;

// RequireComponent: dipendenza dichiarata a livello di tipo.
// Unity impedisce di rimuovere CharacterController se questo script è presente.
[RequireComponent(typeof(CharacterController))]
[AddComponentMenu("Control Script/FPS Input")]
public class FPSInput : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField, Range(1f, 20f)] private float _speed   = 6f;

    [Header("Gravity")]
    [SerializeField] private float _gravityMultiplier = 2f;

    // Costante fisica reale: -9.81 m/s²
    // Separare la costante dal moltiplicatore permette di
    // tweakare la "pesantezza" del personaggio senza alterare
    // la fisica di base.
    private static readonly float Gravity = Physics.gravity.y;

    // La velocità verticale si accumula nel tempo — non è un valore fisso.
    // Questo simula correttamente l'accelerazione gravitazionale.
    private float _verticalVelocity;

    // Cache del componente — GetComponent è costoso se chiamato ogni frame.
    private CharacterController _charController;

    private void Awake()
    {
        // Awake invece di Start: l'inizializzazione interna
        // non dipende da altri componenti, quindi può avvenire prima.
        // TryGetComponent: non alloca in caso di fallimento,
        // più corretto di GetComponent + null check.
        if (!TryGetComponent(out _charController))
            Debug.LogError($"[{nameof(FPSInput)}] CharacterController non trovato.", this);
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        // Input grezzo — valori tra -1 e 1 (o 0 con levette analogiche).
        Vector3 input = new Vector3(
            Input.GetAxis("Horizontal"),
            0f,
            Input.GetAxis("Vertical")
        );

        // ClampMagnitude invece di normalized:
        // - normalized forza sempre lunghezza 1, anche con input analogico parziale.
        //   Risultato: impossibile "camminare piano" con una levetta.
        // - ClampMagnitude preserva i valori sotto _speed (input analogico)
        //   e taglia solo quelli che superano _speed (diagonale da tastiera).
        Vector3 movement = Vector3.ClampMagnitude(input, 1f) * _speed;

        // Gravità accumulativa — simula accelerazione reale.
        // isGrounded: CharacterController calcola se il collider
        // tocca il suolo nel frame corrente.
        if (_charController.isGrounded && _verticalVelocity < 0f)
        {
            // Piccolo valore negativo invece di zero:
            // mantiene il contatto col suolo e stabilizza isGrounded.
            // Con zero esatto, isGrounded può "flickerare" tra i frame.
            _verticalVelocity = -2f;
        }
        else
        {
            // v = v₀ + a·t — equazione del moto uniformemente accelerato.
            // Gravity è negativo, quindi _verticalVelocity decresce ogni frame.
            _verticalVelocity += Gravity * _gravityMultiplier * Time.deltaTime;
        }

        movement.y = _verticalVelocity;

        // TransformDirection: converte il vettore da local space a world space.
        // Necessario perché input.z = "avanti" rispetto all'oggetto,
        // non rispetto al mondo. Senza questa conversione, premi "W"
        // e ti muovi sempre verso il world-forward (asse Z globale),
        // ignorando la rotazione del player.
        movement = transform.TransformDirection(movement);

        // deltaTime applicato al movimento orizzontale ma NON alla gravità
        // perché _verticalVelocity è già stata moltiplicata per deltaTime sopra.
        movement.x *= Time.deltaTime;
        movement.z *= Time.deltaTime;

        _charController.Move(movement);
    }
}

// movement.y = _gravity fisso → _verticalVelocity accumulativa. La caduta accelera realisticamente invece di essere costante.
// normalized → ClampMagnitude — preserva l'input analogico parziale per movimento furtivo o camminata lenta.
// isGrounded check con -2f — stabilizza il rilevamento del suolo ed evita il flickering di isGrounded.
// Physics.gravity.y invece di -9.8f hardcoded — si sincronizza con le impostazioni fisiche del progetto Unity.



































