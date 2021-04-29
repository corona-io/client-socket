using System;

public abstract class State<T>
{
    public Action<T> action;
    
    public State() { action = EnterState; }
    public virtual void EnterState(T entity) { action = StateBehavior; }
    public virtual void StateBehavior(T entity) { }
    public abstract State<T> UpdateState(T entity);
    public virtual void ExitState(T entity) { }
}
