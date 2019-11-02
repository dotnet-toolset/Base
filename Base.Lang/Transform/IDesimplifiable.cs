namespace Base.Lang.Transform
{
    public interface ISimplifiable
    {
        object Simplify();
    }

    public interface IDesimplifiable : ISimplifiable
    {
        void Desimplify(object simple);
    }

}