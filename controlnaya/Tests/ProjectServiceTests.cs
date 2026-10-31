using Microsoft.VisualStudio.TestTools.UnitTesting;
using Server.Services;

namespace Server.Tests;

[TestClass]
public class ProjectServiceTests
{
    [TestMethod]
    public void Create_AssignsIdAndActiveStatus()
    {
        var service = new ProjectService();

        var project = service.Create("Учебный сайт", "csharp");

        Assert.AreEqual(1, project.Id);
        Assert.AreEqual("active", project.Status);
        Assert.AreEqual("csharp", project.Technology);
    }

    [TestMethod]
    public void Create_RejectsShortTitle()
    {
        var service = new ProjectService();

        Assert.ThrowsException<ArgumentException>(() => service.Create("Test", "python"));
    }

    [TestMethod]
    public void Update_ChangesAllFields()
    {
        var service = new ProjectService();
        var project = service.Create("Сайт кафедры", "csharp");

        bool updated = service.Update(project.Id, "Мобильное приложение", "python", "archived");
        var result = service.GetById(project.Id);

        Assert.IsTrue(updated);
        Assert.AreEqual("Мобильное приложение", result!.Title);
        Assert.AreEqual("python", result.Technology);
        Assert.AreEqual("archived", result.Status);
    }

    [TestMethod]
    public void GetTechnologyCounts_ReturnsCountsByTechnology()
    {
        var service = new ProjectService();
        service.Create("Проект на CSharp", "csharp");
        service.Create("Проект на Python", "python");
        service.Create("Ещё Python проект", "python");

        var counts = service.GetTechnologyCounts();

        Assert.AreEqual(1, counts["csharp"]);
        Assert.AreEqual(2, counts["python"]);
        Assert.AreEqual(0, counts["javascript"]);
    }
}
