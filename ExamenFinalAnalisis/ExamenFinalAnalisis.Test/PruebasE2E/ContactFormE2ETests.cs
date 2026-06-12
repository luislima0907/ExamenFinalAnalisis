using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace ExamenFinalAnalisis.Test.PruebasE2E;

public class ContactFormE2ETests : IDisposable
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;
    private const string BaseUrl = "http://localhost:5041";

    public ContactFormE2ETests()
    {
        var options = new ChromeOptions();
        // -- Sin --headless para ver el navegador en acción --
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--start-maximized"); // abre maximizado
        _driver = new ChromeDriver(options);
        _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void ContactForm_FillAndSubmit_ShowsSuccessMessage()
    {
        _driver.Navigate().GoToUrl($"{BaseUrl}/Home/Contact");
        _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("nombre")));

        Thread.Sleep(500); // pausa para que se vea la página cargada

        // Llena campo por campo con pausa entre cada uno
        var nombre = _driver.FindElement(By.Id("nombre"));
        nombre.Click();
        nombre.SendKeys("Luis Test");
        Thread.Sleep(400);

        var email = _driver.FindElement(By.Id("email"));
        email.Click();
        email.SendKeys("luis@test.com");
        Thread.Sleep(400);

        var mensaje = _driver.FindElement(By.Id("mensaje"));
        mensaje.Click();
        mensaje.SendKeys("Mensaje de prueba E2E automatizado con Selenium");
        Thread.Sleep(600);

        // Click en el botón
        var btn = _driver.FindElement(By.Id("btn-enviar"));
        btn.Click();
        Thread.Sleep(500);

        // Verifica el mensaje de éxito
        _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("form-success")));
        var successMsg = _driver.FindElement(By.Id("form-success"));
        Assert.True(successMsg.Displayed);

        Thread.Sleep(1500); // pausa para ver el mensaje antes de cerrar
    }

    [Fact]
    public void HomePage_NavigateToAbout_Works()
    {
        _driver.Navigate().GoToUrl($"{BaseUrl}/");
        _wait.Until(ExpectedConditions.ElementIsVisible(By.LinkText("Acerca de")));

        Thread.Sleep(600);

        _driver.FindElement(By.LinkText("Acerca de")).Click();

        _wait.Until(ExpectedConditions.UrlContains("/Home/About"));
        Thread.Sleep(800); // pausa para ver la página About cargada

        Assert.Contains("/Home/About", _driver.Url);
    }

    public void Dispose()
    {
        _driver.Quit();
        _driver.Dispose();
    }
}