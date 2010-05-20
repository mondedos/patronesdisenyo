using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace System
{
    /// <summary>
    /// Extensiones para la clase String
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// Indica cuando una cadena es nula o vacia, considerando bacía la cadena
        /// de longitud cero o longitud n con n espacios en blanco
        /// </summary>
        /// <param name="cadena">Instancia de un objeto System.String</param>
        /// <returns>Cierto si la cadena es nula o vacía.</returns>
        public static bool IsNullOrEmptyTrim(this string cadena)
        {
            return cadena == null || string.IsNullOrEmpty(cadena.Trim());
        }
        /// <summary>
        /// Elimina los acentos de una cadena.
        /// </summary>
        /// <param name="cadena">Instancia de un objeto System.String</param>
        /// <returns>Nueva cadena sin acentos</returns>
        public static string SinAcentos(this string cadena)
        {
            string consignos = "áàäéèëíìïóòöúùuñÁÀÄÉÈËÍÌÏÓÒÖÚÙÜÑçÇ";
            string sinsignos = "aaaeeeiiiooouuunAAAEEEIIIOOOUUUNcC";

            string sol = cadena;

            for (int i = 0; i < consignos.Length; i++)
            {
                sol = sol.Replace(consignos[i], sinsignos[i]);
            }

            return sol;
        }
        /// <summary>
        /// Sustituye los retornos de carros introducidos por otros sistemas operativos,
        /// por los retornos de carros del sistema operativo actual.
        /// </summary>
        /// <param name="cadena">Instancia de un objeto System.String</param>
        /// <returns>Instancia de un nuevo objeto System.String</returns>
        public static string AjustarRetornosCarro(this string cadena)
        {
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(cadena))
            {
                using (StringReader reader = new StringReader(cadena))
                {
                    string linea = reader.ReadLine();

                    while (linea != null)
                    {
                        sb.AppendLine(linea);
                        linea = reader.ReadLine();
                    }
                }
            }

            return sb.ToString();
        }
    }
}
