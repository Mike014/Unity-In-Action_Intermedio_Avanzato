using UnityEngine;

public class CoordinateDemo : MonoBehaviour
{
    private void Start()
    {
        // Vector3 è la struttura fondamentale per posizioni e direzioni.
        // Tre float: x, y, z — immutabile per design (è una struct, non una class).
        Vector3 position = new Vector3(0f, 0f, -6.7f);

        // Unity espone shorthand statici per gli assi principali.
        // Vector3.forward == new Vector3(0, 0, 1)
        // Vector3.right   == new Vector3(1, 0, 0)
        // Vector3.up      == new Vector3(0, 1, 0)

        // Posizionare un oggetto in world space:
        transform.position = position;

        // La differenza tra world space e local space:
        // transform.position    → relativo al mondo
        // transform.localPosition → relativo al parent
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Spostarsi di 1 unità in avanti:
            transform.position += Vector3.forward * 1f;
        }
    }
}

// Perché Vector3 è una struct e non una class? Perché viene usato migliaia di volte per frame — allocarlo sullo stack anziché sull'heap evita pressione sul Garbage Collector.