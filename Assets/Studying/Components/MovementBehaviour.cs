using UnityEngine;

public class MovementBehaviour : MonoBehaviour, IMovable
{
    [SerializeField] private float _speed = 5f;

    public void Move(Vector3 direction)
    {
        transform.position += direction * _speed * Time.deltaTime;
    }
}
