using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DiagAOI.Runcard;
using Microsoft.Win32.SafeHandles;


namespace DiagAOI
{
    class RuncardAPI
    {

        runcard_wsdlPortTypeClient cliente = new runcard_wsdlPortTypeClient("runcard_wsdlPort");     

        // Funcion para validar serial
        public int validarSerial(string serial)
        {
            string msg;
            int error;

            var status = cliente.getUnitStatus(serial, out error, out msg);

            if (error == 0)
            {
                
                if (status.opcode == "T193" && status.status == "ON HOLD")
                {
                    // Serial validado
                    return 0;
                }
                else
                {
                    // No esta en AOI fuera de flujo
                    return 2;
                }
            }

            else
            {
                // Error con serial en runcard
                return 1;
            }

        }


        // Lista de defectos

        public Dictionary<string,string> ObtenerListaDefectos()
        {
            Dictionary<string, string> diccionario = new Dictionary<string,string>();
           
                int error;
                string msg = string.Empty;
                string Opcode = "T193";

                var listaDefectos = cliente.fetchDefectCodeList(Opcode, out error, out msg);

            
            if (error == 0)
            {

                for (int i = 0; i < listaDefectos.Length; i++)
                {
                   // MessageBox.Show(listaDefectos[i].defect_code);

                    diccionario.Add(listaDefectos[i].defect_code, listaDefectos[i].description);
                }

                return diccionario;

            }
            else
            {
              //  MessageBox.Show("No se detectaron codigos de defecto");
                return diccionario;
            }

        }


        // Funcion para dar pase o mandar a SCRAP

        public string transaccion(string serial, string status, string usuraio, string comentario)
        {

            // response
            int error;
            string msg;

            // variable de warehose
            string warehosebin = string.Empty;
            string warehoseloc = string.Empty;

            // Codigo de defecto
            string codigoDefecto = string.Empty;

            // Turno
            string turno = string.Empty;


            if (comentario != string.Empty)
            {

                codigoDefecto = comentario.Substring(0, 7);

            }


          //  MessageBox.Show(codigoDefecto);


            // Unitstatus

            var statusSerial = cliente.getUnitStatus(serial,out error, out msg);

           // MessageBox.Show(error.ToString());

            if (error == 0 && statusSerial.opcode == "T193")
            {


                // Validamos el estatus para realizar pase o scrap

                if (status == "MOVE" || status == "RELEASE")
                {

                    warehosebin = statusSerial.warehousebin;
                    warehoseloc = statusSerial.warehouseloc;

                    turno = validarTurno();

                    comentario = "Validado por calidad " + turno;

                }

                else
                {
                    warehoseloc = "SCRAP";
                    warehosebin = "SCRAP";

                }

                // Publicamos transactionitem

                transactionItem request = new transactionItem()
                {


                    username = usuraio,
                    transaction = status,
                    workorder = statusSerial.workorder,
                    serial = statusSerial.serial,
                    trans_qty = 1,
                    seqnum = statusSerial.seqnum,
                    opcode = statusSerial.opcode,
                    warehousebin = warehosebin,
                    warehouseloc = warehoseloc,
                    machine_id = "AOI_S1",
                    comment = comentario,
                    defect_code = codigoDefecto
                    


                };

                // data item
                dataItem[] inputData = new dataItem[] { };

                // bomItem
                bomItem[] bomData = new bomItem[] { };


                var transac = cliente.transactUnit(request,inputData,bomData, out msg);

                Console.WriteLine(msg);

              //  MessageBox.Show(msg);

                return msg;



            }
            else
            {
               // MessageBox.Show("Error con serial");
                return "Error con serial";
            }
        }


        public string validarTurno()
        {

            // hora actual
            DateTime hora = DateTime.Now;
            TimeSpan horaActual = hora.TimeOfDay;

            // Turnos Molex

            // Turno 1
            TimeSpan horaInicioT1 = new TimeSpan(6,30,0);
            TimeSpan horaFinT1 = new TimeSpan(14,29,59);

            // Turno 2
            TimeSpan horaInicioT2 = new TimeSpan(14, 30, 0);
            TimeSpan horaFinT2 = new TimeSpan(21, 59, 59);

            // Turno 3
            TimeSpan horaInicioT3 = new TimeSpan(22, 0, 0);
            TimeSpan horaFinT3 = new TimeSpan(6, 29, 59);


            if (horaActual >= horaInicioT1 && horaActual <= horaFinT1 )
            {
              Console.WriteLine("Turno 1 " + horaActual);
                return "Turno 1";
            }

            else if (horaActual >= horaInicioT2 && horaActual <= horaFinT2)
            {
                Console.WriteLine("Turno 2 " + horaActual);
                return "Turno 2";

            }

            else if (horaActual >= horaInicioT3 || horaActual <= horaFinT3)
            {

                Console.WriteLine("Turno 3 " + horaActual);
                return "Turno 3";

            }
            else
            {

                Console.WriteLine("Nada " + horaActual);

                return "";

            }

        }


    }
}
