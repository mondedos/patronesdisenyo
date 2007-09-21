using System;
using System.Collections;
using PatronesDiseño;

namespace Temporizables.Ordenación
{
    public class KEsimo<T> : DivideYVenceras
    {
        T[] a;
        int umbral;
        internal struct PosPiv
        {
            public int u, v;
        }
        /// <summary>
        /// Obtiene el K-esimo elemento de un array de objetos IComparable
        /// </summary>
        /// <param name="array">array de objetos IComparable</param>
        /// <param name="k">K-esimo</param>
        /// <returns>posición</returns>
        public int ResuelveKEsimo(T[] array, int k)
        {
            return ResuelveKEsimo(array, 0, array.Length - 1, k);
        }
        /// <summary>
        /// Obtiene el K-esimo elemento de un array de objetos IComparable
        /// </summary>
        /// <param name="array">array de objetos IComparable</param>
        /// <param name="i">índice izquierdo</param>
        /// <param name="j">índice derecho</param>
        /// <param name="k">K-esimo</param>
        /// <returns>posición</returns>
        public int ResuelveKEsimo(T[] array, int i, int j, int k)
        {
            a = array;
            return ((SKesimo)DyV(new PKesimo(i, j, k, false))).Pos;
        }
        /// <summary>
        /// Separa los mayores del pivote a la izquierda, agrupa a los elementos iguales al pivote, y los menores del
        /// pivote los agrupa a la derecha del pivote.
        /// </summary>
        /// <param name="p">pivote IComparable</param>
        /// <param name="i">índice izquierdo</param>
        /// <param name="j">índice derecho</param>
        /// <returns>posición del pivote</returns>
        PosPiv pivotar(T p, int i, int j)
        {
            int r, k, b = j;
            T ab;
            r = k = i - 1;
            while (k != b)
            {
                ab = a[k + 1];
                int cmp = ((IComparable)ab).CompareTo(p);
                if (cmp == 0)
                    k++;
                if (cmp < 0)
                {
                    a[k + 1] = a[r];
                    a[r] = ab;
                    k++;
                    r++;
                }
                if (cmp > 0)
                {
                    a[k + 1] = a[b];
                    a[b] = ab;
                    b--;
                }
            }
            PosPiv sol = new PosPiv();
            sol.u = r + 1;
            sol.v = b;
            return sol;
        }
        /// <summary>
        /// Tipo que implementa un problema K-esimo
        /// </summary>
        internal class PKesimo : Problema
        {
            public int i, j, k;
            public bool EnMedio;
            public PKesimo(int i, int j, int k, bool b)
            {
                this.i = i;
                this.j = j;
                this.k = k;
                this.EnMedio = b;
            }
        }
        /// <summary>
        /// Tipo que implementa la solución del K-esimo
        /// </summary>
        internal class SKesimo : Solucion
        {
            int pos;
            public SKesimo(int p)
            {
                this.pos = p;
            }
            /// <summary>
            /// Índice del K-esimo
            /// </summary>
            public int Pos
            {
                set
                {
                    pos = value;
                }
                get
                {
                    return pos;
                }
            }
        }
        /// <summary>
        /// Unificamos la única solución de K-esimo
        /// </summary>
        /// <param name="subsoluciones"></param>
        /// <returns></returns>
        public override Solucion Combina(Solucion[] subsoluciones)
        {
            return subsoluciones[0];
        }
        /// <summary>
        /// Divide el problema K-esimo en un problema K-esimo de menor tamaño
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public override Problema[] Divide(Problema p)
        {
            PKesimo m = (PKesimo)p;
            T piv = a[m.i];//optimizable con la pseudoMediana
            PosPiv suv = pivotar(piv, m.i, m.j);
            int u = suv.u, v = suv.v;
            PKesimo[] subp = new PKesimo[1];
            if ((m.i + m.k - 1) < u)
                subp[0] = new PKesimo(m.i, u - 1, m.i + m.k - 1, false);
            else
            {
                if ((v - m.i + 1) < m.k)
                    subp[0] = new PKesimo(v + 1, m.j, m.k - v + m.i - 1, false);
                else
                    subp[0] = new PKesimo(m.i, m.j, m.k, true);
            }
            return subp;
        }
        /// <summary>
        /// Revisa si es el caso base del problema K-esimo
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public override bool esCasoBase(Problema p)
        {
            PKesimo m = (PKesimo)p;
            return (m.EnMedio || (m.j - m.i + 1) <= this.umbral);
        }
        /// <summary>
        /// Resuelve el caso trivial de K-esimo
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public override Solucion resuelveCasoBase(Problema p)
        {
            PKesimo m = (PKesimo)p;
            if (!m.EnMedio)
            {
                MergeSort<T> ord = new MergeSort<T>();
                ord.Ordenar(a, m.i, m.j);//nlogn
            }
            return new SKesimo(m.i - m.k - 1);
        }
    }
    /// <summary>
    /// Algoritmo de Ordenacion de objectos que implementan la
    /// intefaz IComparable. Lo hace en un tiempo optimo de
    /// orden nlogn
    /// </summary>
    public class MergeSort<T> : DivideYVenceras
    {
        /// <summary>
        /// Ordena un array de objetos IComparable
        /// </summary>
        /// <param name="obj">array de objetos</param>
        public void Ordenar(T[] obj)
        {
            Ordenar(obj, 0, obj.Length - 1);
        }
        /// <summary>
        /// Ordena un array de objetos IComparable desde la posición i a la posición j
        /// </summary>
        /// <param name="obj">array de objetos</param>
        /// <param name="i">primero</param>
        /// <param name="j">último</param>
        public void Ordenar(T[] obj, int i, int j)
        {
            MSSolucion<T> s = (MSSolucion<T>)DyV(new MSProblema<T>(obj, i, j));
        }
        /// <summary>
        /// Unifica dos souciones al de ordenación MergeSort
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public override Solucion Combina(Solucion[] s)
        {
            MSSolucion<T> sol1 = (MSSolucion<T>)s[0], sol2 = (MSSolucion<T>)s[1];
            int s1 = sol1.Primero, s2 = sol2.Primero;
            int pos = 0, n = (sol1.Ultimo - s1) + (sol2.Ultimo - s2) + 2;
            T[] array = new T[n];
            while (s1 <= sol1.Ultimo && s2 <= sol2.Ultimo)
            {
                T obj1 = sol1.Array[s1], obj2 = sol2.Array[s2];
                int cmp = ((IComparable)obj1).CompareTo(obj2);
                if (cmp <= 0)
                {
                    array[pos] = obj1;
                    s1++;
                }
                else
                {
                    array[pos] = obj2;
                    s2++;
                }
                pos++;
            }
            //terminar de rellenar lo que queda
            while (s1 <= sol1.Ultimo)
            {
                array[pos] = sol1.Array[s1];
                pos++;
                s1++;
            }
            while (s2 <= sol2.Ultimo)
            {
                array[pos] = sol2.Array[s2];
                pos++;
                s2++;
            }
            MSSolucion<T> sol = new MSSolucion<T>(sol1.Array, sol1.Primero, sol2.Ultimo);
            for (int i = 0; i < array.Length; i++)
                sol.Array[sol.Primero + i] = array[i];
            return sol;
        }
        /// <summary>
        /// Divide el problema de ordenación MergeSort en 2 problemas de ordenación MergeSort
        /// </summary>
        /// <param name="p">Problema</param>
        /// <returns>2 Problemas</returns>
        public override Problema[] Divide(Problema p)
        {
            MSProblema<T> m = (MSProblema<T>)p;
            int medio = (m.Ultimo + m.Primero) / 2;
            MSProblema<T>[] subp = new MSProblema<T>[2];
            subp[0] = new MSProblema<T>(m.Array, m.Primero, medio);
            subp[1] = new MSProblema<T>(m.Array, medio + 1, m.Ultimo);
            return subp;
        }
        /// <summary>
        /// Caso base del problema de ordenación MergeSort
        /// </summary>
        /// <param name="p">Problema</param>
        /// <returns></returns>
        public override bool esCasoBase(Problema p)
        {
            MSProblema<T> m = (MSProblema<T>)p;
            return m.Ultimo == m.Primero;
        }
        /// <summary>
        /// Resulución del Caso base del problema de ordenación MergeSort
        /// </summary>
        /// <param name="p">Problema</param>
        /// <returns>Solucion</returns>
        public override Solucion resuelveCasoBase(Problema p)
        {
            MSProblema<T> m = (MSProblema<T>)p;
            return new MSSolucion<T>(m.Array, m.Primero, m.Ultimo);
        }
        /// <summary>
        /// Clase que implementa el problema del MergeSort
        /// </summary>
        internal class MSProblema<T> : Problema
        {
            T[] array;
            int primero, ultimo;
            public MSProblema(T[] array, int primero, int ultimo)
            {
                this.array = array;
                this.primero = primero;
                this.ultimo = ultimo;
            }
            public MSProblema(T[] array) : this(array, 0, array.Length - 1) { }
            /// <summary>
            /// Array del problema
            /// </summary>
            public T[] Array
            {
                get
                {
                    return this.array;
                }
            }
            /// <summary>
            /// Índice del primer elemento que se considere del array
            /// </summary>
            public int Primero
            {
                get
                {
                    return this.primero;
                }
            }
            /// <summary>
            /// Índice del último elemento que se considere del array
            /// </summary>
            public int Ultimo
            {
                get
                {
                    return this.ultimo;
                }
            }
        }
        /// <summary>
        /// Clase que implementa la solución del MergeSort
        /// </summary>
        internal class MSSolucion<T> : Solucion
        {
            T[] array;
            int primero, ultimo;
            public MSSolucion(T[] array, int primero, int ultimo)
            {
                this.array = array;
                this.primero = primero;
                this.ultimo = ultimo;
            }
            public MSSolucion(T[] array) : this(array, 0, array.Length - 1) { }
            /// <summary>
            /// Array del problema
            /// </summary>
            public T[] Array
            {
                get
                {
                    return this.array;
                }
            }
            /// <summary>
            /// Índice del primer elemento que se considere del array
            /// </summary>
            public int Primero
            {
                get
                {
                    return this.primero;
                }
            }
            /// <summary>
            /// Índice del último elemento que se considere del array
            /// </summary>
            public int Ultimo
            {
                get
                {
                    return this.ultimo;
                }
            }
        }
    }
}



