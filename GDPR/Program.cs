using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data;
using System.Windows.Forms;
using System.IO;
using iTextSharp;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.html;
using iTextSharp.text.html.simpleparser;

namespace GDPR
{
    public class Set
    {

        public string name_hr { get; set; }
        public String id { get; set; }
    }

    class Program
    {
       

        static void Main(string[] args)
        {


            MySqlConnection myConnection = new MySqlConnection();
            myConnection.ConnectionString = "server=localhost;user id=edi;password=password;database=newdb;allowuservariables=True";
            myConnection.Open();
            //execute queries, etc


            if (myConnection is null) myConnection.Close();
            

            List<Set> asd = new List<Set>();
            using (myConnection)
            {
                String sql = "SELECT * FROM page";
                MySqlCommand cmd = new MySqlCommand(sql, myConnection);
                MySqlDataReader reader = cmd.ExecuteReader();

                //while (reader.Read())
                //{
                //    String x = reader.GetString("name_hr");
                //    if (x.Contains("ordinacija"))
                //    {
                //        Console.WriteLine(reader.GetString("registrant"));
                //        Console.WriteLine(reader.GetString("name_en"));
                //        Console.WriteLine(reader.GetString("keywords"));
                //    }
                //}

                reader.Close();


                Console.Write("Unesite hrvatski naziv stranice (name_hr): ");
                String wrd = Console.ReadLine();

                // ::::::::::::::::::::::::: CSV ::::::::::::::::::::::::::::::::::
                MySqlDataAdapter sda = new MySqlDataAdapter();
                    sda.SelectCommand = cmd;

                    DataTable data = new DataTable();
                    sda.Fill(data);

                    BindingSource aSource = new BindingSource();
                    aSource.DataSource = data;

                    DataGridView dgv = new DataGridView();
                    dgv.DataSource = aSource;

                    sda.Update(data);


                    StringBuilder sb = new StringBuilder();

                    string[] columnNames = data.Columns.Cast<DataColumn>().
                                                      Select(column => column.ColumnName).
                                                      ToArray();
                    sb.AppendLine(string.Join(",", columnNames));

                    String[] y;


                    foreach (DataRow row in data.Rows)
                    {
                        y = row.ItemArray.Select(field => field.ToString()).ToArray();

                        if (y[2].Contains(wrd))
                        {
                            string[] fields = row.ItemArray.Select(field => field.ToString()).
                                                       ToArray();
                            sb.AppendLine(string.Join(",", fields));
                        }
                    }

                    
                    File.WriteAllText("test.csv", sb.ToString());
                    Console.WriteLine("CSV stvoren.");

                // ::::::::::::::::::::::::::::::::: PDF :::::::::::::::::::::::::::::::::::::::::::


                System.IO.FileStream fs = new FileStream("gdpr.pdf", FileMode.Create);

                Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                // Create an instance to the PDF file by creating an instance of the PDF 
                // Writer class using the document and the filestrem in the constructor.
                PdfWriter writer = PdfWriter.GetInstance(document, fs);
                document.Open();

                MySqlDataAdapter sda1 = new MySqlDataAdapter();
                sda1.SelectCommand = cmd;
                
                DataTable data1 = new DataTable();
                sda1.Fill(data1);

                BindingSource aSource1 = new BindingSource();
                aSource1.DataSource = data1;

                DataGridView dgv1 = new DataGridView();
                dgv1.DataSource = aSource1;

                sda1.Update(data1);
                StringBuilder sb1 = new StringBuilder();

                string[] columnNames1 = data1.Columns.Cast<DataColumn>().
                                                  Select(column => column.ColumnName).
                                                  ToArray();
                sb1.AppendLine(string.Join(",", columnNames));

                document.Add(new Paragraph(sb1.ToString()));
                    

               

                foreach (DataRow row in data1.Rows)
                {
                    
                    y = row.ItemArray.Select(field => field.ToString()).ToArray();
                                                    
                    if (y[2].Contains(wrd))
                    {
                        

                        sb1.AppendLine(string.Join(",", y));
                        String end = String.Join(";", y);
                        document.Add(new Paragraph(end));
                    }
                    
                }

                Console.WriteLine("PDF stvoren");
                document.Close();
                writer.Close();
                fs.Close();

                //DataTable dt = new DataTable();
                //sda.Fill(dt);

                ////Create a dummy GridView
                //GridView GridView1 = new GridView();
                //GridView1.AllowPaging = false;
                //GridView1.DataSource = dt;
                //GridView1.DataBind();

                
                //myC.ContentType = "application/pdf";
                //Response.AddHeader("content-disposition",
                //    "attachment;filename=DataTable.pdf");
                //Response.Cache.SetCacheability(HttpCacheability.NoCache);
                //StringWriter sw = new StringWriter();
                //GridView1.RenderControl(hw);
                //StringReader sr = new StringReader(sw.ToString());
                //Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 10f, 0f);
                //HTMLWorker htmlparser = new HTMLWorker(pdfDoc);
                //pdfDoc.Open();
                //htmlparser.Parse(sr);
                //pdfDoc.Close();
                //Response.Write(pdfDoc);
                //Response.End();





                //Console.WriteLine(reader.GetString("name_hr"));


                Console.ReadKey();
                myConnection.Close();
                
            }

            
        }
    }
 }
