using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Aksl.Controls
{
    public static class PackIconExtensions
    {
        #region Get IconKind Method
        public static PackIconKind ToPackIconKind(this string iconKind)
        {
            PackIconKind kind = PackIconKind.None;

            _ = Enum.TryParse(iconKind, out kind);

            return kind;
        }
        #endregion
    }
}
