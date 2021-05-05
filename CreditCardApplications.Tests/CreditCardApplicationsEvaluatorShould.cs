using Moq;
using Xunit;

namespace CreditCardApplications.Tests
{
    public class CreditCardApplicationsEvaluatorShould
    {
        [Fact]
        public void AcceptHighIncomeApplications()
        {
            Mock<IFrequentFlyerNumberValidator> mockValidator = new();

            CreditCardApplicationEvaluator sut = new(mockValidator.Object);

            CreditCardApplication application = new() { GrossAnnualIncome = 100_000 };

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.AutoAccepted, decision);
        }

        [Fact]
        public void ReferYoungApplications()
        {
            Mock<IFrequentFlyerNumberValidator> mockValidator = new();
            mockValidator.DefaultValue = DefaultValue.Mock;

            mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);

            CreditCardApplicationEvaluator sut = new(mockValidator.Object);

            CreditCardApplication application = new() { Age = 19 };

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        [Fact]
        public void DeclineLowIncomeApplications()
        {
            Mock<IFrequentFlyerNumberValidator> mockValidator = new();
            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");

            //mockValidator.Setup(x => x.IsValid("x")).Returns(true);
            //mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);
            //mockValidator.Setup(x => x.IsValid(It.Is<string>(number => number.StartsWith("y")))).Returns(true);
            //mockValidator.Setup(x => x.IsValid(It.IsInRange("a", "z", Range.Inclusive))).Returns(true);
            //mockValidator.Setup(x => x.IsValid(It.IsIn("z", "y", "x"))).Returns(true);
            mockValidator.Setup(x => x.IsValid(It.IsRegex("[a-z]"))).Returns(true);

            CreditCardApplicationEvaluator sut = new(mockValidator.Object);

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
            Mock<IFrequentFlyerNumberValidator> mockValidator = new(); // use MockBehavior.Strict for strict mocking

            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");

            mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(false);

            CreditCardApplicationEvaluator sut = new(mockValidator.Object);

            CreditCardApplication application = new();

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        [Fact]
        public void DeclineLowIncomeApplicationOutDemo()
        {
            Mock<IFrequentFlyerNumberValidator> mockValidator = new();

            bool isValid = true;
            mockValidator.Setup(x => x.IsValid(It.IsAny<string>(), out isValid));

            CreditCardApplicationEvaluator sut = new(mockValidator.Object);

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
            Mock<IFrequentFlyerNumberValidator> mockValidator = new();

            mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);
            //mockValidator.Setup(x => x.LicenseKey).Returns(GetLicenseKeyExpiryString);
            //Mock<ILicenseData> mockLicenseData = new();
            //mockLicenseData.Setup(x => x.LicenseKey).Returns("EXPIRED");
            //Mock<IServiceInformation> mockServiceInfo = new();
            //mockServiceInfo.Setup(x => x.License).Returns(mockLicenseData.Object);

            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("EXPIRED");

            CreditCardApplicationEvaluator sut = new(mockValidator.Object);

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
            Mock<IFrequentFlyerNumberValidator> mockValidator = new();

            //mockValidator.SetupProperty(x => x.ValidationMode); // Instructs mock object to remember changes made to its properties
            mockValidator.SetupAllProperties(); // For all properties; use BEFORE any other specific setup, as this will overwrite the specific setup!

            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");

            CreditCardApplicationEvaluator sut = new(mockValidator.Object);

            CreditCardApplication application = new() { Age = 30 };

            sut.Evaluate(application);

            Assert.Equal(ValidationMode.Detailed, mockValidator.Object.ValidationMode);
        }

        [Fact]
        public void ValidateFrequentFlyerNumberForLowIncomeApplications()
        {
            Mock<IFrequentFlyerNumberValidator> mockValidator = new();

            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");

            CreditCardApplicationEvaluator sut = new(mockValidator.Object);

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
            Mock<IFrequentFlyerNumberValidator> mockValidator = new();

            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");

            CreditCardApplicationEvaluator sut = new(mockValidator.Object);

            CreditCardApplication application = new()
            {
                GrossAnnualIncome = 100_000
            };

            sut.Evaluate(application);

            mockValidator.Verify(x => x.IsValid(It.IsAny<string>()), Times.Never);
        }
    }
}
