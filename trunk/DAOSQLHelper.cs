using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Reflection;
using System.ComponentModel;
using System.Data.Common;

namespace DAOSQL
{
    public class ADOHelper
    {
        #region Persistencia ADO multiproveedor


        private string _coneccionBD;
        /// <summary>
        /// Obtiene la cadena de conción a la base de datos
        /// </summary>
        public string CadenaConeccion
        {
            get { return _coneccionBD; }
        }
        private DbProviderFactory _factoria;

        public ADOHelper(DbProviderFactory factoria, string cadenaConeccion)
        {
            _factoria = factoria;
            _coneccionBD = cadenaConeccion;
        }

        /// <summary>
        /// Obtiene una colección de objetos bean persistentes
        /// </summary>
        /// <typeparam name="T">Tipo de datos de los objetos devueltos</typeparam>
        /// <param name="factoria">objeto factoría que es necesaria para conectar con los proveedores de datos ADO</param>
        /// <returns>Colección de objetos persistentes</returns>
        public IList<T> ObtenerObjetosPersistentes<T>() where T : new()
        {
            return ObtenerObjetosPersistentes<T>(null, new Dictionary<string, object>(), CommandType.Text);
        }
        /// <summary>
        /// Obtiene una colección de objetos bean a partir de una consulta sql
        /// </summary>
        /// <typeparam name="T">Tipo de datos de los objetos devueltos</typeparam>
        /// <param name="factoria">objeto factoría que es necesaria para conectar con los proveedores de datos ADO</param>
        /// <param name="sql">consulta sql. Si es null o vacia, entonces será una select simple</param>
        /// <param name="parametrosValor">diccionario de parametros, donde la clave es el nombre del parametro, y el valor es el valor del parametro</param>
        /// <param name="tipoComando">Tipo de comando que devuelve los datos deseados (texto o prodecimiento almacenado)</param>
        /// <returns>Colección de objetos persistentes</returns>
        public IList<T> ObtenerObjetosPersistentes<T>(string sql, IDictionary<string, object> parametrosValor, CommandType tipoComando) where T : new()
        {
            List<T> solucion = new List<T>();

            //calculamos el tipo de T para obtener información sobre los objetos que queremos contruir
            //a partir de la base de datos
            Type tipo = typeof(T);

            //comprobamos que el tipo sea persistente
            if (tipo.IsDefined(typeof(ObjetoPersistente), false))
            {
                string nombreTabla = CalcularNombreTabla(tipo);

                using (IDbConnection connection = _factoria.CreateConnection())
                {
                    connection.ConnectionString = _coneccionBD;
                    IDbDataAdapter lo_Adapt = _factoria.CreateDataAdapter();
                    IDbCommand comandoSelect = _factoria.CreateCommand();

                    //comprobamos si nos han especificado una consulta sql o no
                    if (string.IsNullOrEmpty(sql))
                    {
                        comandoSelect.CommandText = string.Format("select * from {0}", nombreTabla);
                        comandoSelect.CommandType = CommandType.Text;
                    }
                    else
                    {
                        comandoSelect.CommandText = sql;

                        //en este caso como nos han especificado una consulta sql, vemos si hay que rellenar parametros

                        IDataParameterCollection parametrosComando = comandoSelect.Parameters;
                        foreach (KeyValuePair<string, object> var in parametrosValor)
                        {
                            IDbDataParameter pa = comandoSelect.CreateParameter();
                            pa.ParameterName = var.Key;
                            pa.Value = var.Value;

                            parametrosComando.Add(pa);
                        }
                        comandoSelect.CommandType = tipoComando;
                    }
                    comandoSelect.Connection = connection;

                    lo_Adapt.SelectCommand = comandoSelect;

                    DataSet ds = new DataSet();
                    lo_Adapt.Fill(ds);

                    //por cada registro devuelto, creamos un objeto bean
                    foreach (DataRow var in ds.Tables[0].Rows)
                    {
                        T profesional = new T();

                        //rellenamos cada bean
                        ADOHelper.RellenarBean<T>(profesional, var);
                        //y la añadimos a la solución
                        solucion.Add(profesional);
                    }
                }
            }

            return solucion;
        }

        private static string CalcularNombreTabla(Type tipo)
        {
            //generamos el nombre de la tabla por defecto a partir del nombre del tipo
            string nombreTabla = tipo.Name;
            ObjetoPersistente atributo = null;
            //buscamos si existe un nombre especifico que mapea el objeto en la base de datos
            ObjetoPersistente[] attrs = tipo.GetCustomAttributes(typeof(ObjetoPersistente), false) as ObjetoPersistente[];
            if (attrs.Length > 0 && !string.IsNullOrEmpty((atributo = attrs[0]).MapeadoPor))
            {
                nombreTabla = atributo.MapeadoPor;
            }
            return nombreTabla;
        }

        /// <summary>
        /// Inserta un objeto bean persistente en la base de datos
        /// </summary>
        /// <typeparam name="T">tipo objeto bean persistente</typeparam>
        /// <param name="bean">objeto bean persistente</param>
        /// <returns></returns>
        public int InsertarObjetoPersistente<T>(T bean)
        {
            //calculamos el tipo de T para obtener información sobre los objetos que queremos contruir
            //a partir de la base de datos
            Type tipo = typeof(T);

            //comprobamos que el tipo sea persistente
            if (tipo.IsDefined(typeof(ObjetoPersistente), false))
            {
                //generamos el nombre de la tabla por defecto a partir del nombre del tipo
                string nombreTabla = CalcularNombreTabla(tipo);


                using (DbConnection connection = _factoria.CreateConnection())
                {
                    connection.ConnectionString = _coneccionBD;


                    #region Calcular el comando de inserccion
                    DbCommandBuilder comandBuilder = CrearDBCommandBuilder(nombreTabla, connection);

                    DbCommand comandoInsert = comandBuilder.GetInsertCommand(true);

                    #endregion

                    connection.Close();

                    connection.Open();

                    RellenarParametrosFrom<T>(bean, comandoInsert.Parameters);

                    return comandoInsert.ExecuteNonQuery();
                }
            }
            return -1;
        }
        /// <summary>
        /// Actualiza un objeto bean persistente en la base de datos
        /// </summary>
        /// <typeparam name="T">tipo objeto bean persistente</typeparam>
        /// <param name="beanOriginal">objeto bean persistente de referencia a modificar</param>
        /// <param name="beanModificado">objeto bean persistente con los valores a modificar</param>
        /// <returns></returns>
        public int ActualizaObjetoPersistente<T>(T beanOriginal, T beanModificado)
        {
            //calculamos el tipo de T para obtener información sobre los objetos que queremos contruir
            //a partir de la base de datos
            Type tipo = typeof(T);

            //comprobamos que el tipo sea persistente
            if (tipo.IsDefined(typeof(ObjetoPersistente), false))
            {
                //generamos el nombre de la tabla por defecto a partir del nombre del tipo
                string nombreTabla = CalcularNombreTabla(tipo);


                using (DbConnection connection = _factoria.CreateConnection())
                {
                    connection.ConnectionString = _coneccionBD;


                    #region Calcular el comando de inserccion
                    DbCommandBuilder comandBuilder = CrearDBCommandBuilder(nombreTabla, connection);

                    DbCommand comandoInsert = comandBuilder.GetUpdateCommand(true);

                    #endregion

                    connection.Close();

                    connection.Open();
                    DbParameterCollection parametrosComando = comandoInsert.Parameters;

                    //rellenamos las condiciones del comando para buscar el bean a modificar
                    RellenarParametrosFrom<T>(beanOriginal, parametrosComando, "Original_");

                    //rellenamos los valores que queremos modificar
                    RellenarParametrosFrom<T>(beanModificado, parametrosComando);

                    return comandoInsert.ExecuteNonQuery();
                }
            }
            return -1;
        }

        /// <summary>
        /// Calcula un DbCommandBuilder que nos ayudará a generar de forma automática los comandos mas comunes
        /// </summary>
        /// <param name="nombreTabla"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        private DbCommandBuilder CrearDBCommandBuilder(string nombreTabla, DbConnection connection)
        {
            DbDataAdapter lo_Adapt = _factoria.CreateDataAdapter();

            DbCommand comandoSelect = _factoria.CreateCommand();
            comandoSelect.Connection = connection;
            comandoSelect.CommandType = CommandType.Text;
            comandoSelect.CommandText = string.Format("select * from {0}", nombreTabla);

            lo_Adapt.SelectCommand = comandoSelect;

            DbCommandBuilder comandBuilder = _factoria.CreateCommandBuilder();

            comandBuilder.DataAdapter = lo_Adapt;
            return comandBuilder;
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
            //if (resultados)
            //{
            //    Log log = Log.Instance;
            //    log.Insertar(comando.CommandText);

            //    foreach (System.Reflection.FieldInfo campo in campos)
            //    {
            //        string key = campo.Name;

            //        object valor = campo.GetValue(objeto);
            //        log.Insertar(key + " = " + valor);
            //    }
            //}
#endif

            return resultados;

        }

        /// <summary>
        /// Dado un bean y un datarow que contiene campos cuyo nombre coincide con los nombres de las propiedades
        /// del Bean, copia los valores almacenados en el DataRow a las propiedades equivalentes del Bean.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bean"></param>
        /// <param name="fuente"></param>
        public static void RellenarBean<T>(T bean, DataRow fila)
        {
            Type tipo = typeof(T);

            //comprobamos que sea un objeto persistente
            if (tipo.IsDefined(typeof(ObjetoPersistente), false))
            {
                DataColumnCollection columnas = fila.Table.Columns;

                //hacemos bind de todos los atributos de la case
                foreach (FieldInfo var in tipo.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
                {
                    //para cada atributo vemos si es persistente
                    AtributoPersistente[] attrs = var.GetCustomAttributes(typeof(AtributoPersistente), false) as AtributoPersistente[];
                    if (attrs.Length > 0)
                    {
                        //obtenemos el nombre del campo que mapea este atributo
                        string mapeadoPor = attrs[0].MapeadoPor;
                        //si no se ha especificado ningún nombre de columa de la base datos, le asignamos el nombre del atributo
                        if (string.IsNullOrEmpty(mapeadoPor))
                        {
                            mapeadoPor = var.Name;
                        }

                        //vemos si existe una columna que contenga el nombre que se mapea
                        if (columnas.Contains(mapeadoPor))
                        {
                            object nuevoValor = fila[mapeadoPor];

                            //miramos si el elemento de la columana no sea un valor nulo
                            if (!fila.IsNull(mapeadoPor) && nuevoValor is IConvertible)
                            {
                                AsignaValorAtributo<T>(bean, var, nuevoValor);
                            }
                        }
                    }
                }
            }
        }
        public static void RellenarBean<T>(T bean, DbParameterCollection parametros)
        {
            Type tipo = typeof(T);

            //comprobamos que sea un objeto persistente
            if (tipo.IsDefined(typeof(ObjetoPersistente), false))
            {

                foreach (FieldInfo var in tipo.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
                {
                    //para cada atributo vemos si es persistente
                    AtributoPersistente[] attrs = var.GetCustomAttributes(typeof(AtributoPersistente), false) as AtributoPersistente[];
                    if (attrs.Length > 0)
                    {
                        //obtenemos el nombre del campo que mapea este atributo
                        string mapeadoPor = attrs[0].MapeadoPor;
                        //si no se ha especificado ningún nombre de columa de la base datos, le asignamos el nombre del atributo
                        if (string.IsNullOrEmpty(mapeadoPor))
                        {
                            mapeadoPor = var.Name;
                        }

                        if (parametros.Contains(mapeadoPor))
                        {
                            object nuevoValor = parametros[mapeadoPor].Value;

                            AsignaValorAtributo<T>(bean, var, nuevoValor);
                        }
                    }
                }
            }
        }

        private static void AsignaValorAtributo<T>(T bean, FieldInfo var, object nuevoValor)
        {
            Type tipoP = var.FieldType;

            if (tipoP.IsEnum)
            {
                var.SetValue(bean, Enum.Parse(tipoP, nuevoValor.ToString().Replace(' ', '_')));
            }
            else
            {
                var.SetValue(bean, ChangeType(nuevoValor, var.FieldType));
            }
        }
        private void RellenarParametrosFrom<T>(T bean, DbParameterCollection parametros)
        {
            RellenarParametrosFrom<T>(bean, parametros, null);
        }
        private void RellenarParametrosFrom<T>(T bean, DbParameterCollection dbParameterCollection, string prefijo)
        {
            Type tipo = typeof(T);

            //comprobamos que sea un objeto persistente
            if (tipo.IsDefined(typeof(ObjetoPersistente), false))
            {
                //hacemos bind de todos los atributos de la case
                foreach (FieldInfo var in tipo.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
                {
                    //para cada atributo vemos si es persistente
                    AtributoPersistente[] attrs = var.GetCustomAttributes(typeof(AtributoPersistente), false) as AtributoPersistente[];
                    if (attrs.Length > 0)
                    {
                        //obtenemos el nombre del campo que mapea este atributo
                        string mapeadoPor = attrs[0].MapeadoPor;
                        //si no se ha especificado ningún nombre de columa de la base datos, le asignamos el nombre del atributo
                        if (string.IsNullOrEmpty(mapeadoPor))
                        {
                            mapeadoPor = var.Name;
                        }
                        if (!string.IsNullOrEmpty(prefijo))
                        {
                            mapeadoPor = prefijo + mapeadoPor;
                        }

                        //vemos si existe una columna que contenga el nombre que se mapea
                        if (dbParameterCollection.Contains(mapeadoPor))
                        {

                            DbParameter parametro = dbParameterCollection[mapeadoPor];

                            parametro.Value = var.GetValue(bean);
                        }
                    }
                }
            }
        }

        #endregion



        /// <summary>
        /// Dado un bean y un IDictionary que contiene elementos cuya key coincide con los nombres de las propiedades
        /// del Bean, copia los valores del IDictionary a las propiedades equivalentes del Bean.
        /// Las "keys" del IDictonary deben de estar en minusculas para que funcione correctamente.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bean"></param>
        /// <param name="fuente"></param>
        public static void RellenarBean<T>(object bean, IDictionary<string, T> fuente) where T : IConvertible
        {
            Type tipo = bean.GetType();

            foreach (System.Reflection.PropertyInfo propiedad in tipo.GetProperties())
            {
                string nombrePropiedad = propiedad.Name.ToLower();

                if (fuente.ContainsKey(nombrePropiedad))
                {
                    T nuevoValor = fuente[nombrePropiedad];

                    if (propiedad.CanWrite && nuevoValor != null && !string.IsNullOrEmpty(nuevoValor.ToString()))
                    {
                        propiedad.SetValue(bean, ChangeType(nuevoValor, propiedad.PropertyType), null);
                    }
                }
            }
        }
        public static void CopiarPropiedadesTipo<T1, T2>(T1 origen, T2 destino)
        {
            Type tipoPropiedad = null;
            System.Reflection.PropertyInfo pdestino = null;
            foreach (System.Reflection.PropertyInfo var in origen.GetType().GetProperties())
            {
                pdestino = destino.GetType().GetProperty(var.Name);
                if (var.CanRead && pdestino != null && pdestino.CanWrite)
                {
                    object valor = var.GetValue(origen, null);
                    tipoPropiedad = pdestino.PropertyType;
                    if (valor != null)
                    {
                        if (tipoPropiedad.IsEnum)
                        {
                            pdestino.SetValue(destino, Enum.Parse(tipoPropiedad, valor.ToString()), null);
                        }
                        else if (tipoPropiedad.IsArray)
                        {
                            Type tiarr = Type.GetType(tipoPropiedad.FullName, true);
                            object[] avalor = valor as object[];

                            pdestino.SetValue(destino, Activator.CreateInstance(tiarr, new object[] { avalor.Length }), null);


                            object[] nuevoarray = pdestino.GetValue(destino, null) as object[];
                            //copiamos por reflexion los objetos que cuelgan
                            object vi = null;
                            for (int i = 0; i < avalor.Length; i++)
                            {
                                vi = avalor[i];
                                if (vi is ICloneable)
                                {
                                    nuevoarray[i] = ((ICloneable)vi).Clone();
                                }
                            }
                        }
                        else if (valor is IConvertible)
                        {
                            pdestino.SetValue(destino, ChangeType(valor, tipoPropiedad), null);
                        }
                        else if (valor is ICloneable)
                        {
                            pdestino.SetValue(destino, ((ICloneable)valor).Clone(), null);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Nueva version de Conver.ChangeType que es capaz de trabajar con tipos nulables.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="conversionType"></param>
        /// <returns></returns>
        public static object ChangeType(object value, Type conversionType)
        {
            // Note: This if block was taken from Convert.ChangeType as is, and is needed here since we're
            // checking properties on conversionType below.
            if (conversionType == null)
            {
                throw new ArgumentNullException("conversionType");
            } // end if

            // If it's not a nullable type, just pass through the parameters to Convert.ChangeType

            if (conversionType.IsGenericType &&
                conversionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                // It's a nullable type, so instead of calling Convert.ChangeType directly which would throw a
                // InvalidCastException (per http://weblogs.asp.net/pjohnson/archive/2006/02/07/437631.aspx),
                // determine what the underlying type is
                // If it's null, it won't convert to the underlying type, but that's fine since nulls don't really
                // have a type--so just return null
                // Note: We only do this check if we're converting to a nullable type, since doing it outside
                // would diverge from Convert.ChangeType's behavior, which throws an InvalidCastException if
                // value is null and conversionType is a value type.
                if (value == null)
                {
                    return null;
                } // end if

                // It's a nullable type, and not null, so that means it can be converted to its underlying type,
                // so overwrite the passed-in conversion type with this underlying type
                NullableConverter nullableConverter = new NullableConverter(conversionType);
                conversionType = nullableConverter.UnderlyingType;
            } // end if

            // Now that we've guaranteed conversionType is something Convert.ChangeType can handle (i.e. not a
            // nullable type), pass the call on to Convert.ChangeType
            return Convert.ChangeType(value, conversionType);
        }
    }



}

public class ObjectBase : ICloneable
{
    public ObjectBase()
    {

    }

    #region ICloneable Members

    public object Clone()
    {
        //First we create an instance of this specific type.

        object newObject = Activator.CreateInstance(this.GetType());

        //We get the array of fields for the new type instance.

        PropertyInfo[] properties = newObject.GetType().GetProperties();

        int i = 0;

        foreach (PropertyInfo fi in this.GetType().GetProperties())
        {
            //We query if the fiels support the ICloneable interface.

            Type ICloneType = fi.PropertyType.GetInterface("ICloneable", true);

            PropertyInfo propiedad = properties[i];

            if (ICloneType != null)
            {
                //Getting the ICloneable interface from the object.
                ICloneable IClone = (ICloneable)fi.GetValue(this, null);
                if (IClone != null)
                {
                    //We use the clone method to set the new value to the field.
                    propiedad.SetValue(newObject, IClone.Clone(), null);
                }
                else
                {
                    propiedad.SetValue(newObject, fi.GetValue(this, null), null);
                }
            }
            else
            {
                // If the field doesn't support the ICloneable

                // interface then just set it.

                propiedad.SetValue(newObject, fi.GetValue(this, null), null);
            }

            //Now we check if the object support the
            //IEnumerable interface, so if it does
            //we need to enumerate all its items and check if
            //they support the ICloneable interface.
            Type IEnumerableType = fi.PropertyType.GetInterface
            ("IEnumerable", true);
            if (IEnumerableType != null)
            {
                //Get the IEnumerable interface from the field.
                IEnumerable IEnum = (IEnumerable)fi.GetValue(this, null);

                if (IEnum != null)
                {
                    //This version support the IList and the

                    //IDictionary interfaces to iterate on collections.

                    Type IListType = propiedad.PropertyType.GetInterface
                    ("IList", true);
                    Type IDicType = propiedad.PropertyType.GetInterface
                    ("IDictionary", true);

                    int j = 0;
                    if (IListType != null)
                    {
                        //Getting the IList interface.

                        IList list = (IList)propiedad.GetValue(newObject, null);

                        foreach (object obj in IEnum)
                        {
                            //Checking to see if the current item

                            //support the ICloneable interface.

                            ICloneType = obj.GetType().
                            GetInterface("ICloneable", true);

                            if (ICloneType != null)
                            {
                                //If it does support the ICloneable interface,

                                //we use it to set the clone of

                                //the object in the list.

                                ICloneable clone = (ICloneable)obj;

                                list[j] = clone.Clone();
                            }

                            //NOTE: If the item in the list is not

                            //support the ICloneable interface then in the

                            //cloned list this item will be the same

                            //item as in the original list

                            //(as long as this type is a reference type).


                            j++;
                        }
                    }
                    else if (IDicType != null)
                    {
                        //Getting the dictionary interface.

                        IDictionary dic = (IDictionary)propiedad.GetValue(newObject, null);
                        j = 0;

                        foreach (DictionaryEntry de in IEnum)
                        {
                            //Checking to see if the item

                            //support the ICloneable interface.

                            ICloneType = de.Value.GetType().
                            GetInterface("ICloneable", true);

                            if (ICloneType != null)
                            {
                                ICloneable clone = (ICloneable)de.Value;

                                dic[de.Key] = clone.Clone();
                            }
                            j++;
                        }
                    }
                }
            }
            i++;
        }
        return newObject;
    }
    #endregion
}

/// <summary>
/// Indica que un miembro de la clase es persistente
/// </summary>
public class AtributoPersistente : System.Attribute
{
    private string _mapeadoPor;
    /// <summary>
    /// Obtiene o establece el nombre del campo de la base datos que mapea el miembro
    /// </summary>
    public string MapeadoPor
    {
        get { return _mapeadoPor; }
        set { _mapeadoPor = value; }
    }

}
/// <summary>
/// Declara una clase como clase bean o persistente
/// </summary>
public class ObjetoPersistente : System.Attribute
{
    private string _mapeadoPor;
    /// <summary>
    /// Nombre de la tabla que mapea el objeto Persistente
    /// </summary>
    public string MapeadoPor
    {
        get { return _mapeadoPor; }
        set { _mapeadoPor = value; }
    }

}