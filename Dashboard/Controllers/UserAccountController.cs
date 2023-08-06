using Dashboard.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;

namespace Dashboard.Controllers
{
    public class UserAccountController : Controller
    {
        readonly string connectionString = "Data Source=LUTHFI; Integrated Security=true; Initial Catalog=UserAccountDB;";

        public IActionResult ListUserAccount()
        {
            List<UserAccount> userAccount = new();

            using (SqlConnection con = new(connectionString))
            {
                con.Open();

                string query = "sp_GetAllUserAccount";
                SqlCommand cmd = new SqlCommand(query, con);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    userAccount.Add(new UserAccount()
                    {
                        id = int.Parse(reader["id"].ToString()),
                        email = reader["email"].ToString(),
                        password = reader["password"].ToString(),
                        username = reader["username"].ToString()
                    });
                }
            }

            return View(userAccount);
        }

        public ActionResult DetailUserAccount(int id)
        {
            var userAccount = new UserAccount();

            SqlConnection con = new(connectionString);
            con.Open();

            SqlCommand cmd = new("sp_GetUserAccountById", con)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@id", id);

            SqlDataReader reader = cmd.ExecuteReader();
            reader.Read();

            userAccount.id = int.Parse(reader["id"].ToString());
            userAccount.email = reader["email"].ToString();
            userAccount.password = reader["password"].ToString();
            userAccount.username = reader["username"].ToString();

            return View(userAccount);
        }

        public ActionResult CreateUserAccount()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateUserAccount(IFormCollection collection)
        {
            try
            {
                SqlConnection con = new(connectionString);
                con.Open();

                string email = collection["email"];
                string password = collection["password"];
                string username = collection["username"];

                SqlCommand cmd = new("sp_InsertUserAccount", con)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@password", password);
                cmd.Parameters.AddWithValue("@username", username);

                int result = cmd.ExecuteNonQuery();

                return RedirectToAction(nameof(ListUserAccount));
            }
            catch
            {
                return View();
            }
        }

        public ActionResult EditUserAccount(int id)
        {
            var userAccount = new UserAccount();

            SqlConnection con = new(connectionString);
            con.Open();

            SqlCommand cmd = new("sp_GetUserAccountById", con)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@id", id);

            SqlDataReader reader = cmd.ExecuteReader();
            reader.Read();

            userAccount.id = int.Parse(reader["id"].ToString());
            userAccount.email = reader["email"].ToString();
            userAccount.password = reader["password"].ToString();
            userAccount.username = reader["username"].ToString();

            return View(userAccount);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUserAccount(int id, IFormCollection collection)
        {
            try
            {
                SqlConnection con = new(connectionString);
                con.Open();

                string email = collection["email"];
                string password = collection["password"];
                string username = collection["username"];

                string query = "UPDATE UserAccount SET email='" + email + "', password='" + password + "', username='" + username + "' WHERE id='" + id + "'";
                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@password", password);
                cmd.Parameters.AddWithValue("@username", username);

                int result = cmd.ExecuteNonQuery();

                return RedirectToAction(nameof(ListUserAccount));
            }
            catch
            {
                return View();
            }
        }

        public ActionResult DeleteUserAccount(int id)
        {
            var userAccount = new UserAccount();

            SqlConnection con = new(connectionString);
            con.Open();

            SqlCommand cmd = new("sp_GetUserAccountById", con)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@id", id);

            SqlDataReader reader = cmd.ExecuteReader();
            reader.Read();

            userAccount.id = int.Parse(reader["id"].ToString());
            userAccount.email = reader["email"].ToString();
            userAccount.password = reader["password"].ToString();
            userAccount.username = reader["username"].ToString();

            return View(userAccount);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteUserAccount(int id, IFormCollection collection)
        {
            try
            {
                SqlConnection con = new(connectionString);
                con.Open();

                SqlCommand cmd = new("sp_DeleteUserAccount", con)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@id", id);

                cmd.ExecuteNonQuery();
                con.Close();
                return RedirectToAction(nameof(ListUserAccount));
            }
            catch
            {
                return View();
            }
        }

        public UserAccount GetLogin(string email, string password)
        {
            UserAccount userAccount = null;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM UserAccount WHERE email = @email AND password = @password";
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@password", password);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        userAccount = new UserAccount
                        {
                            id = (int)reader["id"],
                            email = (string)reader["email"],
                            password = (string)reader["password"]
                        };
                    }
                }
            }

            return userAccount;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(UserAccount userAccount)
        {
            var authenticatedUser = GetLogin(userAccount.email, userAccount.password);

            if (authenticatedUser == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password");
                return View();
            }

            return RedirectToAction(nameof(ListUserAccount));
        }
    }
}
