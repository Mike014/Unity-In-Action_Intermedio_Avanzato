# Unity In Action

Progetto Unity didattico ispirato al libro *Unity in Action* (Joe Hocking), realizzato nell'ambito del percorso Epicode.
L'obiettivo è costruire un prototipo FPS applicando tecniche di programmazione **intermedia-avanzata** su Unity, con attenzione alla qualità del codice, alle performance e alle best practice di architettura.

---

## Stato del progetto

> **In sviluppo** — Sistema di gameplay in costruzione.

| Area | Stato |
|---|---|
| Fondamentali Unity (MonoBehaviour, lifecycle, coordinate) | Completato |
| Movimento FPS con CharacterController | Completato |
| Camera look con mouse (asse singolo e doppio) | Completato |
| Sistema di sparo con Raycast | Completato |
| Target reattivi al colpo | Completato |
| UI Crosshair | Completato |
| Fisica di impatto (impulso su Rigidbody) | Completato |

---

## Struttura del progetto

```
Assets/
├── Project/
│   └── Scripts/
│       ├── Inputs/
│       │   └── FPSInput.cs          # Movimento player con CharacterController
│       ├── Movement/
│       │   ├── MouseLook.cs         # Controllo camera FPS via mouse
│       │   └── Spin.cs              # Rotazione continua di oggetti
│       ├── Shooting/
│       │   ├── RayShooter.cs        # Sparo a Raycast con UI crosshair
│       │   └── ReactiveTarget.cs    # Bersaglio che reagisce al colpo
│       └── Utils/
│           └── Utils_AllCameras.cs  # Debug utility — ispezione telecamere attive
│
├── Studying/
│   ├── Components/
│   │   ├── IMovable.cs              # Interfaccia di movimento
│   │   └── MovementBehaviour.cs    # Implementazione concreta di IMovable
│   ├── CoordinateDemo/
│   │   └── CoordinateDemo.cs       # World space vs Local space, Vector3
│   └── MonoBehaviour&GameLoop/
│       ├── HelloWorld.cs            # Debug.Log e primo MonoBehaviour
│       ├── LifeCycleDemo.cs         # Lifecycle completo (Awake→OnDestroy)
│       └── OrderTest.cs             # Verifica ordine di esecuzione dei metodi
│
└── Scenes/
    └── Demo.unity                   # Scena principale del prototipo FPS
```

---

## Tecniche di programmazione applicate

### Attributi e Inspector

| Tecnica | Dove | Perché |
|---|---|---|
| `[RequireComponent]` | `FPSInput`, `Spin` | Dipendenza dichiarata a livello di tipo; Unity impedisce la rimozione del componente richiesto |
| `[DisallowMultipleComponent]` | `MouseLook`, `ReactiveTarget` | Previene duplicati che causerebbero comportamenti indefiniti |
| `[AddComponentMenu]` | `FPSInput` | Organizza il menu *Add Component* nell'Inspector |
| `[SerializeField] private` | Tutti i componenti di gioco | Espone il campo all'Inspector senza romper l'incapsulamento |
| `[Header]` + `[Range]` | `FPSInput`, `MouseLook`, `RayShooter` | Organizzazione visiva e validazione dei valori nell'Inspector senza codice di runtime |

### Lifecycle di MonoBehaviour

```
Awake() → OnEnable() → Start() → Update() / FixedUpdate() / LateUpdate() → OnDisable() → OnDestroy()
```

- **`Awake`** — inizializzazione interna (cache di componenti, stato proprio)
- **`Start`** — inizializzazione che dipende da altri componenti (tutti gli `Awake` sono già stati eseguiti)
- **`Update`** — logica frame-by-frame (input, movimento non fisico)
- **`FixedUpdate`** — fisica a intervallo fisso (Rigidbody)
- **`LateUpdate`** — operazioni post-Update (es. camera follow)

### Movimento e fisica

- **`Vector3.ClampMagnitude` vs `normalized`** (`FPSInput`) — `ClampMagnitude` preserva i valori analogici parziali (levette), permettendo movimenti a velocità variabile; `normalized` azzera questa sfumatura.
- **Gravità accumulativa** (`FPSInput`) — `_verticalVelocity` si incrementa ogni frame con `Physics.gravity.y * multiplier * Time.deltaTime`, simulando l'equazione del moto `v = v₀ + a·t`.
- **`isGrounded` con valore `-2f`** (`FPSInput`) — Un piccolo valore negativo invece di zero stabilizza il rilevamento del suolo e previene il flickering di `isGrounded` tra frame consecutivi.
- **`TransformDirection`** (`FPSInput`) — Converte il vettore di input da local space a world space, necessario perché "avanti" per il player è il suo asse locale, non quello globale della scena.
- **`Time.deltaTime`** (tutti) — Rende le velocità framerate-indipendenti.

### Camera e rotazione

- **Euler angles con clamp verticale** (`MouseLook`) — La rotazione verticale viene gestita come `float` accumulato e clamped con `Mathf.Clamp`, poi riassegnata come `Vector3` completo a `localEulerAngles` (l'assegnazione per singolo asse è read-only).
- **`Cursor.lockState = CursorLockMode.Locked`** (`MouseLook`) — Blocca e nasconde il cursore al centro dello schermo; `Escape` fa toggle per pause/debug.
- **`Space.Self` vs `Space.World`** (`Spin`, `MouseLook`) — `Space.Self` ruota rispetto agli assi locali dell'oggetto; `Space.World` rispetto agli assi globali della scena.

### Raycast e sparo

- **`Physics.Raycast` con `LayerMask`** (`RayShooter`) — Il filtro per layer riduce drasticamente il numero di collider che PhysX deve testare, abbassando il costo computazionale del raycast.
- **`out RaycastHit`** (`RayShooter`) — `RaycastHit` è una struct: passarla con `out` evita allocazioni heap; PhysX la popola solo in caso di hit.
- **`Camera.ScreenPointToRay`** (`RayShooter`) — Converte un punto 2D sullo schermo (centro) in un raggio 3D nel world space, partendo dal near clip plane della camera.
- **`AddForceAtPosition` con `ForceMode.Impulse`** (`RayShooter`) — Applica un impulso fisico nel punto esatto di impatto, generando sia traslazione che rotazione realistica (diversamente da `AddForce` che agisce solo sul centro di massa).

### Coroutine

```csharp
private IEnumerator SpawnHitIndicator(Vector3 position, Vector3 normal)
{
    // ... setup ...
    yield return new WaitForSeconds(_indicatorDuration);
    Destroy(sphere);
}
```

- Rispettano `Time.timeScale` (la pausa del gioco mette in pausa anche la coroutine)
- Gestite dal loop di Unity, senza overhead di thread management
- `WaitForSeconds` è pooled internamente — minor pressione sul GC rispetto a `async/await`

### Pattern architetturali

| Pattern | Dove | Applicazione |
|---|---|---|
| **Single Responsibility Principle** | `FPSInput` + `MouseLook` | Movimento e camera look sono componenti separati, ognuno con una sola responsabilità |
| **Separation of Concerns** | `RayShooter` + `ReactiveTarget` | Lo shooter sa *che* ha colpito; il target sa *come* reagire |
| **Guard clause** | `ReactiveTarget._isDying` | Flag booleano che blocca hit multipli durante la sequenza di morte |
| **Interfacce** | `IMovable` + `MovementBehaviour` | Contratto esplicito per il movimento, disaccoppiato dall'implementazione |
| **Component caching** | Tutti i componenti di gioco | `GetComponent` viene chiamato una sola volta in `Awake` e il risultato salvato in un campo privato |

### Performance e GC

- **`TryGetComponent`** invece di `GetComponent + null check` — non alloca memoria in caso di componente assente.
- **Cache del Transform** (`MouseLook`) — La property `transform` ha un piccolo overhead; la cache elimina chiamate ripetute in `Update`.
- **Cache del centro schermo** (`RayShooter`) — Calcolato una sola volta in `Awake` invece di ogni frame.
- **Vector3 cached** (`Spin`) — `_rotationAxis` evita di creare un nuovo `Vector3` ad ogni frame.

---

## Come eseguire il progetto

1. Aprire la cartella con **Unity 2022.3 LTS** o versione compatibile.
2. Aprire la scena `Assets/Scenes/Demo.unity`.
3. Premere **Play**.

**Controlli:**

| Tasto | Azione |
|---|---|
| `WASD` | Movimento player |
| `Mouse` | Mira / rotazione camera |
| `Click sinistro` | Sparo |
| `Escape` | Toggle cursore (pausa/ripresa) |

---

## Riferimenti

- *Unity in Action* — Joe Hocking, Manning Publications
- [Unity Manual — MonoBehaviour lifecycle](https://docs.unity3d.com/Manual/ExecutionOrder.html)
- [Unity Manual — CharacterController](https://docs.unity3d.com/Manual/class-CharacterController.html)
- [Unity Manual — Physics.Raycast](https://docs.unity3d.com/ScriptReference/Physics.Raycast.html)
