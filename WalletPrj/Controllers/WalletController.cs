﻿using System.Data;
using System.Text.RegularExpressions;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace WalletPrj.Controllers
{
    public class WalletController : Controller
    {
        private readonly SqlConnectionStringBuilder _connection = new SqlConnectionStringBuilder()
        {
            DataSource = "DELL",
            InitialCatalog = "MiniWallet",
            UserID = "SA",
            Password = "root",
            TrustServerCertificate = true,
        };

        [ActionName("Index")]
        public IActionResult Index()
        {
            return View("LoginIndex");
        }

        [ActionName("Login")]
        public async Task<IActionResult> Login(WalletModel requestmodel)
        {
            using IDbConnection connection = new SqlConnection(_connection.ConnectionString);
            connection.Open();
            string query = @"SELECT [WalletId]
      ,[WalletUserName]
      ,[FullName]
      ,[MobileNo]
      ,[Balance]
      ,[ImagePath]
  FROM [dbo].[Tbl_Wallet] Where WalletUserName = @WalletUserName and MobileNo = @MobileNo";
            if (string.IsNullOrEmpty(requestmodel.WalletUserName))
            {
                TempData["isSuccess"] = false;
                TempData["message"] = "Name is required!";
                goto Results;
            }
            if (string.IsNullOrEmpty(requestmodel.MobileNo))
            {
                TempData["isSuccess"] = false;
                TempData["message"] = "Mobile No is required!";
                goto Results;
            }
            var res = await connection.QueryFirstOrDefaultAsync<WalletModel>(query, requestmodel);
            if (res is null)
            {
                TempData["isSuccess"] = false;
                TempData["message"] = "Incorrect Username or Mobile No";
                goto Results;
            }
            TempData["isSuccess"] = true;
            TempData["message"] = "Successfully Login!";
            return View("Login", res);
        Results:
            return RedirectToAction("Index");
        }


        [ActionName("Edit")]
        public async Task<IActionResult> Edit(int id)
        {
            using IDbConnection connection = new SqlConnection(_connection.ConnectionString);
            connection.Open();
            string query = @"select * from Tbl_Wallet where WalletId = @WalletId";
            var res = await connection.QueryFirstOrDefaultAsync<WalletModel>(query, new WalletModel
            {
                WalletId = id
            });
            connection.Close();
            return View("Edit", res);
        }

        [HttpPost]
        [ActionName("Update")]

        public async Task<IActionResult> Update(int id, WalletModel requestmodel)
        {
            using IDbConnection connection = new SqlConnection(_connection.ConnectionString);
            connection.Open();
            string query = @"UPDATE [dbo].[Tbl_Wallet]
   SET [WalletUserName] = @WalletUserName
      ,[FullName] = @FullName
      ,[ImagePath] = @ImagePath
 WHERE WalletId = @WalletId";
            var res = await connection.ExecuteAsync(query, new WalletModel
            {
                WalletId = id,
                WalletUserName = requestmodel.WalletUserName,
                FullName = requestmodel.FullName,
                ImagePath = "~/images/" + requestmodel.ImagePath,
            });
           if(res is 1)
            {
                TempData["isSuccess"] = true;
                TempData["message"] = "Update Successful";
                goto Results;
            }
            connection.Close();
        Results:
            return RedirectToAction("Index");
        }


        [ActionName("Register")]
        public IActionResult Register()
        {
            return View("Register");
        }

        [HttpPost]
        [ActionName("Register")]
        public async Task<IActionResult> Register(WalletModel requestmodel)
        {
            requestmodel.Balance = 0;
            requestmodel.ImagePath = "~/images/" + requestmodel.ImagePath; 
            using IDbConnection connection = new SqlConnection(_connection.ConnectionString);
            connection.Open();
            string query1 = @"select * from Tbl_Wallet where WalletUserName = @WalletUserName or MobileNo = @MobileNo";
            var result = await connection.QueryFirstOrDefaultAsync(query1,requestmodel);
            if(result != null)
            {
                TempData["isSuccess"] = false;
                TempData["message"] = "Wallet Username or Mobile No is Already Register";
                goto Results;
            }
            string query = @"INSERT INTO [dbo].[Tbl_Wallet]
           ([WalletUserName]
           ,[FullName]
           ,[MobileNo]
           ,[Balance]
           ,[ImagePath])
     VALUES
           (@WalletUserName
           ,@FullName
           ,@MobileNo
           ,@Balance
           ,@ImagePath)";
          
            if (string.IsNullOrEmpty(requestmodel.WalletUserName))
            {
                TempData["isSuccess"] = false;
                TempData["message"] = "Wallet User Name Is Required";
                goto Results;
            }
            if (string.IsNullOrEmpty(requestmodel.FullName))
            {
                TempData["isSuccess"] = false;
                TempData["message"] = "Full Name Is Required";
                goto Results;
            }
            if (string.IsNullOrEmpty(requestmodel.MobileNo))
            {
                TempData["isSuccess"] = false;
                TempData["message"] = "Full Name Is Required";
                goto Results;
            }
            string pattern = @"^(09|\+959)\d{7,9}$";
            //if (!(requestmodel.MobileNo.Length <= 11) || !(requestmodel.MobileNo.Length >= 11)) 
            //{
            //    TempData["isSuccess"] = false;
            //    TempData["message"] = "Invalid Mobile Number";
            //    goto Results;
            //}
            if (!Regex.IsMatch(requestmodel.MobileNo, pattern))
            {
                TempData["isSuccess"] = false;
                TempData["message"] = "Invalid Mobile Number";
                goto Results;
            }
            var res = await connection.ExecuteAsync(query, requestmodel);
            if (res is 0)
            {
                goto Results;
            }
            connection.Close();
            TempData["isSuccess"] = true;
            TempData["message"] = "Successfully Register";
            return RedirectToAction("Index");
        Results:
            return View("Register");
        }
    }

    public class WalletModel
    {
        public int WalletId { get; set; }
        public string WalletUserName { get; set; }
        public string FullName { get; set; }
        public string? MobileNo { get; set; }
        public int? Balance { get; set; }
        public string? ImagePath { get; set; }
    }
}
