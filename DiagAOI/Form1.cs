using re_enter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Deployment.Application;


namespace DiagAOI
{
    public partial class Form1: Form
    {
        // DWO-0195

        string serialRecortado = string.Empty;

        RuncardAPI runcardAPI = new RuncardAPI();

        // color mensaje
        bool color;

        // Usuario
        string usuario = string.Empty;


        public Form1()
        {
            InitializeComponent();

            if (ApplicationDeployment.IsNetworkDeployed)
            {
                //Get App Version
                Version ver = ApplicationDeployment.CurrentDeployment.CurrentVersion;
                label6.Text = ver.Major + "." + ver.Minor + "." + ver.Build + "." + ver.Revision;
            }

        }


        private void txtSerial_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.Enter)
            {

                string serial = txtSerial.Text.Trim();

                //[)>␞06␝Y7410300000000K␝P85022022␝12V811424803␝T3D25161KL0000011␞␄

                serialRecortado = convertirSerial(serial);

                //Console.WriteLine("Recortado: " + serialRecortado);

                if (serialRecortado != string.Empty)
                {
                    // Funcion para validar serial y su flujo
                    int validacionSerial = runcardAPI.validarSerial(serialRecortado);

                    switch (validacionSerial)
                    {
                        case 0:
                            cbxStatus.Enabled = true;
                            txtSerial.Enabled = false;
                            btnLimpiar.Enabled = true;
                            break;

                        case 1:
                            mostrarMensaje("Error con serial en runcard.",color = false);

                            break;

                        case 2:
                            mostrarMensaje("La unidad no esta en HOLD o esta en otra estacion.",color = false);
                            break;

                        default:
                            mostrarMensaje("No devolvio ningun valor.",color = false);

                            break;

                    }

                }
                else
                {
                    mostrarMensaje("Campo vacio.",color = false);
                }


            }
        }

        // Convertir el serial

        public string convertirSerial(string serial)
        {


            string expresion = @"T.{16}";

            Match match = Regex.Match(serial,expresion);

            if (match.Success)
            {
                Console.WriteLine(match.Value);
                return match.ToString();

            }
            else
            {
                Console.WriteLine("No se encontró la coincidencia.");
                return match.ToString();
            }



        }

        private void Form1_Load(object sender, EventArgs e)
        {
            txtSerial.Focus();

            usuario = Sesion.UsuarioActual;

            // Mostramos el usuario
            label5.Text = usuario;

            Console.WriteLine("Usuario: " + usuario);

            // Llenar combobox de status

            string[] status = { "MOVE", "SCRAP"};
            cbxStatus.Items.AddRange(status);

            // Llenar defectos

            Dictionary<string,string> listaDefectos = new Dictionary<string,string>();

            listaDefectos = runcardAPI.ObtenerListaDefectos();

            // Validamos que contenga algo la lista
            if (listaDefectos != null)
            {

                foreach (var item in listaDefectos)
                {
                    cbxDefecto.Items.Add(item.Key + " " + item.Value);
                }

            }
            else
            {
                mostrarMensaje("No se cargaron los defectos para la estación de trabajo.",color = false);
            }
            
        }


        private void cbxStatus_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (cbxStatus.Text == "SCRAP")
            {

                cbxDefecto.Enabled = true;
                btnAvanzar.Enabled = false;

                label3.Visible = true;
                cbxDefecto.Visible = true;


            }
            else
            {
                btnAvanzar.Enabled = true;
                cbxDefecto.Enabled = false;
                cbxDefecto.SelectedIndex = -1;

                label3.Visible = false;
                cbxDefecto.Visible = false;
            }


        }

        private void cbxDefecto_SelectedIndexChanged(object sender, EventArgs e)
        {


            btnAvanzar.Enabled = true;

        }

        private void btnAvanzar_Click(object sender, EventArgs e)
        {

            // Si es MOVE primero se quita el HOLD
            if (cbxStatus.Text == "MOVE")
            {

                runcardAPI.transaccion(serialRecortado, "RELEASE", usuario, cbxDefecto.Text);

            }

            // Se realiza transaccion
          string resultado =  runcardAPI.transaccion(serialRecortado, cbxStatus.Text, usuario, cbxDefecto.Text);

            if (resultado.Contains("ADVANCE successfully"))
            {

                mostrarMensaje("Unidad liberada, se manda a siguiente estación.", color = true);

            }
            else
            {
                mostrarMensaje("Se manda unidad a SCRAP.", color = false);

            }



            // Limpiar controles
            cbxDefecto.SelectedIndex = -1;
            cbxDefecto.Enabled = false;

            cbxStatus.SelectedIndex = -1;
            cbxStatus.Enabled = false;

            txtSerial.Enabled = false;

            btnLimpiar.Enabled = false;
            btnAvanzar.Enabled = false;

            txtSerial.Text = string.Empty;
            txtSerial.Enabled = true;

            label3.Visible = false;
            cbxDefecto.Visible = false;
            

        }


        // Mostrar Mensaje
        public void mostrarMensaje(string mensaje, bool status)
        {

            // Verificar si ya existe una ventana dentro del panel
            foreach (Control control in panel1.Controls)
            {
                if (control is Form)
                {
                    // Si hay una ventana, la eliminamos
                    control.Dispose();
                }
            }
            Mensaje fmensaje = new Mensaje(mensaje);

            if (status)
            {

                fmensaje.BackColor = System.Drawing.Color.FromArgb(0, 127, 22); // Color verde para éxito

            }
            else
            {

                fmensaje.BackColor = System.Drawing.Color.FromArgb(196, 24, 1); // Color rojo para error

            }


            fmensaje.TopLevel = false;
            fmensaje.Parent = panel1;
            fmensaje.Size = panel1.ClientSize;
            fmensaje.Show();

            // Esperamos 5 segundos para limpiar el mensaje
            // await Task.Delay(14000);

            txtSerial.Focus();
            LimpiarMensaje();

        }


        public async void LimpiarMensaje()
        {

            // Esperamos 5 segundos para limpiar el mensaje
            await Task.Delay(6000);

            // Verificar si ya existe una ventana dentro del panel
            foreach (Control control in panel1.Controls)
            {
                if (control is Form)
                {
                    // Si hay una ventana, la eliminamos
                    control.Dispose();
                }
            }


            Mensaje fmensaje1 = new Mensaje("");

            fmensaje1.BackColor = System.Drawing.Color.FromArgb(229, 225, 225);

            fmensaje1.TopLevel = false;
            fmensaje1.Parent = panel1;
            fmensaje1.Size = panel1.ClientSize;
            fmensaje1.Show();

            txtSerial.Enabled = true;

            txtSerial.Focus();


        }

        public void Limpiar()
        {

            txtSerial.Text = string.Empty;
            txtSerial.Enabled = true;

            // Limpia cbxstatus
            cbxStatus.Enabled = false;
            cbxStatus.SelectedIndex = -1;
            // Limpia cbxdefecto
            cbxDefecto.Enabled = false;
            cbxDefecto.SelectedIndex = -1;
            // Deshabilitar boton de avanzar
            btnAvanzar.Enabled = false;
            // Limpia el serial y deshabilita

            cbxDefecto.Visible = false;
            label3.Visible = false;

        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {

            Limpiar();

        }

        

        private void timer1_Tick(object sender, EventArgs e)
        {

            // cada 15 min se cierra sesion
            timer1.Enabled = false;
            MessageBox.Show("Sesión expirada!");
            Salir();
            

        }


        private void btnSalir_Click(object sender, EventArgs e)
        {

            Salir();

        }

        public void Salir()
        {
            Limpiar();

            this.Hide(); // Oculta el formulario principal

            Login login = new Login();
            login.ShowDialog();
            //var result = login.ShowDialog();

            if (login.DialogResult == DialogResult.OK)
            {
                this.Show();

                label5.Text = Sesion.UsuarioActual;
                usuario = Sesion.UsuarioActual;
                timer1.Enabled = true;
                txtSerial.Focus();

            }
            else
            {
                Application.Exit();
            }
        }
    }
}
