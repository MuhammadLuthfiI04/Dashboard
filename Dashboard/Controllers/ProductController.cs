using Dashboard.Models;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.Data.SqlClient;

namespace Dashboard.Controllers
{
    public class ProductController : Controller
    {
        readonly string connectionString = "Data Source=LUTHFI; Integrated Security=true; Initial Catalog=UserAccountDB;";

        public IActionResult ListProduct()
        {
            List<Product> product = new();

            using (SqlConnection con = new(connectionString))
            {
                con.Open();

                string query = "select * from Product";
                SqlCommand cmd = new SqlCommand(query, con);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    product.Add(new Product()
                    {
                        id = int.Parse(reader["id"].ToString()),
                        product = reader["product"].ToString(),
                        status = int.Parse(reader["status"].ToString()),
                        information = reader["information"].ToString()
                    });
                }
            }

            return View(product);
        }

        public IActionResult Export()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string query = "select * from Product";
                SqlCommand cmd = new SqlCommand(query, con);
                SqlDataReader reader = cmd.ExecuteReader();

                var products = new List<Product>();

                while (reader.Read())
                {
                    products.Add(new Product()
                    {
                        id = int.Parse(reader["id"].ToString()),
                        product = reader["product"].ToString(),
                        status = int.Parse(reader["status"].ToString()),
                        information = reader["information"].ToString()
                    });
                }

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Product");

                    worksheet.Cells.LoadFromCollection(products, true);
                    worksheet.Cells["A1:D1"].Style.Font.Bold = true;

                    worksheet.Cells.AutoFitColumns();

                    var fileName = $"ProductList.xlsx";
                    return File(package.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }

        public IActionResult Import(IFormFile file)
        {
            if (file == null || file.Length <= 0)
            {
                return RedirectToAction("ListProduct");
            }
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            List<Product> newProducts = new List<Product>();

            using (var stream = new MemoryStream())
            {
                file.CopyTo(stream);
                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        newProducts.Add(new Product()
                        {
                            product = worksheet.Cells[row, 1].Value.ToString(),
                            status = int.TryParse(worksheet.Cells[row, 2].Value?.ToString(), out int statusValue) ? statusValue : 0,
                            information = worksheet.Cells[row, 3].Value.ToString()
                        });
                    }
                }
            }

            if (newProducts.Count > 0)
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    string query = "INSERT INTO Product (product, status, information) VALUES (@product, @status, @information)";
                    foreach (var product in newProducts)
                    {
                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@product", product.product);
                            cmd.Parameters.AddWithValue("@status", product.status);
                            cmd.Parameters.AddWithValue("@information", product.information);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }

            return RedirectToAction("ListProduct");
        }

        [HttpGet]
        public IActionResult GetUpdatedProductData()
        {
            try
            {
                List<Product> product = new List<Product>();
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    string query = "SELECT * FROM Product";
                    SqlCommand cmd = new SqlCommand(query, con);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        product.Add(new Product()
                        {
                            id = int.Parse(reader["id"].ToString()),
                            product = reader["product"].ToString(),
                            status = int.Parse(reader["status"].ToString()),
                            information = reader["information"].ToString()
                        });
                    }
                }

                return Json(product);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
    }
}
