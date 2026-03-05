using UnityEngine;
using System.Collections;

// DisallowMultipleComponent: non ha senso avere due ReactiveTarget
// sullo stesso oggetto — uno solo gestisce la morte.
[DisallowMultipleComponent]
public class ReactiveTarget : MonoBehaviour
{
    [Header("Death Settings")]
    // [SerializeField] private float _tiltAngle = -75f;
    [SerializeField] private float _destroyDelay = 1.5f;

    // Flag per evitare hit multipli durante la sequenza di morte.
    // Senza questo, sparare più volte prima del Destroy
    // avvierebbe più coroutine Die() in parllelo
    private bool _isDying = false;

    // Chiamato da RayShooter - è il contratto pubblico di questo componenta
    public void ReactToHit()
    {
        // Guardia: se sta già morendo, ignora i colpi successivi.
        if (_isDying) return;
        _isDying = true;

        // Disabilit il collider immediatamente
        // il raycast non puà più colpire questo oggetto
        if (TryGetComponent(out Collider col))
        {
            col.enabled = false;
        }

        StartCoroutine(Die());
    }

    private IEnumerator Die()
    {
        // Rotate istantaneo — il libro suggerisce tweens per
        // animazioni fluide come passo successivo (es. DOTween).
        // Space.Self: ruota rispetto agli assi locali dell'oggetto,
        // non quelli globali della scena.
        // transform.Rotate(_tiltAngle, 0f, 0f, Space.Self);
        yield return new WaitForSeconds(_destroyDelay);

        // Destroy(this)           → distrugge SOLO questo script
        // Destroy(gameObject)     → distrugge il GameObject intero
        // Errore comune: usare "this" invece di "gameObject".
        Destroy(gameObject);
    }
}























