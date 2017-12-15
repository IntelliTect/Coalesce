using System;
using IntelliTect.Coalesce.TypeDefinition;

namespace IntelliTect.Coalesce.Api.Behaviors
{
    public interface IBehaviorsFactory
    {
        object GetBehaviors(ClassViewModel servedType);

        object GetDefaultBehaviors(ClassViewModel servedType);

    }
}