using Coalesce.Domain;
using Intellitect.ComponentModel.Validation;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.PhantomJS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Coalesce.Web.Tests
{
    public class WebUiTests : IDisposable
    {
        public const int Port = 5000;

        private Process _process;
        public WebUiTests()
        {
            var startInfo = new ProcessStartInfo();
            startInfo.Arguments = $@"run -p ..\..\..\..\..\Coalesce.Web\";
            startInfo.FileName = "dotnet";
            startInfo.WorkingDirectory = @"..\..\..\..\..\Coalesce.Web";
            _process = Process.Start(startInfo);
            // Give it a few seconds to start up.
            System.Threading.Thread.Sleep(7000);
        }

        //[Fact]
        public void BasicUiChrome()
        {
            IWebDriver driver = new ChromeDriver(@"..\..\..\");

            RunUiTest(driver);
        }

        //[Fact]
        public void BasicUiHeadless()
        {
            IWebDriver driver = new PhantomJSDriver(@"..\..\..\");

            RunUiTest(driver);
        }

        private void RunUiTest(IWebDriver driver)
        {
            driver.Manage().Window.Maximize();
            //driver.Navigate().GoToUrl("http://coalesceweb.azurewebsites.net");
            driver.Navigate().GoToUrl($"http://localhost:{Port}");

            IWebElement generatedLink = driver.FindElement(By.PartialLinkText("Generated Views"));
            generatedLink.Click();

            IWebElement personLink = driver.FindElement(By.PartialLinkText("Person"));
            personLink.Click();
            driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(10));

            IWebElement editLink = driver.FindElement(By.CssSelector(@"table tr:nth-child(1) td:nth-child(15) > div > a"));
            editLink.Click();

            IWebElement nameInput = driver.FindElement(By.CssSelector(@"body > div.container.body-content > div > div.panel-body > div > div:nth-child(2) > div > input"));
            nameInput.Click();
            // This is only required for the chrome driver
            for (var i = 0; i < 20; i++) nameInput.SendKeys(Keys.Backspace);

            nameInput.Clear();
            nameInput.SendKeys("AdamTest");

            IWebElement nextInput = driver.FindElement(By.CssSelector(@"body > div.container.body-content > div > div.panel-body > div > div:nth-child(3) > div > input"));
            nextInput.Click();

            IWebElement loading = driver.FindElement(By.ClassName("label-info"));
            Thread.Sleep(3000);

            IWebElement backButton = driver.FindElement(By.CssSelector(@"body > div.container.body-content > div > div.panel-heading > div > button:nth-child(1)"));
            backButton.Click();

            driver.Navigate().Refresh();

            IWebElement nametd = driver.FindElement(By.CssSelector(@"body > div.containerfluid.body-content > div > table > tbody > tr:nth-child(1) > td:nth-child(2) > div"));
            Assert.Equal(nametd.Text, "AdamTest");

            // Table Edit
            IWebElement edit = driver.FindElement(By.CssSelector(@"body > div.containerfluid.body-content > div > div > div.btn-group.pull-right > a:nth-child(3)"));
            edit.Click();

            nameInput = driver.FindElement(By.CssSelector(@"body > div.containerfluid.body-content > div > table > tbody > tr:nth-child(1) > td:nth-child(2) > input"));
            nameInput.Click();
            // This is only required for the chrome driver
            for (var i = 0; i < 20; i++) nameInput.SendKeys(Keys.Backspace);
            nameInput.Clear();
            nameInput.SendKeys("Adam");

            nextInput = driver.FindElement(By.CssSelector(@"body > div.containerfluid.body-content > div > table > tbody > tr:nth-child(1) > td:nth-child(3) > input"));
            nextInput.Click();
            loading = driver.FindElement(By.ClassName("label-info"));
            Thread.Sleep(3000);

            IWebElement readonlyButton = driver.FindElement(By.CssSelector(@"body > div.containerfluid.body-content > div > div > div.btn-group.pull-right > a:nth-child(3)"));
            readonlyButton.Click();

            nametd = driver.FindElement(By.CssSelector(@"body > div.containerfluid.body-content > div > table > tbody > tr:nth-child(1) > td:nth-child(2) > div"));
            Assert.Equal(nametd.Text, "Adam");

            // Show the final results.
            Thread.Sleep(1000);

            driver.Close();

            driver.Quit();

        }


        public void Dispose()
        {
            try
            {
                _process.Kill();
            }
            catch { }
            try
            {
                _process.Dispose();
            }
            catch { }
        }
    }
}
