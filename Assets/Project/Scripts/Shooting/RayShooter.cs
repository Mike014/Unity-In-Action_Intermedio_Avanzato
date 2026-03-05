using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// [RequireComponent(typeof(Camera))]
public class RayShooter : MonoBehaviour
{
    [Header("Raycast")]
    // LayerMask: filtra quali layer fisici vengono considerati dal raycast.
    // Senza filtro, PhysX calcola intersezioni con TUTTI i collider in scena.
    // Con LayerMask, ignori layer irrilevanti (UI, player stesso, ecc.)
    // riducendo drasticamente il costo computazionale.
    [SerializeField] private LayerMask _hitLayers = Physics.DefaultRaycastLayers;

    [SerializeField, Range(1f, 500f)] private float _maxDistance = 100f;

    [Header("Hit Indicator")]
    [SerializeField, Range(0.1f, 3f)] private float _indicatorDuration = 1f;
    [SerializeField, Range(0.1f, 2f)] private float _indicatorScale = 0.3f;

    [Header("Crosshair")]
    // [SerializeField] private string _crossHairSymbol = ".";
    [SerializeField] private Image _crossHairImage;
    [SerializeField, Range(8, 64)] private int _crossHairSize = 32;
    [SerializeField] private Color _crossHairColor = Color.red;

    [Header("Physics Impact")]
    [SerializeField, Range(0f, 500f)] private float _impactForce = 100f;


    // Cache — GetComponent è costoso se chiamato ogni frame.
    // La camera è sul SAME GameObject grazie a RequireComponent.
    // Non serve FindAnyObjectByType — è fragile e lento.
    private Camera _camera;

    // Cache del centro schermo — ricalcolarlo ogni frame è inutile
    // a meno che la risoluzione non cambi a runtime.
    private Vector3 _screenCenter;

    private void Awake()
    {
        _camera = Camera.main;

        if (_camera == null)
        {
            Debug.LogError("[RayShooter] Nessuna Camera con tag 'Maincamera' trova");
            return;
        }

        _screenCenter = new Vector3(_camera.pixelWidth / 2f, _camera.pixelHeight / 2f, 0f);
        InitCrosshHair();
    }

    private void Update()
    {
        // GetMouseButtonDown(0): true SOLO nel frame esatto del clic.
        // Garantisce comportamento semiautomatico — un clic, un colpo.
        // GetMouseButton(0) sarebbe continuo — per armi automatiche.
        if (Input.GetMouseButtonDown(0))
            TryShoot();
    }

    // Inizializza il crosshair una volta sola in Awake.
    // Separato per chiarezza — non mescolare setup UI con setup camera.
    private void InitCrosshHair()
    {
        if (_crossHairImage == null)
        {
            Debug.LogWarning("[RayShooter] Crosshair Image non assegnata nell'Inspector");
            return;
        }

        // Applica colore e dimensione iniziali
        _crossHairImage.color = _crossHairColor;

        // RectTransform controlla posizione e dimensione degli elementi UI.
        // sizeDelta = larghezza e altezza in pixel del Canvas.
        _crossHairImage.rectTransform.sizeDelta = new Vector2(_crossHairSize, _crossHairSize);
    }

    private void TryShoot()
    {
        if (_camera == null) return;

        // ScreenPointToRay: converte un punto 2D sullo schermo
        // in un raggio 3D nel world space, partendo dalla camera.
        // Il raggio parte dal near clip plane della camera.
        Ray ray = _camera.ScreenPointToRay(_screenCenter);

        // out hit: RaycastHit è una struct — passarla out evita
        // allocazioni heap. PhysX la popola solo se c'è un hit.
        // _hitLayers: filtra i collider per layer — ottimizzazione critica.
        // _maxDistance: evita raycast infiniti attraverso tutta la scena.
        if (Physics.Raycast(ray, out RaycastHit hit, _maxDistance, _hitLayers))
        {
            Debug.Log($"[RayShooter] Hit: {hit.collider.gameObject.name} | Point: {hit.point} | Distance: {hit.distance:F2}m", hit.collider.gameObject);
            Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red, _indicatorDuration);

            // Applica impulso fisico se l'oggetto ha un Rigidbody.
            // Questo è indipendente da ReactiveTarget —
            // qualsiasi oggetto fisico risponde all'impatto.
            ApplyImpact(hit, ray);

            // TryGetComponent: cerca ReactiveTarget sul GameObject colpito.
            // Se lo trova → notifica il target.
            // Se non lo trova → spawna l'indicatore visivo generico.
            // Separazione netta delle responsabilità:
            // RayShooter sa CHE ha colpito qualcosa,
            // ReactiveTarget sa COME reagire.
            if (hit.collider.gameObject.TryGetComponent(out ReactiveTarget target))
                target.ReactToHit();
            else
                StartCoroutine(SpawnHitIndicator(hit.point, hit.normal));
        }
        else
        {
            Debug.DrawRay(ray.origin, ray.direction * _maxDistance, Color.red, .5f);
        }
    }

    // Coroutine invece di async Task:
    // - rispetta Time.timeScale (pausa del gioco = pausa della coroutine)
    // - gestita dal loop di Unity, zero overhead di thread management
    // - WaitForSeconds è pooled internamente da Unity → meno GC pressure
    private IEnumerator SpawnHitIndicator(Vector3 position, Vector3 normal)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        // Orientamento rispetto alla normale della superficie colpita.
        // Quaternion.LookRotation: costruisce una rotazione che punta
        // nella direzione della normale — la sfera "emerge" dalla superficie.
        sphere.transform.position = position + normal * (_indicatorScale * .5f);
        Debug.Log($"Lo Scale1 di Sphere è: {sphere.transform.localScale}");
        sphere.transform.rotation = Quaternion.LookRotation(normal);
        Debug.Log($"Lo Scale2 di Sphere è: {sphere.transform.localScale}");
        sphere.transform.localScale = (Vector3.one * _indicatorScale / 3);
        Debug.Log($"Lo Scale3 di Sphere è: {sphere.transform.localScale}");
        
        // Rimuove il collider dalla sfera — non deve interferire
        // con raycast successivi o con la fisica della scena.
        if (sphere.TryGetComponent(out Collider col))
            Destroy(col);

        yield return new WaitForSeconds(_indicatorDuration);

        // Null check: l'oggetto potrebbe essere stato distrutto
        // da un'altra fonte durante l'attesa (es. cambio scena).
        if (sphere != null)
            Destroy(sphere);
    }

    private void ApplyImpact(RaycastHit hit, Ray ray)
    {
        // TryGetComponent: zero allocazione se RigidBody non esiste
        if (!hit.collider.TryGetComponent(out Rigidbody rb)) return;

        // AddForceAtPosition: applica una forza in un punto specifico
        // del corpo rigido — non al centro di massa.
        // Questo genera sia traslazione che rotazione realistica.
        // Confronto:
        // AddForce()              → forza al centro di massa, solo traslazione
        // AddForceAtPosition()    → forza al punto di impatto, traslazione + rotazione
        rb.AddForceAtPosition(ray.direction * _impactForce, // direzione e intensità
        hit.point,  // punto esatto di impatto
        ForceMode.Impulse); // impulso istantaneo, non forza continua
    }

    // Metodo
    // OnGUI è legacy — ha overhead significativo perché
    // ridisegna ogni frame. Accettabile per debug/prototipo,
    // da sostituire con Canvas/UI Toolkit in produzione.
    /*
    private void OnGUI()
    {
        if(_camera == null) return;

        float posX = _camera.pixelWidth / 2f - _crossHairSize / 4f;
        float posY = _camera.pixelHeight / 2f - _crossHairSize / 2f;

        // GUI.color modifica il colore di tutto ciò che viene
        // disegnato dopo questa riga, fino al prossimo reset.
        // Salvare e ripristinare il colore originale è buona pratica
        // per non sporcare altri elementi OnGUI che vengono dopo.
        Color previousColor = GUI.color;
        GUI.color = _crossHairColor;
   
        GUI.Label(
            new Rect(posX, posY, _crossHairSize * 2f, _crossHairSize), _crossHairSymbol
        );

        GUI.color = previousColor; // Ripristina
    }
    */
}

/*
ForceMode.Force          → forza continua, applicata per frame (usa massa)
ForceMode.Impulse        → colpo istantaneo (usa massa)    ← proiettile
ForceMode.VelocityChange → colpo istantaneo (ignora massa) ← tutti si muovono uguale
ForceMode.Acceleration   → forza continua (ignora massa)
*/


