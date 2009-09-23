/*
 * Creado por SharpDevelop.
 * Usuario: riprimen
 * Fecha: 22/09/2009
 * Hora: 12:08
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;

namespace System
{
	using System.Collections.Generic;
	using System.Reflection;
	using System.Linq;
	/// <summary>
	/// Description of TypeExtensions.
	/// </summary>
	public static class TypeExtensions
	{
		/// <summary>
		/// Obtiene una lista de tipos que implementan la interfaz dada.
		/// </summary>
		/// <param name="tipos"></param>
		/// <param name="interfaz">Interfaz</param>
		/// <returns>Lista de tipos que implementan la interfaz</returns>
		public static List<Type> GetTypesImplementsInterface(this IEnumerable<Type> tipos,Type interfaz)
		{
			List<Type> solucion=new List<Type>();
			
			foreach (Type tipo in tipos)
			{
				if (!tipo.IsInterface)
				{
					foreach (Type interfazQueImplementa in tipo.GetInterfaces())
					{
						if (interfazQueImplementa.Equals(interfaz))
						{
							solucion.Add(tipo);
							break;
						}
					}
				}
			}
			return solucion;
		}
		/// <summary>
		/// Obtiene una lista de tipos que herecan de un tipo padre.
		/// </summary>
		/// <param name="tipos"></param>
		/// <param name="padre">Tipo base</param>
		/// <returns>Lista de tipos que heredan de un tipo padre</returns>
		public static List<Type> GetSubTypesOf(this IEnumerable<Type> tipos,Type padre)
		{
			List<Type> solucion=new List<Type>();
			foreach (Type tipo in tipos){
				if (tipo.IsSubclassOf(padre)) {
					solucion.Add(tipo);
				}
			}
			
			return solucion;
		}
		/// <summary>
		/// Modifica un atributo privado declarado en el tipo.
		/// </summary>
		/// <param name="t"></param>
		/// <param name="name">Nombre del atributo privado</param>
		/// <param name="objeto">Objeto a modificar</param>
		/// <param name="valor"></param>
		public static void SetPrivateAttributeValue(this Type t,string name,object objeto,object valor)
		{
			Type tipoObj=objeto.GetType();
			
			if (tipoObj.Equals(t)||tipoObj.IsSubclassOf(t))
			{
				foreach (FieldInfo var in t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
				{
					if (var.Name.Equals(name))
					{
						var.SetValue(objeto, valor);
						break;
					}
				}
			}
		}
	}
}
