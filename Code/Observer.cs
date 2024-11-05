using System;
using Godot;

public class Observer<T>
{
    private T value;
    private Action<T> onValueChanged;

    public T Value {
        get => value;
        set => Set(value);
    }

    public Observer(T value, Action<T> callback = null)
    {
        this.value = value;
        if (callback != null) onValueChanged += callback;
    }

    void Set(T value)
    {
        if (Equals(this.value, value)) return;  
        this.value = value;
        Invoke();
    }

    public void Invoke()
    {
        GD.Print($"Invoking");
        onValueChanged?.Invoke(value);
    }

    public void AddListener(Action<T> callback)
    {
        if (callback == null) return;
        onValueChanged += callback;
    }

    public void RemoveListener(Action<T> callback)
    {
        if (callback == null) return;
        onValueChanged -= callback;
    }

    public void Dispose()
    {
        onValueChanged = null;
        value = default;
    }
}