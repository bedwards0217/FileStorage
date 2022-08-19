using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace ConsoleBlob
{
    class Program
    {
        static void Main(string[] args)
        {
            GetFileData("aaa2.JPG","","","","");

            //Console.WriteLine("Please enter root directory and press enter");
            //string line = Console.ReadLine();  //Pause to check command window for results; hit any key to close window 

            string PathVar = "c:\\temp\\aaa";
            //string PathVar = line;
            string[] filesindirectory = Directory.GetFiles(PathVar, "*.*", SearchOption.AllDirectories);
            List<String> images = new List<string>(filesindirectory.Count());

            foreach (string item in filesindirectory)
            {
                if (item.ToLower().Contains(".pdf") || item.ToLower().Contains(".png") || item.ToLower().Contains(".jpg"))
                //if (item.Contains(".png"))
                {
                    string s = StoreFiles(item);

                    //Console.WriteLine(item);
                    //Console.ReadLine();
                }
            }

        }

        private static string StoreFiles(string pstrPath)
        {
            string lstrFilename = Path.GetFileName(pstrPath);
            string lstrDoctype = "";
            string lstrEntity = "";
            string lstrTmp = "";
            string lstrMonth = "";

            int x = pstrPath.LastIndexOf("\\");
            lstrTmp = pstrPath.Substring(0, x);
            x = lstrTmp.LastIndexOf("\\");
            if (x > 0)
            {
                lstrMonth = lstrTmp.Substring(x + 1);
            }
            else
            {
                lstrMonth = lstrTmp;
            }

            DateTime ldModDate = File.GetLastWriteTime(pstrPath);
            DateTime dbDate = GetFileDate(lstrFilename);
            int iCompare = DateTime.Compare(ldModDate, dbDate);

            if (!(iCompare == 1) )
            {
                return "";
            }

            if (pstrPath.ToLower().Contains("invoice"))
            {
                lstrDoctype = "Invoice";
                int i = lstrFilename.IndexOf(".");
                lstrEntity = lstrFilename.Substring(0, i);
            }
            if (pstrPath.ToLower().Contains("commission"))
            {
                lstrDoctype = "Commission";
                int i = lstrFilename.IndexOf(".");
                lstrEntity = lstrFilename.Substring(0, i);
            }
            if (pstrPath.ToLower().Contains("email"))
            {
                lstrDoctype = "Email";
                int i = lstrFilename.IndexOf(".");
                lstrEntity = lstrFilename.Substring(0, i);
            }

            Stream theStream = null;

            Boolean FileOK = false;
            const int BUFFER_SIZE = 2555555;
            Byte[] lstrBytes = new Byte[BUFFER_SIZE];
            Boolean FileSaved = false;
            
            
            lstrBytes = File.ReadAllBytes(pstrPath);

            try
            {
                StringBuilder strUploadedContent = new StringBuilder("");
                StringBuilder strErrorContent = new StringBuilder("");
                StreamReader sr = new StreamReader(pstrPath);

                theStream = sr.BaseStream; // Upload.PostedFile.InputStream;

                using (Stream s = theStream)
                {
                    using (BinaryReader br = new BinaryReader(s))
                    {
                        byte[] Databytes = br.ReadBytes((Int32)s.Length);

                        //string ConnectionStrings = "Server=localhost\\SQLEXPRESS;Database=Blobtest;Trusted_Connection=True;";
                        //string ConnectionStrings = "SERVER=104.232.44.22;DATABASE=HMCCDN;UID=CDNUser;PWD=BIiJQ?zEWp%OZ0p45K;v";
                        string ConnectionStrings = "SERVER=104.232.44.22;DATABASE=HMCCDN;UID=CDNUser;PWD='BIiJQ?zEWp%OZ0p45K;v'";
                        
                        using (SqlConnection con = new SqlConnection(ConnectionStrings))
                        {
                            string query = "INSERT INTO [DocumentStorage] VALUES (@FileName, @BinaryData, @DocType, @AssociatedEntity, @AssociatedMonth, @ModDate)";

                            //create an object for SQL command class  
                            using (SqlCommand cmd = new SqlCommand(query))
                            {
                                cmd.Connection = con;
                                cmd.Parameters.AddWithValue("@FileName", lstrFilename.Trim());
                                cmd.Parameters.AddWithValue("@BinaryData", Databytes);
                                cmd.Parameters.AddWithValue("@DocType", lstrDoctype);
                                cmd.Parameters.AddWithValue("@AssociatedEntity", lstrEntity);
                                cmd.Parameters.AddWithValue("@AssociatedMonth", lstrMonth);
                                cmd.Parameters.AddWithValue("@ModDate", ldModDate);
                                con.Open();
                                cmd.ExecuteNonQuery();
                                con.Close();
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                //lblError.Text = "File could not be uploaded." + ex.Message.ToString();
                //lblError.Visible = true;
                //FileSaved = false;
                //lblErrors.ForeColor = System.Drawing.Color.Red;
                //lblErrorTitle.Visible = false;
                //pnlErr.Visible = true;
                //btnBack.Visible = true;
                //pnlMain.Visible = false;
                //return;
            }

            return "success";
        }

        private static DateTime GetFileDate(string psFile)
        {
            string lstrsql = "SELECT ModDate FROM [HMCCDN].[dbo].[DocumentStorage] where FileName = '" + psFile + "'";
            string lstrReturn = "";

            SqlConnection gadoAdmin = new SqlConnection("SERVER=104.232.44.22;DATABASE=HMCCDN;UID=CDNUser;PWD='BIiJQ?zEWp%OZ0p45K;v'");
            try
            {
                gadoAdmin.Open();
                SqlCommand sqlAdmin = new SqlCommand(lstrsql, gadoAdmin);
                sqlAdmin.CommandType = CommandType.Text;
                sqlAdmin.CommandText = lstrsql;

                
                lstrReturn = sqlAdmin.ExecuteScalar().ToString();
                
            }
            catch (Exception ex)
            {
                return DateTime.MinValue;
            }
            finally
            {
                gadoAdmin.Close();
            }

            return Convert.ToDateTime(lstrReturn);
        }


        //private static byte[] GetFileData(string psFile)
        //{
        //    string lstrsql = "SELECT BinaryData FROM [HMCCDN].[dbo].[DocumentStorage] where FileName = '" + psFile + "'";
        //    byte[] lstrReturn;
        //    MemoryStream ms = new MemoryStream();

        //    SqlConnection gadoAdmin = new SqlConnection("SERVER=104.232.44.22;DATABASE=HMCCDN;UID=CDNUser;PWD='BIiJQ?zEWp%OZ0p45K;v'");
        //    try
        //    {
        //        gadoAdmin.Open();
        //        SqlCommand sqlAdmin = new SqlCommand(lstrsql, gadoAdmin);
        //        sqlAdmin.CommandType = CommandType.Text;
        //        sqlAdmin.CommandText = lstrsql;
        //        //sqlAdmin.CommandText = @"SELECT BinaryData FROM [HMCCDN].[dbo].[DocumentStorage] where FileName = '" + psFile + "'";


        //        //lstrReturn = sqlAdmin.ExecuteScalar().ToString();

        //        using (var reader = sqlAdmin.ExecuteReader())
        //        {
        //            byte[] buffer = new byte[8040];
        //            long offset = 0;
        //            long read;
        //            //if (reader.Read())
        //            while ((read = reader.GetBytes(0, offset, buffer, 0, buffer.Length)) > 0)
        //            {

        //                offset += read;
        //                destination.Write(buffer, 0, read);
        //                //byte[] raw = (byte[])reader.Items["Contact_CardImage"];

        //                // TODO: do something with the raw data
        //                return raw;
        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        return ms.GetBuffer();
        //    }
        //    finally
        //    {
        //        gadoAdmin.Close();
        //    }


        //    return ms.GetBuffer();
        //}

        private static byte[] GetFileData(string psFile, string psDoctype, string psEntity, string psMonth, string psModDate)
        {
            System.Data.DataSet ds = new System.Data.DataSet();
            //string ls1 = "Select FileName, BlobData from Blob where BlobID = 7";
            string ls1 = "SELECT BinaryData FROM [HMCCDN].[dbo].[DocumentStorage] where ";
            ls1 += " (FileName = '" + psFile + "' or '" + psFile + "' = '') and ";
            ls1 += " (Doctype = '" + psDoctype + "' or '" + psDoctype + "' = '') and ";
            ls1 += " (AssociatedEntity = '" + psEntity + "' or '" + psEntity + "' = '') and ";
            ls1 += " (AssociatedMonth = '" + psMonth + "' or '" + psMonth + "' = '') and ";
            ls1 += " (ModDate = '" + psModDate + "' or '" + psModDate + "' = '') ";

            SqlConnection gadoCM = new SqlConnection("SERVER=104.232.44.22;DATABASE=HMCCDN;UID=CDNUser;PWD='BIiJQ?zEWp%OZ0p45K;v'");
            try
            {
                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter();
                gadoCM.Open();
                SqlCommand sqlCheck = new SqlCommand(ls1, gadoCM);
                sqlCheck.CommandType = CommandType.Text;

                da.SelectCommand = sqlCheck;
                da.Fill(dt);

                Byte[] bytes = (Byte[])dt.Rows[0]["BinaryData"];

                //Response.Buffer = true;
                //Response.Charset = "";
                //Response.Cache.SetCacheability(HttpCacheability.NoCache);
                //Response.AddHeader("content-disposition", "attachment;filename=" + dt.Rows[0]["FileName"].ToString());
                //Response.BinaryWrite(bytes);
                //Response.Flush();
                //Response.End();

                return bytes;
            }
            catch (Exception ex)
            {
                Byte[] nobytes = null;
                return nobytes;
            }
            finally { gadoCM.Close(); }
            
        }
    }
}
