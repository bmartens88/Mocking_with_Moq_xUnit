using Moq;
using Moq.Protected;
using System.Collections.Generic;
using Xunit;

namespace CreditCardApplications.Tests
{
    public class CreditCardApplicationsEvaluatorShould
    {
        private Mock<IFrequentFlyerNumberValidator> mockValidator;
        private CreditCardApplicationEvaluator sut;

        public CreditCardApplicationsEvaluatorShould()
        {
            mockValidator = new();
            mockValidator.SetupAllProperties();
            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");
            mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);
            sut = new(mockValidator.Object);
        }

        [Fact]
        public void AcceptHighIncomeApplications()
        {
            //Mock<IFrequentFlyerNumberValidator> mockValidator = new();

            //CreditCardApplicationEvaluator sut = new(mockValidator.Object);

            CreditCardApplication application = new() { GrossAnnualIncome = 100_000 };

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.AutoAccepted, decision);
        }

        [Fact]
        public void ReferYoungApplications()
        {
            //Mock<IFrequentFlyerNumberValidator> mockValidator = new();
            mockValidator.DefaultValue = DefaultValue.Mock;

            //mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);

            //CreditCardApplicationEvaluator sut = new(mockValidator.Object);

            CreditCardApplication application = new() { Age = 19 };

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        [Fact]
        public void DeclineLowIncomeApplications()
        {
            //Mock<IFrequentFlyerNumberValidator> mockValidator = new();
            //mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");

            //mockValidator.Setup(x => x.IsValid("x")).Returns(true);
            //mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);
            //mockValidator.Setup(x => x.IsValid(It.Is<string>(number => number.StartsWith("y")))).Returns(true);
            //mockValidator.Setup(x => x.IsValid(It.IsInRange("a", "z", Range.Inclusive))).Returns(true);
            //mockValidator.Setup(x => x.IsValid(It.IsIn("z", "y", "x"))).Returns(true);
            mockValidator.Setup(x => x.IsValid(It.IsRegex("[a-z]"))).Returns(true);

            //CreditCardApplicationEvaluator sut = new(mockValidator.Object);

            CreditCardApplication application = new()
            {
                GrossAnnualIncome = 19_999,
                Age = 42,
                FrequentFlyerNumber = "y"
            };

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.AutoDeclined, decision);
        }

        [Fact]
        public void ReferInvalidFrequentFlyerApplications()
        {
            //Mock<IFrequentFlyerNumberValidator> mockValidator = new(); // use MockBehavior.Strict for strict mocking

            //mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");

            mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(false);

            CreditCardApplicationEvaluator sut = new(mockValidator.Object);

            CreditCardApplication application = new();

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        [Fact]
        public void DeclineLowIncomeApplicationOutDemo()
        {
            //Mock<IFrequentFlyerNumberValidator> mockValidator = new();

            bool isValid = true;
            mockValidator.Setup(x => x.IsValid(It.IsAny<string>(), out isValid));

            //CreditCardApplicationEvaluator sut = new(mockValidator.Object);

            CreditCardApplication application = new()
            {
                GrossAnnualIncome = 19_999,
                Age = 42
            };

            CreditCardApplicationDecision decision = sut.EvaluateUsingOut(application);

            Assert.Equal(CreditCardApplicationDecision.AutoDeclined, decision);
        }

        [Fact]
        public void ReferWhenLicenseKeyExpired()
        {
            //Mock<IFrequentFlyerNumberValidator> mockValidator = new();

            //mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);
            //mockValidator.Setup(x => x.LicenseKey).Returns(GetLicenseKeyExpiryString);
            //Mock<ILicenseData> mockLicenseData = new();
            //mockLicenseData.Setup(x => x.LicenseKey).Returns("EXPIRED");
            //Mock<IServiceInformation> mockServiceInfo = new();
            //mockServiceInfo.Setup(x => x.License).Returns(mockLicenseData.Object);

            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("EXPIRED");

            //CreditCardApplicationEvaluator sut = new(mockValidator.Object);

            CreditCardApplication application = new() { Age = 42 };

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        string GetLicenseKeyExpiryString()
        {
            // E.g. read from vendor-supplied constant file
            return "EXPIRED";
        }

        [Fact]
        public void UseDetailedLookupForOlderApplications()
        {
            //Mock<IFrequentFlyerNumberValidator> mockValidator = new();

            //mockValidator.SetupProperty(x => x.ValidationMode); // Instructs mock object to remember changes made to its properties
            //mockValidator.SetupAllProperties(); // For all properties; use BEFORE any other specific setup, as this will overwrite the specific setup!

            //mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");

            //CreditCardApplicationEvaluator sut = new(mockValidator.Object);

            CreditCardApplication application = new() { Age = 30 };

            sut.Evaluate(application);

            Assert.Equal(ValidationMode.Detailed, mockValidator.Object.ValidationMode);
        }

        [Fact]
        public void ValidateFrequentFlyerNumberForLowIncomeApplications()
        {
            //Mock<IFrequentFlyerNumberValidator> mockValidator = new();

            //mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");

            //CreditCardApplicationEvaluator sut = new(mockValidator.Object);

            CreditCardApplication application = new()
            {
                FrequentFlyerNumber = "q"
            };

            sut.Evaluate(application);

            mockValidator.Verify(x => x.IsValid(It.IsAny<string>()), "Frequent flyer numbers should be validated");
        }

        [Fact]
        public void NotValidateFrequentFlyerNumberForHighIncomeApplications()
        {
            //Mock<IFrequentFlyerNumberValidator> mockValidator = new();

            //mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");

            //CreditCardApplicationEvaluator sut = new(mockValidator.Object);

            CreditCardApplication application = new()
            {
                GrossAnnualIncome = 100_000
            };

            sut.Evaluate(application);

            mockValidator.Verify(x => x.IsValid(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void CheckLicenseKeyForLowIncomeApplications()
        {
            //Mock<IFrequentFlyerNumberValidator> mockValidator = new();

            //mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");

            //CreditCardApplicationEvaluator sut = new(mockValidator.Object);

            CreditCardApplication application = new() { GrossAnnualIncome = 99_000 };

            sut.Evaluate(application);

            mockValidator.VerifyGet(x => x.ServiceInformation.License.LicenseKey, Times.Once);
        }

        [Fact]
        public void SetDetailedLookupForOlderApplications()
        {
            //Mock<IFrequentFlyerNumberValidator> mockValidator = new();

            //mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");

            //CreditCardApplicationEvaluator sut = new(mockValidator.Object);

            CreditCardApplication application = new() { Age = 30 };

            sut.Evaluate(application);

            mockValidator.VerifySet(x => x.ValidationMode = It.IsAny<ValidationMode>(), Times.Once);

            //mockValidator.VerifyNoOtherCalls();
        }

        [Fact]
        public void ReferInvalidFrequentFlyerApplications_ReturnValuesSequence()
        {
            //Mock<IFrequentFlyerNumberValidator> mockValidator = new();
            //mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");
            mockValidator.SetupSequence(x => x.IsValid(It.IsAny<string>()))
                .Returns(false)
                .Returns(true);

            //CreditCardApplicationEvaluator sut = new(mockValidator.Object);

            CreditCardApplication application = new() { Age = 25 };

            CreditCardApplicationDecision firstDecision = sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, firstDecision);

            CreditCardApplicationDecision secondDecision = sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.AutoDeclined, secondDecision);
        }

        [Fact]
        public void ReferInvalidFrequentFlyerApplications_MultipleCallsSequence()
        {
            //Mock<IFrequentFlyerNumberValidator> mockValidator = new();
            //mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");

            var frequentFlyerNumbersPassed = new List<string>();
            mockValidator.Setup(x => x.IsValid(Capture.In(frequentFlyerNumbersPassed)));

            //CreditCardApplicationEvaluator sut = new(mockValidator.Object);

            CreditCardApplication application1 = new() { Age = 25, FrequentFlyerNumber = "aa" };
            CreditCardApplication application2 = new() { Age = 25, FrequentFlyerNumber = "bb" };
            CreditCardApplication application3 = new() { Age = 25, FrequentFlyerNumber = "cc" };

            sut.Evaluate(application1);
            sut.Evaluate(application2);
            sut.Evaluate(application3);

            // Assert that IsValid was called three times with "aa", "bb" and "cc"
            Assert.Equal(new List<string> { "aa", "bb", "cc" }, frequentFlyerNumbersPassed);
        }

        [Fact]
        public void ReferFraudRisk()
        {
            //Mock<IFrequentFlyerNumberValidator> mockValidator = new();

            Mock<FraudLookup> mockFraudLookup = new();

            //mockFraudLookup.Setup(x => x.IsFraudRisk(It.IsAny<CreditCardApplication>()))
            //    .Returns(true);
            mockFraudLookup.Protected()
                .Setup<bool>("CheckApplication", ItExpr.IsAny<CreditCardApplication>())
                .Returns(true);

            CreditCardApplicationEvaluator sut = new(mockValidator.Object, mockFraudLookup.Object);

            CreditCardApplication application = new();

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHumanFraudRisk, decision);
        }

        [Fact]
        public void LinqToMocks()
        {
            //Mock<IFrequentFlyerNumberValidator> mockValidator = new();
            //mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");
            //mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);

            IFrequentFlyerNumberValidator mockValidator = Mock.Of<IFrequentFlyerNumberValidator>(
                validator =>
                    validator.ServiceInformation.License.LicenseKey == "OK" &&
                    validator.IsValid(It.IsAny<string>()));

            CreditCardApplicationEvaluator sut = new(mockValidator);

            CreditCardApplication application = new() { Age = 25 };

            CreditCardApplicationDecision decision = sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.AutoDeclined, decision);
        }

        // DEMO FOR MOCKING ASYNCHRONOUS CODE

        //public interface IDemoInterfaceAsync
        //{
        //    Task StartAsync();
        //    Task<int> StopAsync();
        //}

        //var mock = new Mock<IDemoInterfaceAsync>();
        //mock.Setup(x => x.StartAsync()).Returns(Task.CompletedTask);

        //mock.Setup(x => x.StopAsync()).Returns(Task.FromResult(42));
        //mock.Setup(x => x.StopAsync()).ReturnsAsync(42);
    }
}
