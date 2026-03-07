using UnityEngine;

// <T> è un segnaposto per "qualsiasi tipo"
// T non è un tipo reale, è un parametro — come una variabile, ma per i tipi
// Singleton<GameManager>   // T = GameManager
// Singleton<AudioManager>  // T = AudioManager
// Singleton<UIManager>     // T = UIManager
public class Singleton<T> : MonoBehaviour where T : Component
{
    public static T Instance { get; private set; }

    public virtual void Awake ()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
    }
}

/*
PERCHÉ è scritto così:
class Singleton<T> ... where T : Component

La lettera T sta per "Type" (Tipo). È un segnaposto. 
Significa "Questa classe funzionerà per un qualsiasi tipo di dato che decideremo in futuro, ma (where) questo dato deve essere per forza un Componente di Unity". 
Questo ci serve perché altrimenti non potremmo usare funzioni come Destroy(gameObject).

public static T Instance

Invece di public static GameManager instance, usiamo T. Assumerà automaticamente la forma della classe che lo sta usando.

public virtual void Awake()

La parola chiave virtual è vitale nell'ereditarietà. Significa: "Questa è l'inizializzazione base. 
Le classi figlie possono sovrascriverla o aggiungerci pezzi se ne hanno bisogno".

DontDestroyOnLoad(gameObject);

Questa è una novità cruciale introdotta nel testo. Dice a Unity: "Quando carico un nuovo livello (es. dal Main Menu al Livello 1), non distruggere questo GameObject". 
Questo garantisce che i tuoi manager (e i dati che contengono, come il punteggio) sopravvivano per tutta l'esecuzione del gioco.

Come utilizzare lo Script
public class GameManager : Singleton<GameManager>
{
 // GameManager code here...
}
*/