using ASP_KN_P_212.Models;
using ASP_KN_P_212.Services.Hash;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ASP_KN_P_212.Controllers
{
    public class HomeController : Controller
    {
        /* �������� ������ (�����������) - ����� � ����������
        �� �������� �������� �� ������� ��'����. �������
        �������������� ����� �������� - ����� �����������. �� 
        ��������, ��-�����, ��������� ���� �� ������ (readonly) ��,
        ��-�����, ������������ ��������� ��'���� ��� ��������
        �����������. � ���������� ����� �������� ������������� 
        �� ����� ��������� (_logger)
        */
        private readonly ILogger<HomeController> _logger;
        // ��������� ���� ��� ��������� �� �����
        private readonly IHashService _hashService;

        // ������ �� ������������ ��������-��������� � �������� �� � ��
        public HomeController(ILogger<HomeController> logger, IHashService hashService)
        {
            _logger = logger;           // ���������� ��������� �����������, �� ��
            _hashService = hashService; // ������ ��������� ��� �������� ����������
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Intro()
        {
            return View();
        }
        public IActionResult Ioc()  // Inversion of Control
        {
            // ����������� ������������ �������
            ViewData["hash"] = _hashService.Digest("Hello, World!");
            // ViewData - ����������� ��'��� ��� �������� �����
            // �� �������������. ���� ����� �� ������ ["hash"]
            // ����� ���������� � ��������� �������
            return View();
        }

        public IActionResult AboutRazor()
        {
            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
