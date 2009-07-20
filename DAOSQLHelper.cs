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

			DataTable recordSet = new DataTable();
			System.Data.SqlClient.SqlDataAdapter adaptador = new System.Data.SqlClient.SqlDataAdapter(comando);
			adaptador.Fill(recordSet);
			
//			comentado por ser lo anterior mas optimo
//			using (SqlDataReader reader = comando.ExecuteReader())
//			{
//				if (reader.HasRows)
//				{
//					#if DEBUG
//					Log log = Log.Instance;
//					log.Insertar(comando.CommandText);
//
//					int resultadosEncontrados = 0;
//					#endif
//					while (reader.Read())
//					{
//						#if DEBUG
//						resultadosEncontrados++;
//						#endif
//						System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
//
//						object obj = a.CreateInstance(t.FullName);
//						//TODO: GetFields() solo devuelve campos PUBLICOS no privados
//						foreach (System.Reflection.FieldInfo campo in t.GetFields())
//						{
//							string nombreCampo = campo.Name;
//							object valor = reader[nombreCampo];
//							Type tv = valor.GetType();
//							if (!tv.FullName.Equals("System.DBNull"))
//								campo.SetValue(obj, valor);
//						}
//
//						resultados.Add(obj);
//					}
//					#if DEBUG
//					log.Insertar("Tuplas encontradas " + resultadosEncontrados);
//					#endif
//				}
//				reader.Close();
//			}
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
		
		/// <summary>
		/// Dado un bean y un datarow que contiene campos cuyo nombre coincide con los nombres de las propiedades
		/// del Bean, copia los valores almacenados en el DataRow a las propiedades equivalentes del Bean.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="bean"></param>
		/// <param name="fuente"></param>
		public static void RellenarBean<T>(T bean, DataRow fila)
		{
			DataColumnCollection columnas = fila.Table.Columns;
			foreach (System.Reflection.PropertyInfo propiedad in bean.GetType().GetProperties())
			{
				string nombrePropiedad = propiedad.Name.ToLower();

				if (columnas.Contains(nombrePropiedad))
				{
					object nuevoValor = fila[nombrePropiedad];

					if (propiedad.CanWrite && nuevoValor != null && nuevoValor != System.DBNull.Value && nuevoValor is IConvertible)
					{
						Type tipoP = propiedad.PropertyType;

						if (tipoP.IsEnum)
						{
							propiedad.SetValue(bean, Enum.Parse(tipoP, nuevoValor.ToString().Replace(' ', '_')), null);
						}
						else
						{
							propiedad.SetValue(bean, ChangeType(nuevoValor, propiedad.PropertyType), null);
						}
					}
				}
			}
		}
		public static void RellenarBean(object bean, SqlParameterCollection parametros)
		{
			Type tipo = bean.GetType();

			foreach (System.Reflection.PropertyInfo propiedad in tipo.GetProperties())
			{
				string nombrePropiedad = "@" + propiedad.Name.ToLower();

				if (parametros.Contains(nombrePropiedad))
				{
					object nuevoValor = parametros[nombrePropiedad].Value;

					if (propiedad.CanWrite && nuevoValor != null && nuevoValor != System.DBNull.Value && nuevoValor is IConvertible)
					{
						propiedad.SetValue(bean, ChangeType(nuevoValor, propiedad.PropertyType), null);
					}
				}
			}
		}
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
