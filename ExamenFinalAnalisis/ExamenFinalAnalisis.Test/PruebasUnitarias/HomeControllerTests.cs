using ExamenFinalAnalisis.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace ExamenFinalAnalisis.Test.PruebasUnitarias;

public class HomeControllerTests
{
    [Fact]
    public void Index_ReturnsViewResult()
    {
        var logger = Mock.Of<ILogger<HomeController>>();
        var controller = new HomeController(logger);
        var result = controller.Index();
        Assert.IsType<ViewResult>(result);
    }
}