using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GeneralGUI
{
    public class ErrorHelper
    {
        // Returns false if Error is found and displays Messagebox with errormsg inside
        public static bool GenericError(Func<bool> result, string errormsg) => result() ? new Func<bool>(() => { MessageBox.Show(errormsg); return false; })() : true;
    }
}
