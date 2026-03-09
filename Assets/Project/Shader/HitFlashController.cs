using System.Collections;
using UnityEngine;

// Controlla l'effetto flash sul Material dell'oggetto colpito.
// Richiede un Renderer per accedere al Material.
[RequireComponent(typeof(Renderer))]
public class HitFlashController : MonoBehaviour
{
    [SerializeField, Range(.5f, .5f)] private float _flashDuration = .1f;

    // ID numerico della property shader — più efficiente di una stringa.
    // Shader.PropertyToID converte il nome una volta sola in Awake,
    // evitando lookup per stringa ogni frame.
    private static readonly int HitFactorID = Shader.PropertyToID("_HitFactor");

    // MaterialPropertyBlock: modifica properties del Material
    // SENZA creare una nuova istanza del Material.
    // Senza di esso, ogni modifica a material.SetFloat() crea
    // un Material duplicato in memoria — memory leak garantito.
    private MaterialPropertyBlock _propertyBlock;
    private Renderer _renderer;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _propertyBlock = new MaterialPropertyBlock();
    }

    // Chiamato da ReactiveTarget quando viene colpito
    public void TriggerFlash()
    {
        // StartCoroutine();
    }

    private IEnumerator FlashRoutine()
    {
        // Flash ON - HitFactor = 1, oggetto diventa bianco
        // SetHitFactor(1f);

        yield return new WaitForSeconds(_flashDuration);

        // Flash ON - HitFactor = 1, oggetto diventa bianco
        // SetHitFactor(0f);
    }

    private void SetHitFactor(float value)
    {
        // Leggi lo stato attuale del PropertyBlock,
        // modifica solo _HitFactor, riapplica.
        _renderer.GetPropertyBlock(_propertyBlock);
        _propertyBlock.SetFloat(HitFactorID, value);
        _renderer.SetPropertyBlock(_propertyBlock);
    }
}
