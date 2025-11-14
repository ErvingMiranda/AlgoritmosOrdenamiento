using System;
using System.Drawing;
using System.Windows.Forms;

namespace BusquedaYOrdenamientoDemo
{
 public class HelpForm : Form
 {
 private Panel panelAsignacion;
 private Panel panelGlosario;

 public HelpForm()
 {
 Text = "Ayuda - Algoritmos de Ordenamiento";
 StartPosition = FormStartPosition.CenterParent;
 Size = new Size(720,500);
 MinimizeBox = false;
 MaximizeBox = false;
 BackColor = Color.White;

 var panelBotones = new FlowLayoutPanel
 {
 Dock = DockStyle.Top,
 Height =40,
 FlowDirection = FlowDirection.LeftToRight,
 Padding = new Padding(8)
 };
 Controls.Add(panelBotones);

 var btnAsignacion = new Button { Text = "Uso y Asignación", AutoSize = true, FlatStyle = FlatStyle.System };
 var btnGlosario = new Button { Text = "Glosario", AutoSize = true, FlatStyle = FlatStyle.System };
 var toolTip = new ToolTip();
 toolTip.SetToolTip(btnAsignacion, "Explica cómo y cuándo usar cada modo de comparación.");
 toolTip.SetToolTip(btnGlosario, "Describe cada algoritmo y sus casos de uso recomendados.");
 panelBotones.Controls.AddRange(new Control[] { btnAsignacion, btnGlosario });

 // Panel de Asignación y Uso
 panelAsignacion = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
 var lblUso = new Label
 {
 Text = "Uso de los Modos de Comparación",
 Dock = DockStyle.Top,
 Height =24,
 Font = new Font("Segoe UI",11, FontStyle.Bold)
 };
 var txtUso = new RichTextBox
 {
 Dock = DockStyle.Fill,
 ReadOnly = true,
 BorderStyle = BorderStyle.None,
 BackColor = Color.White,
 DetectUrls = false,
 Font = new Font("Segoe UI",10),
 ScrollBars = RichTextBoxScrollBars.Vertical,
 Text =
 "Al pulsar 'Comparar Algoritmos', la aplicación te preguntará cómo proceder:" +
 "\r\n\r\n• Visualizar Carrera (Sí): Inicia una animación en tiempo real donde puedes ver cómo cada algoritmo ordena el mismo conjunto de datos en paralelo. " +
 "Es ideal para entender visualmente las diferencias en sus estrategias. Esta opción solo está disponible para conjuntos de hasta500 elementos para garantizar una animación fluida." +
 "\r\n\r\n• Resultados Rápidos (No): Ejecuta los tres algoritmos sin animación y muestra instantáneamente una tabla con los tiempos de ejecución. Es la única opción para conjuntos grandes (>500 elementos) y es útil cuando solo te interesa el rendimiento puro."
 };
 panelAsignacion.Controls.Add(txtUso);
 panelAsignacion.Controls.Add(lblUso);
 Controls.Add(panelAsignacion);

 // Panel de Glosario
 panelGlosario = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10), Visible = false };
 var lblGlosario = new Label
 {
 Text = "Glosario y Recomendaciones",
 Dock = DockStyle.Top,
 Height =24,
 Font = new Font("Segoe UI",11, FontStyle.Bold)
 };
 var txtGlosario = new RichTextBox
 {
 Dock = DockStyle.Fill,
 ReadOnly = true,
 BorderStyle = BorderStyle.None,
 BackColor = Color.White,
 DetectUrls = false,
 Font = new Font("Segoe UI",10),
 ScrollBars = RichTextBoxScrollBars.Vertical,
 Text =
 "• Burbuja (O(n²)): El más simple. Compara pares adyacentes e intercambia si están en orden incorrecto.\r\n" +
 " Recomendado: Solo para fines educativos o con muy pocos datos.\r\n\r\n" +
 "• Inserción (O(n²)): Construye el arreglo ordenado insertando cada elemento en su posición correcta.\r\n" +
 " Recomendado: Cuando los datos ya están 'casi' ordenados o el conjunto es pequeño.\r\n\r\n" +
 "• QuickSort (O(n log n) promedio): Divide y vencerás con pivote. Muy rápido en promedio.\r\n" +
 " Recomendado: Para la mayoría de los casos con conjuntos de datos medianos a grandes."
 };
 panelGlosario.Controls.Add(txtGlosario);
 panelGlosario.Controls.Add(lblGlosario);
 Controls.Add(panelGlosario);

 btnAsignacion.Click += (s, e) => { panelAsignacion.Visible = true; panelGlosario.Visible = false; };
 btnGlosario.Click += (s, e) => { panelAsignacion.Visible = false; panelGlosario.Visible = true; };

 var btnCerrar = new Button { Text = "Cerrar", Dock = DockStyle.Bottom, Height =34, DialogResult = DialogResult.OK };
 btnCerrar.Click += (s, e) => Close();
 Controls.Add(btnCerrar);
 }
 }
}
