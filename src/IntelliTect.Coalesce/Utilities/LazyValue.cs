using System;

namespace IntelliTect.Coalesce.Utilities;

internal struct LazyValue<T>
{
    bool HasValue;
    T Value;

    public T GetValue(Func<T> getter)
    {
        if (HasValue) return Value;
        Value = getter();
        HasValue = true;
        return Value;
    }

    public void Reset()
    {
        HasValue = false;
        Value = default!;
    }
}