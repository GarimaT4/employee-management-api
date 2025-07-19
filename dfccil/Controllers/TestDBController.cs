using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Crypto.Tls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.UI.WebControls;
using static System.Net.Mime.MediaTypeNames;

namespace dfccil.Controllers
{
    public class TestDBController : ApiController
    {
        [HttpGet]
        [Route("api/GetDepartments")]
       public object GetDepartments()
        { 
            DataSet dataSet = new DataSet();
            try
            {
                SqlConnection conn = new SqlConnection();                
                conn.ConnectionString= getConnectionString();
                conn.Open();
                
                SqlDataAdapter adp = new SqlDataAdapter("exec getDepartments", conn);
                adp.Fill(dataSet);
                conn.Close();
                
            }
            catch (Exception ex)
            {

            }
           
            return dataSet;
        }

        [HttpPost]
        [Route("api/AddDepartment/{Dname}/{Description}")]
        public object AddDepartment(string Dname, string Description)
        {
            try
            {
                SqlConnection conn = new SqlConnection();
                conn.ConnectionString = getConnectionString();
                conn.Open();

                // The original command text with string formatting
                string commandText = string.Format("exec addDepartment '{0}', '{1}'", Dname, Description);

                SqlCommand command = conn.CreateCommand();
                command.CommandText = commandText;

                int result = command.ExecuteNonQuery();

                conn.Close();

                // Return success message if rows were affected
                if (result > 0)
                {
                    return new { Message = "Department added successfully." };
                }
                else
                {
                    return new { Message = "Failed to add department." };
                }
            }
            catch (Exception ex)
            {
                // Return an error message in case of exception
                return new { Message = "An error occurred.", Details = ex.Message };
            }
        }


        [HttpPost]
        [Route("api/UpdateDepartment/{pkDeptid}/{Dname}/{Description}/{status}")]
        public object UpdateDepartment(int pkDeptid, string Dname, string Description, int status)
        {
            DataSet dataSet = new DataSet();
            try
            {
                SqlConnection conn = new SqlConnection();
                conn.ConnectionString = getConnectionString();
                conn.Open();
                string commandText = string.Format("exec updateDepartment {0},'{1}','{2}',{3}",pkDeptid, Dname, Description, status);
                SqlCommand command = conn.CreateCommand();
                command.CommandText = commandText;
                int result = command.ExecuteNonQuery();

                conn.Close();

                if (result != 0)
                {

                }

            }
            catch (Exception ex)
            {


            }
                return dataSet;
        }
        [HttpGet]
        [Route("api/GetEmployees")]
        public object GetEmployees()
        {
            DataSet dataSet = new DataSet();
            try
            {
                SqlConnection conn = new SqlConnection();
                conn.ConnectionString = getConnectionString();
                conn.Open();

                SqlDataAdapter adp = new SqlDataAdapter("exec getEmployees", conn);
                adp.Fill(dataSet);
                conn.Close();

            }
            catch (Exception ex)
            {
                

            }

            return dataSet;
        }
        [HttpPost]
        [Route("api/AddEmployee")]
        public IHttpActionResult AddEmployee()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                string filename = null;

                if (httpRequest.Files.Count > 0)
                {
                    var postedFile = httpRequest.Files[0];
                    filename = Path.GetFileName(postedFile.FileName);

                    // Define the upload path
                    string uploadPath = HttpContext.Current.Server.MapPath("~/Uploads/");

                    // Check if the directory exists, if not, create it
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    // Combine the upload path and file name
                    string filePath = Path.Combine(uploadPath, filename);

                    // Save the file
                    postedFile.SaveAs(filePath);
                }

                // Extract form data
                string ECode = httpRequest.Form["ECode"];
                string Ename = httpRequest.Form["Ename"];
                int Deptid = int.Parse(httpRequest.Form["Deptid"]);
                int Desigid = int.Parse(httpRequest.Form["Desigid"]);
                decimal BasicSal = decimal.Parse(httpRequest.Form["BasicSal"]);
                int Status = 0; // Default status as Active (0)

                // Call AddEmployee method
                using (SqlConnection conn = new SqlConnection(getConnectionString()))
                {
                    conn.Open();
                    using (SqlCommand command = new SqlCommand("addEmployee", conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@Empcode", ECode);
                        command.Parameters.AddWithValue("@Empname", Ename);
                        command.Parameters.AddWithValue("@fkDeptid", Deptid);
                        command.Parameters.AddWithValue("@fkDesigId", Desigid);
                        command.Parameters.AddWithValue("@basicsalary", BasicSal);
                        command.Parameters.AddWithValue("@status", Status);
                        command.Parameters.AddWithValue("@filename", filename);

                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            return Ok(new { message = "Employee added successfully", filename });
                        }
                        else
                        {
                            return BadRequest("Failed to add employee");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception (ex) here as per your logging mechanism
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("api/UpdateEmployee")]
        public async Task<IHttpActionResult> UpdateEmployee()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return StatusCode(HttpStatusCode.UnsupportedMediaType);
            }

            string root = HttpContext.Current.Server.MapPath("~/Uploads/");
            Directory.CreateDirectory(root); // Ensure the directory exists
            var provider = new MultipartFormDataStreamProvider(root);

            try
            {
                // Read the form data and the file data
                await Request.Content.ReadAsMultipartAsync(provider);

                // Extract form data
                var formData = provider.FormData;

                if (!int.TryParse(formData["pkEmpid"], out int pkEmpid))
                {
                    return BadRequest("Missing or invalid employee ID.");
                }
                string Empcode = formData.Get("Empcode");
                string Empname = formData.Get("Empname");
                string filename = formData.Get("filename");

                // Try parsing other fields, using default values if not provided
                int fkDeptid;
                if (!int.TryParse(formData.Get("fkDeptid"), out fkDeptid))
                {
                    fkDeptid = -1;
                }

                int fkDesigId;
                if (!int.TryParse(formData.Get("fkDesigId"), out fkDesigId))
                {
                    fkDesigId = -1;
                }

                decimal BasicSal;
                if (!decimal.TryParse(formData.Get("BasicSal"), out BasicSal))
                {
                    BasicSal = -1;
                }

                int Status;
                if (!int.TryParse(formData.Get("Status"), out Status))
                {
                    Status = -1;
                }

                // Extract and handle file data if present
                if (provider.FileData.Any())
                {
                    var file = provider.FileData.First();
                    string localFileName = file.LocalFileName;
                    string newFileName = Path.GetFileName(file.Headers.ContentDisposition.FileName.Trim('"'));
                    string newFilePath = Path.Combine(root, newFileName);

                    // Replace existing file if it exists
                    if (File.Exists(newFilePath))
                    {
                        File.Delete(newFilePath);
                    }

                    // Move the new file to the target directory
                    File.Move(localFileName, newFilePath);
                    filename = newFileName; // Use the new filename
                }

                // Update the database
                using (SqlConnection conn = new SqlConnection(getConnectionString()))
                {
                    await conn.OpenAsync();
                    using (SqlCommand command = new SqlCommand("UpdateEmployee", conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@pkEmpid", pkEmpid);

                        // Use provided values or current values if not provided
                        command.Parameters.AddWithValue("@Empcode", Empcode);
                        command.Parameters.AddWithValue("@Empname", Empname);
                        command.Parameters.AddWithValue("@fkDeptid", fkDeptid);
                        command.Parameters.AddWithValue("@fkDesigId", fkDesigId );
                        command.Parameters.AddWithValue("@BasicSal", BasicSal  ) ;
                        command.Parameters.AddWithValue("@Status", Status );
                        command.Parameters.AddWithValue("@filename", filename);

                        int result = await command.ExecuteNonQueryAsync();

                        if (result == 0)
                        {
                            return BadRequest("Update failed");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception details if necessary
                return InternalServerError(ex);
            }

            return Ok("Employee updated successfully");
        }


        [HttpGet]
        [Route("api/GetDesignations")]
        public object GetDesignations()
        {
            DataSet dataSet = new DataSet();
            try
            {
                SqlConnection conn = new SqlConnection();
                conn.ConnectionString = getConnectionString();
                conn.Open();

                SqlDataAdapter adp = new SqlDataAdapter("exec getDesignations", conn);
                adp.Fill(dataSet);
                conn.Close();

            }
            catch (Exception ex)
            {
                

            }

            return dataSet;
        }
        [HttpPost]
        [Route("api/AddDesignation/{Designame}/{DesigDescription}")]
        public object AddDesignation(string Designame, string DesigDescription)
        {
            DataSet dataSet = new DataSet();
            try
            {
                SqlConnection conn = new SqlConnection();
                conn.ConnectionString = getConnectionString();
                conn.Open();

                // String formatting as per your original logic
                string commandText = string.Format("exec addDesignation '{0}', '{1}'", Designame, DesigDescription);

                SqlCommand command = conn.CreateCommand();
                command.CommandText = commandText;

                int result = command.ExecuteNonQuery();

                conn.Close();

                // Check the result and return a message if needed
                if (result > 0)
                {
                    // Success, return a success message
                    return new { Message = "Designation added successfully." };
                }
                else
                {
                    // Failure, return a failure message
                    return new { Message = "Failed to add designation." };
                }
            }
            catch (Exception ex)
            {
                // Return a simple error message
                return new { Message = "An error occurred.", Details = ex.Message };
            }
        }

        [HttpPost]
        [Route("api/UpdateDesignation/{pkDesigId}/{Designame}/{DesigDescription}/{status}")]
        public object UpdateDesignation(int pkDesigId, string Designame, string DesigDescription, int status)
        {
            DataSet dataSet = new DataSet();
            try
            {
                SqlConnection conn = new SqlConnection();
                conn.ConnectionString = getConnectionString();
                conn.Open();
                string commandText = string.Format("exec updateDesignation {0},'{1}','{2}',{3}", pkDesigId, Designame, DesigDescription, status);
                SqlCommand command = conn.CreateCommand();
                command.CommandText = commandText;
                int result = command.ExecuteNonQuery();

                conn.Close();

                if (result != 0)
                {

                }

            }
            catch (Exception ex)
            {


            }
            return dataSet;
        }

            public string getConnectionString()
        {
            return "server=DESKTOP-O7AU0NF\\MSSQLSERVER01;database=TestDB;Integrated Security=true;";
        }

    }
}