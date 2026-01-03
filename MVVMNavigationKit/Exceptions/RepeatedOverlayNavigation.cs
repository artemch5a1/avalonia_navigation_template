using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVVMNavigationKit.Exceptions
{
    public class RepeatedOverlayNavigation : Exception
    {
        public RepeatedOverlayNavigation()
            : base("Вложенная оверлейная навигация не поддерживается") { }
    }
}
