namespace BusquedaYOrdenamientoDemo
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;

            // Tamaño del formulario inicial
            this.ClientSize = new System.Drawing.Size(900, 500);
            this.Text = "Comparación de Algoritmos de Ordenamiento";

            // NOTA IMPORTANTE:
            // Los controles no se crean aquí porque el código principal
            // los construye dinámicamente en ConfigurarInterfaz().
            // Lo único que debe existir en este archivo es esta estructura base.
        }

        #endregion
    }
}
