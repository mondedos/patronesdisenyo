/*
 * Creado por SharpDevelop.
 * Usuario: riprimen
 * Fecha: 22/09/2009
 * Hora: 8:51
 *
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;

namespace System.Windows.Forms
{
	using System.Collections.Generic;
	/// <summary>
	/// Clase que engloba métodos extensores de utilidad para el desarrollo
	/// de aplicaciones de formulario con System.Windows.Forms
	/// </summary>
	public static  class WinFormControlExtensions
	{
		/// <summary>
		/// Establece una cadena de caracteres a la propiedad Text de todos
		/// los controles de esta colección.
		/// </summary>
		/// <param name="controles">Colección de controles</param>
		/// <param name="valor">Cadena de caracteres</param>
		public static void SetText<T>(this IEnumerable<T> controles, string valor) where T:Control
		{
			foreach (T var in controles)
			{
				var.Text=valor;
			}
		}
		/// <summary>
		/// Obtiene una lista de controles hijos que son instancias del tipo que se le pasa como argumento.
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		public static List<T> GetControls<T>(this Control c) where T:Control
		{
			Type tipo=typeof(T);
			List<T> solucion=new List<T>();
			
			foreach (Control var in c.Controls)
			{
				solucion.AddRange(var.GetControls<T>());
				if (tipo.IsInstanceOfType(var)) {
					solucion.Add(var as T);
				}
			}
			
			return solucion;
		}
		/// <summary>
		/// Establece la propiedad enable a todos los controles de la colección.
		/// </summary>
		/// <param name="controles"></param>
		/// <param name="valor">Boolean</param>
		public static void SetEnable<T>(this IEnumerable<T> controles,bool valor)where T:Control
		{
			foreach (T var in controles)
			{
				var.Enabled=valor;
			}
		}
		/// <summary>
		/// Establece la propiedad ReadOnly a todos los controles que heredan de <see cref="System.Windows.Forms.TextBoxBase"></see>.
		/// </summary>
		/// <param name="controles"></param>
		/// <param name="valor">Boolean</param>
		public static void SetReadOnly<T>(this IEnumerable<T> controles,bool valor)where T:TextBoxBase
		{
			foreach (T var in controles)
			{
				var.ReadOnly=valor;
			}
		}
		/// <summary>
		/// Establece un valor por defecto a todos los controles de tipo <see cref="System.Windows.Forms.ListControl"></see>.
		/// </summary>
		/// <param name="controles"></param>
		/// <param name="valor"></param>
		public static void SetSelectedValue<T>(this IEnumerable<T> controles,object valor) where T:ListControl
		{
			foreach (T var in controles)
			{
				var.SelectedValue=valor;
			}
		}
		/// <summary>
		/// Busca un control de tipo T cuyo nombre sea el pasado por parámetro.
		/// </summary>
		/// <param name="c"></param>
		/// <param name="name">Nombre del control</param>
		/// <returns>null si no se encuentra o Control si se ha encontrado.</returns>
		public static T FindControl<T>(this Control c,string name) where T:Control
		{
			List<T> controles=c.GetControls<T>();
			T solucion=null;
			
			foreach (T var in controles)
			{
				if (var.Name.Equals(name,StringComparison.InvariantCultureIgnoreCase)) {
					solucion=var;
					break;
				}
			}
			
			return solucion;
		}
	}
}
