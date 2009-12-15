using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OrangeTarifaPlanaCalculador
{
    public partial class Propiedades : Form
    {
        public Propiedades(object objeto)
        {
            InitializeComponent();
            propertyGrid1.SelectedObject = objeto;
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            StringBuilder sb = new StringBuilder(textBox1.Text);

            object objeto = propertyGrid1.SelectedObject;

            Type tipo = objeto.GetType();

            GridItem nuevoItem = e.ChangedItem;

            if (tipo.IsSubclassOf(typeof(Control)))
            {
                //si es un objeto de WindowsForm
                Control control = objeto as Control;
                sb.AppendFormat("{0}.{1} = {2};", control.Name, nuevoItem.Label, nuevoItem.Value).AppendLine();
            }




            textBox1.Text = sb.ToString();
        }
    }
}
