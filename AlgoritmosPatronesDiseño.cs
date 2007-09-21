using System;

namespace PatronesDiseño
{
    /// <summary>
    /// algoritmo de fuerza bruta pero con mayor inteligencia que backtraking
    /// </summary>
    public abstract class RamificaYApoda<T>
    {
        /// <summary>
        /// hace correr el algoritmo
        /// </summary>
        public void RYA()
        {
            EstructuraDatos ed = this.creaEstructuraDatos();
            this.calculaSolucionInicial();
            T o = this.calculaNodoInicial();
            while (!this.fin(o, ed))
            {
                T[] hijos = this.expandir(o);
                for (int i = 0; i < hijos.Length; i++)
                {
                    if (this.esMejorCota(hijos[i]))
                    {
                        if (this.esSolucion(hijos[i]))
                        {
                            this.actualizaSolucion(hijos[i]);
                            this.eliminaNodos(hijos[i], ed);
                        }
                        else
                            ed.añade(hijos[i]);
                    }
                }
                if (ed.esVacia())
                    o = null;
                else
                    o = ed.obtener();
            }
        }
        /// <summary>
        /// elimina los nodos de la estructura de datos
        /// </summary>
        /// <param name="o"></param>
        /// <param name="ed"></param>
        public abstract void eliminaNodos(T o, EstructuraDatos ed);
        /// <summary>
        /// actualiza la solucion optima por otra mas optima
        /// que viene representada por el nodo
        /// </summary>
        /// <param name="o"></param>
        public abstract void actualizaSolucion(T o);
        /// <summary>
        /// cierto si el nodo o es una solución válida
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public abstract bool esSolucion(T o);
        /// <summary>
        /// cierto si consideramos que o tiene mejor cota que
        /// la solucion optima
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public abstract bool esMejorCota(T o);
        /// <summary>
        /// expande los hijos del nodo
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public abstract T[] expandir(T o);
        /// <summary>
        /// corta el algoritmo cuando se llege al fin.
        /// Si o es igual a null retorna cierto
        /// </summary>
        /// <param name="o"></param>
        /// <param name="ed"></param>
        /// <returns></returns>
        public abstract bool fin(T o, EstructuraDatos ed);
        /// <summary>
        /// Primer nodo del arbol
        /// </summary>
        /// <returns></returns>
        public abstract T calculaNodoInicial();
        /// <summary>
        /// Metodo que calcual una solucion incial al problema
        /// Preferentemene un voraz
        /// </summary>
        public abstract void calculaSolucionInicial();
        /// <summary>
        /// Crea la estructura de datos necesaria para que
        /// corra el algoritmo
        /// </summary>
        /// <returns></returns>
        public abstract EstructuraDatos creaEstructuraDatos();
        /// <summary>
        /// Interface que tiene que cumplir la estructura de
        /// datos que se valla a escoger para implementar la
        /// clase RamificaYApoda
        /// </summary>
        public interface EstructuraDatos<T>
        {
            /// <summary>
            /// extrae un objeto de la estructura de datos
            /// </summary>
            /// <returns></returns>
            T obtener();
            /// <summary>
            /// cierto si la estructura de datos ya no tiene mas
            /// elementos a estudiar
            /// </summary>
            /// <returns></returns>
            bool esVacia();
            /// <summary>
            /// añade un objeto a la estructura de datos
            /// </summary>
            /// <param name="o"></param>
            void añade(T o);
        }
    }
    /// <summary>
    /// Clase que implementa el algoritmo de fuerza bruta
    /// backtraking
    /// </summary>
    public abstract class BackTraking
    {
        /// <summary>
        /// algoritmo de bactraking
        /// </summary>
        /// <param name="x"></param>
        public void BT(Etapa x)
        {
            Etapa xsig;
            Candidato cand;

            if (this.esSolucion(x) && this.esMejor(x))
                this.actualizaSolucion(x);
            xsig = this.nuevaEtapa(x);
            cand = this.calculaCandidatos(x);
            while (this.quedanCandidatos(cand))
            {
                this.SeleccionaCandidato(cand);
                if (this.esPrometedor(cand, xsig))
                {
                    this.anotaSolucion(cand, xsig);
                    this.BT(xsig);
                    this.cancelaAnotacion(cand, xsig);
                }
            }
        }
        /// <summary>
        /// se ha dado el caso trivial, y ademas es mejor solucion
        /// por lo que hay que actualizar la solucion
        /// </summary>
        /// <param name="x"></param>
        public abstract void actualizaSolucion(Etapa x);
        /// <summary>
        /// cancela la solucion provisional que se habia anotado antes
        /// </summary>
        /// <param name="cand"></param>
        /// <param name="xsig"></param>
        public abstract void cancelaAnotacion(Candidato cand, Etapa xsig);
        /// <summary>
        /// guarda localmente la solucion provisional
        /// </summary>
        /// <param name="cand"></param>
        /// <param name="xsig"></param>
        public abstract void anotaSolucion(Candidato cand, Etapa xsig);
        /// <summary>
        /// cierto si el candidato promete llegar a la solucion
        /// </summary>
        /// <param name="cand"></param>
        /// <param name="xsig"></param>
        /// <returns></returns>
        public abstract bool esPrometedor(Candidato cand, Etapa xsig);
        /// <summary>
        /// Selecciona el candidato de la iteracion i-esima
        /// </summary>
        /// <param name="cand"></param>
        public abstract void SeleccionaCandidato(Candidato cand);
        /// <summary>
        /// cierto mientras queden candidatos
        /// </summary>
        /// <param name="cand"></param>
        /// <returns></returns>
        public abstract bool quedanCandidatos(Candidato cand);
        /// <summary>
        /// calcula los posibles candidatos de la etapa k-esima
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public abstract Candidato calculaCandidatos(Etapa x);
        /// <summary>
        /// calcula la siguiente etapa
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public abstract Etapa nuevaEtapa(Etapa x);
        /// <summary>
        /// cierto si la solucion se cumple y ademas es mejor resulotado
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public abstract bool esMejor(Etapa x);
        /// <summary>
        /// Cierto si es un caso trivial o es solucion
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public abstract bool esSolucion(Etapa x);
    }
    /// <summary>
    /// Interfaz que debe implementar objeto del tipo Etapa
    /// </summary>
    public interface Etapa { }
    /// <summary>
    /// Interfaz que debe implementar objeto del tipo Candidato
    /// </summary>
    public interface Candidato { }
    /// <summary>
    /// Clase abstracta con el algoritmo de divide y venceras
    /// </summary>
    public abstract class DivideYVenceras
    {
        /// <summary>
        /// Ejecuta el algoritmo de Divide y vencerás
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public Solucion DyV(Problema p)
        {
            if (this.esCasoBase(p))
                return this.resuelveCasoBase(p);
            else
            {
                Problema[] subproblemas = this.Divide(p);
                Solucion[] subsoluciones = new Solucion[subproblemas.Length];
                for (int i = 0; i < subproblemas.Length; i++)
                    subsoluciones[i] = this.DyV(subproblemas[i]);
                return this.Combina(subsoluciones);
            }
        }
        /// <summary>
        /// Comprovar si el caso es trivial
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public abstract bool esCasoBase(Problema p);
        /// <summary>
        /// Resolucion del caso trivial
        /// </summary>
        /// <param name="p"></param>
        public abstract Solucion resuelveCasoBase(Problema p);
        /// <summary>
        /// Divide el problema si no es caso base
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public abstract Problema[] Divide(Problema p);
        /// <summary>
        /// Combinar las soluciones devuelta por el algoritmo en cada paso
        /// </summary>
        /// <param name="subsoluciones"></param>
        /// <returns></returns>
        public abstract Solucion Combina(Solucion[] subsoluciones);
    }
    /// <summary>
    /// Tipo de debe implementar los objetos que representen un objeto solución.
    /// </summary>
    public interface Solucion { }
    /// <summary>
    /// Tipo de debe implementar los objetos que representen un objeto problema.
    /// </summary>
    public interface Problema { }
    /// <summary>
    /// Clase abstracta con el algoritmo voraz
    /// </summary>
    abstract public class Voraz
    {
        public Voraz()
        {
            //
            // TODO: agregar aquí la lógica del constructor
            //
        }
        /// <summary>
        /// ejecuta el algoritmo voraz
        /// </summary>
        public void ResuelveVoraz()
        {
            this.inicializa();
            while (!this.fin())
            {
                this.SeleccionaYElimina();
                if (this.esPrometedor())
                    this.anotaSolucion();
            }
        }
        /// <summary>
        /// metodo abstracto para inicializar el algoritmo
        /// </summary>
        abstract public void inicializa();
        /// <summary>
        /// si es cierto, corta el algoritmo
        /// </summary>
        /// <returns></returns>
        abstract public bool fin();
        /// <summary>
        /// busca candidatos
        /// </summary>
        abstract public void SeleccionaYElimina();
        /// <summary>
        /// mira si el candidato es bueno
        /// </summary>
        /// <returns></returns>
        abstract public bool esPrometedor();
        /// <summary>
        /// anota la solucion
        /// </summary>
        abstract public void anotaSolucion();
    }
}
namespace Temporizadores
{
    /// <summary>
    /// Clase que se utiliza para medir temporalmente la complejidad de los algoritmos.
    /// </summary>
    public class Temporizador
    {
        private int numPruebas;
        private int[] tiempos;
        private int factorRetardo = 1;
        /// <summary>
        /// Inicializa el temporizador a un número de pruebas determinado.
        /// </summary>
        /// <param name="numPruebas"></param>
        public Temporizador(int numPruebas)
        {
            if (numPruebas < 0)
                throw new System.Exception("El numero de pruebas no puede ser negativo.");
            this.numPruebas = numPruebas;
            tiempos = new int[numPruebas];
        }

        /// <summary>
        /// cronometra el tiempo que tarda en ejecutarse un objeto que cumpla la interfaz temporizable
        /// </summary>
        /// <param name="f"></param>
        public void cronometra(Temporizable f)
        {
            int inicio, fin;
            for (int i = 0; i < this.numPruebas; i++)
                for (int j = 0; j < this.factorRetardo; j++)
                {
                    f.procesamientoInicial();
                    inicio = System.DateTime.Now.Millisecond;
                    f.funcionAMedir();
                    fin = System.DateTime.Now.Millisecond;
                    f.procesamientoFinal();
                    tiempos[i] += fin - inicio;
                }
        }
        /// <summary>
        /// establece el factor de retardo
        /// </summary>
        /// <param name="factorRetardo"></param>
        public void setFactorRetardo(int factorRetardo)
        {
            if (factorRetardo < 1)
                throw new System.Exception("El factor de retardo tiene que ser mayor que cero.");
            this.factorRetardo = factorRetardo;
        }
        /// <summary>
        /// Obtiene una cadena representativa de la clase.
        /// </summary>
        /// <returns></returns>
        public override String ToString()
        {
            // por omisión, el mínimo
            return getInformeTiempoMinimo();
        }
        /// <summary>
        /// Devuelve una cadena con información sobre el tiempo mínimo 
        /// de ejecución.
        /// </summary>
        /// <returns></returns>
        public String getInformeTiempoMinimo()
        {
            String s = "Tiempo mínimo: ("
                + getTiempoMinimo()
                + " con un factor retardo de: "
                + getFactorRetardo()
                + ")\tTiempo mínimo real: "
                + (getTiempoMinimo() / (float)factorRetardo) + " ms";
            return s;
        }
        /// <summary>
        /// Una cadena con información sobre el tiempo promedio 
        /// de ejecución.
        /// </summary>
        /// <returns></returns>
        public String getInformeTiempoMedio()
        {
            String s = "Tiempo medio: " + getTiempoPromedio();
            s += " con un factor retardo de: " + getFactorRetardo();
            s += "\nEl tiempo medio real es: ";
            s += (float)getTiempoPromedio() / (float)factorRetardo;
            return s;
        }
        /// <summary>
        /// obtiene el factor de retardo
        /// </summary>
        /// <returns></returns>
        public int getFactorRetardo()
        {
            return factorRetardo;
        }
        /// <summary>
        /// devuelve el numero de pruebas
        /// </summary>
        /// <returns></returns>
        public long getNumPruebas()
        {
            return numPruebas;
        }
        /// <summary>
        /// Crea un objeto temporizador que se ejecutara una sola vez
        /// </summary>
        public Temporizador()
            : this(1)
        {
        }
        /// <summary>
        /// Calcula el tiempo mínimos de las pruebas hechas
        /// </summary>
        /// <returns></returns>
        public long getTiempoMinimo()
        {
            long minimo;

            minimo = tiempos[0]; // al menos existe tiempos[0]
            for (int i = 1; i < numPruebas; i++)
            {
                if (tiempos[i] < minimo)
                {
                    minimo = tiempos[i];
                }
            }
            return minimo;
        }
        /// <summary>
        /// Calcula el tiempo máximo de las pruebas hechas
        /// </summary>
        /// <returns></returns>
        public long getTiempoMaximo()
        {
            long maximo;

            maximo = tiempos[0]; //al menos existe tiempos[0]
            for (int i = 1; i < numPruebas; i++)
            {
                if (tiempos[i] > maximo)
                {
                    maximo = tiempos[i];
                }
            }
            return maximo;
        }
        /// <summary>
        /// Calcula el tiempo promedio de las ejecuciones
        /// </summary>
        /// <returns></returns>
        public float getTiempoPromedio()
        {
            float sumaTiempos = 0.0f;

            for (int i = 0; i < numPruebas; i++)
            {
                sumaTiempos += tiempos[i];
            }
            return sumaTiempos / (float)numPruebas;
        }
    }
    /// <summary>
    /// Interfaz que debe implementar la clase que contenga un algoritmo que se quiera medir.
    /// </summary>
    public interface Temporizable
    {
        /// <summary>
        ///  	 Debe contener lo que haya que hacer antes
        ///  	 de ejecutar el algoritmo que queramos temporizar.
        ///  	 Típicamente contiene inicializaciones o reinicializaciones
        ///  	  de los datos del problema.
        /// </summary>
        void procesamientoInicial();
        /// <summary>
        ///La función cuyo tiempo de ejecución se quiere medir.
        ///Típicamente contendrá una llamada al algoritmo que resuelva 
        ///el problema planteado, pasándole los valores iniciales que 
        ///necesite.
        /// </summary>
        void funcionAMedir();
        /// <summary>
        ///Debe contener lo que haya que hacer después de ejecutar
        ///el algoritmo que se quiere temporizar.
        /// </summary>
        void procesamientoFinal();
    }
}



