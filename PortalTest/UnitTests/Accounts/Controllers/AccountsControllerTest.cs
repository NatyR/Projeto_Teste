using Accounts.API.Entities;
using Accounts.API.Interfaces.Repositories;
using Accounts.API.Interfaces.Services;
using Accounts.API.Services;
using Cards.API.Controllers;
using Google.Cloud.BigQuery.V2;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace PortalTest.UnitTests.Accounts.Controllers
{
    public class AccountsControllerTest
    {
        AccountsController _accountsController;
        Mock<IAccountService> _accountsService;
        Mock<ISolicitationService> _solicitationService;
        Mock<IAccessRepositoryAccounts> _accessRepositoryAccounts;
        Mock<IHttpServiceAccounts> _httpServiceAccounts;

        public AccountsControllerTest()
        {
            _accountsService = new Mock<IAccountService>();
            _solicitationService = new Mock<ISolicitationService>();
            _accessRepositoryAccounts = new Mock<IAccessRepositoryAccounts>();
            _httpServiceAccounts = new Mock<IHttpServiceAccounts>();

            _accountsController = new AccountsController(
                                  _accountsService.Object, 
                                  _solicitationService.Object, 
                                  _httpServiceAccounts.Object, 
                                  _accessRepositoryAccounts.Object);
        }


        [Fact]
        public async Task ReportDismissalDiscount_ShouldReturn200_Happy_Path()
        {
            long? shopId = 23142;
            long? groupId = 1318;
            string filter = System.String.Empty;
            DateTime? dtStart = null;
            DateTime? dtEnd = null;
            int? page = null;
            int? pageSize = null;
            int userId = 240;
            string userMail = "renata.silva@zappts.com.br";
            string userType = "Master";

            List<object> mockList = new();
            
            ReportDismissalDiscount report = new()
            {
                CodigoRetorno = 0,
                MensagemRetorno = "OPERAÇÃO REALIZADA COM SUCESSO",
                TotalPaginas = 1,
                Lista = mockList,
            };

            var claims = new List<Claim>
            {
                new Claim("id", "245"),
                new Claim("userType", "Master"),
                new Claim(ClaimTypes.Email, "renata.silva@zappts.com.br"),
            };

            var identity = new ClaimsIdentity(claims, "test");
            var user = new ClaimsPrincipal(identity);

            _accountsController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user
                }
            };

            _accountsService.Setup(_ => _.ReportDismissalDiscount(shopId, groupId, dtStart, dtEnd, page, pageSize, userId, userMail, userType,filter)).ReturnsAsync(report);
            
            var result = await _accountsController.ReportDismissalDiscount(shopId, groupId, filter, dtStart, dtEnd, page, pageSize);

            var actionResult = Assert.IsType<ActionResult<List<ReportDismissalDiscount>>>(result);
            var objectResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Equal((int)HttpStatusCode.OK, objectResult.StatusCode);

        }


        [Fact]
        public async Task ReportDismissalDiscount_ShouldReturn400_When_There_Is_No_Token()
        {
            long? shopId = 23142;
            long? groupId = 1318;
            string filter = System.String.Empty;
            DateTime? dtStart = null;
            DateTime? dtEnd = null;
            int? page = null;
            int? pageSize = null;
            int userId = 240;
            string userMail = "renata.silva@zappts.com.br";
            string userType = "Master";

            List<object> mockList = new();

            ReportDismissalDiscount report = new()
            {
                CodigoRetorno = 0,
                MensagemRetorno = "",
                TotalPaginas = 1,
                Lista = mockList,
            };

            var result = await _accountsController.ReportDismissalDiscount(shopId, groupId, filter, dtStart, dtEnd, page, pageSize);

            _accountsService.Verify(_ => _.ReportDismissalDiscount(shopId, groupId, dtStart, dtEnd, page, pageSize, userId, userMail, userType, filter), Times.Never());

            var actionResult = Assert.IsType<ActionResult<List<ReportDismissalDiscount>>>(result);
            var objectResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            Assert.Equal((int)HttpStatusCode.BadRequest, objectResult.StatusCode);

        }


        [Fact]
        public async Task ReportDismissalCSV_ShouldReturn400_When_Error_Generating_File()
        {
            long? shopId = 23142;
            long? groupId = 1318;
            string filter = System.String.Empty;
            DateTime? dtStart = null;
            DateTime? dtEnd = null;
            int? page = null;
            int? pageSize = null;
            int userId = 240;
            string userMail = "renata.silva@zappts.com.br";
            string userType = "Master";

            List<object> mockList = new();

            ReportDismissalDiscount report = new()
            {
                CodigoRetorno = 0,
                MensagemRetorno = "OPERAÇÃO REALIZADA COM SUCESSO",
                TotalPaginas = 1,
                Lista = mockList,
            };

            var claims = new List<Claim>
            {
                new Claim("id", "245"),
                new Claim("userType", "Master"),
                new Claim(ClaimTypes.Email, "renata.silva@zappts.com.br"),
            };

            var identity = new ClaimsIdentity(claims, "test");
            var user = new ClaimsPrincipal(identity);

            _accountsController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user
                }
            };

            var preamble = new byte[] { 100, 100, 100 };
            var byteArray = new byte[] { 60, 70, 80 };

            var combinedBytes = new byte[preamble.Length + byteArray.Length];
            Array.Copy(preamble, combinedBytes, preamble.Length);
            Array.Copy(byteArray, 0, combinedBytes, preamble.Length, byteArray.Length);

            var expectedMemoryStream = new MemoryStream(combinedBytes)
            {
                Position = 0
            };

            _accountsService.Setup(s => s.ReportDismissalCSV(shopId, groupId, dtStart, dtEnd, userId, userMail, userType, filter)).ReturnsAsync(expectedMemoryStream); 

            var result = await _accountsController.ReportDismissalCSV(shopId, groupId, filter, dtStart, dtEnd);


            var actionResult = Assert.IsType <BadRequestObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.BadRequest, actionResult.StatusCode);
        }




    }
}
