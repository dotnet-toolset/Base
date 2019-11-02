namespace Base.Collections.Props
{
    public abstract class PropsContainer : IPropsContainer
    {
        private volatile Props _props;

        Props IPropsContainer.GetProps(bool create)
        {
            if (_props != null || !create) return _props;
            lock (this)
                return _props ?? (_props = new Props());
        }
    }
}
