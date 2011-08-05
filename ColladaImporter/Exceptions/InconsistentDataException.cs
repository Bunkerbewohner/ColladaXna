using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Omi.Xna.Collada.Importer.Exceptions
{
    public class InconsistentDataException : ApplicationException
    {
        public InconsistentDataException(string message)
            : base(message)
        {
            
        }

        public static void AreNotEqual(string description, string shouldBe, string be)
        {
            throw new InconsistentDataException(description + " should be '" + shouldBe + "', is '" + 
                be + "'");
        }
    }
}
