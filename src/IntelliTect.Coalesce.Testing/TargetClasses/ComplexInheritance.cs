using System;
using System.Collections;
using System.Collections.Generic;

namespace IntelliTect.Coalesce.Testing.TargetClasses;

// IEnumerable is technically duplicated here.
// We specify it explicitly, and IEnumerable<> also implements IEnumerable.
public class ComplexInheritance : IEnumerable<object>, IEnumerable, IAmInheritedMultipleTimes<string>, IAmInheritedMultipleTimes<int>
{
    public IEnumerator<object> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }
}

public class ComplexInheritanceDerived : ComplexInheritance, IAmInheritedMultipleTimes<ComplexInheritanceDerived> { }

public interface IAmInheritedMultipleTimes<T> : IHaveMultipleTypes<IAmSimple, long, decimal>
{

}

public interface IHaveMultipleTypes<T1, T2, T3>
{

}

public interface IAmSimple
{

}
