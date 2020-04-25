using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;
using StudyFinderAPI.Helpers;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace StudyFinderAPI
{
    public class AuthHelper
    {
        public static byte[] CreateSalt(int size)
        {
            //Generate a cryptographic random number.
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] buff = new byte[size];
            rng.GetBytes(buff);

            // Return a Base64 string representation of the random number.
            return buff;
        }

        public static byte[] GenerateSaltedHash(byte[] plainText, byte[] salt)
        {
            HashAlgorithm algorithm = new SHA256Managed();

            byte[] plainTextWithSaltBytes =
              new byte[plainText.Length + salt.Length];

            for (int i = 0; i < plainText.Length; i++)
            {
                plainTextWithSaltBytes[i] = plainText[i];
            }
            for (int i = 0; i < salt.Length; i++)
            {
                plainTextWithSaltBytes[plainText.Length + i] = salt[i];
            }

            return algorithm.ComputeHash(plainTextWithSaltBytes);
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
    }

    [Route("api/auth")]
    public class AuthenticationController : Controller
    {
        // POST: api/auth/signup
        [HttpPost("signup")]
        public string PostSignUp([FromBody] AuthModel model)
        {

            //validating data received
            if (model.email.Length == 0 || model.password.Length == 0 || model.name.Length == 0)
            {
                return "missing info.";
            }
            if (model.password.Length < 8 || model.password.Length > 20)
            {
                return "pw no length good.";
            }
            if (!new EmailAddressAttribute().IsValid(model.email))
            {
                return "email no good.";
            }

            //hashing password
            byte[] salt = AuthHelper.CreateSalt(8);
            byte[] hashed_password = AuthHelper.GenerateSaltedHash(Encoding.ASCII.GetBytes(model.password), salt);


            //connecting to database
            var dbCon = DBConnection.Instance();
            dbCon.DatabaseName = "StudyFinder";
            if (dbCon.IsConnect())
            {
                //querying database
                string query = $"INSERT into users values ('{model.email}', '{model.name}', '{AuthHelper.ByteArrayToString(salt)}', '{AuthHelper.ByteArrayToString(hashed_password)}')";
                var cmd = new MySqlCommand(query, dbCon.Connection);
                var reader = cmd.ExecuteReader();
                /*while (reader.Read())
                {
                    string someStringFromColumnZero = reader.GetString(0);
                    string someStringFromColumnOne = reader.GetString(1);
                    Console.WriteLine(someStringFromColumnZero + "," + someStringFromColumnOne);
                }*/
                reader.Close();
                dbCon.Close();
            }

            return "Success";
        }

        // GET: api/auth/login
        [HttpPost("login")]
        public String PostLogin([FromBody] AuthModel model)
        {
            //validating data received
            if (model.email.Length == 0 || model.password.Length == 0 || model.name.Length == 0)
            {
                return "missing info.";
            }
            if (model.password.Length < 8 || model.password.Length > 20)
            {
                return "pw no length good.";
            }
            if (!new EmailAddressAttribute().IsValid(model.email))
            {
                return "email no good.";
            }

            SqlCommand command = new SqlCommand("select * from users where email = @email");
            command.Parameters.AddWithValue("@email", model.email);
            IEnumerable<Dictionary<string, object>> result = DBHelper.queryDatabase(command);
            if(result.Any())
            {
                Dictionary<string, object> record = result.ElementAt(0);
                string salt = (string) record["salt"];
            }

            ////connecting to database
            //var dbCon = DBConnection.Instance();
            //dbCon.DatabaseName = "StudyFinder";
            //if (dbCon.IsConnect())
            //{
            //    //querying database: grabs the record of user based on email
            //    string query = $"SELECT * FROM users WHERE email = '{model.email}'";
            //    var cmd = new MySqlCommand(query, dbCon.Connection);
            //    var reader = cmd.ExecuteReader();
            //    //if user is record is found, checks if entered password is correct, else if record is empty returns user is not found
            //    if (reader.HasRows)
            //    {
            //        reader.Read();
            //        string salt = reader.GetString(2);
            //        string pw = reader.GetString(3);
            //        Console.WriteLine("salt: "+salt+" pw: "+pw);

            //        byte[] pw_to_bytes = Encoding.ASCII.GetBytes(salt);
            //        Console.WriteLine("converted_salt: " + AuthHelper.ByteArrayToString(pw_to_bytes));

            //        //salted_pw is the hashed password that results from hasing the user entered password with the salt grabbed from db, converts into a string 
            //        string salted_pw = AuthHelper.ByteArrayToString(AuthHelper.GenerateSaltedHash(Encoding.ASCII.GetBytes(model.password), Encoding.ASCII.GetBytes(salt)));
            //        Console.WriteLine("salted_pw: "+salted_pw);

            //        if (!pw.Equals(salted_pw))
            //        {
            //            return "password is incorrect.";
            //        }
            //    }
            //    else
            //    {
            //        return "user is not found.";
            //    }
            //    /*while (reader.Read())
            //    {
            //        string someStringFromColumnZero = reader.GetString(0);
            //        string someStringFromColumnOne = reader.GetString(1);
            //        Console.WriteLine(someStringFromColumnZero + "," + someStringFromColumnOne);
            //    }*/
            //    reader.Close();
            //    dbCon.Close();
            //}

            return "Success";
        }
    }
}
