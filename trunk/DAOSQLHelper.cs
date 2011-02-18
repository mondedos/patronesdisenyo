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
    public class ADOHelperFactory
    {
        public static ADOHelper CreateADOHelperOracle(string cadena)
        {
            return CreateADOHelper("System.Data.OracleClient", cadena);
        }
        public static ADOHelper CreateADOHelper(string proveedor, string cadena)
        {
            DbProviderFactory factoria = DbProviderFactories.GetFactory(proveedor);

            return new ADOHelper(factoria, cadena);
        }
    }

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
        public int InsertarObjetoPersistente<T>(T bean)
        {
            using (DbConnection connection = _factoria.CreateConnection())
            {
                connection.ConnectionString = _coneccionBD;
                return InsertarObjetoPersistente<T>(bean, connection);
            }
        }
        public int InsertarObjetoPersistente<T>(T bean, DbConnection connection)
        {
            return InsertarObjetoPersistente<T>(bean, connection, null);
        }

        /// <summary>
        /// Inserta un objeto bean persistente en la base de datos
        /// </summary>
        /// <typeparam name="T">tipo objeto bean persistente</typeparam>
        /// <param name="bean">objeto bean persistente</param>
        /// <returns></returns>
        public int InsertarObjetoPersistente<T>(T bean, DbConnection connection, DbTransaction tr)
        {
            //calculamos el tipo de T para obtener información sobre los objetos que queremos contruir
            //a partir de la base de datos
            Type tipo = typeof(T);

            //comprobamos que el tipo sea persistente
            if (tipo.IsDefined(typeof(ObjetoPersistente), false))
            {
                //generamos el nombre de la tabla por defecto a partir del nombre del tipo
                string nombreTabla = CalcularNombreTabla(tipo);


                #region Calcular el comando de inserccion
                DbCommandBuilder comandBuilder = CrearDBCommandBuilder(nombreTabla, connection, tr);

                DbCommand comandoInsert = comandBuilder.GetInsertCommand(true);
                if (tr != null)
                {
                    comandoInsert.Transaction = tr;
                }
                #endregion

                //connection.Close();

                //connection.Open();

                RellenarParametrosFrom<T>(bean, comandoInsert.Parameters);

                return comandoInsert.ExecuteNonQuery();

            }
            return -1;
        }
        public int ActualizaObjetoPersistenteByClavePrimaria<T>(T beanOriginal, T beanModificado)
        {
            using (DbConnection connection = _factoria.CreateConnection())
            {
                connection.ConnectionString = _coneccionBD;
                return ActualizaObjetoPersistenteByClavePrimaria<T>(beanOriginal, beanModificado, connection);
            }
        }
        public int ActualizaObjetoPersistenteByClavePrimaria<T>(T beanOriginal, T beanModificado, DbConnection connection)
        {
            return ActualizaObjetoPersistenteByClavePrimaria<T>(beanOriginal, beanModificado, connection, null);
        }
        public int ActualizaObjetoPersistenteByClavePrimaria<T>(T beanOriginal, T beanModificado, DbConnection connection, DbTransaction tr)
        {
            //calculamos el tipo de T para obtener información sobre los objetos que queremos contruir
            //a partir de la base de datos
            Type tipo = typeof(T);

            //comprobamos que el tipo sea persistente
            if (tipo.IsDefined(typeof(ObjetoPersistente), false))
            {
                //generamos el nombre de la tabla por defecto a partir del nombre del tipo
                string nombreTabla = CalcularNombreTabla(tipo);



                #region Calcular el comando de inserccion
                DbCommandBuilder comandBuilder = CrearDBCommandBuilder(nombreTabla, connection, tr);

                DbCommand comandoInsert = comandBuilder.GetUpdateCommand(true);
                if (tr != null)
                {
                    comandoInsert.Transaction = tr;
                }
                StringBuilder sql = new StringBuilder(comandoInsert.CommandText.Split("WHERE".ToCharArray())[0]);

                AtributoPersistente k = GetFieldInfoPrimaryKey(beanOriginal);

                if (k != null)
                {
                    sql.AppendFormat(" WHERE {0}=:{0}", k.MapeadoPor);
                }

                #endregion

                //connection.Close();

                //connection.Open();

                DbCommand updateCommand = _factoria.CreateCommand();
                if (tr != null)
                {
                    updateCommand.Transaction = tr;
                }
                updateCommand.CommandText = sql.ToString();
                updateCommand.Connection = connection;

                DbParameterCollection parametrosComando = updateCommand.Parameters;

                //rellenamos las condiciones del comando para buscar el bean a modificar
                RellenarParametrosFrom<T>(beanOriginal, parametrosComando, "Original_");

                //rellenamos los valores que queremos modificar
                RellenarParametrosFrom<T>(beanModificado, parametrosComando);

                return updateCommand.ExecuteNonQuery();

            }
            return -1;
        }
        private AtributoPersistente GetFieldInfoPrimaryKey<T>(T bean)
        {
            //calculamos el tipo de T para obtener información sobre los objetos que queremos contruir
            //a partir de la base de datos
            Type tipo = typeof(T);

            foreach (FieldInfo var in tipo.GetFields((BindingFlags.NonPublic | BindingFlags.Instance)))
            {
                AtributoPersistente[] attrs = var.GetCustomAttributes(typeof(AtributoPersistente), false) as AtributoPersistente[];
                if (attrs.Length > 0)
                {
                    return attrs[0];
                }
            }
            return null;
        }
        /// <summary>
        /// Actualiza un objeto bean persistente en la base de datos
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="beanOriginal"></param>
        /// <param name="beanModificado"></param>
        /// <returns></returns>
        public int ActualizaObjetoPersistente<T>(T beanOriginal, T beanModificado)
        {
            using (DbConnection connection = _factoria.CreateConnection())
            {
                connection.ConnectionString = _coneccionBD;
                return ActualizaObjetoPersistente<T>(beanOriginal, beanModificado, connection);
            }
        }
        /// <summary>
        /// Actualiza un objeto bean persistente en la base de datos
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="beanOriginal"></param>
        /// <param name="beanModificado"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public int ActualizaObjetoPersistente<T>(T beanOriginal, T beanModificado, DbConnection connection)
        {
            return ActualizaObjetoPersistente<T>(beanOriginal, beanModificado, connection, null);
        }
        /// <summary>
        /// Actualiza un objeto bean persistente en la base de datos
        /// </summary>
        /// <typeparam name="T">tipo objeto bean persistente</typeparam>
        /// <param name="beanOriginal">objeto bean persistente de referencia a modificar</param>
        /// <param name="beanModificado">objeto bean persistente con los valores a modificar</param>
        /// <returns></returns>
        public int ActualizaObjetoPersistente<T>(T beanOriginal, T beanModificado, DbConnection connection, DbTransaction tr)
        {
            //calculamos el tipo de T para obtener información sobre los objetos que queremos contruir
            //a partir de la base de datos
            Type tipo = typeof(T);

            //comprobamos que el tipo sea persistente
            if (tipo.IsDefined(typeof(ObjetoPersistente), false))
            {
                //generamos el nombre de la tabla por defecto a partir del nombre del tipo
                string nombreTabla = CalcularNombreTabla(tipo);


                #region Calcular el comando de inserccion
                DbCommandBuilder comandBuilder = CrearDBCommandBuilder(nombreTabla, connection, tr);

                DbCommand comandoInsert = comandBuilder.GetUpdateCommand(true);
                if (tr != null)
                {
                    comandoInsert.Transaction = tr;
                }
                #endregion

                //connection.Close();

                //connection.Open();
                DbParameterCollection parametrosComando = comandoInsert.Parameters;

                //rellenamos las condiciones del comando para buscar el bean a modificar
                RellenarParametrosFrom<T>(beanOriginal, parametrosComando, "Original_");

                //rellenamos los valores que queremos modificar
                RellenarParametrosFrom<T>(beanModificado, parametrosComando);

                return comandoInsert.ExecuteNonQuery();
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
            return CrearDBCommandBuilder(nombreTabla, connection, null);
        }
        /// <summary>
        /// Calcula un DbCommandBuilder que nos ayudará a generar de forma automática los comandos mas comunes
        /// </summary>
        /// <param name="nombreTabla"></param>
        /// <param name="connection"></param>
        /// <param name="tr"></param>
        /// <returns></returns>
        private DbCommandBuilder CrearDBCommandBuilder(string nombreTabla, DbConnection connection, DbTransaction tr)
        {
            DbDataAdapter lo_Adapt = _factoria.CreateDataAdapter();

            DbCommand comandoSelect = _factoria.CreateCommand();
            comandoSelect.Connection = connection;
            if (tr != null)
            {
                comandoSelect.Transaction = tr;
            }
            comandoSelect.CommandType = CommandType.Text;
            comandoSelect.CommandText = string.Format("select * from {0}", nombreTabla);

            lo_Adapt.SelectCommand = comandoSelect;

            DbCommandBuilder comandBuilder = _factoria.CreateCommandBuilder();

            comandBuilder.DataAdapter = lo_Adapt;
            return comandBuilder;
        }

        /// <summary>
        /// Obtiene una colección de objetos bean persistentes
        /// </summary>
        /// <typeparam name="T">Tipo de datos de los objetos devueltos</typeparam>
        /// <param name="factoria">objeto factoría que es necesaria para conectar con los proveedores de datos ADO</param>
        /// <returns>Colección de objetos persistentes</returns>
        public IList<T> ObtenerObjetosPersistentes<T>() where T : new()
        {
            using (IDbConnection connection = _factoria.CreateConnection())
            {
                connection.ConnectionString = _coneccionBD;
                return ObtenerObjetosPersistentes<T>(connection);
            }
        }
        /// <summary>
        /// Obtiene una colección de objetos bean persistentes
        /// </summary>
        /// <typeparam name="T">Tipo genérico</typeparam>
        /// <param name="connection"></param>
        /// <returns>Colección de objetos persistentes</returns>
        public IList<T> ObtenerObjetosPersistentes<T>(IDbConnection connection) where T : new()
        {
            return ObtenerObjetosPersistentes<T>(connection, null);
        }
        /// <summary>
        /// Obtiene una colección de objetos bean persistentes
        /// </summary>
        /// <typeparam name="T">Tipo genérico</typeparam>
        /// <param name="connection"></param>
        /// <param name="tr">transacción</param>
        /// <returns>Colección de objetos persistentes</returns>
        public IList<T> ObtenerObjetosPersistentes<T>(IDbConnection connection, DbTransaction tr) where T : new()
        {
            return ObtenerObjetosPersistentes<T>(null, new Dictionary<string, object>(), CommandType.Text, connection, tr);
        }
        /// <summary>
        /// Obtiene una colección de objetos bean persistentes
        /// </summary>
        /// <typeparam name="T">Tipo genérico</typeparam>
        /// <param name="sql">consulta sql. Si es null o vacia, entonces será una select simple</param>
        /// <param name="parametrosValor"></param>
        /// <param name="tipoComando"></param>
        /// <param name="connection"></param>
        /// <returns>Colección de objetos persistentes</returns>
        public IList<T> ObtenerObjetosPersistentes<T>(string sql, IDictionary<string, object> parametrosValor, CommandType tipoComando, IDbConnection connection) where T : new()
        {
            return ObtenerObjetosPersistentes<T>(sql, parametrosValor, tipoComando, connection, null);
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
        public IList<T> ObtenerObjetosPersistentes<T>(string sql, IDictionary<string, object> parametrosValor, CommandType tipoComando, IDbConnection connection, DbTransaction tr) where T : new()
        {
            List<T> solucion = new List<T>();

            //calculamos el tipo de T para obtener información sobre los objetos que queremos contruir
            //a partir de la base de datos
            Type tipo = typeof(T);

            //comprobamos que el tipo sea persistente
            if (tipo.IsDefined(typeof(ObjetoPersistente), false))
            {
                string nombreTabla = CalcularNombreTabla(tipo);


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

            return solucion;
        }
        /// <summary>
        /// Dado un bean y un datarow que contiene campos cuyo nombre coincide con los nombres de las propiedades
        /// del Bean, copia los valores almacenados en el DataRow a las propiedades equivalentes del Bean.
        /// </summary>
        /// <typeparam name="T">Tipo genérico</typeparam>
        /// <param name="bean">objeto a modificar</param>
        /// <param name="fila">fuente de datos</param>
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
        /// <summary>
        /// Dado un bean persistente y una colección de parámetros con valor, rellena todos los atributos mapeados.
        /// </summary>
        /// <typeparam name="T">Tipo genérico</typeparam>
        /// <param name="bean">objeto a modificar</param>
        /// <param name="parametros">Colección de parámetros</param>
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
        /// <summary>
        /// Cambia el valor de un atributo de un bean con un nuevo valor.
        /// </summary>
        /// <typeparam name="T">Tipo genérico</typeparam>
        /// <param name="bean">objeto a modificar</param>
        /// <param name="var">Información sobre el atributo a cambiar</param>
        /// <param name="nuevoValor">nuevo valor</param>
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
        /// <summary>
        /// Rellena la colección de parametros con los valores de los atributos mapeados en la base de datos
        /// </summary>
        /// <typeparam name="T">Tipo genérico</typeparam>
        /// <param name="bean">Obqueto persistente</param>
        /// <param name="parametros">Colección de parametros que se quiere rellenar</param>
        private void RellenarParametrosFrom<T>(T bean, DbParameterCollection parametros)
        {
            RellenarParametrosFrom<T>(bean, parametros, null);
        }
        /// <summary>
        /// Rellena la colección de parametros con los valores de los atributos mapeados en la base de datos
        /// </summary>
        /// <typeparam name="T">Tipo genérico</typeparam>
        /// <param name="bean">Obqueto persistente</param>
        /// <param name="dbParameterCollection">Colección de parametros que se quiere rellenar</param>
        /// <param name="prefijo">prefijo común que tienen todos los parámetros que se quiere rellenar</param>
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
                        AtributoPersistente atributo = attrs[0];
                        string mapeadoPor = atributo.MapeadoPor;
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
                        dbParameterCollection.Add(CreateParameter(mapeadoPor, null));//esta línea es distinta a la versión vb.net. Hay que estudiar si efectivamente debe estar aqui.
                        if (dbParameterCollection.Contains(mapeadoPor))
                        {
                            DbParameter parametro = dbParameterCollection[mapeadoPor];

                            object objeto = var.GetValue(bean);

                            parametro.Value = DBNull.Value;

                            if (objeto != null && !(atributo.PerminteNulo && atributo.ValorNulo.Equals(Convert.ToString(objeto), StringComparison.InvariantCultureIgnoreCase)))
                            {
                                parametro.Value = objeto;
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Crea y rellena un nuevo parámetro
        /// </summary>
        /// <param name="key">Nombre del parámetro</param>
        /// <param name="value">Valor del parámetro</param>
        /// <returns>Parametro rellenado</returns>
        private DbParameter CreateParameter(string key, object value)
        {
            DbParameter parametro = _factoria.CreateParameter();

            parametro.Value = value;
            parametro.ParameterName = key;

            return parametro;
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
    private bool _EsClavePrimaria;
    /// <summary>
    /// Indica si este atributo es clave primaria que lo identifica
    /// </summary>
    public bool EsClavePrimaria
    {
        get { return _EsClavePrimaria; }
        set { _EsClavePrimaria = value; }
    }
    private bool _PerminteNulo;
    /// <summary>
    /// Indica si este atributo permite valores nulos
    /// </summary>
    public bool PerminteNulo
    {
        get { return _PerminteNulo; }
        set { _PerminteNulo = value; }
    }
    private string _ValorNulo;
    /// <summary>
    /// Valor nulo representado como cadena de texto
    /// </summary>
    public string ValorNulo
    {
        get { return _ValorNulo; }
        set { _ValorNulo = value; }
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