using System; // Tipos base del CLR (cadena, arreglos, atributos, etc.)
using System.Windows.Forms; // API de Windows Forms para construir la interfaz gráfica

namespace BusquedaYOrdenamientoDemo
{
    /// <summary>
    /// Punto de entrada de la aplicación Windows Forms.
    /// Configura el entorno visual y lanza el formulario principal.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Atributo requerido por Windows Forms para inicializar el subsistema COM
        /// del hilo principal en modo Single-Threaded Apartment (STA). Necesario
        /// para funcionalidades de UI y controles que interactúan con COM.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Habilita estilos visuales modernos (temas) para los controles.
            Application.EnableVisualStyles();
            // Asegura que el renderizado de texto use GDI+ (por compatibilidad con muchos controles de WinForms).
            Application.SetCompatibleTextRenderingDefault(false);
            // Crea y muestra el formulario principal de la aplicación.
            Application.Run(new Form1());
        }
    }
}
