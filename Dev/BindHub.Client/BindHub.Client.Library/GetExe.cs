/*
 * Developer : Matt Smith (matt@matt40k.co.uk)
 * All code (c) Matthew Smith all rights reserved
 */

using System.IO;
using System.Reflection;

namespace BindHub.Client.Library
{
    internal class GetExe
    {
        /// <summary>
        /// Gets the application version
        /// </summary>
        protected internal static string Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
        }

        /// <summary>
        /// Gets the applications Product.
        /// </summary>
        protected internal static string Product
        {
            get
            {
                string prodName;
                object[] attributes =
                    Assembly.GetExecutingAssembly().GetCustomAttributes(
                        typeof (AssemblyProductAttribute), false);
                if (attributes.Length > 0)
                {
                    var productAttribute = (AssemblyProductAttribute) attributes[0];
                    if (productAttribute.Product != "")
                    {
                        prodName = productAttribute.Product;
                        if (prodName.Length > 9)
                            prodName = prodName.Substring(0, (prodName.Length - 8));
                        return prodName;
                    }
                }
                prodName = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
                if (prodName.Length > 9)
                    prodName = prodName.Substring(0, (prodName.Length - 8));
                return prodName;
            }
        }

        /// <summary>
        /// Gets the applications FileName.
        /// </summary>
        protected internal static string FileName
        {
            get { return Path.GetFileName(Assembly.GetExecutingAssembly().CodeBase); }
        }

        /// <summary>
        /// Gets the filepath of the application
        /// </summary>
        protected internal static string FilePath
        {
            get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase).Substring(6); }
        }
    }
}