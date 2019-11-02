using System;

namespace Base.Cil.Attributes
{
    /// <summary>
    /// Annotate assembly metadata item with a unique ID. This can later be used to locate item via reflection not relying on its name that may be changed during obfuscation etc.  
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Constructor | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
    public class MarkAttribute : Attribute
    {
        public int Id { get; }
        public MarkAttribute(int id)
        {
            Id = id;
        }
    }
}
