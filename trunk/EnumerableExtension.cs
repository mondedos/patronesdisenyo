using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApplication1
{
    public static class EnumerableExtension
    {
        /// <summary>
        /// Realiza la operacion de reordenación aleatoria de los elementos de una enumeración.
        /// </summary>
        /// <typeparam name="T">Tipo de la enumeración</typeparam>
        /// <param name="enumeracion">Enumeración a reordenar</param>
        /// <returns>Array con los elementos de la enumeración reordenados.</returns>
        public static T[] Shuffle<T>(this IEnumerable<T> enumeracion)
        {
            Random random = new Random();

            List<KeyValuePair<int, T>> list = new List<KeyValuePair<int, T>>();

            foreach (T item in enumeracion)
            {
                list.Add(new KeyValuePair<int, T>(random.Next(), item));
            }

            IEnumerable<KeyValuePair<int, T>> sorted = from item in list
                                                       orderby item.Key
                                                       select item;

            T[] array = new T[list.Count];
            int i = 0;

            foreach (KeyValuePair<int, T> item in sorted)
            {
                array[i] = item.Value;
                i++;
            }

            return array;
        }

    }
}
