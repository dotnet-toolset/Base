using System.Threading;
using Base.Collections.Props;
using Base.Lang;

namespace Base.Collections.Async
{
    public abstract class FlowContextBase : Disposable, IPropsContainer
    {
        // don't remove!!! needed for indexed inits
        public object this[IPropKey key]
        {
            get => key.GetObject(this);
            set => key.SetObject(this, value);
        }

        protected override void OnDispose()
        {
            _dynProps.Clear();
        }

        #region IDynPropsContainer Members

        /// <summary>
        /// we need this initialized for 2 reasons:
        /// 1. there is no sense to create FlowContext without props in the first place;
        /// 2. CopyFrom in constructor assumes we have target initialized if source has values, otherwise shit happens
        /// </summary>
        protected readonly Props.Props _dynProps = new Props.Props();

        Props.Props IPropsContainer.GetProps(bool aCreate)
        {
            CheckDisposed();
            return _dynProps;
        }

        #endregion
    }

    public class FlowContext : FlowContextBase
    {
        private static readonly AsyncLocal<FlowContext> CurrentContext = new AsyncLocal<FlowContext>();
        public static FlowContext Current => CurrentContext.Value;

        private readonly FlowContext _parent;

        public FlowContext Parent
        {
            get
            {
                CheckDisposed();
                return _parent;
            }
        }


        public FlowContext()
        {
            var parent = _parent = CurrentContext.Value;
            if (parent != null)
                _dynProps.CopyFrom(parent._dynProps);
            CurrentContext.Value = this;
        }

        protected override void OnDispose()
        {
            if (CurrentContext.Value != this) throw new CodeBug("context stack is corrupted");
            CurrentContext.Value = _parent;
            base.OnDispose();
        }

        public static T Find<T>() where T : FlowContext
        {
            var c = CurrentContext.Value;
            while (c != null)
            {
                if (c is T result) return result;
                c = c.Parent;
            }

            return null;
        }
    }

    public class FlowContextSync : FlowContextBase
    {
        private static readonly ThreadLocal<FlowContextSync> CurrentContext = new ThreadLocal<FlowContextSync>();
        public static FlowContextSync Current => CurrentContext.Value;

        private readonly FlowContextSync _parent;

        public FlowContextSync Parent
        {
            get
            {
                CheckDisposed();
                return _parent;
            }
        }

        public FlowContextSync()
        {
            var parent = _parent = CurrentContext.Value;
            if (parent != null)
                _dynProps.CopyFrom(parent._dynProps);
            CurrentContext.Value = this;
        }

        protected override void OnDispose()
        {
            if (CurrentContext.Value != this) throw new CodeBug("context stack is corrupted");
            CurrentContext.Value = _parent;
            base.OnDispose();
        }

        public static T Find<T>() where T : FlowContextSync
        {
            var c = CurrentContext.Value;
            while (c != null)
            {
                if (c is T result) return result;
                c = c.Parent;
            }

            return null;
        }
    }
}