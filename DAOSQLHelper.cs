using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace DAOSQL
{
    public class DAOSQLHelper
    {
        internal static IList rellenarObjeto(SqlConnection conex, Type t)
        {
            return rellenarObjeto(conex, t, null, null, null);
        }
        /// <summary>
        /// Rellena un objeto bean con los datos de la base de datos
        /// Si no existe ninguna tupla en la base de datos
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="where"></param>
        internal static IList rellenarObjeto(SqlConnection conex, Type t, System.Collections.Hashtable where)
        {
            return rellenarObjeto(conex, t, where, null, null);
        }

        /// <summary>
        /// / Rellena un objeto bean con los datos de la base de datos
        /// Si no existe ninguna tupla en la base de datos.
        /// 
        /// El parametro strWhere se le debe indicar una clausula WHERE válida
        /// y sin parametros.
        /// Por ejmplo: 'cbMarq BETWEEN  (1,5) AND cbMarq = 15'
        /// </summary>
        /// <param name="conex"></param>
        /// <param name="t"></param>
        /// <param name="strWhere"></param>
        /// <returns></returns>
        internal static IList rellenarObjeto(SqlConnection conex, Type t, String strWhere)
        {
            return rellenarObjeto(conex, t, null, strWhere, null);
        }

        internal static IList rellenarObjeto(SqlConnection conex, Type t, System.Collections.Hashtable where, String strWhere, SqlTransaction st)
        {
            IList resultados = new ArrayList();

            StringBuilder query = new StringBuilder();
            query.Append("SELECT * ");
            query.Append(" FROM ");
            query.Append(t.Name);

            if (where != null)
            {
                query.Append(" where ");

                bool primero = true;

                foreach (object k in where.Keys)
                {
                    string key = k.ToString();

                    if (primero)
                        primero = false;
                    else
                    {
                        query.Append(" and ");
                    }
                    query.Append(key);
                    query.Append(" = ");
                    query.Append("@" + key);
                }

            }
            else
            {
                //No hemos rellenado la tabla clave-valor,
                //vemos si hemos pasado la cadena de clausula where
                if (!String.IsNullOrEmpty(strWhere))
                {
                    query.Append(" WHERE ");
                    query.Append(strWhere);
                }
            }

            SqlCommand comando = conex.CreateCommand();

            comando.CommandText = query.ToString();
            if (st != null)
                comando.Transaction = st;
            //Le decimos con que cada alfanumerica comenzara el ct_num
            //El % es el caracter comodin
            if (where != null)
            {
                foreach (object k in where.Keys)
                {
                    string key = k.ToString();
                    comando.Parameters.AddWithValue("@" + key, where[key]);
                }
            }

            using (SqlDataReader reader = comando.ExecuteReader())
            {
                if (reader.HasRows)
                {
#if DEBUG
                    Log log = Log.Instance;
                    log.Insertar(comando.CommandText);

                    int resultadosEncontrados = 0;
#endif
                    while (reader.Read())
                    {
#if DEBUG
                        resultadosEncontrados++;
#endif
                        System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();

                        object obj = a.CreateInstance(t.FullName);
                        //TODO: GetFields() solo devuelve campos PUBLICOS no privados
                        foreach (System.Reflection.FieldInfo campo in t.GetFields())
                        {
                            string nombreCampo = campo.Name;
                            object valor = reader[nombreCampo];
                            Type tv = valor.GetType();
                            if (!tv.FullName.Equals("System.DBNull"))
                                campo.SetValue(obj, valor);
                        }

                        resultados.Add(obj);
                    }
#if DEBUG
                    log.Insertar("Tuplas encontradas " + resultadosEncontrados);
#endif
                }
                reader.Close();
            }
            return resultados;
        }
        /// <summary>
        /// Inserta el objeto en la base de datos
        /// </summary>
        /// <param name="conex"></param>
        /// <param name="objeto"></param>
        /// <returns>cierto si se ha tenido exito en la operacion</returns>
        internal static bool PersistirObjetoNuevo(SqlConnection conex, object objeto)
        {
            return PersistirObjetoNuevo(conex, objeto, null);
        }
        /// <summary>
        /// Inserta el objeto en la base de datos
        /// </summary>
        /// <param name="conex"></param>
        /// <param name="objeto"></param>
        /// <param name="st"></param>
        /// <returns>cierto si se ha tenido exito en la operacion</returns>
        internal static bool PersistirObjetoNuevo(SqlConnection conex, object objeto, SqlTransaction st)
        {
            Type t = objeto.GetType();

            bool resultados = true;

            System.Reflection.FieldInfo[] campos = t.GetFields();

            StringBuilder sb = new StringBuilder();
            StringBuilder valores = new StringBuilder();

            bool primero = true;

            foreach (System.Reflection.FieldInfo campo in campos)
            {
                string key = campo.Name;

                if (primero)
                    primero = false;
                else
                {
                    sb.Append(" ,");
                    valores.Append(" ,");
                }
                sb.Append(key);
                valores.Append("@" + key);
            }

            StringBuilder query = new StringBuilder();
            query.Append("INSERT INTO ");
            query.Append(t.Name);
            query.Append(" (");
            query.Append(sb);
            query.Append(") VALUES ( ");
            query.Append(valores);
            query.Append(")");

            SqlCommand comando = conex.CreateCommand();

            comando.CommandText = query.ToString();
            if (st != null)
                comando.Transaction = st;


            foreach (System.Reflection.FieldInfo campo in campos)
            {
                string key = campo.Name;

                object valor = campo.GetValue(objeto);

                comando.Parameters.AddWithValue("@" + key, valor);
            }

            resultados = comando.ExecuteNonQuery() > 0;

#if DEBUG
            if (resultados)
            {
                Log log = Log.Instance;
                log.Insertar(comando.CommandText);

                foreach (System.Reflection.FieldInfo campo in campos)
                {
                    string key = campo.Name;

                    object valor = campo.GetValue(objeto);
                    log.Insertar(key + " = " + valor);
                }
            }
#endif

            return resultados;
        }

        /// <summary>
        /// Hace un UPDATE de SQL sobre la tabla cuyo nombre debe coincidir
        /// con el de la clase del objeto pasado.
        /// Los nombre de los campos de dicha tabla deben coincidir con nombres de atributos publicos
        /// de la clase del objeto pasado. Los campos privados no se persisten.
        ///
        /// La conexión pasada debe estar abierta.
        /// </summary>
        /// <param name="conex"></param>
        /// <param name="objeto"></param>
        /// <param name="where"></param>
        /// <param name="st"></param>
        /// <returns></returns>
        internal static bool PersistirCambiosObjeto(SqlConnection conex, object objeto, Hashtable where, SqlTransaction st)
        {        

            //TODO: realizar este ´metodo

            Type t = objeto.GetType();

            bool resultados = true;

            System.Reflection.FieldInfo[] campos = t.GetFields();

            StringBuilder sb = new StringBuilder();         
            bool primero = true;

            //Construimos el SET de la clausula SQL UPDATE
            foreach (System.Reflection.FieldInfo campo in campos)
            {
                string key = campo.Name;
                object o = campo.Attributes;
                if (primero)
                    primero = false;
                else
                {
                    sb.Append(" ,");
                    //valores.Append(" ,");
                }
                sb.Append(key);
                sb.Append(" = @" + key);
            }



            //Construimos la clausula WHERE de la SQL UPDATE
            StringBuilder sbWhere = new StringBuilder();
            if (where != null && where.Count > 0)
            {

                primero = true;

                foreach (object k in where.Keys)
                {
                    string key = k.ToString();

                    if (primero)
                        primero = false;
                    else
                    {
                        sbWhere.Append(" AND ");
                    }
                    sbWhere.Append(key);
                    sbWhere.Append(" = ");
                    sbWhere.Append("@" + key);
                }

            }
            else
            {
                //No creo que quieras hacer update en la tabla
                //sin ninguna condición.
                return false;
            }

            /**
            * UPDATE table_name
               SET column_name = new_value
               WHERE column_name = some_value
            * */

            StringBuilder query = new StringBuilder();
            query.Append("UPDATE ");
            query.Append(t.Name);
            query.Append(" SET  ");
            query.Append(sb);
            query.Append(" WHERE  ");
            query.Append(sbWhere);
            query.Append("  ");

            //Creamos el comando
            SqlCommand comando = conex.CreateCommand();
            comando.CommandText = query.ToString();
            if (st != null)
                comando.Transaction = st;

            //Le pasamos los parametros al comando
            foreach (System.Reflection.FieldInfo campo in campos)
            {
                string key = campo.Name;

                object valor = campo.GetValue(objeto);

                comando.Parameters.AddWithValue("@" + key, valor);
            }

            //Ejecutamos
            resultados = comando.ExecuteNonQuery() > 0;

#if DEBUG
            if (resultados)
            {
                Log log = Log.Instance;
                log.Insertar(comando.CommandText);

                foreach (System.Reflection.FieldInfo campo in campos)
                {
                    string key = campo.Name;

                    object valor = campo.GetValue(objeto);
                    log.Insertar(key + " = " + valor);
                }
            }
#endif

            return resultados;

        }
    }
}
