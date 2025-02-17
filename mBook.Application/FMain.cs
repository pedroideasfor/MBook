﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using MBook.Books;
using EyeXFramework;
using System.Diagnostics;
using System.IO.Ports;
using System.Drawing;
using MBook.Domain.Entities;
using MBook.Domain.ValueObjects;
using MBook.Infrastructure.Context;

namespace MBook
{
    public partial class FMain : Form
    {
        int m_iBookId = 0;

        FInterfaceSerial o_fInterfaceSerial;
        SerialPort oSerialPort;

        public FMain()
        {
            InitializeComponent();

            Program.EyeXHost.Connect(behaviorMap1);
            behaviorMap1.Add(circularButton1, new EyeXFramework.GazeAwareBehavior(OnGazeCircularButton) { DelayMilliseconds = 1000 });
                        
            string sImage = "";
            foreach (Book oBook in XMLContext.Instance.Books.Values)
            {
                foreach (Control c in GetControls(splitContainer2.Panel1).Where(x => x is PictureBox))
                {
                    if (c.Name == ("pictureBox" + oBook.Id.ToString()))
                    {
                        sImage = GenDef.BooksDir + oBook.NameId.ToString() + "\\cover.png";
                        ((PictureBox)c).Image = new Bitmap(sImage);
                        ((PictureBox)c).Image.Tag = oBook.Name;
                        behaviorMap1.Add(c, new EyeXFramework.GazeAwareBehavior(OnGaze));
                    }
                }
            }
        }

        public IEnumerable<Control> GetControls(Control c)
        {
            return new[] { c }.Concat(c.Controls.OfType<Control>()
                                              .SelectMany(x => GetControls(x)));
        }

        private void pictureBox_Click(object sender, EventArgs e)
        {
            var ctr = sender as PictureBox;
            if (ctr != null)
            {
               
                ctr.Select();

                richTextBox1.Clear();
                richTextBox1.AppendText("\n");
                richTextBox1.Text = "Título: " + ctr.Image.Tag.ToString();
                Book oBook = Program.bookRepository.GetBook(ctr.Image.Tag.ToString());
                richTextBox1.AppendText("\n");
                richTextBox1.AppendText("ISBN: " + oBook.ISBN);
                richTextBox1.AppendText("\n");
                richTextBox1.AppendText("Autor: " + oBook.Author);
                richTextBox1.AppendText("\n");
                richTextBox1.AppendText("Editora: " + oBook.Editora);

                richTextBox2.Clear();
                richTextBox1.AppendText("\n");
                richTextBox2.AppendText("Descrição: " + oBook.Description);

                m_iBookId = oBook.Id;
                circularButton1.Location = new System.Drawing.Point(ctr.Location.X + ctr.Width - 30, ctr.Location.Y + ctr.Height - 30);
                
            }
        }

        private void pictureBox_DoubleClick(object sender, EventArgs e)
        {
            var pb = sender as PictureBox;                        
            Book oBook = Program.bookRepository.GetBook(pb.Image.Tag.ToString());
            //   
            if (oBook.Pages.Count != 0)
            {
                BookForm bookForm = new BookForm(oBook, null);
                bookForm.Show();
            }
            else
            {
                ChapterForm chapterForm = new ChapterForm(oBook, this);
                chapterForm.Show();
            }
        }

        private void circularButton1_Click(object sender, EventArgs e)
        {
            Book oBook = Program.bookRepository.GetBook(m_iBookId);
            if (oBook.Pages.Count != 0)
            {
                BookForm bookForm = new BookForm(oBook, null);
                bookForm.Show();
            }
            else
            {
                ChapterForm chapterForm = new ChapterForm(oBook, this);
                chapterForm.Show();
            }
        }

        private void OnGaze(object sender, GazeAwareEventArgs e)
        {
            var ctr = sender as PictureBox;
            if (ctr != null)
            {
                if (e.HasGaze)
                {
                    ctr.Select();
                    Book oBook = Program.bookRepository.GetBook(ctr.Image.Tag.ToString());

                    richTextBox1.Clear();
                    richTextBox1.AppendText("\n");
                    richTextBox1.Text = "Título: " + ctr.Image.Tag.ToString();
                    richTextBox1.AppendText("\n");
                    richTextBox1.AppendText("ISBN: " + oBook.ISBN);
                    richTextBox1.AppendText("\n");
                    richTextBox1.AppendText("Autor: " + oBook.Author);
                    richTextBox1.AppendText("\n");
                    richTextBox1.AppendText("Editora: " + oBook.Editora);

                    richTextBox2.Clear();
                    richTextBox1.AppendText("\n");
                    richTextBox2.AppendText("Descrição: " + oBook.Description);

                    m_iBookId = oBook.Id;
                    circularButton1.Location = new Point(ctr.Location.X+ctr.Width-30, ctr.Location.Y+ctr.Height-30);
                }
            }
        }

        private void OnGazeCircularButton(object sender, GazeAwareEventArgs e)
        {
            if (e.HasGaze)
            {
                Book oBook = Program.bookRepository.GetBook(m_iBookId);
                if (oBook.Pages.Count != 0)
                {
                    BookForm bookForm = new BookForm(oBook, null);
                    bookForm.Show();
                }
                else
                {
                    ChapterForm chapterForm = new ChapterForm(oBook, this);
                    chapterForm.Show();
                }
            }
        }

        private void questionárioDeUsabilidadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProcessStartInfo sInfo = new ProcessStartInfo("https://forms.gle/7J1QbS8pEaPWbDGh8");
            Process.Start(sInfo);
            
        }

        private void calibracaoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Program.EyeXHost.LaunchRecalibration();
            var eyeXHostStatus = Program.EyeXHost.ConfigurationStatus;                        
        }

        private void configurarLampadasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FConfigBridge oConfigBridge = new FConfigBridge();
            oConfigBridge.ShowDialog();

            if (oConfigBridge.IP != "")
            {
                string sIP = oConfigBridge.IP;

                Services.HueLogicService.FindBridgeIP(sIP);
                MessageBox.Show("Aperte o botão da Philips Bridge.");
                Services.HueLogicService.ConnectBridge("Philips hue");
                Services.HueLogicService.GetBridge();
            }
        }

        private void configurarPortaSerialToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (o_fInterfaceSerial == null)
            {
                o_fInterfaceSerial = new FInterfaceSerial();
                o_fInterfaceSerial.ShowDialog();

                oSerialPort = new SerialPort();
                oSerialPort = o_fInterfaceSerial.InterfaceSerialPort;
                SerialContext.SetContext(o_fInterfaceSerial.InterfaceSerialPort);
            }
            else
            {
                o_fInterfaceSerial.ShowDialog();
                oSerialPort = o_fInterfaceSerial.InterfaceSerialPort;
            }
        }

        private void FMain_Load(object sender, EventArgs e)
        {
            pictureBox1.Select();
            Book oBook = Program.bookRepository.GetBook(pictureBox1.Image.Tag.ToString());

            richTextBox1.Clear();
            richTextBox1.AppendText("\n");
            richTextBox1.Text = "Título: " + pictureBox1.Image.Tag.ToString();
            richTextBox1.AppendText("\n");
            richTextBox1.AppendText("ISBN: " + oBook.ISBN);
            richTextBox1.AppendText("\n");
            richTextBox1.AppendText("Autor: " + oBook.Author);
            richTextBox1.AppendText("\n");
            richTextBox1.AppendText("Editora: " + oBook.Editora);

            richTextBox2.Clear();
            richTextBox1.AppendText("\n");
            richTextBox2.AppendText("Descrição: " + oBook.Description);

            m_iBookId = oBook.Id;
            circularButton1.Location = new System.Drawing.Point(pictureBox1.Location.X + pictureBox1.Width - 30, pictureBox1.Location.Y + pictureBox1.Height - 30);
        }

        private void FMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            Services.HueLogicService.PutBridge(1, true, 0, 0, 50);
            Services.HueLogicService.PutBridge(2, true, 0, 0, 50);

            if (oSerialPort != null)
            {
                SerialContext.Instance.SerialPort.Write("OUT00");
                SerialContext.Instance.SerialPort.Write("OUT10");
                oSerialPort.Close();
            }
        }

        private void sobreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProcessStartInfo sInfo = new ProcessStartInfo("https://eic.cefet-rj.br/~gpmm/portfolio/leitura-multissensorial/");
            Process.Start(sInfo);
        }
        
        private void circularButton1_MouseEnter(object sender, EventArgs e)
        {
            Book oBook = Program.bookRepository.GetBook(m_iBookId);

            if (oBook.Pages.Count != 0)
            {
                BookForm bookForm = new BookForm(oBook, null);
                bookForm.Show();
            }
            else
            {
                ChapterForm chapterForm = new ChapterForm(oBook, this);
                chapterForm.Show();
            }
        }

        private void sobreToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ProcessStartInfo sInfo = new ProcessStartInfo("https://eic.cefet-rj.br/~gpmm/portfolio/leitura-multissensorial/");
            Process.Start(sInfo);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Services.HueLogicService.PutBridge(1, true, 0, 0, 50);
            Services.HueLogicService.PutBridge(2, true, 0, 0, 50);

            if (oSerialPort != null)
            {
                SerialContext.Instance.SerialPort.Write("OUT00");
                SerialContext.Instance.SerialPort.Write("OUT10");
                oSerialPort.Close();
            }

            this.Close();
        }        
    }
}
