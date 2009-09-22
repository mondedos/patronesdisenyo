/*
 * Creado por SharpDevelop.
 * Usuario: riprimen
 * Fecha: 22/09/2009
 * Hora: 11:58
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;

namespace System.Reflection
{
	using System.Collections.Generic;
	/// <summary>
	/// Description of AssemblyExtensions.
	/// </summary>
	public static class AssemblyExtensions
	{
		/// <summary>
		/// Obtiene una lista de tipos que heredan de un tipo base o bien de una interfaz
		/// </summary>
		/// <param name="ensamblado"></param>
		/// <param name="tipo"></param>
		/// <returns></returns>
		public static List<Type> GetSubTypeOf(this Assembly ensamblado,Type tipo)
		{
			if (tipo.IsInterface) {
				return ensamblado.GetTypes().GetTypesImplementsInterface(tipo);
			}
			else{
				return ensamblado.GetTypes().GetSubTypesOf(tipo);
			}
		}
	}
}
