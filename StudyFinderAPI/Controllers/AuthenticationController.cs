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
        public String PostSignUp([FromBody] AuthModel model)
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
                cmd.ExecuteReader();
                //var reader = cmd.ExecuteReader();
                /*while (reader.Read())
                {
                    string someStringFromColumnZero = reader.GetString(0);
                    string someStringFromColumnOne = reader.GetString(1);
                    Console.WriteLine(someStringFromColumnZero + "," + someStringFromColumnOne);
                }*/
                dbCon.Close();
            }

            return "Success";
        }
    }
}
