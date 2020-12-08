namespace Base.Collections.Props
{
    public abstract class PropsContainer : IPropsContainer
    {
        private volatile Props _props;

        // don't remove!!! needed for indexed inits
        public object this[IPropKey key]
        {
            get => key.GetObject(this);
            set => key.SetObject(this, value);
        }
        Props IPropsContainer.GetProps(bool create)
        {
            if (_props != null || !create) return _props;
            lock (this)
                return _props ??= new Props();
        }
    }
}
