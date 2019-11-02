using System;
using Base.Lang.Attributes;

namespace Base.Lang
{
    [Name("CodeBug")]
    public class CodeBug : Exception
    {
        public CodeBug(string aReason, Exception aInner = null)
            : base("Code BUG - please report: " + aReason, aInner)
        {
            Bugs.Break(Message);
        }

        public CodeBug(Type aType, string aReason, Exception aInner = null)
            : base($"Code BUG in {aType.GetTitle()} - please report: {aReason}", aInner)
        {
            Bugs.Break(Message);
        }

        [Name("Duplicate")]
        public class Duplicate : CodeBug
        {
            public Duplicate(string name)
                : base("duplicate item: "+name)
            {
            }
        }

        [Name("Unreachable")]
        public class Unreachable : CodeBug
        {
            public Unreachable(Exception aInner = null)
                : base("unreachable code", aInner)
            {
            }
        }

        [Name("Unsupported")]
        public class Unsupported : CodeBug
        {
            public Unsupported(Exception aInner = null)
                : base("unsupported operation", aInner)
            {
            }
        }
    }

    public class CodeBug<T> : CodeBug
    {
        public CodeBug(string aReason, Exception aInner = null)
            : base(typeof(T), aReason, aInner)
        {
        }

    }
}
