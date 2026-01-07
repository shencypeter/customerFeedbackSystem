using CustomerFeedbackSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace CustomerFeedbackSystem.Controllers
{

    /// <summary>
    /// ���~�B�z���
    /// </summary>
    /// <param name="logger">log������</param>
    /// <param name="context">��Ʈw�d�ߪ���</param>
    public class ErrorController(ILogger<HomeController> logger, DocControlContext context, IWebHostEnvironment hostingEnvironment) : BaseController(context, hostingEnvironment)
    {

        [Route("Error/{statusCode?}")]
        public IActionResult Index(int? statusCode)
        {
            var sc = statusCode ?? HttpContext.Response?.StatusCode ?? 500;

            // �ШD���T�A���O�~�JError���}�A�^�h����
            if (sc == 200)
            {
                return RedirectToAction("Index", "Home");
            }

            Response.StatusCode = sc; // ���^���X���T
            var vm = ErrorViewModel.FromStatusCode(sc);
            return View("_Error", vm);
        }
    }

    /*
    [Route("Error/{code}")]
    public IActionResult Error(int code)
    {
        return code switch
        {
            401 => View("~/Views/Shared/_Unauthorized.cshtml"),
            403 => View("~/Views/Shared/_Forbidden.cshtml"),
            404 => View("~/Views/Shared/_NotFound.cshtml"),
            500 => View("~/Views/Shared/_InternalServerError.cshtml"),
            _ => View("~/Views/Shared/_Error.cshtml")
        };
    }*/

}
