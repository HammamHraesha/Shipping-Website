using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols;
using ShippingWebsite.Models;

namespace ShippingWebsite.Controllers
{
    public class HomeController : Controller
    {        
        public const string conString = @"Server=.\;Database=CargoDb;User Id=sa; Password=Az123456;Trusted_Connection=True;";
        public static Random random = new Random();

        // DO NOT TOUCH!!
        [HttpGet]
        public IActionResult NewCargo()
        { 
            var model = new CargoModel
            {
                Code = HomeController.random.Next(10000000, 100000000).ToString()
            };

            return View(model);
        }

        // DO NOT TOUCH!!
        private static bool CheckDatabaseExists(string conString, string CargoDb)
        {                   
            bool result = false;
            try
            {
                SqlConnection con = new SqlConnection(conString);
                string sqlQuery = string.Format("SELECT Count(*) FROM sys.databases WHERE Name = '{0}'", CargoDb);

                using (con)
                {
                    using (SqlCommand cmd = new SqlCommand(sqlQuery, con))
                    {
                        con.Open();
                        int databaseID = (int)cmd.ExecuteScalar();
                        con.Close();
                        result = (databaseID > 0);
                    }
                }
            }
            catch 
            {
                return result;
            }

            return result;
        }

        // DO NOT TOUCH!!
        [HttpPost]
        public IActionResult NewCargo(CargoModel model)
        {
            string CargoQuery = "INSERT INTO CargoDb (Code,Name,Quantity,Status,Description) VALUES(@val0, @val1, @val2, @val3, @val4)";
            using (SqlConnection con = new SqlConnection(conString))
            {
                using (SqlCommand cmd = new SqlCommand(CargoQuery, con))
                {           
                    con.Open();                    
                    if (CheckDatabaseExists(conString, "CargoDb"))
                    {
                        cmd.CommandText = CargoQuery;
                        cmd.Parameters.AddWithValue("@val0", model.Code);
                        cmd.Parameters.AddWithValue("@val1", model.Name);
                        cmd.Parameters.AddWithValue("@val2", model.Quantity);
                        cmd.Parameters.AddWithValue("@val3", model.Status);
                        cmd.Parameters.AddWithValue("@val4", model.Description);
                        try
                        {                            
                            cmd.ExecuteNonQuery();
                        }                     
                        catch(SqlException ex)
                        {
                            string msg = "Insert Error:";
                            msg += ex.Message;
                            throw new Exception(msg);
                        }
                    }
                    else
                    {
                        cmd.CommandText = "CREATE TABLE CargoDb(Code VARCHAR(255) NOT NULL PRIMARY KEY, NotRealName VARCHAR(255) NOT NULL, Quantity VARCHAR(255) NOT NULL, Status VARCHAR(255) NOT NULL, Description VARCHAR(255) NOT NULL)";
                        cmd.CommandText = CargoQuery;
                        cmd.Parameters.AddWithValue("@val0", model.Code);
                        cmd.Parameters.AddWithValue("@val1", model.Name);
                        cmd.Parameters.AddWithValue("@val2", model.Quantity);
                        cmd.Parameters.AddWithValue("@val3", model.Status);
                        cmd.Parameters.AddWithValue("@val4", model.Description);
                        try
                        {
                            cmd.ExecuteNonQuery();
                        }
                        catch (SqlException ex)
                        {
                            string msg = "This cargo already exist.";
                            msg += ex.Message;
                            throw new Exception(msg);
                        }
                    }
                    con.Close();
                }
            }           

            return RedirectToAction("ListCargo");
        }

        // DO NOT TOUCH!!
        [HttpGet]
        public IActionResult SearchCargo()
        {
            return View();
        }

        // DO NOT TOUCH!!
        [HttpPost]
        public IActionResult SearchCargo(SearchModel searchModelCode)
        {
            List<CargoModel> SearchCargoDb = new List<CargoModel>();
                        
            string CargoQuery = string.Empty;
            if (int.TryParse(searchModelCode.SearchCode.ToString(), out int number))
            {
                CargoQuery = $"SELECT * FROM CargoDb WHERE Code = {searchModelCode.SearchCode} OR Name = null";
            }
            else
            {
                CargoQuery = $"SELECT * FROM CargoDb WHERE Code = NULL OR Name = '{searchModelCode.SearchCode}'";
            }

            using (SqlConnection con = new SqlConnection(conString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(CargoQuery, con))
                {
                    SqlDataReader sdr = cmd.ExecuteReader();
                    while (sdr.Read())
                    {
                        SearchCargoDb.Add(new CargoModel
                        {
                            Code = sdr["Code"].ToString(),
                            Name = sdr["Name"].ToString(),
                            Quantity = sdr["Quantity"].ToString(),
                            Status = sdr["Status"].ToString(),
                            Description = sdr["Description"].ToString()
                        });
                    }
                    con.Close();
                }
                return View("ListCargo", SearchCargoDb);                  
            }
        }       

        // DO NOT TOUCH!!
        [HttpGet]
        public ActionResult UpdateCargo(CargoModel toUpdateModel)
        {                            
            return View("UpdateCargo", toUpdateModel);           
        }            
           
        [HttpPost]
        [ActionName("UpdateCargo")]
        public IActionResult UpdatingCargo(CargoModel updateModel)
        {            
            string CargoQuery = "UPDATE CargoDb SET Name = @val1, Quantity = @val2, Status = @val3, Description = @val4 WHERE Code = @val0";
            using (SqlConnection con = new SqlConnection(conString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(CargoQuery, con))
                {
                    cmd.CommandText = CargoQuery;
                    cmd.Parameters.AddWithValue("@val0", updateModel.Code);
                    cmd.Parameters.AddWithValue("@val1", updateModel.Name);
                    cmd.Parameters.AddWithValue("@val2", updateModel.Quantity);
                    cmd.Parameters.AddWithValue("@val3", updateModel.Status);
                    cmd.Parameters.AddWithValue("@val4", updateModel.Description);
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        string msg = "Insert Error:";
                        msg += ex.Message;
                        throw new Exception(msg);
                    }
                    
                    con.Close();
                }
            }
            CargoModel model = new CargoModel
            {
                Code = updateModel.Code,
                Name = updateModel.Name,
                Quantity = updateModel.Quantity,
                Status = updateModel.Status,
                Description = updateModel.Description
            };

            // you should added update code - research

            return RedirectToAction("ListCargo");
        }

        // DO NOT TOUCH!!
        [HttpGet]
        public ActionResult ListCargo()
        {
            List<CargoModel> cargoDb = new List<CargoModel>();
            if (CheckDatabaseExists(conString, "CargoDb"))
            {
                string CargoQuery = "SELECT * FROM CargoDb";
                using (SqlConnection con = new SqlConnection(conString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(CargoQuery, con))
                    {
                        SqlDataReader sdr = cmd.ExecuteReader();
                        while (sdr.Read())
                        {
                            cargoDb.Add(new CargoModel
                            {
                                Code = sdr["Code"].ToString(),
                                Name = sdr["Name"].ToString(),
                                Quantity = sdr["Quantity"].ToString(),
                                Status = sdr["Status"].ToString(),
                                Description = sdr["Description"].ToString()
                            });
                        }
                        con.Close();
                    }
                }
            }
            return View(cargoDb);
        }

        // DO NOT TOUCH!! 
        [HttpGet]
        public IActionResult DeleteCargo(CargoModel DeleteModel)
        {
            List<CargoModel> cargoDb = new List<CargoModel>();
            string CargoQuery = $"DELETE FROM CargoDb WHERE Code = {DeleteModel.Code}";
            using (SqlConnection con = new SqlConnection(conString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(CargoQuery, con)
                {
                    CommandText = CargoQuery
                };
                cmd.ExecuteNonQuery();
                SqlDataReader sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    cargoDb.Add(new CargoModel
                    {
                        Code = sdr["Code"].ToString(),
                        Name = sdr["Name"].ToString(),
                        Quantity = sdr["Quantity"].ToString(),
                        Status = sdr["Status"].ToString(),
                        Description = sdr["Description"].ToString()
                    });
                }
                con.Close();
            }
            return RedirectToAction("ListCargo", cargoDb);
        }
    }      
}

