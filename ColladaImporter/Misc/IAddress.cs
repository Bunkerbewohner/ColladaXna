using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Seafarer.Xna.Collada.Importer.Misc
{
    /// <summary>
    /// Address definition as in COLLADA file,
    /// consisting of id and sid, and additionally name
    /// </summary>
    public interface IAddress
    {
        /// <summary>
        /// Name corresponding to the XML attribute "name"
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Document wide unique ID corresponding to the XML attribute "id".
        /// </summary>
        string GlobalID { get; set; }

        /// <summary>
        /// Scoped Identifier corresponding to the XML attribute "sid"
        /// </summary>
        string ScopedID { get; set; }
    }

    /// <summary>
    /// Extensions for classes that implement IAddress
    /// </summary>
    public static class AddressExtensions
    {
        /// <summary>
        /// Determines if two addresses are equal
        /// </summary>
        /// <param name="address">An address</param>
        /// <param name="other">Another address</param>
        /// <returns>True if both addresses are the same</returns>
        public static bool Equals(this IAddress address, IAddress other)
        {
            return address.Name == other.Name &&
                   address.GlobalID == other.GlobalID &&
                   address.ScopedID == other.ScopedID;
        }

        /// <summary>
        /// Returns a part of the address as specified by given name:
        /// "name" returns the "name" attribute
        /// "idref" or "id" returns the "id" attribtue
        /// "sidref" or "sid" returns the "sid" attribute        
        /// </summary>
        /// <param name="address"></param>
        /// <param name="name"></param>
        /// <exception cref="ArgumentException">Does only accept above mentioned names</exception>
        /// <returns>Value (might be empty or null)</returns>
        public static string GetAddressPart(this IAddress address, string name)
        {
            switch (name)
            {
                case "name":
                    return address.Name;

                case "sid":
                case "sidref":
                    return address.ScopedID;

                case "id":
                case "idref":
                    return address.GlobalID;

                default:
                    throw new ArgumentException("Invalid address part name");
            }
        }
    }
}
