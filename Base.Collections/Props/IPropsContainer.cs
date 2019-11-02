using System;
using System.Collections.Generic;
using System.Text;

namespace Base.Collections.Props
{
    public interface IPropsContainer
    {
        Props GetProps(bool create);
    }
}
