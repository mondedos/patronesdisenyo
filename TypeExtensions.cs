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
	/// <summary>
	/// Description of TypeExtensions.
	/// </summary>
	public static class TypeExtensions
	{
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
	}
}
