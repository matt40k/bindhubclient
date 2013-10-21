/*
 * Developer : Matt Smith (matt@matt40k.co.uk)
 * All code (c) Matthew Smith all rights reserved
 */

using System.IO;
using System.Reflection;

namespace BindHub.Client.Library
{
    class GetExe
    {
        /// <summary>
        /// Gets the application version
        /// </summary>
        protected internal static string Version
        {
            get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
        }

        /// <summary>
        /// Gets the applications Product.
        /// </summary>
        protected internal static string Product
        {
            get
            {
                object[] attributes =
                    System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(
                        typeof (AssemblyProductAttribute), false);
                if (attributes.Length > 0)
                {
                    var productAttribute = (AssemblyProductAttribute)attributes[0];
                    if (productAttribute.Product != "")
                    {
                        return productAttribute.Product;
                    }
                }
                return Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        /// <summary>
        /// Gets the applications FileName.
        /// </summary>
        protected internal static string FileName
        {
            get
            {
                return Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
            }
        }


        protected internal static string FilePath
        {
            get
            {
                return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).Substring(6);
            }
        }
    }
}
