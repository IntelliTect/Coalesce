using System;
using IntelliTect.Coalesce.TypeDefinition;

namespace IntelliTect.Coalesce.Api.Behaviors;

public interface IBehaviorsFactory
{
    object GetBehaviors(ClassViewModel servedType, ClassViewModel declaredFor);

    Type GetBehaviorsType(ClassViewModel servedType, ClassViewModel declaredFor);

    IBehaviors<TServed> GetBehaviors<TServed>(ClassViewModel declaredFor)
        where TServed : class;

    object GetDefaultBehaviors(ClassViewModel servedType);

}
