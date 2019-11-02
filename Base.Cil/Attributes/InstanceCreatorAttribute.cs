using System;

namespace Base.Cil.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class InstanceCreatorAttribute : Attribute
    {
        public Type Class { get; }

        public InstanceCreatorAttribute(Type @class)
        {
            Class = @class;
        }
    }
}
