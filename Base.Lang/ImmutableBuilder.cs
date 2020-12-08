namespace Base.Lang
{
    public abstract class ImmutableBuilder<T> : IImmutableBuilder<T>
        where T : class
    {
        protected bool Immutable;

        public bool IsMutable => !Immutable;

        protected void EnsureMutable()
        {
            if (Immutable) throw new CodeBug("object is immutable");
        }

        public virtual T Build()
        {
            // EnsureMutable(); we don't need this
            Immutable = true;
            return this as T;
        }
    }
}
