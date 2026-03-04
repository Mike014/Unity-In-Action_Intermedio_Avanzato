using UnityEngine;

public class OrderTest : MonoBehaviour
{
    private void Awake() => Debug.Log("A");
    private void OnEnable() => Debug.Log("B");
    private void Start() => Debug.Log("C");
    private void Update() => Debug.Log("D");
}
