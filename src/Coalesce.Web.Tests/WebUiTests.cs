using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.PhantomJS;
using System;
using System.Diagnostics;
using System.Threading;
using Xunit;
using System.Management;
using Coalesce.Web.Tests.Helpers;

namespace Coalesce.Web.Tests
{
    public class WebUiTests : IDisposable
    {
        public const int Port = 5000;

        private Process _process;
        public WebUiTests()
        {
            _process = Processes.StartDotNet();
        }

        [Fact]
        public void BasicUiChrome()
        {
            IWebDriver driver = new ChromeDriver();
            try
            {
                RunUiTest(driver);
            }
            finally
            {
                driver.Dispose();
            }
        }

        [Fact]
        public void BasicUiHeadless()
        {
            IWebDriver driver = new PhantomJSDriver();
            try
            {
                RunUiTest(driver);
            }
            finally
            {
                driver.Dispose();
            }
        }

        private void RunUiTest(IWebDriver driver)
        {
            driver.Manage().Window.Maximize();
            //driver.Navigate().GoToUrl("http://coalesceweb.azurewebsites.net");
            driver.Navigate().GoToUrl($"http://localhost:{Port}");

            IWebElement generatedLink = driver.FindElement(By.PartialLinkText("Check out the Demo"));
            generatedLink.Click();

            IWebElement personLink = driver.FindElement(By.PartialLinkText("Person"));
            personLink.Click();
            driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(10));

            IWebElement editLink = driver.FindElement(By.CssSelector(@"table tr:nth-child(1) td:nth-child(11) > div > a"));
            editLink.Click();

            IWebElement nameInput = driver.FindElement(By.CssSelector(@"body > div.container.body-content > div > div.panel-body > div > div:nth-child(3) > div > input"));
            nameInput.Click();
            // This is only required for the chrome driver
            for (var i = 0; i < 20; i++) nameInput.SendKeys(Keys.Backspace);

            nameInput.Clear();
            nameInput.SendKeys("AdamTest");

            IWebElement nextInput = driver.FindElement(By.CssSelector(@"body > div.container.body-content > div > div.panel-body > div > div:nth-child(4) > div > input"));
            nextInput.Click();

            IWebElement loading = driver.FindElement(By.ClassName("label-info"));
            Thread.Sleep(3000);

            IWebElement backButton = driver.FindElement(By.CssSelector(@"body > div.container.body-content > div > div.panel-heading > div > button:nth-child(1)"));
            backButton.Click();

            Thread.Sleep(3000);

            driver.Navigate().Refresh();

            IWebElement nametd = driver.FindElement(By.CssSelector(@"body > div > div:nth-child(3) > div > table > tbody > tr:nth-child(1) > td:nth-child(2) > div"));
            Assert.Equal(nametd.Text, "AdamTest");

            // Table Edit
            IWebElement edit = driver.FindElement(By.CssSelector(@"body > div > div:nth-child(1) > div > div.btn-group.pull-right > a:nth-child(3)"));
            edit.Click();

            nameInput = driver.FindElement(By.CssSelector(@"body > div > div:nth-child(3) > div > table > tbody > tr:nth-child(1) > td:nth-child(2) > input"));
            nameInput.Click();
            // This is only required for the chrome driver
            for (var i = 0; i < 20; i++) nameInput.SendKeys(Keys.Backspace);
            nameInput.Clear();
            nameInput.SendKeys("Adam");

            nextInput = driver.FindElement(By.CssSelector(@"body > div > div:nth-child(3) > div > table > tbody > tr:nth-child(1) > td:nth-child(3) > input"));
            nextInput.Click();
            loading = driver.FindElement(By.ClassName("label-info"));
            Thread.Sleep(3000);

            IWebElement readonlyButton = driver.FindElement(By.CssSelector(@"body > div > div:nth-child(1) > div > div.btn-group.pull-right > a:nth-child(3)"));
            readonlyButton.Click();

            nametd = driver.FindElement(By.CssSelector(@"body > div > div:nth-child(3) > div > table > tbody > tr:nth-child(1) > td:nth-child(2) > div"));
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
