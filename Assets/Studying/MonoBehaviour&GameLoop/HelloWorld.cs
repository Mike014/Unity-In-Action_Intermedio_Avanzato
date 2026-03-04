using UnityEngine;

// MonoBehaviour è la classe base che "parla" con Unity.
// Senza di essa, la tua classe è C# puro — Unity non la vede come componente.
public class HelloWorld : MonoBehaviour
{
    // Start() → chiamato UNA VOLTA, al primo frame in cui il componente è attivo.
    // Usalo per inizializzazioni, non per logica continua.
    private void Start()
    {
        // Debug.Log scrive nella Console. Il punto è questo:
        // non è Console.WriteLine() del C# standard —
        // Debug.Log è consapevole del contesto Unity e può
        // mostrare lo stack trace e il GameObject sorgente.
        // il percorso di chiamate a funzioni che il tuo programma ha fatto partendo dall'inizio
        Debug.Log("Hello World");

        // Versione più utile in sviluppo:
        // Il secondo parametro linka il messaggio all'oggetto in scena.
        // Cliccando il messaggio in Console, Unity seleziona l'oggetto.
        Debug.Log($"[{gameObject.name}] Inizializzato.", this);
    }

    // Update() → chiamato OGNI FRAME.
    // Frequenza variabile — dipende dal framerate, non dal tempo reale.
    // Problema: se il gioco gira a 30fps vs 120fps, il comportamento cambia.
    private void Update()
    {
        // MAI usare Update per fisica o input critico senza delta time.
        // Esempio sbagliato:
        // transform.position += new Vector3(0.1f, 0, 0); // velocità framerate-dipendente

        // Esempio corretto:
        // transform.position += Vector3.right * speed * Time.deltaTime;
    }
}