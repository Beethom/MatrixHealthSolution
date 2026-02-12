using Microsoft.AspNetCore.Mvc;
using MatrixHealthSolution.Models.ViewModels;
using MatrixHealthSolution.Services;

namespace MatrixHealthSolution.Controllers;

public class ContactController : Controller
{
    private readonly EmailService _email;
    private readonly IConfiguration _config;

    public ContactController(EmailService email, IConfiguration config)
    {
        _email = email;
        _config = config;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(new ContactVM());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ContactVM vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var adminEmail = _config["Email:Admin"];

        var html = $@"
            <h2>New Contact Message</h2>
            <p><strong>Name:</strong> {vm.Name}</p>
            <p><strong>Email:</strong> {vm.Email}</p>
            <p><strong>Subject:</strong> {vm.Subject}</p>
            <hr />
            <p>{vm.Message.Replace("\n", "<br/>")}</p>
        ";

        await _email.SendAsync(
            adminEmail!,
            $"Contact Form: {vm.Subject}",
            html
        );

        TempData["Success"] = "Your message has been sent. We'll get back to you soon.";
        return RedirectToAction(nameof(Index));
    }
}
