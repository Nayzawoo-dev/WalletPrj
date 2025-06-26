using System.Data;
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
      ,[Image]
  FROM [dbo].[Tbl_Wallet] Where WalletUserName = @WalletUserName and MobileNo = @MobileNo";
            var res = await connection.QueryFirstOrDefaultAsync<WalletModel>(query,requestmodel);
            if(res is null)
            {
                TempData["isSuccess"] = false;
                TempData["message"] = "Incorrect Username or Password";
                return RedirectToAction("Index");
            }
            return View(connection);
        }
    }

    public class WalletModel
    {
        public int WalletId { get; set; }
        public string WalletUserName { get; set; }
        public string FullName { get; set; }
        public string MobileNo { get; set; }
        public int Balance { get; set; }
        public string Image {  get; set; }
    }
}
