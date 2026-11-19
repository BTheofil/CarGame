using System;
using UnityEngine;

[RequireComponent(typeof(CarController))]
public class CarDamage : MonoBehaviour {

    [SerializeField] private float maxHealth = 100f;
    [SerializeField, Min(0.1f)] private float strength = 1f;
    [SerializeField] private float impactThreshold = 0.3f;

    public event Action Destroyed;

    public float Health { get; private set; }
    public float HealthRatio => Health / maxHealth;
    public bool IsWrecked => Health <= 0f;

    private CarController car;

    private void Awake() {
        car = GetComponent<CarController>();
        Health = maxHealth;
    }

    private void FixedUpdate() {
        if (IsWrecked) return;
        
        float damage = (car.ImpactSpeed - impactThreshold) / strength;
        if (damage <= 0f) return;
        Health = Mathf.Max(0f, Health - damage);
       
        if (IsWrecked) {
            Debug.Log($"{name} wrecked");
            Destroyed?.Invoke();
        }
    }
}
