using EasySheba.Services;
using Microsoft.AspNetCore.Mvc;

namespace EasySheba.Controllers
{
    public class ContactController : Controller
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<ContactController> _logger;

        public ContactController(IEmailService emailService, ILogger<ContactController> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(ContactMessage message)
        {
            // Ensure we have a message (fallback to Request.Form if model binder failed)
            try
            {
                if (message == null) message = new ContactMessage();

                // Try to populate from Request.Form if any field is missing
                var form = Request.Form;
                if (string.IsNullOrWhiteSpace(message.Name) && form.ContainsKey("Name"))
                    message.Name = form["Name"].ToString();
                if (string.IsNullOrWhiteSpace(message.Email) && form.ContainsKey("Email"))
                    message.Email = form["Email"].ToString();
                if (string.IsNullOrWhiteSpace(message.Subject) && form.ContainsKey("Subject"))
                    message.Subject = form["Subject"].ToString();
                if (string.IsNullOrWhiteSpace(message.Message) && form.ContainsKey("Message"))
                    message.Message = form["Message"].ToString();

                _logger.LogInformation("Contact form submitted. Name: {Name}, Email: {Email}, Subject: {Subject}",
                    message.Name, message.Email, message.Subject);

                // Check required fields
                if (string.IsNullOrWhiteSpace(message.Name) ||
                    string.IsNullOrWhiteSpace(message.Email) ||
                    string.IsNullOrWhiteSpace(message.Subject) ||
                    string.IsNullOrWhiteSpace(message.Message))
                {
                    _logger.LogWarning("Contact form validation failed. Form keys: {Keys}", string.Join(',', form.Keys));
                    TempData["ContactError"] = "Please fill all required fields.";
                    return RedirectToAction("About", "Home");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while reading contact form data");
                TempData["ContactError"] = "Invalid form submission.";
                return RedirectToAction("About", "Home");
            }

            try
            {
                var (Success, ErrorMessage) = await _emailService.SendContactEmailAsync(message);
                if (Success)
                {
                    TempData["ContactSuccess"] = "Your message has been sent successfully! We'll get back to you soon.";
                }
                else
                {
                    var userMsg = "Failed to send message. Please try again later.";
                    // In development, include error detail
                    if (HttpContext.RequestServices.GetService(typeof(Microsoft.AspNetCore.Hosting.IWebHostEnvironment)) is Microsoft.AspNetCore.Hosting.IWebHostEnvironment env && env.IsDevelopment())
                    {
                        userMsg = ErrorMessage ?? userMsg;
                    }
                    TempData["ContactError"] = userMsg;
                    _logger.LogWarning("Contact form failed to send message from {Email}: {Error}", message.Email, ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                TempData["ContactError"] = "An error occurred while sending your message. Please try again later.";
                _logger.LogError(ex, "Exception when sending contact message from {Email}", message.Email);
            }

            return RedirectToAction("About", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> TestEmail()
        {
            try
            {
                var testMessage = new ContactMessage
                {
                    Name = "Test Patient",
                    Email = "test@example.com",
                    Subject = "Test Message",
                    Message = "This is a test message from EasySheba."
                };

                var (Success, ErrorMessage) = await _emailService.SendContactEmailAsync(testMessage);

                if (Success)
                {
                    return Content("✅ SUCCESS! Email sent to professionalhani7@gmail.com");
                }
                else
                {
                    return Content($"❌ FAILED! Could not send email. Error: {ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                return Content($"❌ ERROR: {ex.Message}");
            }
        }
    }
}