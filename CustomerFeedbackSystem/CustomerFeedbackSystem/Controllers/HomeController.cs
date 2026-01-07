using System.Diagnostics;
using CustomerFeedbackSystem.Models;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerFeedbackSystem.Controllers;

/// <summary>
/// ����
/// </summary>
/// <param name="logger">log������</param>
/// <param name="context">��Ʈw�d�ߪ���</param>
/// <param name="hostingEnvironment">���������ܼ�</param>
public class HomeController(ILogger<HomeController> logger, DocControlContext context, IWebHostEnvironment hostingEnvironment) : BaseController(context, hostingEnvironment)
{

    /// <summary>
    /// mockup page
    /// </summary>
    /// <returns></returns>
    public IActionResult PeoplePurchaseDemo()
    {
        return View();
    }

    /// <summary>
    /// �n�J��P���W�����J�f�e��
    /// </summary>
    /// <returns></returns>
    [Route("[controller]")]
    [Route("/Welcome")]
    [Route("/")]
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// �w��t�Ω|���i�J�e��
    /// </summary>
    /// <returns></returns>
    [Route("/Control")]
    [Route("/Purchase")]
    [Route("/Control/Index")]
    [Route("/Purchase/Index")]
    [Authorize(Roles = DocRoleStrings.Anyone + "," + PurchaseRoleStrings.Anyone)]
    public IActionResult SystemIndex()
    {
        var path = HttpContext.Request.Path.Value?.ToLowerInvariant();

        switch (path)
        {
            case "/control/index":
                ViewData["Title"] = "���޲z�t��";
                TempData["Menu"] = "Document";
                break;
            case "/purchase/index":
                ViewData["Title"] = "�q�l���ʨt��";
                TempData["Menu"] = "Purchase";
                break;
        }

        return View();
    }

}
