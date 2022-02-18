﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Timers;
using System.ServiceProcess;
using System.Net;
using System.Net.Mail;

namespace MensajeroC
{
    public partial class Bot_MensajeroC : ServiceBase
    {   
        // mensaje que se graba de exito o error
        string mensajeLog = "";
        // conexion a la base de datos
        public static string serv = ConfigurationManager.AppSettings["serv"].ToString();
        public static string port = ConfigurationManager.AppSettings["port"].ToString();
        public static string usua = ConfigurationManager.AppSettings["user"].ToString();
        public static string cont = ConfigurationManager.AppSettings["pass"].ToString();
        public static string data = ConfigurationManager.AppSettings["data"].ToString();
        public static string ctl = ConfigurationManager.AppSettings["ConnectionLifeTime"].ToString();
        string lapso = ConfigurationManager.AppSettings["lapsoTseg"].ToString();                                // tiempo en segundos
        string coror = ConfigurationManager.AppSettings["corrOrige"].ToString();                                // correo enviador
        string corde = ConfigurationManager.AppSettings["corrDesti"].ToString();                                // correo destino
        string asuco = ConfigurationManager.AppSettings["asuntoCor"].ToString();                                // asunto del correo
        string smtpn = ConfigurationManager.AppSettings["nomSerCor"].ToString();                                // servidor smpt
        string nupto = ConfigurationManager.AppSettings["numPtoSer"].ToString();                                // puerto smpt
        string pasco = ConfigurationManager.AppSettings["passCorre"].ToString();                                // clave del correo enviador
        string DB_CONN_STR = "server=" + serv + ";uid=" + usua + ";pwd=" + cont + ";database=" + data + ";";

        public Bot_MensajeroC()
        {
            InitializeComponent();
            eventos_MensajeroC = new EventLog();
            if (!EventLog.SourceExists("Bot_MensajeroC"))
            {
                EventLog.CreateEventSource("Bot_MensajeroC", "Application");
            }
            eventos_MensajeroC.Source = "Bot_MensajeroC";
            eventos_MensajeroC.Log = "Application";
        }
        protected override void OnStart(string[] args)
        {
            // escribe en el log de eventos del sistema - inicio del mensajero
            eventos_MensajeroC.WriteEntry("Inicio del Bot_MensajeroC");

            Timer timer1 = new Timer();
            timer1.Interval = int.Parse(lapso) * 1000;          // en xml es segundos, en c# es milisegundos, por eso multiplicamos por 1000
            timer1.Enabled = true;
            timer1.Elapsed += timer1_Tick;
            timer1.Start();

        }
        protected override void OnStop()
        {
            // escribe en el log de eventos del sistema - detención del mensajero
            eventos_MensajeroC.WriteEntry("Detención del Bot_MensajeroC");
            //timer1.Enabled = false;
            //timer1.Stop();
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(DB_CONN_STR))
            {
                conn.Open();
                if (conn.State == System.Data.ConnectionState.Open)
                {
                    DataTable dt = new DataTable();
                    // lee registros nuevos
                    if (plan_lector(conn, dt) == true)
                    {
                        // envía correos
                        if (mensajero(conn, dt) == false)
                        {
                            mensajeLog = "No se puede enviar los correos, error en la lectura de datos o en el envío";
                            escribirLineaFichero();
                        }
                    }
                    else
                    {
                        // mensaje de error con la lectura de la tabla
                        mensajeLog = "No se puede leer los datos de la tabla o no existen datos";
                        escribirLineaFichero();
                    }
                }
                else
                {
                    // mensaje de no hay conexión a la b.d.
                    mensajeLog = "No se puede conectar con la base de datos";
                    escribirLineaFichero();
                }
            }
        }
        private bool plan_lector(SqlConnection conn, DataTable dt)
        {
            bool retorna = false;
            string jala = "select * from dbo.TM_TABLA_MULTIPLE where co_item_tabla='0'";    // esto se tiene que cambiar al verdadero
            using (SqlCommand micon = new SqlCommand(jala, conn))
            {
                using (SqlDataAdapter da = new SqlDataAdapter(micon))
                {
                    da.Fill(dt);
                    if (dt.Rows.Count > 0) retorna = true;
                }
            }
            return retorna;
        }
        private bool mensajero(SqlConnection conn, DataTable dt)
        {
            bool retorna = false;
            foreach (DataRow row in dt.Rows)
            {
                string cuerpo = getHtml(row.ItemArray[0].ToString(),    // nombre del fulano
                    row.ItemArray[1].ToString(),        // fecha/hora
                    row.ItemArray[7].ToString(),        // correo destino
                    row.ItemArray[8].ToString());       // otro dato

                if (Email(cuerpo) == true)
                {
                    // marca registro como "correo enviado"
                    if (envia_correo(conn, int.Parse(row.ItemArray[10].ToString())) == true)    // campo id del registro
                    {
                        retorna = true;
                    }
                    else
                    {
                        mensajeLog = "No se puede actualizar el campo de marca de envío";
                        escribirLineaFichero();
                    }
                }
            }
            return retorna;
        }
        // esquema del coreo electrónico - cuerpo del mensaje en HTML
        public static string getHtml(string nomb, string feho, string corr, string otro)
        {
            try
            {
                string messageBody = "<font>Horario de marcación entrada/salida: </font><br><br>";
                if (nomb.Length < 1) return messageBody;
                string htmlTableStart = "<table style=\"border-collapse:collapse; text-align:center;\" >";
                string htmlTableEnd = "</table>";
                string htmlHeaderRowStart = "<tr style=\"background-color:#6FA1D2; color:#ffffff;\">";
                string htmlHeaderRowEnd = "</tr>";
                string htmlTrStart = "<tr style=\"color:#555555;\">";
                string htmlTrEnd = "</tr>";
                string htmlTdStart = "<td style=\" border-color:#5c87b2; border-style:solid; border-width:thin; padding: 5px;\">";
                string htmlTdEnd = "</td>";
                messageBody += htmlTableStart;
                messageBody += htmlHeaderRowStart;
                messageBody += htmlTdStart + "Nombre trabajador" + htmlTdEnd;
                messageBody += htmlTdStart + "Fecha/Hora" + htmlTdEnd;
                messageBody += htmlTdStart + "Email" + htmlTdEnd;
                messageBody += htmlTdStart + "Teléfono" + htmlTdEnd;
                messageBody += htmlHeaderRowEnd;
                //Loop all the rows from grid vew and added to html td  
                {
                    messageBody = messageBody + htmlTrStart;
                    messageBody = messageBody + htmlTdStart + nomb + htmlTdEnd; //adding nombre
                    messageBody = messageBody + htmlTdStart + feho + htmlTdEnd; //adding fecha hora
                    messageBody = messageBody + htmlTdStart + corr + htmlTdEnd; //adding correo
                    messageBody = messageBody + htmlTdStart + otro + htmlTdEnd; //adding celular  
                    messageBody = messageBody + htmlTrEnd;
                }
                messageBody = messageBody + htmlTableEnd;
                return messageBody; // return HTML Table as string from this function  
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        // envío del mensaje
        private bool Email(string htmlString)
        {
            bool retorna = false;
            try
            {
                MailMessage message = new MailMessage();
                SmtpClient smtp = new SmtpClient();
                message.From = new MailAddress(coror);      //  "neto.solorzano@solorsoft.com"
                message.To.Add(new MailAddress(corde));     //  "lucio.solorzano@gmail.com"
                message.Subject = asuco;        // "Prueba del boot mensajero";
                message.IsBodyHtml = true;      //  false;
                message.Body = htmlString;      // "Probando - este es el cuerpo del mensaje";
                smtp.Port = int.Parse(nupto);   // 26;  // 465;    // 587;
                smtp.Host = smtpn;              // "mail.solorsoft.com";               // "smtp.gmail.com";
                smtp.EnableSsl = false;     // true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(coror, pasco);     // "neto.solorzano@solorsoft.com", "190969Sorol"
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

                smtp.Send(message);
                retorna = true;
            }
            catch (Exception) { }
            return retorna;
        }
        // marca el registro indicado como correo enviado
        private bool envia_correo(SqlConnection conn, int nreg)
        {
            bool retorna = false;
            string actua = "update xxx set campo='enviado' where id=@nreg";
            using (SqlCommand micon = new SqlCommand(actua, conn))
            {
                try
                {
                    micon.Parameters.AddWithValue("@nreg", nreg);
                    micon.ExecuteNonQuery();
                    retorna = true;
                }
                catch { 
                
                }
            }
            return retorna;
        }
        //Escribe el mensaje de la propiedad mensajeLog en un fichero en la carpeta del ejecutable
        public void escribirLineaFichero()
        {
            try
            {
                FileStream fs = new FileStream(@AppDomain.CurrentDomain.BaseDirectory +
                    "estado.log", FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter m_streamWriter = new StreamWriter(fs);
                m_streamWriter.BaseStream.Seek(0, SeekOrigin.End);
                //Quitar posibles saltos de línea del mensaje
                mensajeLog = mensajeLog.Replace(Environment.NewLine, " | ");
                mensajeLog = mensajeLog.Replace("\r\n", " | ").Replace("\n", " | ").Replace("\r", " | ");
                m_streamWriter.WriteLine(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + " " + mensajeLog);
                m_streamWriter.Flush();
                m_streamWriter.Close();
            }
            catch
            {
                //Silenciosa
            }
        }
    }
}
