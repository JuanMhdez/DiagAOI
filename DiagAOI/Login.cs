using MySqlConnector;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DiagAOI
{
    public partial class Login: Form
    {
        public Login()
        {
            InitializeComponent();
        }

        public string usernane = string.Empty;

        private void btnTest_Click(object sender, EventArgs e)
        {

            Ingresar();
            
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.Enter)
            {

                Ingresar();

            }
        }


        public void Ingresar()
        {
            if (txtUsuario.Text != string.Empty && txtPassword.Text != string.Empty)
            {


                try
                {
                    using (MySqlConnection conexion = new MySqlConnection($"server=10.39.2.91;port=3306;user id={txtUsuario.Text};password={txtPassword.Text};database=runcard;"))
                    {
                        conexion.Open();
                        Console.WriteLine("Conexion exitosa!!");

                        Sesion.UsuarioActual = txtUsuario.Text.Trim();
                        this.DialogResult = DialogResult.OK;
                        this.Close();

                    }

                }
                catch (Exception ex)
                {

                    MessageBox.Show("No se pudo iniciar sesión, usuario invalido o error en la conexión en la base de datos: ", "Error", MessageBoxButtons.OK);
                }

            }
            else
            {
                MessageBox.Show("No se llenaron todos los campos!");
            }
        }
    }

    }

