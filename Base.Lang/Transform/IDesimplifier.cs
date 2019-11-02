using System;

namespace Base.Lang.Transform
{
    public interface IDesimplifier
    {
        IDesimplifier Parent { get; }
        object Destination { get; }
        Type DestinationType { get; }
        object Simple { get; }
        object FieldOrIndex { get; }

        object Desimplify();
        object Desimplify(Type destType);
    }
}
