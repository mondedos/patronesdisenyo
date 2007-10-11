using System;
using System.Text;
using System.IO;


public class Log
{
    private StringBuilder sb =new StringBuilder();
    private string _directorioTemporal = "", _archivoLog = "";
    private bool _enable = true;
    private static Log instance = null;
    static readonly object padlock = new object();
    /// <summary>
    /// Obtiene una instancia del sistema de log
    /// </summary>
    public static Log Instance
    {
        get
        {
            lock (padlock)
            {
                if (instance == null)
                    instance =new Log();

                return instance;
            }
        }
    }
    /// <summary>
    /// Directorio temporal del fichero de log
    /// </summary>
    public string DirectorioTemporal
    {
        get { return _directorioTemporal; }
        set { _directorioTemporal = value; }
    }
    /// <summary>
    /// Archivo donde se almacenará el log
    /// </summary>
    public string Archivo
    {
        get { return _archivoLog; }
        set { _archivoLog = value; }
    }
    /// <summary>
    /// Indica si está activo o no el sistema de log
    /// </summary>
    public bool Activo
    {
        get { return _enable; }
        set { _enable = value; }
    }
    private Log()
    {
    }
    /// <summary>
    /// Inserta un mensaje en el sistema de log
    /// </summary>
    /// <param name="mensaje"></param>
    public void Insertar(string mensaje)
    {
        if (_enable)
        {
            sb.Append(DateTime.Now.ToShortDateString());
            sb.Append(" ");
            sb.Append(DateTime.Now.ToShortTimeString());
            sb.Append(" : ");
            sb.Append(mensaje);
            sb.Append(" \r\n");
        }
    }
    /// <summary>
    /// Escribe el fichero de log en el arcchivo especificado
    /// </summary>
    public void escribirFichero()
    {
        if (_enable)
        {
            try
            {
                if (System.IO.Directory.Exists(_directorioTemporal))
                {
                    string ficherocompleto = _directorioTemporal + _archivoLog;
                    FileStream fs;
                    // Si no existe el archivo de log lo creamos, en otro caso añadimos informacion sobre el mismo.
                    if (!System.IO.File.Exists(ficherocompleto))
                        fs = new FileStream(ficherocompleto, FileMode.CreateNew, FileAccess.Write);
                    else
                        fs = new FileStream(ficherocompleto, FileMode.Append, FileAccess.Write);

                    StreamWriter m_streamWriter = new StreamWriter(fs);
                    m_streamWriter.BaseStream.Seek(0, SeekOrigin.End);
                    m_streamWriter.WriteLine(sb.ToString());
                    m_streamWriter.Flush();
                    m_streamWriter.Close();
                    sb =new StringBuilder();
                }
            }
            catch (Exception) { }
        }
    }
}