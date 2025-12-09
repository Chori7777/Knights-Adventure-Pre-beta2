using System;

public interface IAttackPattern
{
    void StartAttack();
    void StopAttack();
    event Action OnFinished;
}
