namespace MensajeroC
{
    partial class Bot_MensajeroC
    {
        /// <summary> 
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de componentes

        /// <summary> 
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.eventos_MensajeroC = new System.Diagnostics.EventLog();
            ((System.ComponentModel.ISupportInitialize)(this.eventos_MensajeroC)).BeginInit();
            // 
            // eventos_MensajeroC
            // 
            this.eventos_MensajeroC.Log = "Application";
            // 
            // Bot_MensajeroC
            // 
            this.ServiceName = "Bot_MensajeroC";
            ((System.ComponentModel.ISupportInitialize)(this.eventos_MensajeroC)).EndInit();

        }

        #endregion

        private System.Diagnostics.EventLog eventos_MensajeroC;
    }
}
