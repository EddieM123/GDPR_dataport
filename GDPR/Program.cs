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
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace GDPR
{
    class Program
    {
        private static object console;

        static void Main(string[] args)
        {
            MySqlConnection myConnection = new MySqlConnection();
            myConnection.ConnectionString = "server=localhost;user id=root;password=password;database=gdpr;allowuservariables=True";
            myConnection.Open();


            if (myConnection is null) myConnection.Close();

            using (myConnection)
            {
                string sql = "select * from page";
                MySqlCommand cmd = new MySqlCommand(sql, myConnection);
                MySqlDataReader reader = cmd.ExecuteReader();

                //while (reader.read())
                //{
                //    string x = reader.getstring("name_hr");
                //    if (x.contains("ministarstvo"))
                //    {
                //        console.writeline(reader.getstring("registrant"));
                //        console.writeline(reader.getstring("name_en"));
                //        console.writeline(reader.getstring("keywords"));
                //    }
                //}

                reader.Close();



                // :::::::::::::::::::: DB Backup :::::::::::::::::::::::::

                //string file = "C:/Users/Edi/Desktop/backup.sql";
                //using (MySqlCommand cmd2 = new MySqlCommand())
                //{
                //    using (MySqlBackup mb = new MySqlBackup(cmd2))
                //    {
                //        cmd2.Connection = myConnection;
                //        mb.ExportToFile(file);
                //        myConnection.Close();
                //    }
                //}











                // SqlDataReader dr = sqlCommand1.ExecuteReader(CommandBehavior.KeyInfo);

                Console.Write("Unesite hrvatski naziv stranice (name_hr), ili ključnu riječ: ");
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

                string[] columnNames = data.Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToArray();
                sb.AppendLine(string.Join(",", columnNames));

                String[] y;

                foreach (DataRow row in data.Rows)
                {
                    y = row.ItemArray.Select(field => field.ToString()).ToArray();
                    if (y[2].ToLower().Contains(wrd.ToLower()))
                    {
                        Console.WriteLine("Pronađena stranica: " + y[2] + "     - " + y[1]);
                        string[] fields = row.ItemArray.Select(field => field.ToString()).ToArray();
                        sb.AppendLine(string.Join(",", fields));
                    }
                }


                File.WriteAllText("C:/Users/Edi/Desktop/gdpr/gdpr.csv", sb.ToString());
                Console.WriteLine("CSV stvoren.");

                // ::::::::::::::::::::::::::::::::: PDF :::::::::::::::::::::::::::::::::::::::::::


                System.IO.FileStream fs = new FileStream("C:/Users/Edi/Desktop/gdpr/gdpr" + ".pdf", FileMode.Create, FileAccess.Write, FileShare.None);

                Document document = new Document(PageSize.A4, 25, 25, 30, 30);
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

                string[] columnNames1 = data1.Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToArray();
                sb1.AppendLine(string.Join(",", columnNames));

                document.Add(new Paragraph(sb1.ToString()));
                document.Add(new Paragraph(" "));


                foreach (DataRow row in data1.Rows)
                {
                    y = row.ItemArray.Select(field => field.ToString()).ToArray();
                    if (y[2].ToLower().Contains(wrd.ToLower()))
                    {
                        for (int i = 0; i < columnNames1.Length; i++)
                        {
                            document.Add(new Paragraph(columnNames1[i].ToString() + ":  " + y[i].ToString()));
                        }
                        document.Add(new Paragraph("  "));
                    }
                }

                Console.WriteLine("PDF stvoren");
                document.Close();
                writer.Close();
                fs.Close();

                // ::::::::: MySqlDump, Situational use
                // Process.Start(@"C:\Program Files\MySQL\MySQL Server 5.5\bin\mysqldump.exe", ("-u root -p[password] page > test2.sql"));




                Console.WriteLine("Citanje iz dump file-a");
                
                myConnection.Close();





                // DRUGIO DIO




                Console.Write("Please enter your query: ");
                string query = System.Console.ReadLine();
                Regex regex = new Regex(@"\b" + query + @"\b", RegexOptions.IgnoreCase);
                string line;
                List<string> columns = new List<string>();
                List<string> values = new List<string>();

                // Read the file and display it line by line.  
                System.IO.StreamReader file = new System.IO.StreamReader("C:/Users/Edi/Desktop/gdpr/Backup.sql");
                while ((line = file.ReadLine()) != null)
                {
                    string[] rijeci = line.Split();
                    if (rijeci[0] == "INSERT")
                    {
                        //pokupi nazive stupaca
                        rijeci = (line.Split(new Char[] { '(', ')' }))[1].Split(',');
                        for (int i = 0; i < rijeci.Length; i++)
                        {
                            rijeci[i] = rijeci[i].Trim(new Char[] { ' ', '`', ',' });
                            if (!columns.Contains(rijeci[i]))
                            {
                                columns.Add(rijeci[i]);
                            }
                        }
                    }

                    if (line.Contains("VALUES"))
                    {
                        // pokupi sve retke dodane u tablicu
                        bool end = false;
                        while (true)
                        {
                            line = file.ReadLine();
                            if (line[line.Length - 1] == ';')
                            {
                                end = true;
                                line = line.Trim(';');
                            }
                            values.Add(line.Trim(new Char[] { '(', ')', ' ', '\t', ',' }));
                            if (end)
                            {
                                break;
                            }

                        }

                    }
                }
                List<string> results = new List<string>();

                foreach (string item in values)
                {
                    // postoji li traženi pojam u retku
                    if (regex.IsMatch(item))
                    {
                        results.Add(item);
                    }
                }
                if (!results.Any())
                {
                    Console.WriteLine("NO MATCHES");
                    Console.ReadKey();
                    System.Environment.Exit(0);
                }
                //------------------Ispis na konzolu---------------------
                /*foreach (string item in results)
                {
                    List<string> val = splitval(item);

                    for (int i = 0; i < columns.Count(); i++)
                    {
                        Console.WriteLine(columns[i] + ": " + val[i]);
                    }
                    Console.WriteLine();
                    Console.WriteLine();
                }*/
                // ---------------------------PDF ------------------------------------
                Console.WriteLine("Stvaram PDF-a");

                BaseFont arial = BaseFont.CreateFont("c:\\windows\\fonts\\arial.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                Font NormalFont = new iTextSharp.text.Font(arial, 12, Font.NORMAL);
                Font boldFont = new iTextSharp.text.Font(arial, 12, iTextSharp.text.Font.BOLD);

                FileStream fs1 = new FileStream("C:/Users/Edi/Desktop/gdpr/" + query + ".pdf", FileMode.Create, FileAccess.Write, FileShare.None);
                Document doc = new Document();
                PdfWriter writer1 = PdfWriter.GetInstance(doc, fs1);
                doc.Open();
                foreach (string item in results)
                {
                    List<string> val = splitval(item);

                    for (int i = 0; i < columns.Count(); i++)
                    {

                        doc.Add(new Paragraph
                    {
                        new Chunk(columns[i] + ": ", boldFont),
                        new Chunk(val[i], NormalFont
                        )
                    });

                    }
                    doc.Add(new Paragraph("\n"));
                    doc.Add(new Paragraph("\n"));
                }

                doc.Close();
                Console.WriteLine("PDF stvoren\n");
                //------------------------CSV----------------------------------------
                Console.WriteLine("Stvaram CSV");
                string text = "";
                text = String.Join(",", columns) + ",";
                foreach (string item in results)
                {
                    List<string> val = splitval(item);
                    text += String.Join(",", val);
                }

                File.WriteAllText("C:/Users/Edi/Desktop/gdpr/" + query + ".csv", text);
                Console.WriteLine("CSV stvoren\n");

                file.Close();
                // Suspend the screen.  
                System.Console.ReadLine();
            }

            //Podijeli vrijednosti u retku po stupcima
             
        }

        static List<string> splitval(String line)
        {
            List<string> sval = new List<string>();
            string value = "";
            bool ignore = false;
            bool flag = false;
            foreach (char character in line)
            {
                if (character == '\\')
                {
                    flag = true;
                }
                else if (character == '\'' && flag == false)
                {
                    ignore = !ignore;
                }
                else if (character != ',' || ignore == true)
                {
                    value += character;
                    flag = false;
                }
                else
                {

                    sval.Add(value);
                    value = "";
                }
            }
            sval.Add(value);
            return sval;


        }
    }
 }
