using UnityEngine;

public class Destroyer : MonoBehaviour
{
    [SerializeField] float lifetime = 0.05f;
    void Start() => Destroy(gameObject, lifetime);
}