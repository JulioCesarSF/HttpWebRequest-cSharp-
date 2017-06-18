using BoletimFIAP.WebAcess;
using BoletimFIAP.WebAcess.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BoletimFIAP
{
    public partial class Form1 : Form
    {
        Thread thread;
        delegate void ListBoxStatus(string mensagem);

        private volatile bool pararThread = true;

        private volatile int tempo = 30 * 1000;

        private volatile int tentativasAcesso = 0;
        private volatile int verificacaoBoletim = 0;

        public Form1()
        {
            InitializeComponent();
            cbTempo.SelectedIndex = 0;
        }

        private void btnIniciar_Click(object sender, EventArgs e)
        {
            if (btnIniciar.Text.Equals("Iniciar") && pararThread)
            {
                pararThread = false;
                thread = new Thread(Iniciar);
                thread.Start();
                btnIniciar.Text = "Parar";
            }
            else
            {
                pararThread = true;
                btnIniciar.Text = "Iniciar";
                if (thread.IsAlive) thread.Abort();
                AddItemStatus("Parou.");
            }
        }

        private void txtRm_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true;
        }

        private void Iniciar()
        {

            this.Invoke((MethodInvoker)(() => AddItemStatus("Verificando conexão...")));
            if (WebUtils.internet())
            {
                this.Invoke((MethodInvoker)(() => AddItemStatus("Verificando acesso...")));
                while (!WebUtils.site(Web.URL_BASE))
                {
                    tentativasAcesso++;
                    this.Invoke((MethodInvoker)(() => AddItemStatus("Site inacessível/fora do ar [" + tentativasAcesso + "]. Auto-Retry...")));
                }

                this.Invoke((MethodInvoker)(() => AddItemStatus("Iniciando..")));
                if (Web.LoginPost(txtRm.Text, txtSenha.Text))
                {
                    this.Invoke((MethodInvoker)(() => AddItemStatus("Adquirindo tokens.")));
                    while (!Web.PosLoginGET())
                    {
                        this.Invoke((MethodInvoker)(() => AddItemStatus("Autenticação [FAIL]. Restarting..")));
                        this.Invoke((MethodInvoker)(() => SetButton(true)));
                    }

                    this.Invoke((MethodInvoker)(() => AddItemStatus("Autenticação [OK].")));

                    while (!pararThread)
                    {
                        if (!Web.PegarBoletim())
                        {
                            this.Invoke((MethodInvoker)(() => AddItemStatus("Boletim [FAIL][" + verificacaoBoletim + "].")));
                            this.Invoke((MethodInvoker)(() => SetButton(true)));
                        }
                        else
                        {
                            verificacaoBoletim++;
                            this.Invoke((MethodInvoker)(() => AddItemStatus("Boletim [OK][" + verificacaoBoletim + "].")));
                            if (Web.novaNota)
                            {
                                this.Invoke((MethodInvoker)(() => AddItemStatus("Saiu nota nova!")));
                                this.Invoke((MethodInvoker)(() => ShowBallonNota()));
                            }
                        }

                        Thread.Sleep(tempo);
                    }
                }
                else
                {
                    this.Invoke((MethodInvoker)(() => AddItemStatus("RM/Senha incorretos!")));
                    this.Invoke((MethodInvoker)(() => SetButton(false)));
                }
            }
            else
            {
                this.Invoke((MethodInvoker)(() => AddItemStatus("Sem conexão!")));
                this.Invoke((MethodInvoker)(() => SetButton(false)));
            }
        }

        private void AddItemStatus(string mensagem)
        {
            lbStatus.Items.Add(PegarData(mensagem));
            lbStatus.SelectedIndex = lbStatus.Items.Count - 1;
        }

        private string PegarData(string mensagem)
        {
            return "[" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "] - " + mensagem;
        }

        private int PegarTempo()
        {
            if (cbTempo.SelectedIndex == 0) return 30 * 1000;
            else if (cbTempo.SelectedIndex == 1) return 60 * 1000;
            else if (cbTempo.SelectedIndex == 2) return 120 * 1000;
            else if (cbTempo.SelectedIndex == 3) return 5 * 60 * 1000;
            else if (cbTempo.SelectedIndex == 4) return 10 * 60 * 1000;
            else if (cbTempo.SelectedIndex == 5) return 15 * 60 * 1000;
            else return 30 * 1000;
        }

        private void cbTempo_SelectedIndexChanged(object sender, EventArgs e)
        {
            tempo = PegarTempo();
        }

        private void SetButton(bool reset)
        {            
            btnIniciar.PerformClick();
            if (reset) btnIniciar.PerformClick();
        }

        private void mostrarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Show();
        }

        private void sairToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetButton(false);
            Application.Exit();
        }

        private void Form1_Move(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                notifyIcon1.ShowBalloonTip(1000, "Aplicação", "Estou minimizado.", ToolTipIcon.Info);
            }

        }

        private void ShowBallonNota()
        {
            notifyIcon1.ShowBalloonTip(1000, "Saiu uma nota", "Vá conferir.", ToolTipIcon.Info);
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.BringToFront();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(!pararThread)
                SetButton(false);
        }
    }
}
