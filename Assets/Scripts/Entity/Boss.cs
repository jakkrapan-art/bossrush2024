using System;
using UnityEngine;

public class Boss : Entity
{
    private float _hungryPoint = 50.00f;
    [SerializeField]
    private float _hungryPointDecrease = 0.5f;
    private void Awake()
    {
        _stateMachine = new BossStateMachine();
    }
    private void Start()
    {

    }

    private void FixedUpdate()
    {
        _hungryPoint -= _hungryPointDecrease;
        handleStateByHungryPoint();
    }

    private void handleStateByHungryPoint()
    {
        if (getHungryPointPercentage() >= 30.00f)
        {

        }
    }

    private float getHungryPointPercentage()
    {
        return _hungryPoint / 100.00f;
    }
}